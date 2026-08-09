using Google.Apis.Sheets.v4.Data;
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

    /// <summary>
    /// The canonical column's raw validation name (e.g. "RANGE_SERVICE"), if any - each domain's own
    /// <c>Validation</c> enum member name, exactly as stored on <see cref="Models.Google.SheetCellModel.Validation"/>.
    /// Resolving this into a concrete <see cref="ValidationRule"/> is domain-specific (see
    /// <see cref="Managers.SheetManagerBase{TEntity}.GetDataValidation(ColumnInsertionInfo)"/>), so it's
    /// carried here only as the raw input to that resolution (GitHub issue #103).
    /// </summary>
    public string? Validation { get; set; }

    /// <summary>
    /// The resolved data validation rule for this column, if any. Left null when the column is
    /// detected - like <see cref="SheetId"/>, it's filled in afterward by
    /// <see cref="Managers.SheetManagerBase{TEntity}"/> via its <c>GetDataValidation</c> hook, since
    /// resolving <see cref="Validation"/> requires domain-specific knowledge Core doesn't have.
    /// </summary>
    public DataValidationRule? ValidationRule { get; set; }
}
