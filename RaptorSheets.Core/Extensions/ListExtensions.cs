using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;

namespace RaptorSheets.Core.Extensions;

public static class ListExtensions
{
    public static void AddColumn(this List<SheetCellModel> headers, SheetCellModel header)
    {
        header.Column = SheetHelpers.GetColumnName(headers.Count);
        header.Index = headers.Count;
        headers.Add(header);
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
}