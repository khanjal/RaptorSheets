using RaptorSheets.Core.Enums;

namespace RaptorSheets.Core.Models;

/// <summary>
/// Information about a column that needs to be inserted into a sheet.
/// </summary>
public class ColumnInsertionInfo
{
    /// <summary>
    /// The name of the sheet where the column should be inserted.
    /// </summary>
    public string SheetName { get; set; } = "";

    /// <summary>
    /// The sheet ID from Google Sheets.
    /// </summary>
    public int SheetId { get; set; }

    /// <summary>
    /// The column index where the insertion should occur (0-based).
    /// </summary>
    public int ColumnIndex { get; set; }

    /// <summary>
    /// The name of the column header being inserted.
    /// </summary>
    public string ColumnName { get; set; } = "";

    /// <summary>
    /// The column letter (e.g., "A", "B", "Z", "AA").
    /// </summary>
    public string ColumnLetter { get; set; } = "";

    /// <summary>
    /// The canonical column's formula, if it has one (e.g. an ARRAYFORMULA-driven header) - written
    /// into the header cell instead of plain text so a re-inserted formula column actually computes
    /// something, rather than only getting its header text back (GitHub issue #53).
    /// </summary>
    public string? Formula { get; set; }

    /// <summary>The canonical column's number format, if any (see <see cref="Models.Google.SheetCellModel.Format"/>).</summary>
    public Format? Format { get; set; }

    /// <summary>The canonical column's custom number format pattern, if any.</summary>
    public string? FormatPattern { get; set; }

    /// <summary>The canonical column's note, if any - restored onto the re-inserted header cell.</summary>
    public string? Note { get; set; }

    /// <summary>
    /// Whether the canonical column is protected - like <see cref="Models.Google.SheetCellModel.Protect"/>,
    /// this makes an empty <see cref="Formula"/> still count as "write a formula cell" rather than
    /// plain text (see <see cref="Helpers.SheetHelpers"/>'s header-cell building for the same
    /// convention used at sheet-creation time).
    /// </summary>
    public bool Protect { get; set; }
}
