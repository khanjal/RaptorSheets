using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Helpers;
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
        var batchUpdateSpreadsheetRequest = GenerateSheetsHelpers.Generate(sheets);

        try
        {
            // Move default sheet (e.g., "Sheet1") to end in the same batch to minimize API calls
            var spreadsheetInfo = await _googleSheetService.GetSheetInfo();
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
            // Ignore errors in moving default sheet; proceed with creation
        }

        var response = await _googleSheetService.BatchUpdateSpreadsheet(batchUpdateSpreadsheetRequest);

        var sheetEntity = new SheetEntity();

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

        var response = await _googleSheetService.GetBatchData(sheets);
        Spreadsheet? spreadsheetInfo;

        if (response == null)
        {
            spreadsheetInfo = await _googleSheetService.GetSheetInfo();
            messages.AddRange(await HandleMissingSheets(spreadsheetInfo));
        }
        else
        {
            messages.Add(MessageHelpers.CreateInfoMessage($"Retrieved sheet(s): {stringSheetList}", MessageTypeEnum.GET_SHEETS));
            
            var ranges = sheets.Select(sheet => $"{sheet}!{GoogleConfig.HeaderRange}").ToList();
            spreadsheetInfo = await _googleSheetService.GetSheetInfo(ranges);

            if (spreadsheetInfo != null)
            {
                messages.AddRange(CheckSheetHeaders(spreadsheetInfo));
            }

            data = GigSheetHelpers.MapData(response) ?? new SheetEntity();
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
            var properties = sheetInfo.FirstOrDefault(x => x.Name == sheetName);

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
