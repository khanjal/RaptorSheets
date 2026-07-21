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
    /// <summary>
    /// Name of the throwaway safety sheet <see cref="GoogleSheetManagerBase{TEntity}.DeleteSheets"/>
    /// creates (via a domain's <see cref="GoogleSheetManagerBase{TEntity}.GenerateSheetsRequest"/>)
    /// when deleting every existing sheet, since Google Sheets requires at least one to remain.
    /// Public so a domain's sheet-request generator can recognize this specific ad-hoc name and build
    /// a bare AddSheet request for it instead of treating it as an unknown/invalid sheet.
    /// </summary>
    public const string TempSheetName = "TempSheet";

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

    /// <summary>
    /// Builds the AddSheet batch-update request(s) for the given sheet names, using the domain's own
    /// sheet configuration (headers, formatting, validation, colors). Backs <see cref="CreateSheets"/>
    /// and <see cref="DeleteSheets"/> below (DeleteSheets only needs it for temp-sheet creation).
    /// Domains whose sheet creation is keyed off a closed enum rather than arbitrary names (Stock
    /// today) can leave this unoverridden; the base <c>CreateSheets(List&lt;string&gt;, ...)</c>/
    /// <c>DeleteSheets(List&lt;string&gt;)</c> then simply aren't usable and shouldn't be called -
    /// the domain keeps its own enum-based CreateSheets instead.
    /// </summary>
    protected virtual BatchUpdateSpreadsheetRequest GenerateSheetsRequest(List<string> sheetNames)
    {
        throw new NotSupportedException(
            $"{GetType().Name} does not override GenerateSheetsRequest, so GoogleSheetManagerBase.CreateSheets" +
            "(List<string>, ...) / DeleteSheets(List<string>) are unavailable. Use this domain's own " +
            "sheet-creation API instead.");
    }

    #region Sheet Creation

    private const string DefaultSheetName = "Sheet1";

    /// <summary>
    /// Creates every canonical sheet for this domain.
    /// </summary>
    public async Task<TEntity> CreateAllSheets()
    {
        return await CreateSheets(new List<string>(_canonicalSheetNames));
    }

    /// <summary>
    /// Creates sheets, optionally at specific positions. If <paramref name="existingIndexMap"/> is
    /// provided its keys overlapping <paramref name="sheets"/> are treated as desired indices;
    /// otherwise positions are computed from <see cref="_canonicalSheetNames"/> ordering. Also
    /// relocates Google's default "Sheet1" (if present and otherwise untouched) to the end of the
    /// spreadsheet in the same batch, to minimize API calls.
    /// </summary>
    public async Task<TEntity> CreateSheets(List<string> sheets, Dictionary<string, int>? existingIndexMap = null)
    {
        var entity = new TEntity();
        var batchUpdateSpreadsheetRequest = GenerateSheetsRequest(sheets);

        // Fetch spreadsheet info once and reuse below to avoid duplicate API calls
        Spreadsheet? spreadsheetInfo = null;

        try
        {
            // Move default sheet (e.g., "Sheet1") to end in the same batch to minimize API calls
            spreadsheetInfo = await _googleSheetService.GetSheetInfo();
            var defaultSheet = spreadsheetInfo?.Sheets?.FirstOrDefault(s =>
                string.Equals(s.Properties.Title, DefaultSheetName, StringComparison.OrdinalIgnoreCase));

            if (defaultSheet != null && defaultSheet.Properties.SheetId.HasValue)
            {
                var existingCount = spreadsheetInfo!.Sheets!.Count;
                var targetIndex = GoogleRequestHelpers.ComputeEndIndex(existingCount, sheets.Count);
                batchUpdateSpreadsheetRequest.Requests.Add(
                    GoogleRequestHelpers.GenerateUpdateSheetIndex(defaultSheet.Properties.SheetId.Value, targetIndex)
                );
            }
        }
        catch
        {
            // Warn but proceed with creation
            entity.Messages.Add(MessageHelpers.CreateWarningMessage(
                "Could not move default sheet to end; proceeding with creation",
                MessageTypeEnum.CREATE_SHEET));
        }

        // Attempt to compute desired positions for requested sheets and add index update
        // requests so created sheets are placed in the expected order.
        try
        {
            IList<Request>? insertionRequests = null;
            int existingRawCount = 0;

            // Reuse `spreadsheetInfo` fetched earlier when possible to avoid an extra API call.
            Spreadsheet? currentInfo = spreadsheetInfo;
            if (currentInfo == null)
            {
                try
                {
                    currentInfo = await _googleSheetService.GetSheetInfo();
                }
                catch
                {
                    // ignore - ordering may still be possible using provided maps
                }
            }

            // If the caller passed a map whose keys overlap the requested sheets, treat it
            // as a desired-index map (title -> desired index for newly-created sheets).
            var providedMapIsDesiredIndices = existingIndexMap != null && existingIndexMap.Keys.Any(k => sheets.Contains(k, StringComparer.OrdinalIgnoreCase));

            if (providedMapIsDesiredIndices)
            {
                // We'll apply the provided indices directly below without calling the ordering helper
            }
            else
            {
                if (existingIndexMap != null && existingIndexMap.Count > 0)
                {
                    existingRawCount = currentInfo?.Sheets?.Count ?? existingIndexMap.Count;
                    insertionRequests = SheetOrderingHelper.BuildAddSheetRequests(existingIndexMap, existingRawCount, _canonicalSheetNames);
                }
                else if (currentInfo != null)
                {
                    insertionRequests = SheetOrderingHelper.BuildAddSheetRequests(currentInfo, _canonicalSheetNames);
                }
            }

            // Determine the mapping from title -> desired index either from the ordering helper
            // or directly from the provided desired-index map.
            var targetIndexMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            if (providedMapIsDesiredIndices && existingIndexMap != null)
            {
                foreach (var kv in existingIndexMap.Where(kv => sheets.Contains(kv.Key, StringComparer.OrdinalIgnoreCase)))
                    targetIndexMap[kv.Key] = kv.Value;
            }
            else if (insertionRequests != null)
            {
                foreach (var r in insertionRequests)
                {
                    var title = r?.AddSheet?.Properties?.Title;
                    var idx = r?.AddSheet?.Properties?.Index;
                    if (!string.IsNullOrEmpty(title) && idx.HasValue)
                        targetIndexMap[title] = idx.Value;
                }
            }

            if (targetIndexMap.Count > 0)
            {
                // Find AddSheet requests we will actually send (generated by GenerateSheetsRequest)
                var createdAdds = batchUpdateSpreadsheetRequest.Requests
                    .Where(r => r.AddSheet != null && r.AddSheet.Properties != null && !string.IsNullOrEmpty(r.AddSheet.Properties.Title))
                    .ToList();

                // Assign desired Index directly on the AddSheet properties so sheets are created at the target index
                foreach (var add in createdAdds)
                {
                    var title = add.AddSheet.Properties.Title;
                    if (string.IsNullOrEmpty(title)) continue;

                    if (targetIndexMap.TryGetValue(title, out var desiredIndex) && desiredIndex >= 0)
                    {
                        add.AddSheet.Properties.Index = desiredIndex;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Non-fatal - proceed without ordering if we couldn't compute it
            _logger.LogWarning(ex, "Unable to compute insertion indices");
        }

        var response = await _googleSheetService.BatchUpdateSpreadsheet(batchUpdateSpreadsheetRequest);

        // No sheets created if null.
        if (response == null)
        {
            foreach (var sheet in sheets)
            {
                entity.Messages.Add(MessageHelpers.CreateErrorMessage($"{sheet} not created", MessageTypeEnum.CREATE_SHEET));
            }

            return entity;
        }

        var sheetTitles = response.Replies.Where(x => x.AddSheet != null).Select(x => x.AddSheet.Properties.Title).ToList();

        foreach (var sheetTitle in sheetTitles)
        {
            entity.Messages.Add(MessageHelpers.CreateWarningMessage($"{sheetTitle.ToUpperInvariant()} created", MessageTypeEnum.CREATE_SHEET));
        }

        return entity;
    }

    #endregion

    #region Sheet Deletion

    /// <summary>
    /// Deletes every canonical sheet for this domain.
    /// </summary>
    public async Task<TEntity> DeleteAllSheets()
    {
        return await DeleteSheets(new List<string>(_canonicalSheetNames));
    }

    /// <summary>
    /// Deletes the given sheets. Google Sheets requires at least one sheet to remain in a
    /// spreadsheet, so if deleting every existing sheet, a temporary safety sheet is created first
    /// (via <see cref="GenerateSheetsRequest"/>) and left in place afterward.
    /// </summary>
    public async Task<TEntity> DeleteSheets(List<string> sheets)
    {
        var entity = new TEntity();
        try
        {
            var existingSheetsToDelete = await GetExistingSheetsToDelete(sheets, entity);
            if (existingSheetsToDelete.Count == 0) return entity;

            var allTabNames = await GetAllSheetTabNames();
            var needsTempSheet = NeedsTempSheet(existingSheetsToDelete, allTabNames);
            var tempSheetName = needsTempSheet ? TempSheetName : null;

            var requests = BuildDeletionRequests(existingSheetsToDelete, tempSheetName);

            if (!string.IsNullOrEmpty(tempSheetName))
            {
                entity.Messages.Add(MessageHelpers.CreateInfoMessage(
                    $"Creating '{tempSheetName}' as safety sheet to maintain spreadsheet integrity",
                    MessageTypeEnum.DELETE_SHEET));
            }

            entity.Messages.Add(MessageHelpers.CreateInfoMessage(
                $"Deleting {existingSheetsToDelete.Count} of {allTabNames.Count} sheets",
                MessageTypeEnum.DELETE_SHEET));

            var batchRequest = new BatchUpdateSpreadsheetRequest { Requests = requests };
            var result = await _googleSheetService.BatchUpdateSpreadsheet(batchRequest);

            if (result != null)
            {
                entity.Messages.Add(MessageHelpers.CreateInfoMessage(
                    "Sheet deletion completed successfully",
                    MessageTypeEnum.DELETE_SHEET));
            }
            else
            {
                entity.Messages.Add(MessageHelpers.CreateErrorMessage(
                    "Sheet deletion failed - unable to execute batch request",
                    MessageTypeEnum.DELETE_SHEET));
            }
        }
        catch (Exception ex)
        {
            entity.Messages.Add(MessageHelpers.CreateErrorMessage(
                $"Error deleting sheets: {ex.Message}",
                MessageTypeEnum.DELETE_SHEET));
        }

        return entity;
    }

    private async Task<List<PropertyEntity>> GetExistingSheetsToDelete(List<string> sheets, TEntity entity)
    {
        var allSheetProperties = await GetAllSheetProperties();
        var existingSheets = allSheetProperties
            .Where(p => !string.IsNullOrEmpty(p.Id) &&
                       int.TryParse(p.Id, out _) &&
                       sheets.Contains(p.Name, StringComparer.OrdinalIgnoreCase))
            .ToList();

        if (existingSheets.Count == 0)
        {
            entity.Messages.Add(MessageHelpers.CreateInfoMessage(
                "No sheets found to delete",
                MessageTypeEnum.DELETE_SHEET));
        }

        return existingSheets;
    }

    private List<Request> BuildDeletionRequests(List<PropertyEntity> sheetsToDelete, string? tempSheetName)
    {
        var requests = new List<Request>();

        if (!string.IsNullOrEmpty(tempSheetName))
        {
            var tempRequests = GenerateSheetsRequest([tempSheetName]).Requests;
            requests.AddRange(tempRequests);
        }

        var deleteRequests = GoogleRequestHelpers.GenerateDeleteSheetRequests(sheetsToDelete);
        requests.AddRange(deleteRequests);

        return requests;
    }

    private static bool NeedsTempSheet(List<PropertyEntity> sheetsToDelete, List<string> allTabNames)
    {
        var sheetsToDeleteNames = sheetsToDelete.Select(s => s.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Check if we're deleting all existing sheets (excluding any existing TempSheet)
        var remainingSheets = allTabNames.Where(tabName =>
            !sheetsToDeleteNames.Contains(tabName) &&
            !tabName.Equals(TempSheetName, StringComparison.OrdinalIgnoreCase)).ToList();

        // Only need a temp sheet if we're deleting all non-temp sheets
        return remainingSheets.Count == 0;
    }

    #endregion

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
