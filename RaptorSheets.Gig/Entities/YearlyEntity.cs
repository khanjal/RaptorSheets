using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using RaptorSheets.Core.Attributes;
using RaptorSheets.Gig.Constants;

namespace RaptorSheets.Gig.Entities;

[SuppressMessage("Major Code Smell", "S4144:Properties should not be duplicated", Justification = "Intentional duplication for sheet mapping")]
public class YearlyEntity
{
    [JsonPropertyName("rowId")]
    public int RowId { get; set; }

    [JsonPropertyName("year")]
    [ColumnOrder(SheetsConfig.HeaderNames.Year)]
    public int Year { get; set; }

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

    [JsonPropertyName("amt/trip")]
    [ColumnOrder(SheetsConfig.HeaderNames.AmountPerTrip)]
    public decimal AmountPerTrip { get; set; }

    [JsonPropertyName("distance")]
    [ColumnOrder(SheetsConfig.HeaderNames.Distance)]
    public decimal Distance { get; set; }

    [JsonPropertyName("amt/dist")]
    [ColumnOrder(SheetsConfig.HeaderNames.AmountPerDistance)]
    public decimal AmountPerDistance { get; set; }

    [JsonPropertyName("time")]
    [ColumnOrder(SheetsConfig.HeaderNames.TimeTotal)]
    public string Time { get; set; } = "";

    [JsonPropertyName("amt/hour")]
    [ColumnOrder(SheetsConfig.HeaderNames.AmountPerTime)]
    public decimal AmountPerTime { get; set; }

    [ColumnOrder(SheetsConfig.HeaderNames.AmountPerDay)]
    public decimal AmountPerDay { get; set; }

    [JsonPropertyName("average")]
    [ColumnOrder(SheetsConfig.HeaderNames.Average)]
    public decimal Average { get; set; }
}
