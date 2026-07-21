using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Entities;
using RaptorSheets.Home.Constants;
using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Home.Entities;

[ExcludeFromCodeCoverage]
public class PaintEntity : SheetRowEntityBase
{
    [Column(SheetsConfig.HeaderNames.Brand, isInput: true)]
    public string Brand { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Type, isInput: true)]
    public string Type { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Color, isInput: true)]
    public string Color { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Location, isInput: true, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangeRoom)]
    public string Location { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Remaining, isInput: true)]
    public string Remaining { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Size, isInput: true)]
    public string Size { get; set; } = "";
}
