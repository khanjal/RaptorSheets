using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Home.Constants;
using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Home.Entities;

[ExcludeFromCodeCoverage]
public class PowerEntity : SheetRowEntityBase
{
    [Column(SheetsConfig.HeaderNames.Location, isInput: true, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangeRoom)]
    public string Location { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Type, isInput: true)]
    public string Type { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Position, isInput: true)]
    public string Position { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Amps, isInput: true, formatType: FormatEnum.NUMBER)]
    public int? Amps { get; set; }

    [Column(SheetsConfig.HeaderNames.Grounded, isInput: true, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.Boolean)]
    public bool Grounded { get; set; }

    [Column(SheetsConfig.HeaderNames.GFI, isInput: true, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.Boolean)]
    public bool GFI { get; set; }
}
