using RaptorSheets.Core.Attributes;
using RaptorSheets.Gig.Constants;
using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Gig.Entities;

[ExcludeFromCodeCoverage]
public class RegionEntity : SheetRowEntityBase
{
    // Entity-specific property first
    [Column(SheetsConfig.HeaderNames.Region, jsonPropertyName: "region")]
    public string Region { get; set; } = "";

    // CommonTripSheetHeaders pattern
    [Column(SheetsConfig.HeaderNames.Trips, jsonPropertyName: "trips")]
    public int Trips { get; set; }

    // CommonIncomeHeaders
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

    // CommonTravelHeaders
    [Column(SheetsConfig.HeaderNames.AmountPerTrip)]
    public decimal AmountPerTrip { get; set; }

    [Column(SheetsConfig.HeaderNames.Distance, jsonPropertyName: "distance")]
    public decimal Distance { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerDistance)]
    public decimal AmountPerDistance { get; set; }

    // Visit properties
    [Column(SheetsConfig.HeaderNames.VisitFirst, jsonPropertyName: "firstTrip")]
    public string FirstTrip { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.VisitLast, jsonPropertyName: "lastTrip")]
    public string LastTrip { get; set; } = "";
}
