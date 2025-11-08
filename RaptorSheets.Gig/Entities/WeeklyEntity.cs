using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Enums;
using RaptorSheets.Gig.Constants;

namespace RaptorSheets.Gig.Entities;

public class WeeklyEntity : SheetRowEntityBase
{
    [Column(SheetsConfig.HeaderNames.Week, FieldTypeEnum.String, jsonPropertyName: "week")]
    public string Week { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Trips, FieldTypeEnum.Integer, jsonPropertyName: "trips")]
    public int Trips { get; set; }

    [Column(SheetsConfig.HeaderNames.Days, FieldTypeEnum.Integer, jsonPropertyName: "days")]
    public int Days { get; set; }

    // Financial properties
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

    [Column(SheetsConfig.HeaderNames.TimeTotal, FieldTypeEnum.Duration, jsonPropertyName: "time")]
    public string Time { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.AmountPerTime, FieldTypeEnum.Currency, jsonPropertyName: "amt/hour")]
    public decimal AmountPerTime { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerDay, FieldTypeEnum.Currency)]
    public decimal AmountPerDay { get; set; }

    [Column(SheetsConfig.HeaderNames.Average, FieldTypeEnum.Currency, jsonPropertyName: "average")]
    public decimal Average { get; set; }

    [Column(SheetsConfig.HeaderNames.Number, FieldTypeEnum.Integer, jsonPropertyName: "#")]
    public int Number { get; set; }

    [Column(SheetsConfig.HeaderNames.Year, FieldTypeEnum.Integer, jsonPropertyName: "year")]
    public int Year { get; set; }

    [Column(SheetsConfig.HeaderNames.DateBegin, FieldTypeEnum.DateTime, jsonPropertyName: "begin")]
    public string Begin { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.DateEnd, FieldTypeEnum.DateTime, jsonPropertyName: "end")]
    public string End { get; set; } = "";
}
