using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RaptorSheets.Core.Helpers
{
    /// <summary>
    /// Helper that builds AddSheet requests which insert missing sheets
    /// in positions that reflect the requested sheet ordering.
    /// </summary>
    public static class SheetOrderingHelper
    {
        public static IList<Request> BuildAddSheetRequests(Spreadsheet spreadsheetInfo, List<string> requestedSheets)
        {
            var requests = new List<Request>();

            if (requestedSheets == null || requestedSheets.Count == 0)
            {
                return requests;
            }

            // Build a list of existing sheet titles -> index
            var existingList = new List<(string Title, int Index)>();
            if (spreadsheetInfo?.Sheets != null)
            {
                existingList = spreadsheetInfo.Sheets
                    .Select((s, idx) => (Title: s?.Properties?.Title ?? string.Empty, Index: s?.Properties?.Index ?? idx))
                    .Where(x => !string.IsNullOrEmpty(x.Title))
                    .ToList();
            }

            var existingIndexMap = existingList
                .ToDictionary(e => e.Title, e => e.Index, StringComparer.OrdinalIgnoreCase);

            // Determine insertion index for each missing sheet based on the next requested sheet
            // that already exists; if none found, append at the end preserving requested order.
            var insertionEntries = new List<(string Name, int TargetIndex, int OriginalOrder)>();
            int appendCounter = 0;

            for (int i = 0; i < requestedSheets.Count; i++)
            {
                var requestedName = requestedSheets[i];
                if (!existingIndexMap.ContainsKey(requestedName))
                {
                    int? nextExistingIndex = null;
                    for (int j = i + 1; j < requestedSheets.Count; j++)
                    {
                        var nextRequested = requestedSheets[j];
                        if (existingIndexMap.TryGetValue(nextRequested, out var idx))
                        {
                            nextExistingIndex = idx;
                            break;
                        }
                    }

                    int targetIndex;
                    if (nextExistingIndex.HasValue)
                    {
                        targetIndex = nextExistingIndex.Value;
                    }
                    else
                    {
                        // Append at the end (preserve relative order for multiple new sheets)
                        targetIndex = existingList.Count + appendCounter;
                        appendCounter++;
                    }

                    insertionEntries.Add((requestedName, targetIndex, insertionEntries.Count));
                }
            }

            // Insert in descending target index order (and descending original order when equal)
            // so repeated inserts before the same index preserve the requested sequence.
            var orderedInsertions = insertionEntries
                .OrderByDescending(x => x.TargetIndex)
                .ThenByDescending(x => x.OriginalOrder)
                .ToList();

            foreach (var entry in orderedInsertions)
            {
                var add = new Request
                {
                    AddSheet = new AddSheetRequest
                    {
                        Properties = new SheetProperties
                        {
                            Title = entry.Name,
                            Index = entry.TargetIndex
                        }
                    }
                };

                requests.Add(add);
            }

            return requests;
        }
    }
}
