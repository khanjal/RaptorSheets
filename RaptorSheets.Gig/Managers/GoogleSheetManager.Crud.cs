using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Logging;
using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Helpers;

namespace RaptorSheets.Gig.Managers;

/// <summary>
/// CRUD operations for Google Sheets.
/// Handles Create, Read, Update, and Delete operations.
/// </summary>
public partial class GoogleSheetManager
{
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

    public async Task<SheetEntity> GetAllSheets()
    {
        var sheets = GenerateSheetsHelpers.GetSheetNames();
        var response = await GetSheets(sheets);
        return response ?? new SheetEntity();
    }

    public async Task<SheetEntity> GetSheets(List<string> sheets)
    {
        var data = new SheetEntity();
        var messages = new List<MessageEntity>();
        var stringSheetList = string.Join(", ", sheets.Select(t => t.ToString()));

        // First attempt: try to fetch directly
        var response = await _googleSheetService.GetBatchData(sheets, null);
        Spreadsheet? spreadsheetInfo = null;

        // If the batch fetch failed, check for missing sheets and restore them.
        if (response == null)
        {
            try
            {
                spreadsheetInfo = await _googleSheetService.GetSheetInfo();
                // If we couldn't fetch spreadsheet metadata, skip restoration to avoid creating all sheets
                if (spreadsheetInfo == null)
                {
                    _logger.LogWarning("Unable to fetch spreadsheet metadata; skipping missing-sheet restoration");
                }
                else
                {
                    // Pass the full ordered sheet list so indices are computed correctly
                    var allSheets = GenerateSheetsHelpers.GetSheetNames();
                    var missingIndexMap = SheetInitializationHelper.GetMissingSheets(spreadsheetInfo, allSheets);
                    if (missingIndexMap.Count > 0)
                    {
                        // CreateSheets applies full config (headers, formats, protections) and uses the index map for ordering
                        var createResult = await CreateSheets(missingIndexMap);

                        // If creation returned errors, surface them and return immediately.
                        if (createResult.Messages.Any(m => m.Level == MessageLevelEnum.ERROR.GetDescription()))
                        {
                            messages.AddRange(createResult.Messages);
                            var errorReturn = new SheetEntity();
                            errorReturn.Messages.AddRange(messages);
                            return errorReturn;
                        }

                        // Creation succeeded. Do not attempt an immediate re-fetch — return an informational
                        // SheetEntity instructing the caller to retry the GetSheets call later so Google
                        // has time to materialize data in newly-created auxiliary sheets.
                        var createdNames = string.Join(", ", missingIndexMap.Keys);
                        var info = MessageHelpers.CreateInfoMessage(
                            $"Created missing sheets: {createdNames}. Sheets may take a few seconds to become readable — please retry the request shortly.",
                            MessageTypeEnum.GET_SHEETS);

                        var createdReturn = new SheetEntity();
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
        else
        {
            messages.Add(MessageHelpers.CreateInfoMessage($"Retrieved sheet(s): {stringSheetList}", MessageTypeEnum.GET_SHEETS));

            // Cheap metadata-only call (no grid data / no ranges) - used only to detect unknown/extra
            // sheet tabs. Known-sheet header validation (including reordered/renamed columns) is
            // already done below by GigSheetHelpers.MapData using the header row already present in
            // the batchGet response, so a second, expensive IncludeGridData=true round trip across
            // all sheet ranges isn't needed here.
            if (spreadsheetInfo == null)
            {
                spreadsheetInfo = await _googleSheetService.GetSheetInfo();
            }

            if (spreadsheetInfo != null)
            {
                messages.AddRange(CheckUnknownSheets(spreadsheetInfo));
            }

            data = GigSheetHelpers.MapData(response) ?? new SheetEntity();

            // Auto-heal: if any expected INPUT columns are missing entirely, insert them at their
            // correct position (matching the canonical header order, not appended at the end) and
            // write their header text. HideHeaderName columns (populated by a spilling QUERY
            // formula, e.g. Delivery/Location's Pay/Tips/.../First Trip/Last Trip) are never
            // candidates here - HeaderHelpers.CheckSheetHeaders already excludes them, since
            // inserting one would land inside the query's contiguous spill range and break it.
            // Detection reuses the header row already in `response` (no extra API call); SheetId
            // comes from the same cheap metadata fetch used above for CheckUnknownSheets. Only the
            // actual insert costs a real API call, and only when something is genuinely missing.
            var missingColumns = GigSheetHelpers.DetectMissingColumns(response);
            if (missingColumns.Count > 0 && spreadsheetInfo?.Sheets != null)
            {
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

                var insertResult = await InsertMissingColumns(missingColumns);
                messages.AddRange(insertResult.Messages);
            }
        }

        if (spreadsheetInfo != null)
        {
            data.Properties.Name = spreadsheetInfo.Properties.Title;
        }

        data.Messages.AddRange(messages);

        return data;
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
}
