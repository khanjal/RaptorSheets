using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Enums;
using RaptorSheets.Gig.Constants;
using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Gig.Entities;

[ExcludeFromCodeCoverage]
public class NameEntity : SheetRowEntityBase
{
    [Column(SheetsConfig.HeaderNames.Name, FieldTypeEnum.String)]
    public string Name { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Trips, FieldTypeEnum.Integer)]
    public int Trips { get; set; }

    [Column(SheetsConfig.HeaderNames.Pay, FieldTypeEnum.Accounting)]
    public decimal? Pay { get; set; }

    [Column(SheetsConfig.HeaderNames.Tips, FieldTypeEnum.Accounting)]
    public decimal? Tip { get; set; }

    [Column(SheetsConfig.HeaderNames.Bonus, FieldTypeEnum.Accounting)]
    public decimal? Bonus { get; set; }

    [Column(SheetsConfig.HeaderNames.Total, FieldTypeEnum.Accounting)]
    public decimal? Total { get; set; }

    [Column(SheetsConfig.HeaderNames.Cash, FieldTypeEnum.Accounting)]
    public decimal? Cash { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerTrip, FieldTypeEnum.Accounting)]
    public decimal AmountPerTrip { get; set; }

    [Column(SheetsConfig.HeaderNames.Distance, FieldTypeEnum.Number, jsonPropertyName: "distance")]
    public decimal Distance { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerDistance, FieldTypeEnum.Accounting)]
    public decimal AmountPerDistance { get; set; }

    [Column(SheetsConfig.HeaderNames.VisitFirst, FieldTypeEnum.String)]
    public string FirstTrip { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.VisitLast, FieldTypeEnum.String)]
    public string LastTrip { get; set; } = "";
}