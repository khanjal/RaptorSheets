using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Enums;
using RaptorSheets.Gig.Constants;
using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Gig.Entities;

[ExcludeFromCodeCoverage]
public class WeekdayEntity : SheetRowEntityBase
{
    [Column(SheetsConfig.HeaderNames.Day, jsonPropertyName: "day")]
    public int Day { get; set; }

    [Column(SheetsConfig.HeaderNames.Weekday, jsonPropertyName: "weekday")]
    public string Weekday { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Trips, jsonPropertyName: "trips")]
    public int Trips { get; set; }

    [Column(SheetsConfig.HeaderNames.Days, jsonPropertyName: "days")]
    public int Days { get; set; }

    // Financial properties
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

    [Column(SheetsConfig.HeaderNames.TimeTotal, FormatEnum.DURATION, jsonPropertyName: "time")]
    public string Time { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.AmountPerTime)]
    public decimal AmountPerTime { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerDay)]
    public decimal AmountPerDay { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountCurrent, jsonPropertyName: "dailyAverage")]
    public decimal DailyAverage { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPrevious, jsonPropertyName: "dailyPrevAverage")]
    public decimal PreviousDailyAverage { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerPreviousDay, jsonPropertyName: "currentAmount")]
    public decimal CurrentAmount { get; set; }

    public decimal PreviousAmount { get; set; }
}
