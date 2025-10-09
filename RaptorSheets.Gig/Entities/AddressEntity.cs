using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Enums;
using RaptorSheets.Gig.Constants;
using System.Text.Json.Serialization;

namespace RaptorSheets.Gig.Entities;

public class AddressEntity 
{
    [JsonPropertyName("rowId")]
    public int RowId { get; set; }

    [Column(SheetsConfig.HeaderNames.Address, FieldTypeEnum.String, jsonPropertyName: "address")]
    public string Address { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Trips, FieldTypeEnum.Integer, jsonPropertyName: "trips")]
    public int Trips { get; set; }

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

    [Column(SheetsConfig.HeaderNames.AmountPerTrip, FieldTypeEnum.Currency, jsonPropertyName: "amt/trip")]
    public decimal AmountPerTrip { get; set; }

    [Column(SheetsConfig.HeaderNames.Distance, FieldTypeEnum.Number, jsonPropertyName: "distance")]
    public decimal Distance { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerDistance, FieldTypeEnum.Currency, jsonPropertyName: "amt/dist")]
    public decimal AmountPerDistance { get; set; }

    [Column(SheetsConfig.HeaderNames.VisitFirst, FieldTypeEnum.String, jsonPropertyName: "first trip")]
    public string FirstTrip { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.VisitLast, FieldTypeEnum.String, jsonPropertyName: "last trip")]
    public string LastTrip { get; set; } = "";

    [JsonPropertyName("saved")]
    public bool Saved { get; set; }
}