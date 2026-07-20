using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Helpers;

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

    #region Header Validation

    /// <summary>
    /// Checks a spreadsheet's tab names for sheets that don't correspond to any known Gig sheet.
    /// Only needs sheet tab metadata (no grid/cell data), so it's safe to call with a cheap
    /// <c>GetSheetInfo()</c> (no ranges) result. Known-sheet header validation (missing/renamed/
    /// reordered columns) is handled separately, per-sheet, using data already fetched via batchGet.
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

    /// <summary>
    /// Physically inserts columns detected as missing by <see cref="CheckSheetHeaders(Spreadsheet, out Dictionary{string, List{ColumnInsertionInfo}})"/>
    /// at their expected position, and writes the header text into each newly-inserted column.
    /// </summary>
    public async Task<SheetEntity> InsertMissingColumns(Dictionary<string, List<ColumnInsertionInfo>> missingColumns)
    {
        return await ColumnInsertionHelper.InsertMissingColumnsAsync<SheetEntity>(_googleSheetService, missingColumns);
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
            return GigSheetHelpers.GetSheetLayout(sheet);
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
