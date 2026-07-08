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

            var existingList = GetExistingList(spreadsheetInfo);
            var existingIndexMap = existingList
                .ToDictionary(e => e.Title, e => e.Index, StringComparer.OrdinalIgnoreCase);

            var insertionEntries = ComputeInsertionEntries(requestedSheets, existingIndexMap, existingList.Count);

            var orderedInsertions = insertionEntries
                .OrderByDescending(x => x.TargetIndex)
                .ThenByDescending(x => x.OriginalOrder)
                .ToList();

            return BuildRequestsFromEntries(orderedInsertions);
        }

        private static List<(string Title, int Index)> GetExistingList(Spreadsheet spreadsheetInfo)
        {
            var existingList = new List<(string Title, int Index)>();

            if (spreadsheetInfo?.Sheets == null)
            {
                return existingList;
            }

            existingList = spreadsheetInfo.Sheets
                .Select((s, idx) => (Title: s?.Properties?.Title ?? string.Empty, Index: s?.Properties?.Index ?? idx))
                .Where(x => !string.IsNullOrEmpty(x.Title))
                .ToList();

            return existingList;
        }

        private static List<(string Name, int TargetIndex, int OriginalOrder)> ComputeInsertionEntries(
            List<string> requestedSheets,
            Dictionary<string, int> existingIndexMap,
            int existingCount)
        {
            var insertionEntries = new List<(string Name, int TargetIndex, int OriginalOrder)>();
            int appendCounter = 0;

            for (int i = 0; i < requestedSheets.Count; i++)
            {
                var requestedName = requestedSheets[i];
                if (existingIndexMap.ContainsKey(requestedName))
                {
                    continue;
                }

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

                int targetIndex = nextExistingIndex ?? (existingCount + appendCounter);
                if (!nextExistingIndex.HasValue)
                {
                    appendCounter++;
                }

                insertionEntries.Add((requestedName, targetIndex, insertionEntries.Count));
            }

            return insertionEntries;
        }

        private static IList<Request> BuildRequestsFromEntries(IEnumerable<(string Name, int TargetIndex, int OriginalOrder)> entries)
        {
            var requests = new List<Request>();

            foreach (var entry in entries)
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
