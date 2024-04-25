using GigRaptorLib.Models;

namespace GigRaptorLib.Utilities.Extensions;

public static class ListExtensions
{
    public static void AddColumn(this List<SheetCellModel> headers, SheetCellModel header)
    {
        var column = SheetHelper.GetColumnName(headers.Count);
        header.Column = column;
        header.Index = headers.Count;
        headers.Add(header);
    }

    public static void AddItems<T>(this List<T> list, int number)
    {
        for (int i = 0; i < number; i++)
        {
            list.Add(default);
        }
    }
}