using RaptorSheets.Core.Enums;

namespace RaptorSheets.Core.Models.Google;

public class SheetCellModel
{
    public string Name { get; set; } = "";
    public int Index { get; set; }
    public string Column { get; set; } = "";
    public string Range { get; set; } = "";
    public string HeaderlessRange { get; set; } = "";
    public string Formula { get; set; } = "";

    // When true, the header's name will not be written to the sheet (useful when
    // a QUERY formula will populate the header text but formatting still needs to apply).
    public bool HideHeaderName { get; set; } = false;
    
    public Format? Format { get; set; }
    public string? FormatPattern { get; set; }

    // Google's own coarse NumberFormat.Type (e.g. "DATE", "NUMBER", "CURRENCY", "TEXT") as read back
    // from a live sheet. Unlike Format above - which is this library's own, more specific enum and
    // can be ambiguous or null for a pattern it doesn't recognize (e.g. a custom NumberFormatPattern) -
    // this is always exact whenever a live cell has a format, at the cost of being coarser.
    public string? RawFormatType { get; set; }
    public bool Protect { get; set; } = false;
    public string Validation { get; set; } = "";
    public string Note { get; set; } = "";

    // Opt-in: gives this column's data (excluding the header row) a Google Sheets named range,
    // named "{SheetName}_{HeaderName}". See ColumnAttribute.NamedRange.
    public bool NamedRange { get; set; } = false;
}