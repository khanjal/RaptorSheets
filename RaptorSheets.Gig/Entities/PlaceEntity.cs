using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Enums;
using RaptorSheets.Gig.Constants;

namespace RaptorSheets.Gig.Entities;

public class PlaceEntity : SheetRowEntityBase
{
    // Entity-specific property first
    [Column(SheetsConfig.HeaderNames.Place, FieldTypeEnum.String, jsonPropertyName: "place")]
    public string Place { get; set; } = "";

    // CommonTripSheetHeaders pattern
    [Column(SheetsConfig.HeaderNames.Trips, FieldTypeEnum.Integer, jsonPropertyName: "trips")]
    public int Trips { get; set; }

    // CommonIncomeHeaders
    [Column(SheetsConfig.HeaderNames.Pay, FieldTypeEnum.Currency, jsonPropertyName: "pay")]
    public decimal? Pay { get; set; }

    [Column(SheetsConfig.HeaderNames.Tips, FieldTypeEnum.Currency, jsonPropertyName: "tip")]
    public decimal? Tip { get; set; }

    [Column(SheetsConfig.HeaderNames.Bonus, FieldTypeEnum.Currency, jsonPropertyName: "bonus")]
    public decimal? Bonus { get; set; }

    [Column(SheetsConfig.HeaderNames.Total, FieldTypeEnum.Currency, jsonPropertyName: "total")]
    public decimal? Total { get; set; }

    [Column(SheetsConfig.HeaderNames.Cash, FieldTypeEnum.Currency, jsonPropertyName: "cash")]
    public decimal? Cash { get; set; }

    // CommonTravelHeaders
    [Column(SheetsConfig.HeaderNames.AmountPerTrip, FieldTypeEnum.Currency)]
    public decimal AmountPerTrip { get; set; }

    [Column(SheetsConfig.HeaderNames.Distance, FieldTypeEnum.Number, jsonPropertyName: "distance")]
    public decimal Distance { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerDistance, FieldTypeEnum.Currency)]
    public decimal AmountPerDistance { get; set; }

    // Visit properties
    [Column(SheetsConfig.HeaderNames.VisitFirst, FieldTypeEnum.String, jsonPropertyName: "firstTrip")]
    public string FirstTrip { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.VisitLast, FieldTypeEnum.String, jsonPropertyName: "lastTrip")]
    public string LastTrip { get; set; } = "";
}
