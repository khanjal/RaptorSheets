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

    private static IEnumerable<MessageEntity> GetHeaderMessagesForKnownSheets(IEnumerable<Sheet> knownSheets)
    {
        foreach (var sheet in knownSheets)
        {
            var sheetName = sheet.Properties.Title;
            var sheetHeader = HeaderHelpers.GetHeadersFromCellData(sheet.Data?[0]?.RowData?[0]?.Values);

            if (s_headerFactories.TryGetValue(sheetName, out var factory))
            {
                foreach (var msg in HeaderHelpers.CheckSheetHeaders(sheetHeader, factory()))
                {
                    yield return msg;
                }
            }
        }
    }

    private static IEnumerable<MessageEntity> GetUnknownSheetWarnings(IEnumerable<Sheet> unknownSheets)
    {
        foreach (var sheet in unknownSheets)
        {
            yield return MessageHelpers.CreateWarningMessage($"Sheet {sheet.Properties.Title} does not match any known sheet name", MessageTypeEnum.CHECK_SHEET);
        }
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

    // Sheet name -> SheetModel factory (case-insensitive) used for header checks
    private static readonly Dictionary<string, Func<SheetModel>> s_headerFactories = new(StringComparer.OrdinalIgnoreCase)
    {
        { SheetsConfig.SheetNames.Addresses, AddressMapper.GetSheet },
        { SheetsConfig.SheetNames.Daily, DailyMapper.GetSheet },
        { SheetsConfig.SheetNames.Expenses, () => GenericSheetMapper<ExpenseEntity>.GetSheet(SheetsConfig.ExpenseSheet) },
        { SheetsConfig.SheetNames.Monthly, MonthlyMapper.GetSheet },
        { SheetsConfig.SheetNames.Names, NameMapper.GetSheet },
        { SheetsConfig.SheetNames.Places, PlaceMapper.GetSheet },
        { SheetsConfig.SheetNames.Deliveries, DeliveryMapper.GetSheet },
        { SheetsConfig.SheetNames.Locations, LocationMapper.GetSheet },
        { SheetsConfig.SheetNames.Regions, RegionMapper.GetSheet },
        { SheetsConfig.SheetNames.Services, ServiceMapper.GetSheet },
        { SheetsConfig.SheetNames.Setup, () => GenericSheetMapper<SetupEntity>.GetSheet(SheetsConfig.SetupSheet) },
        { SheetsConfig.SheetNames.Shifts, ShiftMapper.GetSheet },
        { SheetsConfig.SheetNames.Trips, TripMapper.GetSheet },
        { SheetsConfig.SheetNames.Types, TypeMapper.GetSheet },
        { SheetsConfig.SheetNames.Weekdays, WeekdayMapper.GetSheet },
        { SheetsConfig.SheetNames.Weekly, WeeklyMapper.GetSheet },
        { SheetsConfig.SheetNames.Yearly, YearlyMapper.GetSheet }
    };

    public static List<MessageEntity> CheckSheetHeaders(Spreadsheet sheetInfoResponse)
    {
        var messages = new List<MessageEntity>();

        if (sheetInfoResponse == null)
        {
            messages.Add(MessageHelpers.CreateErrorMessage($"Unable to retrieve sheet(s)", MessageTypeEnum.GENERAL));
            return messages;
        }
        // Separate known and unknown sheets to simplify logic
        var sheets = sheetInfoResponse.Sheets ?? new List<Sheet>();
        var knownSheets = sheets.Where(s => s_headerFactories.ContainsKey(s.Properties.Title));
        var unknownSheets = sheets.Except(knownSheets);

        var headerMessages = GetHeaderMessagesForKnownSheets(knownSheets).ToList();
        messages.AddRange(GetUnknownSheetWarnings(unknownSheets));

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
            // Use the centralized factory map for sheet layouts (case-insensitive)
            if (string.IsNullOrEmpty(sheet))
                return null;

            if (s_headerFactories.TryGetValue(sheet, out var factory))
                return factory();

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
}
