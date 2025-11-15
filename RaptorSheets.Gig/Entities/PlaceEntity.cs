using RaptorSheets.Core.Attributes;
using RaptorSheets.Gig.Constants;
using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Gig.Entities;
[ExcludeFromCodeCoverage]
public class PlaceEntity : SheetRowEntityBase
{
    [Column(SheetsConfig.HeaderNames.Place, jsonPropertyName: "place")]
    public string Place { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Trips, jsonPropertyName: "trips")]
    public int Trips { get; set; }

    [Column(SheetsConfig.HeaderNames.Pay, jsonPropertyName: "pay")]
    public decimal? Pay { get; set; }

    [Column(SheetsConfig.HeaderNames.Tips, jsonPropertyName: "tip")]
    public decimal? Tip { get; set; }

    [Column(SheetsConfig.HeaderNames.Bonus, jsonPropertyName: "bonus")]
    public decimal? Bonus { get; set; }

    [Column(SheetsConfig.HeaderNames.Total, jsonPropertyName: "total")]
    public decimal? Total { get; set; }

    [Column(SheetsConfig.HeaderNames.Cash, jsonPropertyName: "cash")]
    public decimal? Cash { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerTrip)]
    public decimal AmountPerTrip { get; set; }

    [Column(SheetsConfig.HeaderNames.Distance, jsonPropertyName: "distance")]
    public decimal Distance { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerDistance)]
    public decimal AmountPerDistance { get; set; }

    [Column(SheetsConfig.HeaderNames.VisitFirst, jsonPropertyName: "firstTrip")]
    public string FirstTrip { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.VisitLast, jsonPropertyName: "lastTrip")]
    public string LastTrip { get; set; } = "";
}
