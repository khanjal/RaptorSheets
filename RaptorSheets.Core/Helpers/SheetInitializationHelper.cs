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
        /// <summary>
        /// Ensure requested sheets exist. Returns a list of sheet titles that are present
        /// after the operation (this includes existing sheets and any that were created).
        /// The returned list may include extra sheets that were not explicitly requested.
        /// </summary>
        public static async Task<List<string>> EnsureMissingSheetsCreatedAsync(IGoogleSheetService sheetService, List<string> sheets)
        {
            if (sheetService == null) throw new ArgumentNullException(nameof(sheetService));
            if (sheets == null || sheets.Count == 0) return new List<string>();

            try
            {
                // Get current spreadsheet info (may come from cache)
                var spreadsheetInfo = await sheetService.GetSheetInfo();

                // Build a set of existing sheet titles (case-insensitive)
                var existingTitles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                if (spreadsheetInfo?.Sheets != null)
                {
                    foreach (var s in spreadsheetInfo.Sheets)
                    {
                        var title = s?.Properties?.Title;
                        if (!string.IsNullOrEmpty(title)) existingTitles.Add(title);
                    }
                }

                // Determine which requested sheets are missing
                var missingSheets = sheets.Where(s => !existingTitles.Contains(s)).ToList();

                if (missingSheets.Count > 0)
                {
                    // Build add-sheet requests (helper gracefully handles null/empty spreadsheetInfo)
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
                        else
                        {
                            // If creation succeeded, refresh spreadsheet info (service invalidates cache on successful update)
                            spreadsheetInfo = await sheetService.GetSheetInfo();
                        }
                    }
                }

                // Recompute the final found titles from the refreshed (or original) spreadsheet info
                var found = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                if (spreadsheetInfo?.Sheets != null)
                {
                    foreach (var s in spreadsheetInfo.Sheets)
                    {
                        var title = s?.Properties?.Title;
                        if (!string.IsNullOrEmpty(title)) found.Add(title);
                    }
                }

                // Ensure the returned list includes requested names even if the spreadsheet info was unavailable
                foreach (var req in sheets)
                {
                    if (!string.IsNullOrWhiteSpace(req)) found.Add(req);
                }

                return found.ToList();
            }
            catch (Exception ex)
            {
                // Do not fail here; callers will continue to attempt their operation. Return requested list as fallback.
                Console.WriteLine($"Warning while ensuring sheets exist: {ex.Message}");
                return sheets.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            }
        }
    }
}
