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
    public FormatEnum? Format { get; set; }
    public bool Protect { get; set; } = false;
    public string Validation { get; set; } = "";
    public string Note { get; set; } = "";
}