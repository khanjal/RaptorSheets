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
}
