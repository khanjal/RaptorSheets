using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaptorSheets.Core.Helpers
{
    /// <summary>
    /// Encapsulates logic to ensure requested sheets exist in a spreadsheet.
    /// Extracted for testability and reuse.
    /// </summary>
    public static class SheetInitializationHelper
    {
        public static async Task EnsureMissingSheetsCreatedAsync(IGoogleSheetService sheetService, List<string> sheets)
        {
            if (sheetService == null) throw new ArgumentNullException(nameof(sheetService));
            if (sheets == null || sheets.Count == 0) return;

            try
            {
                var spreadsheetInfo = await sheetService.GetSheetInfo();
                if (spreadsheetInfo != null && spreadsheetInfo.Sheets != null)
                {
                    var existingTitles = spreadsheetInfo.Sheets
                        .Where(s => s?.Properties?.Title != null)
                        .Select(s => s.Properties.Title)
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    var missingSheets = sheets.Where(s => !existingTitles.Contains(s)).ToList();

                    if (missingSheets.Count > 0)
                    {
                        var requests = SheetOrderingHelper.BuildAddSheetRequests(spreadsheetInfo, sheets);
                        if (requests != null && requests.Count > 0)
                        {
                            var batchUpdate = new Google.Apis.Sheets.v4.Data.BatchUpdateSpreadsheetRequest
                            {
                                Requests = requests.ToList()
                            };

                            var createResponse = await sheetService.BatchUpdateSpreadsheet(batchUpdate);
                            if (createResponse == null)
                            {
                                Console.WriteLine($"Warning: failed to create missing sheets: {string.Join(',', missingSheets)}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Do not fail here; callers will continue to attempt their operation
                Console.WriteLine($"Warning while ensuring sheets exist: {ex.Message}");
            }
        }
    }
}
