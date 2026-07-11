using Google.Apis.Sheets.v4.Data;

namespace RaptorSheets.Core.Helpers
{
    /// <summary>
    /// Pure helper that computes missing sheet information from already-fetched spreadsheet metadata.
    /// Does not perform any I/O — callers are responsible for fetching <see cref="Spreadsheet"/> info.
    /// </summary>
    public static class SheetInitializationHelper
    {
        /// <summary>
        /// Returns a mapping of missing sheet titles to their desired insertion index.
        /// <paramref name="allSheets"/> should be the complete ordered list of sheets the
        /// spreadsheet is expected to have — this is required for correct index computation.
        /// </summary>
        public static Dictionary<string, int> GetMissingSheets(Spreadsheet? spreadsheetInfo, List<string> allSheets)
        {
            if (allSheets == null || allSheets.Count == 0)
                return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            var existingTitles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (spreadsheetInfo?.Sheets != null)
            {
                foreach (var s in spreadsheetInfo.Sheets)
                {
                    var title = s?.Properties?.Title;
                    if (!string.IsNullOrEmpty(title))
                        existingTitles.Add(title);
                }
            }

            var missingSheets = allSheets.Where(s => !existingTitles.Contains(s)).ToList();

            if (missingSheets.Count == 0)
                return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            var missingIndexMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            // Pass the full allSheets list so ordering helper can compute correct target indices
            // relative to all expected sheets, not just the missing subset.
            var addRequests = SheetOrderingHelper.BuildAddSheetRequests(spreadsheetInfo!, allSheets);
            if (addRequests != null)
            {
                foreach (var req in addRequests)
                {
                    var title = req?.AddSheet?.Properties?.Title;
                    var idx = req?.AddSheet?.Properties?.Index;
                    if (!string.IsNullOrEmpty(title) && idx.HasValue)
                        missingIndexMap[title] = idx.Value;
                }
            }

            // Fallback: include any missing sheets that didn't get an index from the ordering helper
            foreach (var missing in missingSheets.Where(m => !missingIndexMap.ContainsKey(m)))
                missingIndexMap[missing] = -1;

            return missingIndexMap;
        }
    }
}
