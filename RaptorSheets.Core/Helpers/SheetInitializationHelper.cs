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

            var existingTitles = GetExistingTitles(spreadsheetInfo);
            var missingSheets = allSheets.Where(s => !existingTitles.Contains(s)).ToList();

            if (missingSheets.Count == 0)
                return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            return BuildMissingIndexMap(spreadsheetInfo, allSheets, missingSheets);
        }

        private static HashSet<string> GetExistingTitles(Spreadsheet? spreadsheetInfo)
        {
            var existingTitles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (spreadsheetInfo?.Sheets == null)
                return existingTitles;

            foreach (var s in spreadsheetInfo.Sheets)
            {
                var title = s?.Properties?.Title;
                if (!string.IsNullOrEmpty(title))
                    existingTitles.Add(title);
            }

            return existingTitles;
        }

        private static Dictionary<string, int> BuildMissingIndexMap(Spreadsheet? spreadsheetInfo, List<string> allSheets, List<string> missingSheets)
        {
            var missingIndexMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            if (spreadsheetInfo == null)
            {
                foreach (var missing in missingSheets)
                    missingIndexMap[missing] = -1;

                return missingIndexMap;
            }

            var addRequests = SheetOrderingHelper.BuildAddSheetRequests(spreadsheetInfo, allSheets);
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

            foreach (var missing in missingSheets.Where(m => !missingIndexMap.ContainsKey(m)))
                missingIndexMap[missing] = -1;

            return missingIndexMap;
        }
    }
}
