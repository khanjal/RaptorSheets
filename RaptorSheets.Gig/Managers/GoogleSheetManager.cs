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

namespace RaptorSheets.Gig.Managers;

public interface IGoogleSheetManager
{
    public Task<SheetEntity> ChangeSheetData(List<string> sheets, SheetEntity sheetEntity);
    public Task<SheetEntity> CreateAllSheets();
    public Task<SheetEntity> CreateSheets(List<string> sheets);
    public Task<SheetEntity> DeleteSheets(List<string> sheets);
    public Task<SheetEntity> GetSheet(string sheet);
    public Task<SheetEntity> GetAllSheets();
    public Task<SheetEntity> GetSheets(List<string> sheets);
    public Task<List<PropertyEntity>> GetAllSheetProperties();
    public Task<List<PropertyEntity>> GetSheetProperties(List<string> sheets);
    public Task<Spreadsheet?> GetSpreadsheetInfo(List<string>? ranges = null);
    public Task<BatchGetValuesByDataFilterResponse?> GetBatchData(List<string> sheets);
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

    public async Task<SheetEntity> DeleteSheets(List<string> sheets)
    {
        var sheetEntity = new SheetEntity();
        
        try
        {
            // Get sheet properties to find sheet IDs
            var sheetProperties = await GetSheetProperties(sheets);
            
            if (sheetProperties.Count == 0)
            {
                sheetEntity.Messages.Add(MessageHelpers.CreateWarningMessage("No sheets found to delete", MessageTypeEnum.DELETE_SHEET));
                return sheetEntity;
            }

            // Filter out sheets that don't have valid IDs (they may not exist)
            var existingSheetProperties = sheetProperties
                .Where(p => !string.IsNullOrEmpty(p.Id))
                .ToList();

            if (existingSheetProperties.Count == 0)
            {
                sheetEntity.Messages.Add(MessageHelpers.CreateInfoMessage("No existing sheets to delete", MessageTypeEnum.DELETE_SHEET));
                return sheetEntity;
            }

            sheetEntity.Messages.Add(MessageHelpers.CreateInfoMessage($"Found {existingSheetProperties.Count} sheets to delete: {string.Join(", ", existingSheetProperties.Select(p => $"{p.Name}({p.Id})"))} ", MessageTypeEnum.DELETE_SHEET));

            // Check if we're trying to delete all sheets in the spreadsheet
            var spreadsheetInfo = await GetSpreadsheetInfo();
            var totalSheetsInSpreadsheet = spreadsheetInfo?.Sheets?.Count ?? 0;
            
            sheetEntity.Messages.Add(MessageHelpers.CreateInfoMessage($"Total sheets in spreadsheet: {totalSheetsInSpreadsheet}, attempting to delete: {existingSheetProperties.Count}", MessageTypeEnum.DELETE_SHEET));

            // If we're deleting all or most sheets, create a temporary placeholder first
            bool needsPlaceholder = existingSheetProperties.Count >= totalSheetsInSpreadsheet;
            string? placeholderSheetId = null;

            if (needsPlaceholder)
            {
                sheetEntity.Messages.Add(MessageHelpers.CreateInfoMessage("Creating placeholder sheet to allow deletion of all sheets", MessageTypeEnum.DELETE_SHEET));
                
                // Create a temporary placeholder sheet so we can delete all the target sheets
                var placeholderName = $"TempPlaceholder_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
                var placeholderRequest = new BatchUpdateSpreadsheetRequest
                {
                    Requests = new List<Request>
                    {
                        new Request
                        {
                            AddSheet = new AddSheetRequest
                            {
                                Properties = new SheetProperties
                                {
                                    Title = placeholderName
                                }
                            }
                        }
                    }
                };

                var placeholderResult = await _googleSheetService.BatchUpdateSpreadsheet(placeholderRequest);
                if (placeholderResult?.Replies?.Count > 0 && placeholderResult.Replies[0].AddSheet != null)
                {
                    placeholderSheetId = placeholderResult.Replies[0].AddSheet.Properties.SheetId.ToString();
                    sheetEntity.Messages.Add(MessageHelpers.CreateInfoMessage($"Created temporary placeholder sheet: {placeholderName} (ID: {placeholderSheetId})", MessageTypeEnum.DELETE_SHEET));
                }
                else
                {
                    sheetEntity.Messages.Add(MessageHelpers.CreateErrorMessage("Failed to create placeholder sheet for deletion", MessageTypeEnum.DELETE_SHEET));
                    return sheetEntity;
                }
            }

            // Sort sheets by their order in the entity definition to ensure consistent deletion order
            var entitySheetOrder = SheetsConfig.SheetUtilities.GetAllSheetNames();
            var orderedSheetProperties = existingSheetProperties
                .OrderBy(p => entitySheetOrder.IndexOf(p.Name))
                .ToList();

            // Use the helper to create delete sheet requests in reverse order (delete from end to beginning)
            // This prevents index shifting issues during batch deletion
            var deleteRequests = GoogleRequestHelpers.GenerateDeleteSheetRequests(orderedSheetProperties.AsEnumerable().Reverse().ToList());

            if (deleteRequests.Count == 0)
            {
                sheetEntity.Messages.Add(MessageHelpers.CreateWarningMessage("No valid sheet IDs found for deletion", MessageTypeEnum.DELETE_SHEET));
                return sheetEntity;
            }

            sheetEntity.Messages.Add(MessageHelpers.CreateInfoMessage($"Preparing to delete {deleteRequests.Count} sheets", MessageTypeEnum.DELETE_SHEET));

            var batchRequest = new BatchUpdateSpreadsheetRequest
            {
                Requests = deleteRequests
            };

            // Execute the deletion
            var result = await _googleSheetService.BatchUpdateSpreadsheet(batchRequest);

            if (result != null)
            {
                sheetEntity.Messages.Add(MessageHelpers.CreateInfoMessage($"Batch delete request completed successfully", MessageTypeEnum.DELETE_SHEET));
                
                // Get the names of successfully processed sheets
                var validSheetNames = orderedSheetProperties.Select(p => p.Name).ToList();

                foreach (var sheetName in validSheetNames)
                {
                    sheetEntity.Messages.Add(MessageHelpers.CreateInfoMessage($"Sheet {sheetName} deletion requested", MessageTypeEnum.DELETE_SHEET));
                }

                // Clean up placeholder if we created one and it hasn't been deleted
                if (needsPlaceholder && !string.IsNullOrEmpty(placeholderSheetId))
                {
                    // We'll let the calling code handle placeholder cleanup after creating new sheets
                    // Store the placeholder ID in a message for later cleanup
                    sheetEntity.Messages.Add(MessageHelpers.CreateInfoMessage($"PlaceholderSheetId:{placeholderSheetId}", MessageTypeEnum.DELETE_SHEET));
                }
            }
            else
            {
                sheetEntity.Messages.Add(MessageHelpers.CreateErrorMessage("Failed to delete sheets - batch request returned null", MessageTypeEnum.DELETE_SHEET));
            }
        }
        catch (Exception ex)
        {
            // Handle specific Google API errors
            if (ex.Message.Contains("Cannot delete") || ex.Message.Contains("permission"))
            {
                sheetEntity.Messages.Add(MessageHelpers.CreateWarningMessage($"Unable to delete sheets: {ex.Message}", MessageTypeEnum.DELETE_SHEET));
            }
            else
            {
                sheetEntity.Messages.Add(MessageHelpers.CreateErrorMessage($"Error deleting sheets: {ex.Message}", MessageTypeEnum.DELETE_SHEET));
            }
        }

        return sheetEntity;
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
        var sheets = Enum.GetValues(typeof(SheetEnum)).Cast<SheetEnum>().ToList();
        return await GetSheetProperties(sheets.Select(t => t.GetDescription()).ToList());
    }

    public async Task<List<PropertyEntity>> GetSheetProperties(List<string> sheets)
    {
        var properties = new List<PropertyEntity>();
        
        // Get both headers and first column in a single call for efficiency
        var combinedRanges = sheets.SelectMany(sheet => new[]
        {
            $"{sheet}!{GoogleConfig.HeaderRange}",  // Headers (1:1)
            $"{sheet}!{GoogleConfig.RowRange}"      // First column data (A:A)
        }).ToList();
        
        var sheetInfo = await _googleSheetService.GetSheetInfo(combinedRanges);

        foreach (var sheet in sheets)
        {
            var property = new PropertyEntity();
            
            var sheetHeaderValues = "";
            int maxRow = 0;
            int maxRowValue = 1; // Default to header row
            var sheetId = "";

            // Find the sheet in the response
            var sheetData = sheetInfo?.Sheets.FirstOrDefault(x => x.Properties.Title == sheet);
            
            if (sheetData != null)
            {
                sheetId = sheetData.Properties.SheetId.ToString() ?? "";
                maxRow = sheetData.Properties.GridProperties.RowCount ?? 1000;

                // The Google Sheets API should return data for both ranges we requested
                // We need to process each GridData in the Data collection
                if (sheetData.Data != null && sheetData.Data.Count > 0)
                {
                    foreach (var dataRange in sheetData.Data)
                    {
                        if (dataRange?.RowData != null && dataRange.RowData.Count > 0)
                        {
                            // Determine if this is header data or column data based on structure
                            var firstRow = dataRange.RowData[0];
                            var hasMultipleColumns = firstRow?.Values?.Count > 1;
                            var hasMultipleRows = dataRange.RowData.Count > 1;

                            if (!hasMultipleRows && hasMultipleColumns)
                            {
                                // This is likely the headers (1:1 range) - single row, multiple columns
                                sheetHeaderValues = string.Join(",", firstRow.Values
                                    .Where(x => x.FormattedValue != null)
                                    .Select(x => x.FormattedValue)
                                    .ToList());
                            }
                            else if (hasMultipleRows && !hasMultipleColumns)
                            {
                                // This is likely the A:A column data - multiple rows, single column
                                // Find the last row with a value (excluding header at index 0)
                                for (int i = dataRange.RowData.Count - 1; i > 0; i--)
                                {
                                    var cell = dataRange.RowData[i]?.Values?.FirstOrDefault();
                                    if (cell != null && !string.IsNullOrEmpty(cell.FormattedValue))
                                    {
                                        maxRowValue = i + 1; // +1 because row index is zero-based
                                        break;
                                    }
                                }
                            }
                            else if (hasMultipleRows && hasMultipleColumns)
                            {
                                // This might be a full range response - check if first row looks like headers
                                var possibleHeaders = firstRow?.Values;
                                if (possibleHeaders != null && possibleHeaders.Count > 1)
                                {
                                    // Extract headers from first row
                                    sheetHeaderValues = string.Join(",", possibleHeaders
                                        .Where(x => x.FormattedValue != null)
                                        .Select(x => x.FormattedValue)
                                        .ToList());
                                }

                                // Find last row with data in first column
                                for (int i = dataRange.RowData.Count - 1; i > 0; i--)
                                {
                                    var cell = dataRange.RowData[i]?.Values?.FirstOrDefault();
                                    if (cell != null && !string.IsNullOrEmpty(cell.FormattedValue))
                                    {
                                        maxRowValue = i + 1;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    // If we don't get data, there might be an issue - log this for debugging
                    // but don't fail completely
                    Console.WriteLine($"Warning: No data returned for sheet '{sheet}' ranges");
                }
            }
            else
            {
                // Sheet not found in response - this is an issue that should be logged
                Console.WriteLine($"Warning: Sheet '{sheet}' not found in API response");
            }

            property.Id = sheetId;
            property.Name = sheet;
            property.Attributes.Add(PropertyEnum.HEADERS.GetDescription(), sheetHeaderValues);
            property.Attributes.Add(PropertyEnum.MAX_ROW.GetDescription(), maxRow.ToString());
            property.Attributes.Add(PropertyEnum.MAX_ROW_VALUE.GetDescription(), maxRowValue.ToString());

            properties.Add(property);
        }

        return properties;
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
}
