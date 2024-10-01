using RLE.Core.Models.Google;

namespace RLE.Core.Extensions;

public static class ObjectExtensions
{
    public static string GetColumn(this SheetModel sheet, string header)
    {
        return $"{sheet?.Headers.FirstOrDefault(x => x.Name == header)?.Column}";
    }

    public static string GetIndex(this SheetModel sheet, string header)
    {
        return $"{sheet?.Headers.FirstOrDefault(x => x.Name == header)?.Index}";
    }

    public static string GetRange(this SheetModel sheet, string header, int row = 1)
    {
        var column = sheet.GetColumn(header);
        return $"{sheet.Name}!{column}{row}:{column}";
    }

    public static string GetLocalRange(this SheetModel sheet, string header, int row = 1)
    {
        var column = sheet.GetColumn(header);
        return $"{column}{row}:{column}";
    }
}