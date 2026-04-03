using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Models;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Helpers;
using RaptorSheets.Gig.Mappers;

namespace RaptorSheets.Gig.Managers;

/// <summary>
/// Metadata and property operations for Google Sheets.
/// Handles sheet properties, headers, layouts, and validation.
/// </summary>
public partial class GoogleSheetManager
{
    #region Sheet Properties

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

    #endregion

    #region Header Validation

    public static List<MessageEntity> CheckSheetHeaders(Spreadsheet sheetInfoResponse)
    {
        return CheckSheetHeaders(sheetInfoResponse, out _);
    }

    /// <summary>
    /// Checks sheet headers and optionally returns information about missing columns.
    /// </summary>
    /// <param name="sheetInfoResponse">The spreadsheet info from Google Sheets</param>
    /// <param name="missingColumns">Output dictionary of missing columns per sheet (empty if none found)</param>
    /// <returns>Validation messages</returns>
    public static List<MessageEntity> CheckSheetHeaders(
        Spreadsheet sheetInfoResponse,
        out Dictionary<string, List<ColumnInsertionInfo>> missingColumns)
    {
        var messages = new List<MessageEntity>();
        missingColumns = new Dictionary<string, List<ColumnInsertionInfo>>();

        if (sheetInfoResponse == null)
        {
            messages.Add(MessageHelpers.CreateErrorMessage($"Unable to retrieve sheet(s)", MessageTypeEnum.GENERAL));
            return messages;
        }

        var headerMessages = new List<MessageEntity>();
        
        // Loop through sheets to check headers
        foreach (var sheet in sheetInfoResponse.Sheets)
        {
            var sheetName = sheet.Properties.Title;
            var sheetHeader = HeaderHelpers.GetHeadersFromCellData(sheet.Data?[0]?.RowData?[0]?.Values);
            var sheetModel = GetSheetModelFromMapper(sheetName);

            if (sheetModel == null)
            {
                messages.Add(MessageHelpers.CreateWarningMessage(
                    $"Sheet {sheetName} does not match any known sheet name", 
                    MessageTypeEnum.CHECK_SHEET));
                continue;
            }

            // Check headers and get missing column info
            var sheetMessages = HeaderHelpers.CheckSheetHeaders(sheetHeader, sheetModel, out var insertionInfo);
            headerMessages.AddRange(sheetMessages);

            // If there are missing columns, add sheet ID and store them
            if (insertionInfo.Count > 0)
            {
                foreach (var info in insertionInfo)
                {
                    info.SheetId = sheet.Properties.SheetId ?? 0;
                }
                missingColumns[sheetName] = insertionInfo;
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

    /// <summary>
    /// Inserts missing columns into sheets based on validation results.
    /// </summary>
    /// <param name="missingColumns">Dictionary of sheet names to missing column information</param>
    /// <returns>Result entity with messages about the insertion operation</returns>
    public async Task<SheetEntity> InsertMissingColumns(Dictionary<string, List<ColumnInsertionInfo>> missingColumns)
    {
        var sheetEntity = new SheetEntity();

        if (missingColumns == null || missingColumns.Count == 0)
        {
            sheetEntity.Messages.Add(MessageHelpers.CreateInfoMessage(
                "No missing columns to insert",
                MessageTypeEnum.CHECK_SHEET));
            return sheetEntity;
        }

        var requests = new List<Request>();

        foreach (var (sheetName, columns) in missingColumns)
        {
            // Sort columns by index in descending order to insert from right to left
            // This prevents column shifting issues
            var sortedColumns = columns.OrderByDescending(c => c.ColumnIndex).ToList();

            foreach (var column in sortedColumns)
            {
                // Insert the column
                requests.Add(GoogleRequestHelpers.GenerateInsertColumnDimension(
                    column.SheetId,
                    column.ColumnIndex,
                    column.ColumnIndex + 1,
                    inheritFromBefore: true));

                // Update the header cell with the column name
                var headerRow = new RowData
                {
                    Values = new List<CellData>
                    {
                        new CellData
                        {
                            UserEnteredValue = new ExtendedValue { StringValue = column.ColumnName }
                        }
                    }
                };

                requests.Add(GoogleRequestHelpers.GenerateUpdateCellsRequest(
                    column.SheetId,
                    0, // Header row index
                    new List<RowData> { headerRow }));

                sheetEntity.Messages.Add(MessageHelpers.CreateInfoMessage(
                    $"Inserting column '{column.ColumnName}' at index {column.ColumnIndex} in sheet '{sheetName}'",
                    MessageTypeEnum.CHECK_SHEET));
            }
        }

        // Execute the batch request
        var batchRequest = new BatchUpdateSpreadsheetRequest { Requests = requests };
        var result = await _googleSheetService.BatchUpdateSpreadsheet(batchRequest);

        if (result != null)
        {
            sheetEntity.Messages.Add(MessageHelpers.CreateInfoMessage(
                $"Successfully inserted {missingColumns.Sum(kv => kv.Value.Count)} missing column(s)",
                MessageTypeEnum.CHECK_SHEET));
        }
        else
        {
            sheetEntity.Messages.Add(MessageHelpers.CreateErrorMessage(
                "Failed to insert missing columns",
                MessageTypeEnum.CHECK_SHEET));
        }

        return sheetEntity;
    }

    #endregion

    #region Sheet Layouts

    /// <summary>
    /// Gets the strongly-typed sheet layout/configuration for a specific sheet.
    /// This includes formulas, colors, notes, formats, and all other sheet properties.
    /// Useful for examining what the expected sheet structure should be.
    /// </summary>
    /// <param name="sheet">The name of the sheet to get configuration for</param>
    /// <returns>SheetModel containing the complete sheet configuration, or null if sheet not found</returns>
    public SheetModel? GetSheetLayout(string sheet)
    {
        return GetSheetModelFromMapper(sheet);
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

    /// <summary>
    /// Gets the sheet model from the appropriate mapper based on sheet name.
    /// This returns the model with headers generated from entities and all configuration.
    /// </summary>
    private static SheetModel? GetSheetModelFromMapper(string sheetName)
    {
        try
        {
            return sheetName switch
            {
                var s when string.Equals(s, SheetsConfig.SheetNames.Addresses, StringComparison.OrdinalIgnoreCase) 
                    => AddressMapper.GetSheet(),
                var s when string.Equals(s, SheetsConfig.SheetNames.Daily, StringComparison.OrdinalIgnoreCase) 
                    => DailyMapper.GetSheet(),
                var s when string.Equals(s, SheetsConfig.SheetNames.Expenses, StringComparison.OrdinalIgnoreCase) 
                    => GenericSheetMapper<ExpenseEntity>.GetSheet(SheetsConfig.ExpenseSheet),
                var s when string.Equals(s, SheetsConfig.SheetNames.Monthly, StringComparison.OrdinalIgnoreCase) 
                    => MonthlyMapper.GetSheet(),
                var s when string.Equals(s, SheetsConfig.SheetNames.Names, StringComparison.OrdinalIgnoreCase) 
                    => NameMapper.GetSheet(),
                var s when string.Equals(s, SheetsConfig.SheetNames.Places, StringComparison.OrdinalIgnoreCase) 
                    => PlaceMapper.GetSheet(),
                var s when string.Equals(s, SheetsConfig.SheetNames.Regions, StringComparison.OrdinalIgnoreCase) 
                    => RegionMapper.GetSheet(),
                var s when string.Equals(s, SheetsConfig.SheetNames.Services, StringComparison.OrdinalIgnoreCase) 
                    => ServiceMapper.GetSheet(),
                var s when string.Equals(s, SheetsConfig.SheetNames.Setup, StringComparison.OrdinalIgnoreCase) 
                    => GenericSheetMapper<SetupEntity>.GetSheet(SheetsConfig.SetupSheet),
                var s when string.Equals(s, SheetsConfig.SheetNames.Shifts, StringComparison.OrdinalIgnoreCase) 
                    => ShiftMapper.GetSheet(),
                var s when string.Equals(s, SheetsConfig.SheetNames.Trips, StringComparison.OrdinalIgnoreCase) 
                    => TripMapper.GetSheet(),
                var s when string.Equals(s, SheetsConfig.SheetNames.Types, StringComparison.OrdinalIgnoreCase) 
                    => TypeMapper.GetSheet(),
                var s when string.Equals(s, SheetsConfig.SheetNames.Weekdays, StringComparison.OrdinalIgnoreCase) 
                    => WeekdayMapper.GetSheet(),
                var s when string.Equals(s, SheetsConfig.SheetNames.Weekly, StringComparison.OrdinalIgnoreCase) 
                    => WeeklyMapper.GetSheet(),
                var s when string.Equals(s, SheetsConfig.SheetNames.Yearly, StringComparison.OrdinalIgnoreCase) 
                    => YearlyMapper.GetSheet(),
                _ => null
            };
        }
        catch
        {
            return null;
        }
    }

    #endregion

    #region Formatting Operations

    /// <summary>
    /// Reapplies formatting to a sheet based on its configuration in SheetsConfig.
    /// This allows updating column formats, colors, borders, and protection without changing data.
    /// </summary>
    /// <param name="sheet">The sheet name to reapply formatting to.</param>
    /// <param name="options">Formatting options to apply. If null, uses Common defaults.</param>
    /// <returns>SheetEntity with messages indicating success or errors.</returns>
    public async Task<SheetEntity> ReapplyFormatting(string sheet)
    {
        return await ReapplyFormatting(new List<string> { sheet });
    }

    /// <summary>
    /// Reapplies all formatting to multiple sheets based on their configurations in SheetsConfig.
    /// </summary>
    /// <param name="sheets">The sheet names to reapply formatting to.</param>
    /// <returns>SheetEntity with messages indicating success or errors.</returns>
    public async Task<SheetEntity> ReapplyFormatting(List<string> sheets)
    {
        var sheetEntity = new SheetEntity();

        if (sheets == null || sheets.Count == 0)
        {
            sheetEntity.Messages.Add(MessageHelpers.CreateWarningMessage(
                "No sheets specified for formatting reapplication",
                MessageTypeEnum.APPLY_FORMAT));
            return sheetEntity;
        }

        try
        {
            // Get spreadsheet info to find sheet IDs
            var spreadsheetInfo = await _googleSheetService.GetSheetInfo();
            if (spreadsheetInfo?.Sheets == null || spreadsheetInfo.Sheets.Count == 0)
            {
                sheetEntity.Messages.Add(MessageHelpers.CreateErrorMessage(
                    "Unable to retrieve sheet information",
                    MessageTypeEnum.APPLY_FORMAT));
                return sheetEntity;
            }

            var batchUpdateRequest = new BatchUpdateSpreadsheetRequest { Requests = new List<Request>() };

            foreach (var sheet in sheets)
            {
                var sheetInfo = spreadsheetInfo.Sheets.FirstOrDefault(s =>
                    string.Equals(s.Properties.Title, sheet, StringComparison.OrdinalIgnoreCase));

                if (sheetInfo == null)
                {
                    sheetEntity.Messages.Add(MessageHelpers.CreateWarningMessage(
                        $"Sheet '{sheet}' not found in spreadsheet",
                        MessageTypeEnum.APPLY_FORMAT));
                    continue;
                }

                var sheetId = sheetInfo.Properties.SheetId.GetValueOrDefault();
                var sheetModel = GetSheetConfigurationInternal(sheet);

                if (sheetModel == null)
                {
                    sheetEntity.Messages.Add(MessageHelpers.CreateWarningMessage(
                        $"No configuration found for sheet '{sheet}'",
                        MessageTypeEnum.APPLY_FORMAT));
                    continue;
                }

                // Add all formatting requests for this sheet
                GenerateFormattingRequests(
                    batchUpdateRequest,
                    sheetModel,
                    sheetId,
                    sheetInfo);

                sheetEntity.Messages.Add(MessageHelpers.CreateWarningMessage(
                    $"Formatting queued for {sheet}",
                    MessageTypeEnum.APPLY_FORMAT));
            }

            // Execute batch update if there are requests
            if (batchUpdateRequest.Requests.Count > 0)
            {
                var response = await _googleSheetService.BatchUpdateSpreadsheet(batchUpdateRequest);
                if (response != null)
                {
                    sheetEntity.Messages.Add(MessageHelpers.CreateWarningMessage(
                        $"Formatting applied successfully to {sheets.Count} sheet(s)",
                        MessageTypeEnum.APPLY_FORMAT));
                }
                else
                {
                    sheetEntity.Messages.Add(MessageHelpers.CreateErrorMessage(
                        "Failed to apply formatting - no response from API",
                        MessageTypeEnum.APPLY_FORMAT));
                }
            }
        }
        catch (Exception ex)
        {
            sheetEntity.Messages.Add(MessageHelpers.CreateErrorMessage(
                $"Error reapplying formatting: {ex.Message}",
                MessageTypeEnum.APPLY_FORMAT));
        }

        return sheetEntity;
    }

    /// <summary>
    /// Gets the sheet configuration from SheetsConfig for a given sheet name.
    /// </summary>
    public static SheetModel GetSheetConfiguration(string sheetName)
    {
        var config = GetSheetConfigurationInternal(sheetName);
        if (config == null)
        {
            throw new ArgumentException($"Unknown sheet name: {sheetName}", nameof(sheetName));
        }
        return config;
    }

    private static SheetModel? GetSheetConfigurationInternal(string sheetName)
    {
        return sheetName switch
        {
            _ when string.Equals(sheetName, SheetsConfig.SheetNames.Trips, StringComparison.OrdinalIgnoreCase) 
                => SheetsConfig.TripSheet,
            _ when string.Equals(sheetName, SheetsConfig.SheetNames.Shifts, StringComparison.OrdinalIgnoreCase) 
                => SheetsConfig.ShiftSheet,
            _ when string.Equals(sheetName, SheetsConfig.SheetNames.Expenses, StringComparison.OrdinalIgnoreCase) 
                => SheetsConfig.ExpenseSheet,
            _ when string.Equals(sheetName, SheetsConfig.SheetNames.Addresses, StringComparison.OrdinalIgnoreCase) 
                => SheetsConfig.AddressSheet,
            _ when string.Equals(sheetName, SheetsConfig.SheetNames.Names, StringComparison.OrdinalIgnoreCase) 
                => SheetsConfig.NameSheet,
            _ when string.Equals(sheetName, SheetsConfig.SheetNames.Places, StringComparison.OrdinalIgnoreCase) 
                => SheetsConfig.PlaceSheet,
            _ when string.Equals(sheetName, SheetsConfig.SheetNames.Regions, StringComparison.OrdinalIgnoreCase) 
                => SheetsConfig.RegionSheet,
            _ when string.Equals(sheetName, SheetsConfig.SheetNames.Services, StringComparison.OrdinalIgnoreCase) 
                => SheetsConfig.ServiceSheet,
            _ when string.Equals(sheetName, SheetsConfig.SheetNames.Types, StringComparison.OrdinalIgnoreCase) 
                => SheetsConfig.TypeSheet,
            _ when string.Equals(sheetName, SheetsConfig.SheetNames.Daily, StringComparison.OrdinalIgnoreCase) 
                => SheetsConfig.DailySheet,
            _ when string.Equals(sheetName, SheetsConfig.SheetNames.Weekly, StringComparison.OrdinalIgnoreCase) 
                => SheetsConfig.WeeklySheet,
            _ when string.Equals(sheetName, SheetsConfig.SheetNames.Monthly, StringComparison.OrdinalIgnoreCase) 
                => SheetsConfig.MonthlySheet,
            _ when string.Equals(sheetName, SheetsConfig.SheetNames.Yearly, StringComparison.OrdinalIgnoreCase) 
                => SheetsConfig.YearlySheet,
            _ when string.Equals(sheetName, SheetsConfig.SheetNames.Setup, StringComparison.OrdinalIgnoreCase) 
                => SheetsConfig.SetupSheet,
            _ => null
        };
    }

    /// <summary>
    /// Generates all formatting API requests for a sheet based on its configuration.
    /// </summary>
    private static void GenerateFormattingRequests(
        BatchUpdateSpreadsheetRequest batchRequest,
        SheetModel sheetModel,
        int sheetId,
        Google.Apis.Sheets.v4.Data.Sheet sheetInfo)
    {
        // Apply column formats
        if (sheetModel.Headers.Count > 0)
        {
            foreach (var header in sheetModel.Headers.Where(h => h.Format.HasValue))
            {
                if (!string.IsNullOrEmpty(header.FormatPattern))
                {
                    batchRequest.Requests.Add(
                        GoogleRequestHelpers.GenerateUpdateNumberFormat(
                            sheetId,
                            0, // Start from first row
                            sheetInfo.Data?.FirstOrDefault()?.RowData?.Count ?? 1000, // To end
                            header.Index,
                            header.Index + 1,
                            header.Format!.Value.GetDescription(),
                            header.FormatPattern));
                }
            }
        }

        // Apply tab color
        if (sheetModel.TabColor != ColorEnum.BLACK)
        {
            var color = SheetHelpers.GetColor(sheetModel.TabColor);
            batchRequest.Requests.Add(
                GoogleRequestHelpers.GenerateUpdateTabColor(
                    sheetId,
                    color.Red ?? 0,
                    color.Green ?? 0,
                    color.Blue ?? 0));
        }

        // Apply alternating row colors (cell color)
        if (sheetModel.CellColor != ColorEnum.BLACK)
        {
            var color = SheetHelpers.GetColor(sheetModel.CellColor);
            batchRequest.Requests.Add(
                GoogleRequestHelpers.GenerateUpdateCellColor(
                    sheetId,
                    1, // Start from row 2 (after header)
                    sheetInfo.Data?.FirstOrDefault()?.RowData?.Count ?? 1000,
                    0,
                    sheetModel.Headers.Count,
                    color.Red ?? 0,
                    color.Green ?? 0,
                    color.Blue ?? 0));
        }

        // Apply frozen rows/columns
        batchRequest.Requests.Add(
            GoogleRequestHelpers.GenerateUpdateFrozenRowsColumns(
                sheetId,
                sheetModel.FreezeRowCount,
                sheetModel.FreezeColumnCount));

        // Apply protection if configured
        if (sheetModel.ProtectSheet)
        {
            batchRequest.Requests.Add(
                GoogleRequestHelpers.GenerateProtectSheet(
                    sheetId,
                    $"Protected: {sheetModel.Name}"));
        }
    }

    #endregion
}
