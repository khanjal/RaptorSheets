using System.Text.Json.Serialization;
using RaptorSheets.Core.Attributes;
using RaptorSheets.Gig.Constants;

namespace RaptorSheets.Gig.Entities;

public class WeekdayEntity
{
    [JsonPropertyName("rowId")]
    public int RowId { get; set; }

    [JsonPropertyName("day")]
    [ColumnOrder(SheetsConfig.HeaderNames.Day)]
    public int Day { get; set; }

    [JsonPropertyName("weekday")]
    [ColumnOrder(SheetsConfig.HeaderNames.Weekday)]
    public string Weekday { get; set; } = "";

    [JsonPropertyName("trips")]
    [ColumnOrder(SheetsConfig.HeaderNames.Trips)]
    public int Trips { get; set; }

    [JsonPropertyName("days")]
    [ColumnOrder(SheetsConfig.HeaderNames.Days)]
    public int Days { get; set; }

    // Financial properties in correct position
    [JsonPropertyName("pay")]
    [ColumnOrder(SheetsConfig.HeaderNames.Pay)]
    public decimal? Pay { get; set; }

    [JsonPropertyName("tip")]
    [ColumnOrder(SheetsConfig.HeaderNames.Tips)]
    public decimal? Tip { get; set; }

    [JsonPropertyName("bonus")]
    [ColumnOrder(SheetsConfig.HeaderNames.Bonus)]
    public decimal? Bonus { get; set; }

    [JsonPropertyName("total")]
    [ColumnOrder(SheetsConfig.HeaderNames.Total)]
    public decimal? Total { get; set; }

    [JsonPropertyName("cash")]
    [ColumnOrder(SheetsConfig.HeaderNames.Cash)]
    public decimal? Cash { get; set; }

    [ColumnOrder(SheetsConfig.HeaderNames.AmountPerTrip)]
    public decimal AmountPerTrip { get; set; }

    [JsonPropertyName("distance")]
    [ColumnOrder(SheetsConfig.HeaderNames.Distance)]
    public decimal Distance { get; set; }

    [ColumnOrder(SheetsConfig.HeaderNames.AmountPerDistance)]
    public decimal AmountPerDistance { get; set; }

    [JsonPropertyName("time")]
    [ColumnOrder(SheetsConfig.HeaderNames.TimeTotal)]
    public string Time { get; set; } = "";

    [ColumnOrder(SheetsConfig.HeaderNames.AmountPerTime)]
    public decimal AmountPerTime { get; set; }

    [ColumnOrder(SheetsConfig.HeaderNames.AmountPerDay)]
    public decimal AmountPerDay { get; set; }

    [JsonPropertyName("dailyAverage")]
    [ColumnOrder(SheetsConfig.HeaderNames.AmountCurrent)]
    public decimal DailyAverage { get; set; }

    [JsonPropertyName("dailyPrevAverage")]
    [ColumnOrder(SheetsConfig.HeaderNames.AmountPrevious)]
    public decimal PreviousDailyAverage { get; set; }

    [JsonPropertyName("currentAmount")]
    [ColumnOrder(SheetsConfig.HeaderNames.AmountPerPreviousDay)]
    public decimal CurrentAmount { get; set; }

    [JsonPropertyName("previousAmount")]
    public decimal PreviousAmount { get; set; }
}
