using System.Text.Json.Serialization;
using RaptorSheets.Core.Attributes;
using RaptorSheets.Gig.Constants;

namespace RaptorSheets.Gig.Entities;

public class WeeklyEntity
{
    [JsonPropertyName("rowId")]
    public int RowId { get; set; }

    [JsonPropertyName("week")]
    [SheetOrder(SheetsConfig.HeaderNames.Week)]
    public string Week { get; set; } = "";

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

    [SheetOrder(SheetsConfig.HeaderNames.AmountPerDay)]
    public decimal AmountPerDay { get; set; }

    [JsonPropertyName("average")]
    [SheetOrder(SheetsConfig.HeaderNames.Average)]
    public decimal Average { get; set; }

    [JsonPropertyName("#")]
    [SheetOrder(SheetsConfig.HeaderNames.Number)]
    public int Number { get; set; }

    [JsonPropertyName("year")]
    [SheetOrder(SheetsConfig.HeaderNames.Year)]
    public int Year { get; set; }

    [JsonPropertyName("begin")]
    [SheetOrder(SheetsConfig.HeaderNames.DateBegin)]
    public string Begin { get; set; } = "";

    [JsonPropertyName("end")]
    [SheetOrder(SheetsConfig.HeaderNames.DateEnd)]
    public string End { get; set; } = "";
}