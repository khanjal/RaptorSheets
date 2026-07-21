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

    #endregion

    #region Create Operations

    public async Task<SheetEntity> CreateAllSheets()
    {
        return await CreateSheets(SheetsConfig.SheetUtilities.GetAllSheetNames());
    }

    public async Task<SheetEntity> CreateSheets(List<string> sheets)
    {
        // Delegate to the overload that accepts an optional existing index map
        return await CreateSheets(sheets, null);
    }

    /// <summary>
    /// Creates sheets and uses the provided existing title->index map to compute insertion indices
    /// (avoids an extra GetSheetInfo call if the caller already has spreadsheet metadata).
    /// </summary>
    public async Task<SheetEntity> CreateSheets(List<string> sheets, Dictionary<string,int>? existingIndexMap)
    {
        var sheetEntity = new SheetEntity();
        var batchUpdateSpreadsheetRequest = GenerateSheetsHelpers.Generate(sheets);

        // Fetch spreadsheet info once and reuse below to avoid duplicate API calls
        Spreadsheet? spreadsheetInfo = null;

        try
        {
            // Move default sheet (e.g., "Sheet1") to end in the same batch to minimize API calls
            spreadsheetInfo = await _googleSheetService.GetSheetInfo();
            var defaultSheet = spreadsheetInfo?.Sheets?.FirstOrDefault(s =>
                string.Equals(s.Properties.Title, "Sheet1", StringComparison.OrdinalIgnoreCase));

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
            sheetEntity.Messages.Add(MessageHelpers.CreateWarningMessage(
                "Could not move default sheet to end; proceeding with creation",
                MessageTypeEnum.CREATE_SHEET));
        }

        // Attempt to compute desired positions for requested sheets and add index update
        // requests so created sheets are placed in the expected order.
        try
        {
            // Attempt to compute desired positions for requested sheets and add index update
            // requests so created sheets are placed in the expected order.
            IList<Request>? insertionRequests = null;
            int existingRawCount = 0;

            // Get the canonical ordered sheet list for index computation
            var allSheets = GenerateSheetsHelpers.GetSheetNames();

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
                    insertionRequests = SheetOrderingHelper.BuildAddSheetRequests(existingIndexMap, existingRawCount, allSheets);
                }
                else if (currentInfo != null)
                {
                    insertionRequests = SheetOrderingHelper.BuildAddSheetRequests(currentInfo, allSheets);
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
                // Find AddSheet requests we will actually send (generated by GenerateSheetsHelpers)
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
                sheetEntity.Messages.Add(MessageHelpers.CreateErrorMessage($"{sheet} not created", MessageTypeEnum.CREATE_SHEET));
            }

            return sheetEntity;
        }

        var sheetTitles = response.Replies.Where(x => x.AddSheet != null).Select(x => x.AddSheet.Properties.Title).ToList();

        foreach (var sheetTitle in sheetTitles)
        {
            sheetEntity.Messages.Add(MessageHelpers.CreateWarningMessage($"{sheetTitle.ToUpperInvariant()} created", MessageTypeEnum.CREATE_SHEET));
        }

        return sheetEntity;
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

    #region Delete Operations

    public async Task<SheetEntity> DeleteAllSheets()
    {
        return await DeleteSheets(SheetsConfig.SheetUtilities.GetAllSheetNames());
    }

    public async Task<SheetEntity> DeleteSheets(List<string> sheets)
    {
        var sheetEntity = new SheetEntity();
        try
        {
            var existingSheetsToDelete = await GetExistingSheetsToDelete(sheets, sheetEntity);
            if (existingSheetsToDelete.Count == 0) return sheetEntity;

            var allTabNames = await GetAllSheetTabNames();
            var needsTempSheet = NeedsTempSheet(existingSheetsToDelete, allTabNames);
            var tempSheetName = needsTempSheet ? "TempSheet" : null;

            var requests = BuildDeletionRequests(existingSheetsToDelete, tempSheetName);

            if (!string.IsNullOrEmpty(tempSheetName))
            {
                sheetEntity.Messages.Add(MessageHelpers.CreateInfoMessage(
                    $"Creating '{tempSheetName}' as safety sheet to maintain spreadsheet integrity",
                    MessageTypeEnum.DELETE_SHEET));
            }

            sheetEntity.Messages.Add(MessageHelpers.CreateInfoMessage(
                $"Deleting {existingSheetsToDelete.Count} of {allTabNames.Count} sheets",
                MessageTypeEnum.DELETE_SHEET));

            var batchRequest = new BatchUpdateSpreadsheetRequest { Requests = requests };
            var result = await _googleSheetService.BatchUpdateSpreadsheet(batchRequest);

            if (result != null)
            {
                sheetEntity.Messages.Add(MessageHelpers.CreateInfoMessage(
                    "Sheet deletion completed successfully",
                    MessageTypeEnum.DELETE_SHEET));
            }
            else
            {
                sheetEntity.Messages.Add(MessageHelpers.CreateErrorMessage(
                    "Sheet deletion failed - unable to execute batch request",
                    MessageTypeEnum.DELETE_SHEET));
            }
        }
        catch (Exception ex)
        {
            sheetEntity.Messages.Add(MessageHelpers.CreateErrorMessage(
                $"Error deleting sheets: {ex.Message}",
                MessageTypeEnum.DELETE_SHEET));
        }

        return sheetEntity;
    }

    private async Task<List<PropertyEntity>> GetExistingSheetsToDelete(List<string> sheets, SheetEntity sheetEntity)
    {
        var allSheetProperties = await GetAllSheetProperties();
        var existingSheets = allSheetProperties
            .Where(p => !string.IsNullOrEmpty(p.Id) &&
                       int.TryParse(p.Id, out _) &&
                       sheets.Contains(p.Name, StringComparer.OrdinalIgnoreCase))
            .ToList();

        if (existingSheets.Count == 0)
        {
            sheetEntity.Messages.Add(MessageHelpers.CreateInfoMessage(
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
            var tempRequests = GenerateSheetsHelpers.Generate([tempSheetName]).Requests;
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
            !tabName.Equals("TempSheet", StringComparison.OrdinalIgnoreCase)).ToList();

        // Only need a temp sheet if we're deleting all non-temp sheets
        return remainingSheets.Count == 0;
    }

    #endregion

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
