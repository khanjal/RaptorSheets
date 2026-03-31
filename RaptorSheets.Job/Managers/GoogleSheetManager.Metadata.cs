using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Job.Helpers;

namespace RaptorSheets.Job.Managers;

/// <summary>
/// Metadata and property access for Google Sheets.
/// Provides access to sheet properties, headers, and layout information.
/// </summary>
public partial class GoogleSheetManager
{
    public async Task<List<PropertyEntity>> GetAllSheetProperties()
    {
        var sheetNames = GenerateSheetsHelpers.GetSheetNames();
        return await GetSheetProperties(sheetNames);
    }

    public async Task<List<PropertyEntity>> GetSheetProperties(List<string> sheets)
    {
        var properties = new List<PropertyEntity>();
        var spreadsheetInfo = await _googleSheetService.GetSheetInfo();

        if (spreadsheetInfo?.Sheets == null)
            return properties;

        foreach (var sheet in sheets)
        {
            var sheetInfo = spreadsheetInfo.Sheets.FirstOrDefault(s =>
                string.Equals(s.Properties.Title, sheet, StringComparison.OrdinalIgnoreCase));

            if (sheetInfo != null)
            {
                properties.Add(new PropertyEntity
                {
                    Name = sheetInfo.Properties.Title ?? "",
                    Id = (sheetInfo.Properties.SheetId ?? 0).ToString()
                });
            }
        }

        return properties;
    }

    public async Task<List<string>> GetAllSheetTabNames()
    {
        var spreadsheetInfo = await _googleSheetService.GetSheetInfo();
        return spreadsheetInfo?.Sheets?.Select(s => s.Properties.Title ?? "").ToList() ?? new List<string>();
    }

    public SheetModel? GetSheetLayout(string sheet)
    {
        return JobSheetHelpers.GetSheetLayout(sheet);
    }

    public List<SheetModel> GetSheetLayouts(List<string> sheets)
    {
        return JobSheetHelpers.GetSheetLayouts(sheets);
    }
}
