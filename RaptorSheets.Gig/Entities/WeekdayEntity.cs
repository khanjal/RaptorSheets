using System.Text.Json.Serialization;
using RaptorSheets.Core.Attributes;
using RaptorSheets.Gig.Constants;

namespace RaptorSheets.Gig.Entities;

public class WeekdayEntity
{
    [JsonPropertyName("rowId")]
    public int RowId { get; set; }

    [JsonPropertyName("day")]
    [SheetOrder(SheetsConfig.HeaderNames.Day)]
    public int Day { get; set; }

    [JsonPropertyName("weekday")]
    [SheetOrder(SheetsConfig.HeaderNames.Weekday)]
    public string Weekday { get; set; } = "";

    [JsonPropertyName("trips")]
    [SheetOrder(SheetsConfig.HeaderNames.Trips)]
    public int Trips { get; set; }

    [JsonPropertyName("days")]
    [SheetOrder(SheetsConfig.HeaderNames.Days)]
    public int Days { get; set; }

    // Financial properties in correct position
    [JsonPropertyName("pay")]
    [SheetOrder(SheetsConfig.HeaderNames.Pay)]
    public decimal? Pay { get; set; }

    [JsonPropertyName("tip")]
    [SheetOrder(SheetsConfig.HeaderNames.Tips)]
    public decimal? Tip { get; set; }

    [JsonPropertyName("bonus")]
    [SheetOrder(SheetsConfig.HeaderNames.Bonus)]
    public decimal? Bonus { get; set; }

    [JsonPropertyName("total")]
    [SheetOrder(SheetsConfig.HeaderNames.Total)]
    public decimal? Total { get; set; }

    [JsonPropertyName("cash")]
    [SheetOrder(SheetsConfig.HeaderNames.Cash)]
    public decimal? Cash { get; set; }

    [SheetOrder(SheetsConfig.HeaderNames.AmountPerTrip)]
    public decimal AmountPerTrip { get; set; }

    [JsonPropertyName("distance")]
    [SheetOrder(SheetsConfig.HeaderNames.Distance)]
    public decimal Distance { get; set; }

    [SheetOrder(SheetsConfig.HeaderNames.AmountPerDistance)]
    public decimal AmountPerDistance { get; set; }

    [JsonPropertyName("time")]
    [SheetOrder(SheetsConfig.HeaderNames.TimeTotal)]
    public string Time { get; set; } = "";

    [SheetOrder(SheetsConfig.HeaderNames.AmountPerTime)]
    public decimal AmountPerTime { get; set; }

    [SheetOrder(SheetsConfig.HeaderNames.AmountPerDay)]
    public decimal AmountPerDay { get; set; }

    [JsonPropertyName("dailyAverage")]
    [SheetOrder(SheetsConfig.HeaderNames.AmountCurrent)]
    public decimal DailyAverage { get; set; }

    [JsonPropertyName("dailyPrevAverage")]
    [SheetOrder(SheetsConfig.HeaderNames.AmountPrevious)]
    public decimal PreviousDailyAverage { get; set; }

    [JsonPropertyName("currentAmount")]
    [SheetOrder(SheetsConfig.HeaderNames.AmountPerPreviousDay)]
    public decimal CurrentAmount { get; set; }

    [JsonPropertyName("previousAmount")]
    public decimal PreviousAmount { get; set; }
}