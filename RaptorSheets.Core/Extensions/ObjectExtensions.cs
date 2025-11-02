using RaptorSheets.Core.Models.Google;

namespace RaptorSheets.Core.Extensions;

public static class ObjectExtensions
{
    public static string GetColumn(this SheetModel sheet, string header)
    {
        ArgumentNullException.ThrowIfNull(sheet);

        return $"{sheet.Headers.FirstOrDefault(x => x.Name == header)?.Column}";
    }

    public static string GetIndex(this SheetModel sheet, string header)
    {
        ArgumentNullException.ThrowIfNull(sheet);

        return $"{sheet.Headers.FirstOrDefault(x => x.Name == header)?.Index}";
    }

    public static string GetRange(this SheetModel sheet, string header, int row = 1)
    {
        ArgumentNullException.ThrowIfNull(sheet);

        var column = sheet.GetColumn(header);
        return string.IsNullOrEmpty(column) ? $"{sheet.Name}!" : $"{sheet.Name}!{column}{row}:{column}";
    }

    public static string GetLocalRange(this SheetModel sheet, string header, int row = 1)
    {
        ArgumentNullException.ThrowIfNull(sheet);

        var column = sheet.GetColumn(header);
        return string.IsNullOrEmpty(column) ? "" : $"{column}{row}:{column}";
    }

    public static string GetRangeBetweenColumns(this SheetModel sheet, string startHeader, string endHeader)
    {
        ArgumentNullException.ThrowIfNull(sheet);

        var startCol = sheet.GetColumn(startHeader);
        var endCol = sheet.GetColumn(endHeader);
        return $"{sheet.Name}!{startCol}:{endCol}";
    }
}