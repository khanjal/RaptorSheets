using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Entities;
using RaptorSheets.Gig.Mappers;
using RaptorSheets.Core.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Core.Services;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Gig.Helpers;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Common.Mappers;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Core.Models.Google;

namespace RaptorSheets.Gig.Managers;

public interface IGoogleSheetManager
{
    public Task<SheetEntity> ChangeSheetData(List<string> sheets, SheetEntity sheetEntity);
    public Task<SheetEntity> CreateAllSheets();
    public Task<SheetEntity> CreateSheets(List<string> sheets);
    public Task<SheetEntity> DeleteAllSheets();
    public Task<SheetEntity> DeleteSheets(List<string> sheets);
    public Task<SheetEntity> GetSheet(string sheet);
    public Task<SheetEntity> GetAllSheets();
    public Task<SheetEntity> GetSheets(List<string> sheets);
    public Task<List<PropertyEntity>> GetAllSheetProperties();
    public Task<List<PropertyEntity>> GetSheetProperties(List<string> sheets);
    public Task<List<string>> GetAllSheetTabNames();
    public Task<Spreadsheet?> GetSpreadsheetInfo(List<string>? ranges = null);
    public Task<BatchGetValuesByDataFilterResponse?> GetBatchData(List<string> sheets);
    public SheetModel? GetSheetLayout(string sheet);
    public List<SheetModel> GetSheetLayouts(List<string> sheets);
}

public class GoogleSheetManager : IGoogleSheetManager
{
    private readonly GoogleSheetService _googleSheetService;

    public GoogleSheetManager(string accessToken, string spreadsheetId)
    {
        _googleSheetService = new GoogleSheetService(accessToken, spreadsheetId);
    }

    public GoogleSheetManager(Dictionary<string, string> parameters, string spreadsheetId)
    {
        _googleSheetService = new GoogleSheetService(parameters, spreadsheetId);
    }

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
            if (TryAddSheetChange(sheet, sheetEntity, changes))
                continue;

            sheetEntity.Messages.Add(MessageHelpers.CreateErrorMessage($"{ActionTypeEnum.UPDATE} data: {sheet} not supported", MessageTypeEnum.GENERAL));
        }
        return changes;
    }

    private static bool TryAddSheetChange(string sheet, SheetEntity sheetEntity, Dictionary<string, object> changes)
    {
        if (string.Equals(sheet, SheetsConfig.SheetNames.Expenses, StringComparison.OrdinalIgnoreCase) && sheetEntity.Expenses.Count > 0)
        {
            changes.Add(sheet, sheetEntity.Expenses);
            return true;
        }
        if (string.Equals(sheet, SheetsConfig.SheetNames.Setup, StringComparison.OrdinalIgnoreCase) && sheetEntity.Setup.Count > 0)
        {
            changes.Add(sheet, sheetEntity.Setup);
            return true;
        }
        if (string.Equals(sheet, SheetsConfig.SheetNames.Shifts, StringComparison.OrdinalIgnoreCase) && sheetEntity.Shifts.Count > 0)
        {
            changes.Add(sheet, sheetEntity.Shifts);
            return true;
        }
        if (string.Equals(sheet, SheetsConfig.SheetNames.Trips, StringComparison.OrdinalIgnoreCase) && sheetEntity.Trips.Count > 0)
        {
            changes.Add(sheet, sheetEntity.Trips);
            return true;
        }
        return false;
    }

    private static List<Request> BuildBatchUpdateRequests(Dictionary<string, object> changes, List<PropertyEntity> sheetInfo, SheetEntity sheetEntity)
    {
        var requests = new List<Request>();
        foreach (var change in changes)
        {
            var sheetName = change.Key;
            var properties = sheetInfo.FirstOrDefault(x => x.Name == sheetName);
            
            if (string.Equals(sheetName, SheetsConfig.SheetNames.Expenses, StringComparison.OrdinalIgnoreCase))
            {
                requests.AddRange(GigRequestHelpers.ChangeExpensesSheetData(change.Value as List<ExpenseEntity> ?? [], properties));
            }
            else if (string.Equals(sheetName, SheetsConfig.SheetNames.Setup, StringComparison.OrdinalIgnoreCase))
            {
                requests.AddRange(GigRequestHelpers.ChangeSetupSheetData(change.Value as List<SetupEntity> ?? [], properties));
            }
            else if (string.Equals(sheetName, SheetsConfig.SheetNames.Shifts, StringComparison.OrdinalIgnoreCase))
            {
                requests.AddRange(GigRequestHelpers.ChangeShiftSheetData(change.Value as List<ShiftEntity> ?? [], properties));
            }
            else if (string.Equals(sheetName, SheetsConfig.SheetNames.Trips, StringComparison.OrdinalIgnoreCase))
            {
                requests.AddRange(GigRequestHelpers.ChangeTripSheetData(change.Value as List<TripEntity> ?? [], properties));
            }
            
            sheetEntity.Messages.Add(MessageHelpers.CreateInfoMessage($"Saving data: {sheetName.ToUpperInvariant()}", MessageTypeEnum.SAVE_DATA));
        }
        return requests;
    }

    public static List<MessageEntity> CheckSheetHeaders(Spreadsheet sheetInfoResponse)
    {
        var messages = new List<MessageEntity>();

        if (sheetInfoResponse == null)
        {
            messages.Add(MessageHelpers.CreateErrorMessage($"Unable to retrieve sheet(s)", MessageTypeEnum.GENERAL));
            return messages;
        }

        var headerMessages = new List<MessageEntity>();
        // Loop through sheets to check headers.
        foreach (var sheet in sheetInfoResponse.Sheets)
        {
            var sheetName = sheet.Properties.Title;
            var sheetHeader = HeaderHelpers.GetHeadersFromCellData(sheet.Data?[0]?.RowData?[0]?.Values);

            switch (sheetName)
            {
                case var s when string.Equals(s, SheetsConfig.SheetNames.Addresses, StringComparison.OrdinalIgnoreCase):
                    headerMessages.AddRange(HeaderHelpers.CheckSheetHeaders(sheetHeader, AddressMapper.GetSheet()));
                    break;
                case var s when string.Equals(s, SheetsConfig.SheetNames.Daily, StringComparison.OrdinalIgnoreCase):
                    headerMessages.AddRange(HeaderHelpers.CheckSheetHeaders(sheetHeader, DailyMapper.GetSheet()));
                    break;
                case var s when string.Equals(s, SheetsConfig.SheetNames.Expenses, StringComparison.OrdinalIgnoreCase):
                    headerMessages.AddRange(HeaderHelpers.CheckSheetHeaders(sheetHeader, ExpenseMapper.GetSheet()));
                    break;
                case var s when string.Equals(s, SheetsConfig.SheetNames.Monthly, StringComparison.OrdinalIgnoreCase):
                    headerMessages.AddRange(HeaderHelpers.CheckSheetHeaders(sheetHeader, MonthlyMapper.GetSheet()));
                    break;
                case var s when string.Equals(s, SheetsConfig.SheetNames.Names, StringComparison.OrdinalIgnoreCase):
                    headerMessages.AddRange(HeaderHelpers.CheckSheetHeaders(sheetHeader, NameMapper.GetSheet()));
                    break;
                case var s when string.Equals(s, SheetsConfig.SheetNames.Places, StringComparison.OrdinalIgnoreCase):
                    headerMessages.AddRange(HeaderHelpers.CheckSheetHeaders(sheetHeader, PlaceMapper.GetSheet()));
                    break;
                case var s when string.Equals(s, SheetsConfig.SheetNames.Regions, StringComparison.OrdinalIgnoreCase):
                    headerMessages.AddRange(HeaderHelpers.CheckSheetHeaders(sheetHeader, RegionMapper.GetSheet()));
                    break;
                case var s when string.Equals(s, SheetsConfig.SheetNames.Services, StringComparison.OrdinalIgnoreCase):
                    headerMessages.AddRange(HeaderHelpers.CheckSheetHeaders(sheetHeader, ServiceMapper.GetSheet()));
                    break;
                case var s when string.Equals(s, SheetsConfig.SheetNames.Setup, StringComparison.OrdinalIgnoreCase):
                    headerMessages.AddRange(HeaderHelpers.CheckSheetHeaders(sheetHeader, SetupMapper.GetSheet()));
                    break;
                case var s when string.Equals(s, SheetsConfig.SheetNames.Shifts, StringComparison.OrdinalIgnoreCase):
                    headerMessages.AddRange(HeaderHelpers.CheckSheetHeaders(sheetHeader, ShiftMapper.GetSheet()));
                    break;
                case var s when string.Equals(s, SheetsConfig.SheetNames.Trips, StringComparison.OrdinalIgnoreCase):
                    headerMessages.AddRange(HeaderHelpers.CheckSheetHeaders(sheetHeader, TripMapper.GetSheet()));
                    break;
                case var s when string.Equals(s, SheetsConfig.SheetNames.Types, StringComparison.OrdinalIgnoreCase):
                    headerMessages.AddRange(HeaderHelpers.CheckSheetHeaders(sheetHeader, TypeMapper.GetSheet()));
                    break;
                case var s when string.Equals(s, SheetsConfig.SheetNames.Weekdays, StringComparison.OrdinalIgnoreCase):
                    headerMessages.AddRange(HeaderHelpers.CheckSheetHeaders(sheetHeader, WeekdayMapper.GetSheet()));
                    break;
                case var s when string.Equals(s, SheetsConfig.SheetNames.Weekly, StringComparison.OrdinalIgnoreCase):
                    headerMessages.AddRange(HeaderHelpers.CheckSheetHeaders(sheetHeader, WeeklyMapper.GetSheet()));
                    break;
                case var s when string.Equals(s, SheetsConfig.SheetNames.Yearly, StringComparison.OrdinalIgnoreCase):
                    headerMessages.AddRange(HeaderHelpers.CheckSheetHeaders(sheetHeader, YearlyMapper.GetSheet()));
                    break;
                default:
                    messages.Add(MessageHelpers.CreateWarningMessage($"Sheet {sheet.Properties.Title} does not match any known sheet name", MessageTypeEnum.CHECK_SHEET));
                    break;
            }
        }

        if (headerMessages.Count > 0)
        {
            messages.Add(MessageHelpers.CreateWarningMessage($"Found sheet header issue(s)", MessageTypeEnum.CHECK_SHEET));
            messages.AddRange(headerMessages);
        }
        else
        {
            messages.Add(MessageHelpers.CreateInfoMessage($"No sheet header issues found", MessageTypeEnum.CHECK_SHEET));
        }

        return messages;
    }

    public async Task<SheetEntity> CreateAllSheets()
    {
        return await CreateSheets(SheetsConfig.SheetUtilities.GetAllSheetNames());
    }

    public async Task<SheetEntity> CreateSheets(List<string> sheets)
    {
        var batchUpdateSpreadsheetRequest = GenerateSheetsHelpers.Generate(sheets);
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

    public async Task<List<PropertyEntity>> GetAllSheetProperties()
    {
        return await GetSheetProperties(SheetsConfig.SheetUtilities.GetAllSheetNames());
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
                    var processedProperty = SheetPropertyHelper.ProcessSheetData(property.Name, sheetInfo);
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

    private async Task<List<MessageEntity>> HandleMissingSheets(Spreadsheet? spreadsheet)
    {
        var messages = new List<MessageEntity>();
        if (spreadsheet != null)
        {
            var missingSheets = SheetHelpers.CheckSheets<SheetEnum>(spreadsheet);
            missingSheets.AddRange(SheetHelpers.CheckSheets<Common.Enums.SheetEnum>(spreadsheet));

            if (missingSheets.Count != 0)
            {
                messages.AddRange(SheetHelpers.CheckSheets(missingSheets));
                messages.AddRange((await CreateSheets(missingSheets)).Messages);
            }
        }
        else
        {
            messages.Add(MessageHelpers.CreateErrorMessage($"Unable to retrieve sheet(s)", MessageTypeEnum.GET_SHEETS));
        }

        return messages;
    }

    public async Task<Spreadsheet?> GetSpreadsheetInfo(List<string>? ranges = null)
    {
        return await _googleSheetService.GetSheetInfo(ranges);
    }

    public async Task<BatchGetValuesByDataFilterResponse?> GetBatchData(List<string> sheets)
    {
        return await _googleSheetService.GetBatchData(sheets);
    }

    /// <summary>
    /// Gets the strongly-typed sheet layout/configuration for a specific sheet.
    /// This includes formulas, colors, notes, formats, and all other sheet properties.
    /// Useful for examining what the expected sheet structure should be.
    /// </summary>
    /// <param name="sheet">The name of the sheet to get configuration for</param>
    /// <returns>SheetModel containing the complete sheet configuration, or null if sheet not found</returns>
    public SheetModel? GetSheetLayout(string sheet)
    {
        try
        {
            // Use the existing helper to get the appropriate mapper's sheet model
            if (string.Equals(sheet, SheetsConfig.SheetNames.Addresses, StringComparison.OrdinalIgnoreCase))
                return AddressMapper.GetSheet();
            if (string.Equals(sheet, SheetsConfig.SheetNames.Daily, StringComparison.OrdinalIgnoreCase))
                return DailyMapper.GetSheet();
            if (string.Equals(sheet, SheetsConfig.SheetNames.Expenses, StringComparison.OrdinalIgnoreCase))
                return ExpenseMapper.GetSheet();
            if (string.Equals(sheet, SheetsConfig.SheetNames.Monthly, StringComparison.OrdinalIgnoreCase))
                return MonthlyMapper.GetSheet();
            if (string.Equals(sheet, SheetsConfig.SheetNames.Names, StringComparison.OrdinalIgnoreCase))
                return NameMapper.GetSheet();
            if (string.Equals(sheet, SheetsConfig.SheetNames.Places, StringComparison.OrdinalIgnoreCase))
                return PlaceMapper.GetSheet();
            if (string.Equals(sheet, SheetsConfig.SheetNames.Regions, StringComparison.OrdinalIgnoreCase))
                return RegionMapper.GetSheet();
            if (string.Equals(sheet, SheetsConfig.SheetNames.Services, StringComparison.OrdinalIgnoreCase))
                return ServiceMapper.GetSheet();
            if (string.Equals(sheet, SheetsConfig.SheetNames.Setup, StringComparison.OrdinalIgnoreCase))
                return SetupMapper.GetSheet();
            if (string.Equals(sheet, SheetsConfig.SheetNames.Shifts, StringComparison.OrdinalIgnoreCase))
                return ShiftMapper.GetSheet();
            if (string.Equals(sheet, SheetsConfig.SheetNames.Trips, StringComparison.OrdinalIgnoreCase))
                return TripMapper.GetSheet();
            if (string.Equals(sheet, SheetsConfig.SheetNames.Types, StringComparison.OrdinalIgnoreCase))
                return TypeMapper.GetSheet();
            if (string.Equals(sheet, SheetsConfig.SheetNames.Weekdays, StringComparison.OrdinalIgnoreCase))
                return WeekdayMapper.GetSheet();
            if (string.Equals(sheet, SheetsConfig.SheetNames.Weekly, StringComparison.OrdinalIgnoreCase))
                return WeeklyMapper.GetSheet();
            if (string.Equals(sheet, SheetsConfig.SheetNames.Yearly, StringComparison.OrdinalIgnoreCase))
                return YearlyMapper.GetSheet();

            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the strongly-typed sheet layouts/configurations for multiple sheets.
    /// This includes formulas, colors, notes, formats, and all other sheet properties.
    /// Useful for examining what the expected sheet structure should be.
    /// </summary>
    /// <param name="sheets">List of sheet names to get configurations for</param>
    /// <returns>List of SheetModels containing complete sheet configurations (excludes sheets not found)</returns>
    public List<SheetModel> GetSheetLayouts(List<string> sheets)
    {
        var sheetModels = new List<SheetModel>();

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
}
