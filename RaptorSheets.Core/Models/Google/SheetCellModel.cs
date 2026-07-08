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
    
    public FormatEnum? Format { get; set; }
    public string? FormatPattern { get; set; }
    public bool Protect { get; set; } = false;
    public string Validation { get; set; } = "";
    public string Note { get; set; } = "";
}