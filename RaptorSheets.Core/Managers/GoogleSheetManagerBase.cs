using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models;
using RaptorSheets.Core.Registries;
using RaptorSheets.Core.Services;

namespace RaptorSheets.Core.Managers;

/// <summary>
/// Shared construction/plumbing for domain-specific Google Sheet managers (Gig, Stock, and future
/// domains). Holds the service + logger and the three constructors so no domain package hand-rolls
/// that boilerplate. Domain managers should inherit the generic
/// <see cref="GoogleSheetManagerBase{TEntity}"/> below, which adds the registry-backed read/metadata/
/// heal/layout orchestration; this non-generic base exists only to share the constructor plumbing.
/// </summary>
public abstract class GoogleSheetManagerBase
{
    protected readonly IGoogleSheetService _googleSheetService;
    protected readonly ILogger _logger;

    protected GoogleSheetManagerBase(IGoogleSheetService googleSheetService, ILogger? logger = null)
    {
        _googleSheetService = googleSheetService ?? throw new ArgumentNullException(nameof(googleSheetService));
        _logger = logger ?? NullLogger.Instance;
    }

    protected GoogleSheetManagerBase(string accessToken, string spreadsheetId, ILogger? logger = null)
        : this(new GoogleSheetService(accessToken, spreadsheetId, logger), logger)
    {
    }

    protected GoogleSheetManagerBase(Dictionary<string, string> parameters, string spreadsheetId, ILogger? logger = null)
        : this(new GoogleSheetService(parameters, spreadsheetId, logger), logger)
    {
    }
}

/// <summary>
/// Generic, registry-backed base for a domain's Google Sheet manager. Each domain package (Gig,
/// Stock, and future Job/Home) supplies its own strongly-typed <typeparamref name="TEntity"/>, its
/// <see cref="SheetRegistry{TEntity}"/> (headers, row mapping, missing-column detection), and its
/// canonical ordered sheet-name list; in return it inherits the entire read/metadata/heal/layout
/// surface implemented once here - GetSheets orchestration, property/tab-name lookups, layout access,
/// unknown-tab/header checks, and missing-column healing - instead of re-copying it per domain.
///
/// Only the genuinely domain-specific pieces remain in each domain manager: how missing sheets get
/// (re)created (<see cref="CreateMissingSheetsAsync"/>), and write operations that need domain
/// request-builders (CreateSheets ordering, ChangeSheetData, DeleteSheets).
/// </summary>
/// <typeparam name="TEntity">The domain's top-level SheetEntity type.</typeparam>
public abstract class GoogleSheetManagerBase<TEntity> : GoogleSheetManagerBase
    where TEntity : class, ISheetEntity, new()
{
    protected readonly SheetRegistry<TEntity> _registry;
    protected readonly List<string> _canonicalSheetNames;

    protected GoogleSheetManagerBase(IGoogleSheetService googleSheetService, SheetRegistry<TEntity> registry,
        List<string> canonicalSheetNames, ILogger? logger = null)
        : base(googleSheetService, logger)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _canonicalSheetNames = canonicalSheetNames ?? throw new ArgumentNullException(nameof(canonicalSheetNames));
    }

    protected GoogleSheetManagerBase(string accessToken, string spreadsheetId, SheetRegistry<TEntity> registry,
        List<string> canonicalSheetNames, ILogger? logger = null)
        : base(accessToken, spreadsheetId, logger)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _canonicalSheetNames = canonicalSheetNames ?? throw new ArgumentNullException(nameof(canonicalSheetNames));
    }

    protected GoogleSheetManagerBase(Dictionary<string, string> parameters, string spreadsheetId, SheetRegistry<TEntity> registry,
        List<string> canonicalSheetNames, ILogger? logger = null)
        : base(parameters, spreadsheetId, logger)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _canonicalSheetNames = canonicalSheetNames ?? throw new ArgumentNullException(nameof(canonicalSheetNames));
    }

    /// <summary>
    /// Domain-specific creation of sheets found missing entirely during <see cref="GetSheets"/>'s
    /// self-heal path. Given a title-&gt;desiredIndex map, creates them and returns the resulting
    /// entity (used to detect creation errors and to build the "created, please retry" message).
    /// Gig supplies its ordered/indexed CreateSheets; Stock supplies its enum-based CreateSheets.
    /// </summary>
    protected abstract Task<TEntity> CreateMissingSheetsAsync(Dictionary<string, int> missingIndexMap);

    #region Read

    /// <summary>
    /// Shared "get sheets" orchestration: batchGet -&gt; (self-heal missing sheets on failure) -&gt;
    /// unknown-tab detection -&gt; map data -&gt; auto-heal missing columns -&gt; spreadsheet name.
    /// Every domain manager inherits this directly; only the sheet-name type at its public API
    /// boundary and how missing sheets get (re)created differ per domain.
    /// </summary>
    /// <param name="sheetNames">Sheet names to fetch (provider/tab names, not domain enum values).</param>
    public async Task<TEntity> GetSheets(List<string> sheetNames)
    {
        var data = new TEntity();
        var messages = new List<MessageEntity>();
        var stringSheetList = string.Join(", ", sheetNames);

        var response = await _googleSheetService.GetBatchData(sheetNames, null);
        Spreadsheet? spreadsheetInfo = null;

        if (response == null)
        {
            try
            {
                spreadsheetInfo = await _googleSheetService.GetSheetInfo();

                if (spreadsheetInfo == null)
                {
                    _logger.LogWarning("Unable to fetch spreadsheet metadata; skipping missing-sheet restoration");
                }
                else
                {
                    var missingIndexMap = SheetInitializationHelper.GetMissingSheets(spreadsheetInfo, _canonicalSheetNames);

                    if (missingIndexMap.Count > 0)
                    {
                        var createResult = await CreateMissingSheetsAsync(missingIndexMap);

                        if (createResult.Messages.Any(m => m.Level == MessageLevelEnum.ERROR.GetDescription()))
                        {
                            messages.AddRange(createResult.Messages);
                            var errorReturn = new TEntity();
                            errorReturn.Messages.AddRange(messages);
                            return errorReturn;
                        }

                        var createdNames = string.Join(", ", missingIndexMap.Keys);
                        var info = MessageHelpers.CreateInfoMessage(
                            $"Created missing sheets: {createdNames}. Sheets may take a few seconds to become readable — please retry the request shortly.",
                            MessageTypeEnum.GET_SHEETS);

                        var createdReturn = new TEntity();
                        createdReturn.Messages.Add(info);
                        return createdReturn;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while restoring missing sheets");
            }
        }

        if (response == null)
        {
            messages.Add(MessageHelpers.CreateErrorMessage($"Unable to retrieve sheet(s): {stringSheetList}", MessageTypeEnum.GET_SHEETS));
            data.Messages.AddRange(messages);
            return data;
        }

        messages.Add(MessageHelpers.CreateInfoMessage($"Retrieved sheet(s): {stringSheetList}", MessageTypeEnum.GET_SHEETS));

        // Cheap metadata-only call (no ranges / no grid data) - used for unknown-tab detection and
        // the spreadsheet title. Known-sheet header validation already happens below via
        // registry.MapData using the header row already present in the batchGet response.
        spreadsheetInfo ??= await _googleSheetService.GetSheetInfo();

        if (spreadsheetInfo != null)
        {
            messages.AddRange(_registry.CheckUnknownSheets(spreadsheetInfo));
        }

        data = _registry.MapData(response) ?? new TEntity();

        await AutoHealMissingColumnsAsync(response, spreadsheetInfo, messages);

        if (spreadsheetInfo != null)
        {
            data.Properties.Name = spreadsheetInfo.Properties.Title;
        }

        data.Messages.AddRange(messages);

        return data;
    }

    /// <summary>
    /// Fetches every canonical sheet for this domain.
    /// </summary>
    public async Task<TEntity> GetAllSheets()
    {
        return await GetSheets(new List<string>(_canonicalSheetNames));
    }

    public async Task<Spreadsheet?> GetSpreadsheetInfo(List<string>? ranges = null)
    {
        return await _googleSheetService.GetSheetInfo(ranges);
    }

    public async Task<BatchGetValuesByDataFilterResponse?> GetBatchData(List<string> sheets)
    {
        return await _googleSheetService.GetBatchData(sheets);
    }

    #endregion

    #region Sheet Properties

    public async Task<List<PropertyEntity>> GetAllSheetProperties()
    {
        return await GetSheetProperties(new List<string>(_canonicalSheetNames));
    }

    public async Task<List<PropertyEntity>> GetSheetProperties(List<string> sheets)
    {
        var properties = new List<PropertyEntity>();

        // STEP 1: Get all existing sheet tab names first (no ranges parameter)
        var existingTabNames = await GetAllSheetTabNames();

        // STEP 2: Filter requested sheets to only those that exist
        var existingSheets = sheets.Where(requestedSheet =>
            existingTabNames.Any(existingTab =>
                string.Equals(requestedSheet, existingTab, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        // STEP 3: Build properties for all requested sheets
        foreach (var sheet in sheets)
        {
            var sheetExists = existingSheets.Any(s =>
                string.Equals(s, sheet, StringComparison.OrdinalIgnoreCase));

            if (sheetExists)
            {
                // Sheet exists - will process it below
                properties.Add(new PropertyEntity { Name = sheet });
            }
            else
            {
                // Sheet doesn't exist - return default property structure
                properties.Add(new PropertyEntity
                {
                    Name = sheet,
                    Id = "",  // Empty ID indicates sheet doesn't exist
                    Attributes = new Dictionary<string, string>
                    {
                        { PropertyEnum.HEADERS.GetDescription(), "" },
                        { PropertyEnum.MAX_ROW.GetDescription(), "1000" },
                        { PropertyEnum.MAX_ROW_VALUE.GetDescription(), "1" }
                    }
                });
            }
        }

        // STEP 4: Only request ranges for existing sheets
        if (existingSheets.Count > 0)
        {
            var combinedRanges = SheetPropertyHelper.BuildCombinedRanges(existingSheets);
            var sheetInfo = await _googleSheetService.GetSheetInfo(combinedRanges);

            // STEP 5: Process data for existing sheets only
            for (int i = 0; i < properties.Count; i++)
            {
                var property = properties[i];

                // Only process sheets that exist
                if (!string.IsNullOrEmpty(property.Id) || existingSheets.Any(s =>
                    string.Equals(s, property.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    var processedProperty = SheetPropertyHelper.ProcessSheetData(property.Name, sheetInfo, _logger);
                    properties[i] = processedProperty;
                }
            }
        }

        return properties;
    }

    /// <summary>
    /// Gets all sheet tab names directly from Google Sheets API.
    /// Uses spreadsheets.get method to retrieve sheet metadata efficiently.
    /// </summary>
    public async Task<List<string>> GetAllSheetTabNames()
    {
        var spreadsheetInfo = await _googleSheetService.GetSheetInfo();

        if (spreadsheetInfo?.Sheets == null)
        {
            return new List<string>();
        }

        return spreadsheetInfo.Sheets
            .Select(sheet => sheet.Properties.Title)
            .Where(title => !string.IsNullOrEmpty(title))
            .ToList();
    }

    #endregion

    #region Header Management / Column Healing

    /// <summary>
    /// Physically inserts columns detected as missing (via a domain's static CheckSheetHeaders
    /// out-overload or via <see cref="SheetRegistry{TEntity}.DetectMissingColumns"/>) at their
    /// expected position, and writes the header text into each newly-inserted column.
    /// </summary>
    public async Task<TEntity> InsertMissingColumns(Dictionary<string, List<ColumnInsertionInfo>> missingColumns)
    {
        return await ColumnInsertionHelper.InsertMissingColumnsAsync<TEntity>(_googleSheetService, missingColumns);
    }

    /// <summary>
    /// If any expected INPUT columns are missing entirely from <paramref name="response"/>, inserts
    /// them at their correct canonical position and writes their header text. HideHeaderName columns
    /// (populated by a spilling QUERY formula) are never candidates - the registry already excludes
    /// them. Detection reuses the header row already in <paramref name="response"/> (no extra API
    /// call); only the actual insert costs a real API call, and only when something is genuinely missing.
    /// </summary>
    protected async Task AutoHealMissingColumnsAsync(
        BatchGetValuesByDataFilterResponse response,
        Spreadsheet? spreadsheetInfo,
        List<MessageEntity> messages)
    {
        var missingColumns = _registry.DetectMissingColumns(response);

        if (missingColumns.Count == 0 || spreadsheetInfo?.Sheets == null)
        {
            return;
        }

        foreach (var (sheetName, columns) in missingColumns)
        {
            var sheetId = spreadsheetInfo.Sheets
                .FirstOrDefault(s => string.Equals(s.Properties.Title, sheetName, StringComparison.OrdinalIgnoreCase))
                ?.Properties.SheetId ?? 0;

            foreach (var column in columns)
            {
                column.SheetId = sheetId;
            }
        }

        var insertResult = await ColumnInsertionHelper.InsertMissingColumnsAsync<TEntity>(_googleSheetService, missingColumns);
        messages.AddRange(insertResult.Messages);
    }

    #endregion

    #region Sheet Layouts

    /// <summary>
    /// Gets the strongly-typed sheet layout/configuration (formulas, colors, notes, formats) for a
    /// specific sheet, or null if the sheet is unknown to this domain's registry.
    /// </summary>
    public Models.Google.SheetModel? GetSheetLayout(string sheet)
    {
        try
        {
            return _registry.GetSheetLayout(sheet);
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the strongly-typed sheet layouts/configurations for multiple sheets (skips any not found).
    /// </summary>
    public List<Models.Google.SheetModel> GetSheetLayouts(List<string> sheets)
    {
        var sheetModels = new List<Models.Google.SheetModel>();

        foreach (var sheet in sheets)
        {
            var sheetModel = GetSheetLayout(sheet);
            if (sheetModel != null)
            {
                sheetModels.Add(sheetModel);
            }
        }

        return sheetModels;
    }

    #endregion
}
