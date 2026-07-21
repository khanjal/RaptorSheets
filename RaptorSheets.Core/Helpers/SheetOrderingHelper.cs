using Google.Apis.Sheets.v4.Data;

namespace RaptorSheets.Core.Helpers;

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

        // Input validation: ensure no null/whitespace names and no duplicates (case-insensitive)
        if (requestedSheets.Any(s => string.IsNullOrWhiteSpace(s)))
        {
            throw new ArgumentException("requestedSheets contains null or empty sheet names", nameof(requestedSheets));
        }

        var normalizedRequested = requestedSheets.Select(s => s.Trim()).ToList();
        var duplicates = normalizedRequested
            .GroupBy(s => s, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicates.Count > 0)
        {
            throw new ArgumentException($"Duplicate sheet names found: {string.Join(", ", duplicates)}", nameof(requestedSheets));
        }

        var existingList = GetExistingList(spreadsheetInfo);
        var existingIndexMap = existingList
            .ToDictionary(e => e.Title, e => e.Index, StringComparer.OrdinalIgnoreCase);

        // Use the raw sheet collection count (including any filtered entries) when computing append offsets
        var existingRawCount = spreadsheetInfo?.Sheets?.Count ?? existingList.Count;

        var insertionEntries = ComputeInsertionEntries(normalizedRequested, existingIndexMap, existingRawCount);

        var orderedInsertions = insertionEntries
            .OrderByDescending(x => x.TargetIndex)
            .ThenByDescending(x => x.OriginalOrder)
            .ToList();

        return BuildRequestsFromEntries(orderedInsertions);
    }

    /// <summary>
    /// Builds AddSheet requests given an existing title->index map and the raw existing sheet count.
    /// This avoids requiring a full <see cref="Spreadsheet"/> when the caller already has an index map.
    /// </summary>
    public static IList<Request> BuildAddSheetRequests(Dictionary<string, int> existingIndexMap, int existingRawCount, List<string> requestedSheets)
    {
        var requests = new List<Request>();

        if (requestedSheets == null || requestedSheets.Count == 0)
        {
            return requests;
        }

        // Input validation: ensure no null/whitespace names and no duplicates (case-insensitive)
        if (requestedSheets.Any(s => string.IsNullOrWhiteSpace(s)))
        {
            throw new ArgumentException("requestedSheets contains null or empty sheet names", nameof(requestedSheets));
        }

        var normalizedRequested = requestedSheets.Select(s => s.Trim()).ToList();
        var duplicates = normalizedRequested
            .GroupBy(s => s, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicates.Count > 0)
        {
            throw new ArgumentException($"Duplicate sheet names found: {string.Join(", ", duplicates)}", nameof(requestedSheets));
        }

        var insertionEntries = ComputeInsertionEntries(normalizedRequested, existingIndexMap ?? new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase), existingRawCount);

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

    /// <summary>
    /// Orders sheet titles by their desired index (negative/missing indices - no preference -
    /// sort last), with a stable alphabetical tiebreak. Used when creating sheets from a
    /// title-&gt;desiredIndex map to produce a deterministic creation order.
    /// </summary>
    public static List<string> OrderSheetTitlesByIndex(Dictionary<string, int>? sheetsWithIndices)
    {
        if (sheetsWithIndices == null || sheetsWithIndices.Count == 0)
        {
            return [];
        }

        return sheetsWithIndices
            .OrderBy(kv => kv.Value < 0 ? int.MaxValue : kv.Value)
            .ThenBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
            .Select(kv => kv.Key)
            .ToList();
    }
}
