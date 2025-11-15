using RaptorSheets.Core.Attributes;
using RaptorSheets.Gig.Constants;
using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Gig.Entities;

[ExcludeFromCodeCoverage]
public class RegionEntity : SheetRowEntityBase
{
    // Entity-specific property first
    [Column(SheetsConfig.HeaderNames.Region)]
    public string Region { get; set; } = "";

    // CommonTripSheetHeaders pattern
    [Column(SheetsConfig.HeaderNames.Trips)]
    public int Trips { get; set; }

    // CommonIncomeHeaders
    [Column(SheetsConfig.HeaderNames.Pay)]
    public decimal? Pay { get; set; }

    [Column(SheetsConfig.HeaderNames.Tips)]
    public decimal? Tip { get; set; }

    [Column(SheetsConfig.HeaderNames.Bonus)]
    public decimal? Bonus { get; set; }

    [Column(SheetsConfig.HeaderNames.Total)]
    public decimal? Total { get; set; }

    [Column(SheetsConfig.HeaderNames.Cash)]
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
