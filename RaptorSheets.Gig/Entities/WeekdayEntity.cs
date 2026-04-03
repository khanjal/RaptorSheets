using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Enums;
using RaptorSheets.Gig.Constants;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace RaptorSheets.Gig.Entities;

[ExcludeFromCodeCoverage]
public class WeekdayEntity : SheetRowEntityBase
{
    [Header(SheetsConfig.HeaderNames.Day)]
    public int Day { get; set; }

    [Header(SheetsConfig.HeaderNames.Weekday)]
    public string Weekday { get; set; } = "";

    [Header(SheetsConfig.HeaderNames.Trips)]
    public int Trips { get; set; }

    [Header(SheetsConfig.HeaderNames.Days)]
    public int Days { get; set; }

    // Financial properties
    [Header(SheetsConfig.HeaderNames.Pay)]
    [Format(FormatEnum.ACCOUNTING)]
    public decimal? Pay { get; set; }

    [Header(SheetsConfig.HeaderNames.Tips)]
    [Format(FormatEnum.ACCOUNTING)]
    public decimal? Tip { get; set; }

    [Header(SheetsConfig.HeaderNames.Bonus)]
    [Format(FormatEnum.ACCOUNTING)]
    public decimal? Bonus { get; set; }

    [Header(SheetsConfig.HeaderNames.Total)]
    [Format(FormatEnum.ACCOUNTING)]
    public decimal? Total { get; set; }

    [Header(SheetsConfig.HeaderNames.Cash)]
    [Format(FormatEnum.ACCOUNTING)]
    public decimal? Cash { get; set; }

    [Header(SheetsConfig.HeaderNames.AmountPerTrip)]
    [Format(FormatEnum.ACCOUNTING)]
    public decimal AmountPerTrip { get; set; }

    [Header(SheetsConfig.HeaderNames.Distance)]
    [Format(FormatEnum.DISTANCE)]
    public decimal Distance { get; set; }

    [Header(SheetsConfig.HeaderNames.AmountPerDistance)]
    [Format(FormatEnum.ACCOUNTING)]
    public decimal AmountPerDistance { get; set; }

    [Header(SheetsConfig.HeaderNames.TimeTotal)]
    [Format(FormatEnum.DURATION)]
    public string Time { get; set; } = string.Empty;

    [Header(SheetsConfig.HeaderNames.AmountPerTime)]
    [Format(FormatEnum.ACCOUNTING)]
    public decimal AmountPerTime { get; set; }

    [Header(SheetsConfig.HeaderNames.AmountPerDay)]
    [Format(FormatEnum.ACCOUNTING)]
    [JsonPropertyName("dailyAverage")]
    public decimal AmountPerDay { get; set; }

    [Header(SheetsConfig.HeaderNames.AmountCurrent)]
    [Format(FormatEnum.ACCOUNTING)]
    [JsonPropertyName("currentAmount")]
    public decimal CurrentAmount { get; set; }

    [Header(SheetsConfig.HeaderNames.AmountPrevious)]
    [Format(FormatEnum.ACCOUNTING)]
    [JsonPropertyName("previousAmount")]
    public decimal PreviousAmount { get; set; }

    [Header(SheetsConfig.HeaderNames.AmountPerPreviousDay)]
    [Format(FormatEnum.ACCOUNTING)]
    [JsonPropertyName("dailyPrevAverage")]
    public decimal AmountPerPrevDay { get; set; }
}
