using System.Text.Json.Serialization;
using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Enums;
using RaptorSheets.Gig.Constants;

namespace RaptorSheets.Gig.Entities;

public class NameEntity
{
    [JsonPropertyName("rowId")]
    public int RowId { get; set; }

    // Entity-specific property first
    [Column(SheetsConfig.HeaderNames.Name, FieldTypeEnum.String)]
    public string Name { get; set; } = "";

    // CommonTripSheetHeaders pattern: Trips, CommonIncomeHeaders, CommonTravelHeaders, VisitFirst, VisitLast
    [Column(SheetsConfig.HeaderNames.Trips, FieldTypeEnum.Integer)]
    public int Trips { get; set; }

    // CommonIncomeHeaders
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

    // CommonTravelHeaders
    [Column(SheetsConfig.HeaderNames.AmountPerTrip, FieldTypeEnum.Currency)]
    public decimal AmountPerTrip { get; set; }

    [Column(SheetsConfig.HeaderNames.Distance, FieldTypeEnum.Number, jsonPropertyName: "distance")]
    public decimal Distance { get; set; }  // Fixed: changed from int to decimal

    [Column(SheetsConfig.HeaderNames.AmountPerDistance, FieldTypeEnum.Currency)]
    public decimal AmountPerDistance { get; set; }

    // Visit properties
    [Column(SheetsConfig.HeaderNames.VisitFirst, FieldTypeEnum.String)]
    public string FirstTrip { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.VisitLast, FieldTypeEnum.String)]
    public string LastTrip { get; set; } = "";

    [JsonPropertyName("saved")]
    public bool Saved { get; set; }
}