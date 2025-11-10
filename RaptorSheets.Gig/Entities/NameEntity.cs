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

    [Column(SheetsConfig.HeaderNames.Pay, FieldTypeEnum.Currency)]
    public decimal? Pay { get; set; }

    [Column(SheetsConfig.HeaderNames.Tips, FieldTypeEnum.Currency)]
    public decimal? Tip { get; set; }

    [Column(SheetsConfig.HeaderNames.Bonus, FieldTypeEnum.Currency)]
    public decimal? Bonus { get; set; }

    [Column(SheetsConfig.HeaderNames.Total, FieldTypeEnum.Currency)]
    public decimal? Total { get; set; }

    [Column(SheetsConfig.HeaderNames.Cash, FieldTypeEnum.Currency)]
    public decimal? Cash { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerTrip, FieldTypeEnum.Currency)]
    public decimal AmountPerTrip { get; set; }

    [Column(SheetsConfig.HeaderNames.Distance, FieldTypeEnum.Number, jsonPropertyName: "distance")]
    public decimal Distance { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerDistance, FieldTypeEnum.Currency)]
    public decimal AmountPerDistance { get; set; }

    [Column(SheetsConfig.HeaderNames.VisitFirst, FieldTypeEnum.String)]
    public string FirstTrip { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.VisitLast, FieldTypeEnum.String)]
    public string LastTrip { get; set; } = "";
}