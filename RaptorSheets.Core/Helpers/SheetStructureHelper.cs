using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Models.Google;

namespace RaptorSheets.Core.Helpers;

/// <summary>
/// Reads a sheet's structure back from a live grid-data <see cref="Spreadsheet"/> response - the
/// mirror image of how a <see cref="SheetModel"/> is built from <c>[Column]</c> config for sheet
/// generation (see <see cref="Mappers.GenericSheetMapper{T}.GetSheet"/>). Populates the same
/// <see cref="SheetModel"/>/<see cref="SheetCellModel"/> shape used by
/// <c>ISheetManager&lt;TEntity&gt;.GetSheetLayout(s)</c>, except from what's actually on the sheet
/// rather than from configured expectations.
///
/// Some fields carry different semantics on a live read than on a configured layout - most notably
/// <see cref="SheetCellModel.Validation"/>, which on a configured layout holds a domain-specific enum
/// token (e.g. Gig's <c>Validation.RANGE_SERVICE</c>) but here can only be the raw condition read off
/// the sheet (e.g. "ONE_OF_RANGE:=Names!A2:A") - Core has no way to reverse a domain's own dispatch
/// table. <see cref="SheetCellModel.Format"/> is a best-effort match against this library's own
/// write-side patterns and can be null for a pattern it doesn't recognize (a custom
/// <c>NumberFormatPattern</c>, for instance) - <see cref="SheetCellModel.FormatPattern"/> and
/// <see cref="SheetCellModel.RawFormatType"/> are always exact when a format exists.
/// <see cref="SheetModel.CellColor"/> is never populated - it's only ever used at write time for
/// alternating-row banding (<see cref="GoogleRequestHelpers.GenerateBandingRequest"/>), not stored as
/// a per-cell or per-sheet scalar that could be read back.
///
/// Format/validation are read from row *index 1* (the first data row, A1 row "2"), not the header
/// row - confirmed against <see cref="GoogleRequestHelpers.GenerateRepeatCellRequest"/>, whose
/// <c>RepeatCellRequest</c> always sets <c>GridRange.StartRowIndex = 1</c> before writing
/// format/validation, so the header cell itself never carries a <c>NumberFormat</c>/
/// <c>DataValidation</c> - only its plain text/formula and <see cref="SheetCellModel.Note"/> do (see
/// <c>SheetHelpers.BuildHeaderCell</c>). A caller must fetch at least 2 rows for format/validation to
/// be visible at all.
/// </summary>
public static class SheetStructureHelper
{
    /// <summary>
    /// Parses every sheet in <paramref name="spreadsheet"/> whose title matches one of
    /// <paramref name="sheetNames"/> (case-insensitive). Sheets not present in the spreadsheet, or
    /// present but not requested, are silently skipped - mirroring
    /// <see cref="Registries.SheetRegistry{TEntity}.GetSheetLayouts"/>'s skip-unknown behavior.
    /// </summary>
    public static Dictionary<string, SheetModel> ParseSheetStructures(Spreadsheet spreadsheet, IEnumerable<string> sheetNames)
    {
        ArgumentNullException.ThrowIfNull(spreadsheet);
        ArgumentNullException.ThrowIfNull(sheetNames);

        var result = new Dictionary<string, SheetModel>(StringComparer.OrdinalIgnoreCase);

        if (spreadsheet.Sheets == null)
        {
            return result;
        }

        var requested = new HashSet<string>(sheetNames, StringComparer.OrdinalIgnoreCase);

        foreach (var sheet in spreadsheet.Sheets)
        {
            var title = sheet.Properties?.Title;

            if (title == null || !requested.Contains(title))
            {
                continue;
            }

            result[title] = ParseSheetStructure(sheet);
        }

        return result;
    }

    /// <summary>
    /// Parses one grid-data <see cref="Sheet"/> into a <see cref="SheetModel"/>. Requires
    /// <c>IncludeGridData = true</c> on the request that produced it (see
    /// <see cref="IGoogleSheetService.GetSheetInfo(List{string}?, System.Threading.CancellationToken)"/>).
    /// Name/note/formula/protection come from row 0 (the header row); format/validation come from row
    /// 1 (the first data row) - see the type-level doc comment for why. A fetch that only covers row 0
    /// will report every column's format/validation as absent, not because the sheet has none, but
    /// because that row was never in range.
    /// </summary>
    public static SheetModel ParseSheetStructure(Sheet sheet)
    {
        ArgumentNullException.ThrowIfNull(sheet);

        var properties = sheet.Properties ?? new SheetProperties();

        var model = new SheetModel
        {
            Id = properties.SheetId ?? 0,
            Name = properties.Title ?? "",
            FreezeRowCount = properties.GridProperties?.FrozenRowCount ?? 0,
            FreezeColumnCount = properties.GridProperties?.FrozenColumnCount ?? 0,
            ProtectSheet = HasWholeSheetProtection(sheet.ProtectedRanges)
        };

        if (SheetHelpers.TryGetSheetColor(properties.TabColor, out var tabColor))
        {
            model.TabColor = tabColor;
        }

        var rowData = sheet.Data?.ElementAtOrDefault(0)?.RowData;
        var headerCells = rowData?.ElementAtOrDefault(0)?.Values;

        if (headerCells == null)
        {
            return model;
        }

        // Every header cell is written with the same TextFormat.ForegroundColor (see
        // SheetHelpers.BuildHeaderCell) - the sheet-level FontColor only physically exists on each
        // cell, so the first one that has it stands in for the whole sheet.
        var foreground = headerCells
            .Select(c => c.UserEnteredFormat?.TextFormat?.ForegroundColor)
            .FirstOrDefault(c => c != null);

        if (foreground != null && SheetHelpers.TryGetSheetColor(foreground, out var fontColor))
        {
            model.FontColor = fontColor;
        }

        var formatCells = rowData?.ElementAtOrDefault(1)?.Values;

        for (var index = 0; index < headerCells.Count; index++)
        {
            var cell = headerCells[index];

            if (cell.FormattedValue == null)
            {
                continue;
            }

            var formatCell = formatCells?.ElementAtOrDefault(index);
            model.Headers.Add(ParseHeaderCell(cell, formatCell, index, sheet.ProtectedRanges));
        }

        return model;
    }

    private static SheetCellModel ParseHeaderCell(CellData cell, CellData? formatCell, int index, IList<ProtectedRange>? protectedRanges)
    {
        var header = new SheetCellModel
        {
            Name = cell.FormattedValue ?? "",
            Index = index,
            Column = SheetHelpers.GetColumnName(index),
            Note = cell.Note ?? "",
            Formula = cell.UserEnteredValue?.FormulaValue ?? "",
            Validation = BuildValidationDescription(formatCell?.DataValidation),
            Protect = CellIsProtected(index, protectedRanges)
        };

        var numberFormat = formatCell?.UserEnteredFormat?.NumberFormat;

        if (numberFormat != null)
        {
            header.RawFormatType = numberFormat.Type;
            header.FormatPattern = numberFormat.Pattern;
            header.Format = ResolveFormat(numberFormat.Type, numberFormat.Pattern);
        }

        return header;
    }

    private static string BuildValidationDescription(DataValidationRule? rule)
    {
        var type = rule?.Condition?.Type;

        if (string.IsNullOrEmpty(type))
        {
            return "";
        }

        var values = rule!.Condition.Values;

        return values == null || values.Count == 0
            ? type
            : $"{type}:{string.Join(",", values.Select(v => v.UserEnteredValue))}";
    }

    private static bool CellIsProtected(int columnIndex, IList<ProtectedRange>? protectedRanges)
    {
        return protectedRanges != null && protectedRanges.Any(pr => pr.Range != null && RangeCoversCell(pr.Range, 0, columnIndex));
    }

    private static bool HasWholeSheetProtection(IList<ProtectedRange>? protectedRanges)
    {
        return protectedRanges != null && protectedRanges.Any(pr => pr.Range != null && IsWholeSheetRange(pr.Range));
    }

    // Per the Sheets API, a ProtectedRange whose Range has no start/end bounds at all protects the
    // entire sheet rather than a cell range.
    private static bool IsWholeSheetRange(GridRange range)
    {
        return range.StartRowIndex == null && range.EndRowIndex == null
            && range.StartColumnIndex == null && range.EndColumnIndex == null;
    }

    private static bool RangeCoversCell(GridRange range, int row, int column)
    {
        var rowStart = range.StartRowIndex ?? 0;
        var rowEnd = range.EndRowIndex ?? int.MaxValue;
        var colStart = range.StartColumnIndex ?? 0;
        var colEnd = range.EndColumnIndex ?? int.MaxValue;

        return row >= rowStart && row < rowEnd && column >= colStart && column < colEnd;
    }

    // Reverse of SheetHelpers.GetCellFormat(Format): Type alone is ambiguous (e.g. DATE, WEEKDAY,
    // DURATION and TIME all use Type=DATE), so only an exact (Type, Pattern) pair disambiguates. A
    // custom pattern (ColumnAttribute.NumberFormatPattern) won't be in this table and resolves to null.
    private static readonly Dictionary<(string Type, string? Pattern), Format> PatternToFormat = new()
    {
        [(CellFormatPatterns.CellFormatNumber, CellFormatPatterns.Accounting)] = Format.ACCOUNTING,
        [(CellFormatPatterns.CellFormatNumber, CellFormatPatterns.Currency)] = Format.CURRENCY,
        [(CellFormatPatterns.CellFormatDate, CellFormatPatterns.Date)] = Format.DATE,
        [(CellFormatPatterns.CellFormatNumber, CellFormatPatterns.Distance)] = Format.DISTANCE,
        [(CellFormatPatterns.CellFormatDate, CellFormatPatterns.Duration)] = Format.DURATION,
        [(CellFormatPatterns.CellFormatNumber, CellFormatPatterns.Number)] = Format.NUMBER,
        [(CellFormatPatterns.CellFormatText, null)] = Format.TEXT,
        [(CellFormatPatterns.CellFormatDate, CellFormatPatterns.Time)] = Format.TIME,
        [(CellFormatPatterns.CellFormatDate, CellFormatPatterns.Weekday)] = Format.WEEKDAY,
    };

    private static Format? ResolveFormat(string? type, string? pattern)
    {
        return type != null && PatternToFormat.TryGetValue((type, pattern), out var format) ? format : null;
    }
}
