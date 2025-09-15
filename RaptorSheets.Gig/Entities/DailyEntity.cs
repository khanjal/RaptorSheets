using System.Text.Json.Serialization;
using RaptorSheets.Core.Attributes;
using RaptorSheets.Gig.Constants;

namespace RaptorSheets.Gig.Entities;

public class DailyEntity
{
    [JsonPropertyName("rowId")]
    public int RowId { get; set; }

    [JsonPropertyName("date")]
    [SheetOrder(SheetsConfig.HeaderNames.Date)]
    public string Date { get; set; } = "";

    [JsonPropertyName("trips")]
    [SheetOrder(SheetsConfig.HeaderNames.Trips)]
    public int Trips { get; set; }

    // Financial properties in correct position (after Trips)
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

    [JsonPropertyName("amt/trip")]
    [SheetOrder(SheetsConfig.HeaderNames.AmountPerTrip)]
    public decimal AmountPerTrip { get; set; }

    [JsonPropertyName("distance")]
    [SheetOrder(SheetsConfig.HeaderNames.Distance)]
    public decimal Distance { get; set; }

    [JsonPropertyName("amt/dist")]
    [SheetOrder(SheetsConfig.HeaderNames.AmountPerDistance)]
    public decimal AmountPerDistance { get; set; }

    [JsonPropertyName("time")]
    [SheetOrder(SheetsConfig.HeaderNames.TimeTotal)]
    public string Time { get; set; } = "";

    [JsonPropertyName("amt/hour")]
    [SheetOrder(SheetsConfig.HeaderNames.AmountPerTime)]
    public decimal AmountPerTime { get; set; }

    [JsonPropertyName("day")]
    [SheetOrder(SheetsConfig.HeaderNames.Day)]
    public string Day { get; set; } = "";

    [SheetOrder(SheetsConfig.HeaderNames.Weekday)]
    public string Weekday { get; set; } = "";

    [JsonPropertyName("week")]
    [SheetOrder(SheetsConfig.HeaderNames.Week)]
    public string Week { get; set; } = "";

    [JsonPropertyName("month")]
    [SheetOrder(SheetsConfig.HeaderNames.Month)]
    public string Month { get; set; } = "";
}