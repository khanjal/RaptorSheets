using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Logging;
using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Managers;
using RaptorSheets.Core.Models;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Helpers;

namespace RaptorSheets.Gig.Managers;

/// <summary>
/// Main interface for Google Sheet operations in the Gig domain.
/// Provides CRUD operations, metadata access, and demo data functionality.
/// </summary>
public interface IGoogleSheetManager
{
    // CRUD Operations
    Task<SheetEntity> ChangeSheetData(List<string> sheets, SheetEntity sheetEntity);
    Task<SheetEntity> CreateAllSheets();
    Task<SheetEntity> CreateSheets(List<string> sheets);
    Task<SheetEntity> DeleteAllSheets();
    Task<SheetEntity> DeleteSheets(List<string> sheets);
    Task<SheetEntity> GetSheet(string sheet);
    Task<SheetEntity> GetAllSheets();
    Task<SheetEntity> GetSheets(List<string> sheets);

    // Metadata & Properties
    Task<List<PropertyEntity>> GetAllSheetProperties();
    Task<List<PropertyEntity>> GetSheetProperties(List<string> sheets);
    Task<List<string>> GetAllSheetTabNames();
    Task<Spreadsheet?> GetSpreadsheetInfo(List<string>? ranges = null);
    Task<BatchGetValuesByDataFilterResponse?> GetBatchData(List<string> sheets);
    SheetModel? GetSheetLayout(string sheet);
    List<SheetModel> GetSheetLayouts(List<string> sheets);

    // Header Management
    Task<SheetEntity> InsertMissingColumns(Dictionary<string, List<ColumnInsertionInfo>> missingColumns);

    // Demo Data Generation
    SheetEntity GenerateDemoData(DateTime? startDate = null, DateTime? endDate = null, int? seed = null);
}

/// <summary>
/// Main Google Sheet Manager for the Gig domain. Handles all interactions with the Google Sheets API.
///
/// Domain-agnostic read/metadata/layout/heal orchestration (GetSheets, GetAllSheets, sheet
/// properties, tab names, layouts, InsertMissingColumns, GetSpreadsheetInfo, GetBatchData) is
/// inherited from <see cref="GoogleSheetManagerBase{TEntity}"/>. This class adds only the Gig-specific
/// pieces: constructors, the CreateMissingSheetsAsync self-heal hook, and the domain write operations
/// (ordered CreateSheets, ChangeSheetData, DeleteSheets) plus demo-data generation and the static
/// header-check helpers.
/// </summary>
public class GoogleSheetManager : GoogleSheetManagerBase<SheetEntity>, IGoogleSheetManager
{
    #region Construction

    public GoogleSheetManager(RaptorSheets.Core.Services.IGoogleSheetService googleSheetService, ILogger? logger = null)
        : base(googleSheetService, GigSheetHelpers.Registry, GenerateSheetsHelpers.GetSheetNames(), logger)
    {
    }

    public GoogleSheetManager(string accessToken, string spreadsheetId, ILogger? logger = null)
        : base(accessToken, spreadsheetId, GigSheetHelpers.Registry, GenerateSheetsHelpers.GetSheetNames(), logger)
    {
    }

    public GoogleSheetManager(Dictionary<string, string> parameters, string spreadsheetId, ILogger? logger = null)
        : base(parameters, spreadsheetId, GigSheetHelpers.Registry, GenerateSheetsHelpers.GetSheetNames(), logger)
    {
    }

    /// <summary>
    /// Restores sheets found missing entirely during <see cref="GoogleSheetManagerBase{TEntity}.GetSheets"/>
    /// self-heal, using Gig's own ordered/indexed creation so the desired positions are preserved.
    /// </summary>
    protected override Task<SheetEntity> CreateMissingSheetsAsync(Dictionary<string, int> missingIndexMap)
    {
        return CreateSheets(missingIndexMap);
    }

    /// <summary>
    /// Backs <see cref="GoogleSheetManagerBase{TEntity}.CreateSheets"/> and
    /// <see cref="GoogleSheetManagerBase{TEntity}.DeleteSheets"/> (for temp-sheet creation) with
    /// Gig's fully-configured AddSheet requests (headers, formatting, validation, colors).
    /// </summary>
    protected override BatchUpdateSpreadsheetRequest GenerateSheetsRequest(List<string> sheetNames)
    {
        return GenerateSheetsHelpers.Generate(sheetNames);
    }

    #endregion

    #region Create Operations

    // CreateAllSheets() and CreateSheets(List<string>, Dictionary<string,int>?) are inherited from
    // GoogleSheetManagerBase<SheetEntity>, backed by GenerateSheetsRequest above. This 1-arg overload
    // only exists because C# requires exact arity to implicitly satisfy IGoogleSheetManager's
    // single-parameter CreateSheets(List<string>) - an inherited method's optional parameter doesn't
    // count for interface matching the way it does for ordinary callers.
    public async Task<SheetEntity> CreateSheets(List<string> sheets)
    {
        return await CreateSheets(sheets, null);
    }

    /// <summary>
    /// Creates sheets using a title->desiredIndex map. The map's keys are the sheet titles to create.
    /// </summary>
    public async Task<SheetEntity> CreateSheets(Dictionary<string,int> sheetsWithIndices)
    {
        if (sheetsWithIndices == null || sheetsWithIndices.Count == 0)
        {
            return await CreateSheets(new List<string>());
        }

        // Order titles deterministically using a helper (stable, testable).
        var sheets = CreateSheetsHelpers.OrderSheetTitlesByIndex(sheetsWithIndices);

        return await CreateSheets(sheets, sheetsWithIndices);
    }

    #endregion

    #region Read Operations

    public async Task<SheetEntity> GetSheet(string sheet)
    {
        var sheetExists = GenerateSheetsHelpers.GetSheetNames()
            .Any(name => string.Equals(name, sheet, StringComparison.OrdinalIgnoreCase));

        if (!sheetExists)
        {
            return new SheetEntity { Messages = [MessageHelpers.CreateErrorMessage($"Sheet {sheet.ToUpperInvariant()} does not exist", MessageTypeEnum.GET_SHEETS)] };
        }

        return await GetSheets([sheet]);
    }

    // GetAllSheets(), GetSheets(List<string>), GetSpreadsheetInfo, and GetBatchData are inherited
    // from GoogleSheetManagerBase<SheetEntity>. Missing-sheet self-heal is wired via the overridden
    // CreateMissingSheetsAsync above (Gig's ordered/indexed CreateSheets).

    #endregion

    #region Update Operations

    // Static readonly dictionary to avoid recreation on every call
    private static readonly Dictionary<string, (Func<SheetEntity, int> GetCount, Func<SheetEntity, object> GetData)> _sheetAccessors =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [SheetsConfig.SheetNames.Expenses] = (entity => entity.Expenses.Count, entity => entity.Expenses),
            [SheetsConfig.SheetNames.Setup] = (entity => entity.Setup.Count, entity => entity.Setup),
            [SheetsConfig.SheetNames.Shifts] = (entity => entity.Shifts.Count, entity => entity.Shifts),
            [SheetsConfig.SheetNames.Trips] = (entity => entity.Trips.Count, entity => entity.Trips)
        };

    public async Task<SheetEntity> ChangeSheetData(List<string> sheets, SheetEntity sheetEntity)
    {
        var changes = GetSheetChanges(sheets, sheetEntity);

        if (changes.Count == 0)
        {
            sheetEntity.Messages.Add(MessageHelpers.CreateWarningMessage("No data to change", MessageTypeEnum.GENERAL));
            return sheetEntity;
        }

        var sheetInfo = await GetSheetProperties(sheets);
        var batchUpdateSpreadsheetRequest = new BatchUpdateSpreadsheetRequest
        {
            Requests = BuildBatchUpdateRequests(changes, sheetInfo, sheetEntity)
        };

        var batchUpdateSpreadsheetResponse = await _googleSheetService.BatchUpdateSpreadsheet(batchUpdateSpreadsheetRequest);

        if (batchUpdateSpreadsheetResponse == null)
        {
            var spreadsheetInfo = await _googleSheetService.GetSheetInfo();
            if (spreadsheetInfo != null)
            {
                sheetEntity.Messages.AddRange(await HandleMissingSheets(spreadsheetInfo));
            }
            sheetEntity.Messages.Add(MessageHelpers.CreateErrorMessage($"Unable to save data", MessageTypeEnum.SAVE_DATA));
        }

        return sheetEntity;
    }

    private static Dictionary<string, object> GetSheetChanges(List<string> sheets, SheetEntity sheetEntity)
    {
        var changes = new Dictionary<string, object>();
        foreach (var sheet in sheets)
        {
            var result = TryAddSheetChange(sheet, sheetEntity, changes);
            if (result == null)
            {
                // Only add error if the sheet is not recognized at all
                sheetEntity.Messages.Add(MessageHelpers.CreateErrorMessage($"{ActionTypeEnum.UPDATE} data: {sheet} not supported", MessageTypeEnum.GENERAL));
            }
            // If result is false (recognized but no data), do nothing
        }
        return changes;
    }

    /// <summary>
    /// Attempts to add sheet data to the changes dictionary if the sheet is recognized and contains data.
    /// </summary>
    /// <param name="sheet">The name of the sheet to check (case-insensitive)</param>
    /// <param name="sheetEntity">The entity containing all sheet data</param>
    /// <param name="changes">Dictionary to add changes to if data exists</param>
    /// <returns>
    /// true if the sheet was recognized and data was added to changes;
    /// false if the sheet was recognized but contains no data;
    /// null if the sheet name is not recognized
    /// </returns>
    private static bool? TryAddSheetChange(string sheet, SheetEntity sheetEntity, Dictionary<string, object> changes)
    {
        // Check if the sheet is recognized
        if (!_sheetAccessors.TryGetValue(sheet, out var accessor))
        {
            return null; // Sheet not recognized
        }

        // Check if the sheet has data
        if (accessor.GetCount(sheetEntity) > 0)
        {
            changes.Add(sheet, accessor.GetData(sheetEntity));
            return true; // Data added successfully
        }

        return false; // Recognized but no data
    }

    private static List<Request> BuildBatchUpdateRequests(Dictionary<string, object> changes, List<PropertyEntity> sheetInfo, SheetEntity sheetEntity)
    {
        var requests = new List<Request>();
        foreach (var change in changes)
        {
            var sheetName = change.Key;
                var properties = sheetInfo.FirstOrDefault(x => string.Equals(x.Name, sheetName, StringComparison.OrdinalIgnoreCase));

            switch (sheetName)
            {
                case SheetsConfig.SheetNames.Expenses:
                    requests.AddRange(GigRequestHelpers.ChangeExpensesSheetData(change.Value as List<ExpenseEntity> ?? [], properties));
                    break;
                case SheetsConfig.SheetNames.Setup:
                    requests.AddRange(GigRequestHelpers.ChangeSetupSheetData(change.Value as List<SetupEntity> ?? [], properties));
                    break;
                case SheetsConfig.SheetNames.Shifts:
                    requests.AddRange(GigRequestHelpers.ChangeShiftSheetData(change.Value as List<ShiftEntity> ?? [], properties));
                    break;
                case SheetsConfig.SheetNames.Trips:
                    requests.AddRange(GigRequestHelpers.ChangeTripSheetData(change.Value as List<TripEntity> ?? [], properties));
                    break;
            }

            sheetEntity.Messages.Add(MessageHelpers.CreateInfoMessage($"Saving data: {sheetName.ToUpperInvariant()}", MessageTypeEnum.SAVE_DATA));
        }
        return requests;
    }

    #endregion

    // DeleteAllSheets() and DeleteSheets(List<string>) are inherited from
    // GoogleSheetManagerBase<SheetEntity>, backed by GenerateSheetsRequest above (for temp-sheet
    // creation) and GetAllSheetProperties/GetAllSheetTabNames (also inherited).

    #region Header Validation

    /// <summary>
    /// Checks a spreadsheet's tab names for sheets that don't correspond to any known Gig sheet.
    /// Only needs sheet tab metadata (no grid/cell data), so it's safe to call with a cheap
    /// <c>GetSheetInfo()</c> (no ranges) result. Known-sheet header validation (missing/renamed/
    /// reordered columns) is handled separately, per-sheet, using data already fetched via batchGet.
    /// Static so callers can use it off the type without a manager instance; thin shim over
    /// <see cref="GigSheetHelpers"/>.
    /// </summary>
    public static List<MessageEntity> CheckUnknownSheets(Spreadsheet sheetInfoResponse)
    {
        return GigSheetHelpers.CheckUnknownSheets(sheetInfoResponse);
    }

    public static List<MessageEntity> CheckSheetHeaders(Spreadsheet sheetInfoResponse)
    {
        return GigSheetHelpers.CheckSheetHeaders(sheetInfoResponse);
    }

    /// <summary>
    /// Same as <see cref="CheckSheetHeaders(Spreadsheet)"/>, but also reports which columns are
    /// missing entirely and where they should be inserted, for use with <see cref="InsertMissingColumns"/>.
    /// </summary>
    public static List<MessageEntity> CheckSheetHeaders(Spreadsheet sheetInfoResponse, out Dictionary<string, List<ColumnInsertionInfo>> missingColumns)
    {
        return GigSheetHelpers.CheckSheetHeaders(sheetInfoResponse, out missingColumns);
    }

    #endregion

    #region Demo Data Generation

    /// <summary>
    /// Generates demo data without inserting it into the spreadsheet.
    /// Allows inspection, modification, or testing before insertion.
    /// This is the core method - consuming applications can wrap this with convenience methods.
    /// </summary>
    /// <param name="startDate">Start date for demo data (defaults to 30 days ago)</param>
    /// <param name="endDate">End date for demo data (defaults to today)</param>
    /// <param name="seed">Optional seed for deterministic/reproducible demo data (useful for testing)</param>
    /// <returns>SheetEntity populated with realistic demo data (Shifts, Trips, Expenses)</returns>
    public SheetEntity GenerateDemoData(DateTime? startDate = null, DateTime? endDate = null, int? seed = null)
    {
        var start = startDate ?? DateTime.Today.AddDays(-30);
        var end = endDate ?? DateTime.Today;

        return DemoHelpers.GenerateDemoData(start, end, seed);
    }

    #endregion

    #region Private Helpers

    private async Task<List<MessageEntity>> HandleMissingSheets(Spreadsheet? spreadsheet)
    {
        var messages = new List<MessageEntity>();
        if (spreadsheet != null)
        {
            var missingSheets = SheetHelpers.CheckSheets<SheetEnum>(spreadsheet);

            if (missingSheets.Count != 0)
            {
                messages.AddRange(SheetHelpers.CheckSheets(missingSheets));

                // Compute a title->desiredIndex map for missing sheets using the canonical ordered sheet list.
                // This ensures insertion indices are computed relative to the full expected ordering,
                // not just the missing subset (avoids incorrectly appending sheets).
                var allSheets = GenerateSheetsHelpers.GetSheetNames();
                var missingIndexMap = SheetInitializationHelper.GetMissingSheets(spreadsheet, allSheets);

                messages.AddRange((await CreateSheets(missingIndexMap)).Messages);
            }
        }
        else
        {
            messages.Add(MessageHelpers.CreateErrorMessage($"Unable to retrieve sheet(s)", MessageTypeEnum.GET_SHEETS));
        }

        return messages;
    }

    #endregion
}
