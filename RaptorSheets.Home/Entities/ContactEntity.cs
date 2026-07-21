using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Entities;
using RaptorSheets.Home.Constants;
using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Home.Entities;

[ExcludeFromCodeCoverage]
public class ContactEntity : SheetRowEntityBase
{
    [Column(SheetsConfig.HeaderNames.Name, isInput: true)]
    public string Name { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Number, isInput: true)]
    public string Number { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.AltNumber, isInput: true)]
    public string AltNumber { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Address, isInput: true)]
    public string Address { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Description, isInput: true)]
    public string Description { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Retired, isInput: true, note: ColumnNotes.Retired, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.Boolean)]
    public bool Retired { get; set; }
}
