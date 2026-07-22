using RaptorSheets.Core.Models.Google;

namespace RaptorSheets.Core.Extensions;

public static class ObjectExtensions
{
    public static string GetColumn(this SheetModel sheet, string header)
    {
        ArgumentNullException.ThrowIfNull(sheet);

        var match = sheet.Headers.FirstOrDefault(x => x.Name == header);
        return $"{match?.Column}";
    }

    public static string GetIndex(this SheetModel sheet, string header)
    {
        ArgumentNullException.ThrowIfNull(sheet);

        var match = sheet.Headers.FirstOrDefault(x => x.Name == header);
        return $"{match?.Index}";
    }

    /// <summary>
    /// Cross-sheet range, e.g. <c>'Tickers'!B1:B</c>. The sheet name is always single-quoted, even
    /// for single-word names that don't strictly require it in A1 notation - Sheets accepts the
    /// quoted form unconditionally, and a single canonical shape (rather than "quoted only when the
    /// name has a space") is both a correctness safety net for future multi-word sheet names and
    /// what <see cref="Registries.SheetRegistry{TEntity}.GetDependents"/> pattern-matches against to
    /// detect cross-sheet formula dependencies automatically.
    /// </summary>
    public static string GetRange(this SheetModel sheet, string header, int row = 1)
    {
        ArgumentNullException.ThrowIfNull(sheet);

        var column = sheet.GetColumn(header);
        return string.IsNullOrEmpty(column) ? $"'{sheet.Name}'!" : $"'{sheet.Name}'!{column}{row}:{column}";
    }

    public static string GetLocalRange(this SheetModel sheet, string header, int row = 1)
    {
        ArgumentNullException.ThrowIfNull(sheet);

        var column = sheet.GetColumn(header);
        return string.IsNullOrEmpty(column) ? "" : $"{column}{row}:{column}";
    }

    /// <summary>
    /// See <see cref="GetRange"/> for why the sheet name is always single-quoted.
    /// </summary>
    public static string GetRangeBetweenColumns(this SheetModel sheet, string startHeader, string endHeader)
    {
        ArgumentNullException.ThrowIfNull(sheet);

        var startCol = sheet.GetColumn(startHeader);
        var endCol = sheet.GetColumn(endHeader);
        return $"'{sheet.Name}'!{startCol}:{endCol}";
    }
}