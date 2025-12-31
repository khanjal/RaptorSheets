using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Mappers;
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
        var messages = new List<MessageEntity>();

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

            switch (sheetName)
            {
                case var s when string.Equals(s, SheetsConfig.SheetNames.Addresses, StringComparison.OrdinalIgnoreCase):
                    headerMessages.AddRange(HeaderHelpers.CheckSheetHeaders(sheetHeader, AddressMapper.GetSheet()));
                    break;
                case var s when string.Equals(s, SheetsConfig.SheetNames.Daily, StringComparison.OrdinalIgnoreCase):
                    headerMessages.AddRange(HeaderHelpers.CheckSheetHeaders(sheetHeader, DailyMapper.GetSheet()));
                    break;
                case var s when string.Equals(s, SheetsConfig.SheetNames.Expenses, StringComparison.OrdinalIgnoreCase):
                    headerMessages.AddRange(HeaderHelpers.CheckSheetHeaders(sheetHeader, GenericSheetMapper<ExpenseEntity>.GetSheet(SheetsConfig.ExpenseSheet)));
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
                    headerMessages.AddRange(HeaderHelpers.CheckSheetHeaders(sheetHeader, GenericSheetMapper<SetupEntity>.GetSheet(SheetsConfig.SetupSheet)));
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
        try
        {
            // Use the existing helper to get the appropriate mapper's sheet model
            if (string.Equals(sheet, SheetsConfig.SheetNames.Addresses, StringComparison.OrdinalIgnoreCase))
                return AddressMapper.GetSheet();
            if (string.Equals(sheet, SheetsConfig.SheetNames.Daily, StringComparison.OrdinalIgnoreCase))
                return DailyMapper.GetSheet();
            if (string.Equals(sheet, SheetsConfig.SheetNames.Expenses, StringComparison.OrdinalIgnoreCase))
                return GenericSheetMapper<ExpenseEntity>.GetSheet(SheetsConfig.ExpenseSheet);
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
                return GenericSheetMapper<SetupEntity>.GetSheet(SheetsConfig.SetupSheet);
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

    #endregion

    #region Formatting Operations

    /// <summary>
    /// Reapplies formatting to a sheet based on its configuration in SheetsConfig.
    /// This allows updating column formats, colors, borders, and protection without changing data.
    /// </summary>
    /// <param name="sheet">The sheet name to reapply formatting to.</param>
    /// <param name="options">Formatting options to apply. If null, uses Common defaults.</param>
    /// <returns>SheetEntity with messages indicating success or errors.</returns>
    public async Task<SheetEntity> ReapplyFormatting(string sheet, FormattingOptionsEntity? options = null)
    {
        return await ReapplyFormatting(new List<string> { sheet }, options ?? FormattingOptionsEntity.Common);
    }

    /// <summary>
    /// Reapplies formatting to multiple sheets based on their configurations in SheetsConfig.
    /// </summary>
    /// <param name="sheets">The sheet names to reapply formatting to.</param>
    /// <param name="options">Formatting options to apply. If null, uses Common defaults.</param>
    /// <returns>SheetEntity with messages indicating success or errors.</returns>
    public async Task<SheetEntity> ReapplyFormatting(List<string> sheets, FormattingOptionsEntity? options = null)
    {
        var sheetEntity = new SheetEntity();
        var formattingOptions = options ?? FormattingOptionsEntity.Common;

        if (!formattingOptions.HasAnyOptions)
        {
            sheetEntity.Messages.Add(MessageHelpers.CreateWarningMessage(
                "No formatting options selected for reapplication",
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

                // Add formatting requests based on options
                GenerateFormattingRequests(
                    batchUpdateRequest,
                    sheetModel,
                    sheetId,
                    sheetInfo,
                    formattingOptions);

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
    /// Generates formatting API requests based on the sheet configuration and options.
    /// </summary>
    private static void GenerateFormattingRequests(
        BatchUpdateSpreadsheetRequest batchRequest,
        SheetModel sheetModel,
        int sheetId,
        Google.Apis.Sheets.v4.Data.Sheet sheetInfo,
        FormattingOptionsEntity options)
    {
        // Reapply column formats
        if (options.ReapplyColumnFormats && sheetModel.Headers.Count > 0)
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

        // Reapply tab color
        if (options.ReapplyColors && sheetModel.TabColor != ColorEnum.BLACK)
        {
            var color = SheetHelpers.GetColor(sheetModel.TabColor);
            batchRequest.Requests.Add(
                GoogleRequestHelpers.GenerateUpdateTabColor(
                    sheetId,
                    color.Red ?? 0,
                    color.Green ?? 0,
                    color.Blue ?? 0));
        }

        // Reapply alternating row colors (cell color)
        if (options.ReapplyColors && sheetModel.CellColor != ColorEnum.BLACK)
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

        // Reapply frozen rows/columns
        if (options.ReapplyFrozenRows)
        {
            batchRequest.Requests.Add(
                GoogleRequestHelpers.GenerateUpdateFrozenRowsColumns(
                    sheetId,
                    sheetModel.FreezeRowCount,
                    sheetModel.FreezeColumnCount));
        }

        // Reapply protection
        if (options.ReapplyProtection && sheetModel.ProtectSheet)
        {
            batchRequest.Requests.Add(
                GoogleRequestHelpers.GenerateProtectSheet(
                    sheetId,
                    $"Protected: {sheetModel.Name}"));
        }
    }

    #endregion
}
