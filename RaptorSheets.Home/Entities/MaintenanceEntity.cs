using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Home.Constants;
using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Home.Entities;

[ExcludeFromCodeCoverage]
public class MaintenanceEntity : SheetRowEntityBase
{
    [Column(SheetsConfig.HeaderNames.Date, isInput: true, note: ColumnNotes.DateFormat, formatType: FormatEnum.DATE)]
    public string Date { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Problem, isInput: true)]
    public string Problem { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.CompanyPerson, isInput: true, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangeContact)]
    public string CompanyPerson { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Number, isInput: true)]
    public string Number { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Solution, isInput: true)]
    public string Solution { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Amount, isInput: true, formatType: FormatEnum.ACCOUNTING)]
    public decimal? Amount { get; set; }
}
