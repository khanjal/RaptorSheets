using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;

namespace RaptorSheets.Core.Extensions;

public static class ListExtensions
{
    public static void AddColumn(this List<SheetCellModel> headers, SheetCellModel? header)
    {
        if (header != null)
        {
            header.Column = SheetHelpers.GetColumnName(headers.Count);
            header.Index = headers.Count;
        }
        headers.Add(header!);
    }

    public static void UpdateColumns(this List<SheetCellModel> headers)
    {
        var sheetHeaders = headers.ToList();

        headers.Clear();
        foreach (var header in sheetHeaders)
        {
            headers.AddColumn(header);
        }
    }

    public static void AddItems<T>(this List<T> list, int number)
    {
        for (int i = 0; i < number; i++)
        {
            list.Add(default!);
        }
    }

    public static T GetRandomItem<T>(this List<T> list)
    {
        if (list == null || list.Count == 0)
        {
            throw new ArgumentException("The list cannot be null or empty.", nameof(list));
        }

        var rng = new Random();
        int index = rng.Next(list.Count);
        return list[index];
    }

    /// <summary>
    /// <summary>
    /// Keep the first <paramref name="leadingCount"/> existing headers and pad with
    /// empty placeholders so the final header count equals the original count
    /// observed before modification. Useful when you want to preserve the
    /// total header width but only keep a small number of actual header cells.
    /// </summary>
    /// <param name="leadingCount">Number of leading headers to keep (e.g. 1)</param>
    public static void EnsureHeaderPlaceholders(this List<SheetCellModel> headers, int leadingCount)
    {
        if (headers == null) return;

        // Ensure indexes are assigned and capture the original total
        headers.UpdateColumns();
        int originalTotal = headers.Count;

        var kept = new List<SheetCellModel>();
        for (int i = 0; i < Math.Min(Math.Max(0, leadingCount), headers.Count); i++)
        {
            kept.Add(headers[i]);
        }

        headers.Clear();
        headers.AddRange(kept);

        // Pad to the original total with empty placeholders
        while (headers.Count < originalTotal)
        {
            headers.Add(new SheetCellModel { Name = "" });
        }

        headers.UpdateColumns();
    }
}