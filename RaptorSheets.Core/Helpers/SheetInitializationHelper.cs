using RaptorSheets.Core.Services;

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
        public static async Task<(List<string> Found, bool Created)> EnsureMissingSheetsCreatedAsync(IGoogleSheetService sheetService, List<string> sheets)
        {
            if (sheetService == null) throw new ArgumentNullException(nameof(sheetService));
            if (sheets == null || sheets.Count == 0) return (new List<string>(), false);

            try
            {
                // Get current spreadsheet info
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

                var createdAny = false;
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
                            // Mark that we at least attempted and got a non-null response
                            createdAny = true;
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

                return (found.ToList(), createdAny);
            }
            catch (Exception ex)
            {
                // Do not fail here; callers will continue to attempt their operation. Return requested list as fallback.
                Console.WriteLine($"Warning while ensuring sheets exist: {ex.Message}");
                return (sheets.Distinct(StringComparer.OrdinalIgnoreCase).ToList(), false);
            }
        }

        /// <summary>
        /// Returns a mapping of missing sheet titles to their desired insertion index.
        /// The returned dictionary contains only sheet names that are missing from the
        /// spreadsheet and the index that callers should use when creating them.
        /// This does not create any sheets; it only inspects the spreadsheet and computes
        /// target indexes for missing names.
        /// </summary>
        public static async Task<Dictionary<string,int>> GetMissingSheetsAsync(IGoogleSheetService sheetService, List<string> sheets)
        {
            if (sheetService == null) throw new ArgumentNullException(nameof(sheetService));
            if (sheets == null || sheets.Count == 0) return new Dictionary<string,int>(StringComparer.OrdinalIgnoreCase);

            try
            {
                var spreadsheetInfo = await sheetService.GetSheetInfo();

                var existingTitles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var titleIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                if (spreadsheetInfo?.Sheets != null)
                {
                    for (int i = 0; i < spreadsheetInfo.Sheets.Count; i++)
                    {
                        var s = spreadsheetInfo.Sheets[i];
                        var title = s?.Properties?.Title;
                        if (!string.IsNullOrEmpty(title))
                        {
                            existingTitles.Add(title);
                            // Prefer the explicit Index property when present, otherwise fall back to list position
                            var idx = s?.Properties?.Index ?? i;
                            titleIndex[title] = idx;
                        }
                    }
                }

                var missingSheets = sheets.Where(s => !existingTitles.Contains(s)).ToList();

                var missingIndexMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                // Compute target indexes for missing sheets so callers can create them at the right positions.
                // Leverage SheetOrderingHelper which returns AddSheet requests with desired Index values.
                try
                {
                    var addRequests = SheetOrderingHelper.BuildAddSheetRequests(spreadsheetInfo, sheets);
                    if (addRequests != null)
                    {
                        foreach (var req in addRequests)
                        {
                            var add = req?.AddSheet;
                            var title = add?.Properties?.Title;
                            var idx = add?.Properties?.Index;
                            if (!string.IsNullOrEmpty(title) && idx.HasValue)
                            {
                                // Only include entries for sheets that are actually missing
                                if (missingSheets.Contains(title, StringComparer.OrdinalIgnoreCase))
                                {
                                    missingIndexMap[title] = idx.Value;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // If ordering helper fails for some reason, log and continue — callers will still get missing names but no indexes.
                    Console.WriteLine($"Warning computing target indexes for missing sheets: {ex.Message}");
                }

                return missingIndexMap;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning while checking missing sheets: {ex.Message}");
                return new Dictionary<string,int>(StringComparer.OrdinalIgnoreCase);
            }
        }
    }
}
