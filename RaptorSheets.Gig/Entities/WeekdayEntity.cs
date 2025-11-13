using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Enums;
using RaptorSheets.Gig.Constants;
using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Gig.Entities;

[ExcludeFromCodeCoverage]
public class WeekdayEntity : SheetRowEntityBase
{
    [Column(SheetsConfig.HeaderNames.Day, FieldTypeEnum.Integer, jsonPropertyName: "day")]
    public int Day { get; set; }

    [Column(SheetsConfig.HeaderNames.Weekday, FieldTypeEnum.String, jsonPropertyName: "weekday")]
    public string Weekday { get; set; } = "";

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

    [Column(SheetsConfig.HeaderNames.AmountPerTrip, FieldTypeEnum.Currency)]
    public decimal AmountPerTrip { get; set; }

    [Column(SheetsConfig.HeaderNames.Distance, FieldTypeEnum.Number, jsonPropertyName: "distance")]
    public decimal Distance { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerDistance, FieldTypeEnum.Currency)]
    public decimal AmountPerDistance { get; set; }

    [Column(SheetsConfig.HeaderNames.TimeTotal, FieldTypeEnum.Duration, jsonPropertyName: "time")]
    public string Time { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.AmountPerTime, FieldTypeEnum.Currency)]
    public decimal AmountPerTime { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerDay, FieldTypeEnum.Currency)]
    public decimal AmountPerDay { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountCurrent, FieldTypeEnum.Currency, jsonPropertyName: "dailyAverage")]
    public decimal DailyAverage { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPrevious, FieldTypeEnum.Currency, jsonPropertyName: "dailyPrevAverage")]
    public decimal PreviousDailyAverage { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerPreviousDay, FieldTypeEnum.Currency, jsonPropertyName: "currentAmount")]
    public decimal CurrentAmount { get; set; }

    public decimal PreviousAmount { get; set; }
}
