using RLE.Core.Constants;
using RLE.Core.Models.Google;

namespace RLE.Core.Extensions;

public static class ListExtensions
{
    public static void AddColumn(this List<SheetCellModel> headers, SheetCellModel header)
    {
        var letters = GoogleConfig.ColumnLetters;
        var value = string.Empty;

        if (headers.Count >= letters.Length)
            value += letters[headers.Count / letters.Length - 1];

        value += letters[headers.Count % letters.Length];

        // var column = SheetHelper.GetColumnName(headers.Count); // TODO: Split sheet helper for core and gig
        header.Column = value;
        header.Index = headers.Count;
        headers.Add(header);
    }

    public static void UpdateColumns(this List<SheetCellModel> headers)
    {
        var sheetHeaders = headers.ToList();
        var letters = GoogleConfig.ColumnLetters;
        var value = string.Empty;

        headers.Clear();
        foreach (var header in sheetHeaders)
        {
            value = "";

            if (headers.Count >= letters.Length)
                value += letters[headers.Count / letters.Length - 1];

            value += letters[headers.Count % letters.Length];

            header.Column = value;
            header.Index = headers.Count;
            headers.Add(header);
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