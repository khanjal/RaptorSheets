using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Gig.Constants;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace RaptorSheets.Gig.Entities;

[ExcludeFromCodeCoverage]
public class WeekdayEntity : SheetRowEntityBase
{
    [Column(SheetsConfig.HeaderNames.Day)]
    public int Day { get; set; }

    [Column(SheetsConfig.HeaderNames.Weekday)]
    public string Weekday { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Trips)]
    public int Trips { get; set; }

    [Column(SheetsConfig.HeaderNames.Days)]
    public int Days { get; set; }

    // Financial properties
    [Column(SheetsConfig.HeaderNames.Pay, Format.ACCOUNTING)]
    public decimal? Pay { get; set; }

    [Column(SheetsConfig.HeaderNames.Tips, Format.ACCOUNTING)]
    public decimal? Tip { get; set; }

    [Column(SheetsConfig.HeaderNames.Bonus, Format.ACCOUNTING)]
    public decimal? Bonus { get; set; }

    [Column(SheetsConfig.HeaderNames.Total, Format.ACCOUNTING)]
    public decimal? Total { get; set; }

    [Column(SheetsConfig.HeaderNames.Cash, Format.ACCOUNTING)]
    public decimal? Cash { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerTrip, Format.ACCOUNTING)]
    public decimal AmountPerTrip { get; set; }

    [Column(SheetsConfig.HeaderNames.Distance, Format.DISTANCE)]
    public decimal Distance { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerDistance, Format.ACCOUNTING)]
    public decimal AmountPerDistance { get; set; }

    [Column(SheetsConfig.HeaderNames.TimeTotal, Format.DURATION)]
    public string Time { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.AmountPerTime, Format.ACCOUNTING)]
    public decimal AmountPerTime { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerDay, Format.ACCOUNTING)]
    [JsonPropertyName("dailyAverage")]
    public decimal AmountPerDay { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountCurrent, Format.ACCOUNTING)]
    [JsonPropertyName("currentAmount")]
    public decimal CurrentAmount { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPrevious, Format.ACCOUNTING)]
    [JsonPropertyName("previousAmount")]
    public decimal PreviousAmount { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerPreviousDay, Format.ACCOUNTING)]
    [JsonPropertyName("dailyPrevAverage")]
    public decimal AmountPerPrevDay { get; set; }
}
