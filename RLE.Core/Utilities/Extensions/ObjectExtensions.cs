using RLE.Core.Enums;
using RLE.Core.Models.Google;

namespace RLE.Core.Utilities.Extensions;

public static class ObjectExtensions
{
    public static string GetColumn(this SheetModel sheet, HeaderEnum header)
    {
        return $"{sheet?.Headers.FirstOrDefault(x => x.Name == header.DisplayName())?.Column}";
    }

    public static string GetIndex(this SheetModel sheet, HeaderEnum header)
    {
        return $"{sheet?.Headers.FirstOrDefault(x => x.Name == header.DisplayName())?.Index}";
    }

    public static string GetRange(this SheetModel sheet, HeaderEnum header, int row = 1)
    {
        var column = sheet.GetColumn(header);
        return $"{sheet.Name}!{column}{row}:{column}";
    }

    public static string GetLocalRange(this SheetModel sheet, HeaderEnum header, int row = 1)
    {
        var column = sheet.GetColumn(header);
        return $"{column}{row}:{column}";
    }
}