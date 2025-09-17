using System.Text.Json.Serialization;
using RaptorSheets.Core.Attributes;
using RaptorSheets.Gig.Constants;

namespace RaptorSheets.Gig.Entities;

public class NameEntity
{
    [JsonPropertyName("rowId")]
    public int RowId { get; set; }

    // Entity-specific property first
    [JsonPropertyName("name")]
    [ColumnOrder(SheetsConfig.HeaderNames.Name)]
    public string Name { get; set; } = "";

    // CommonTripSheetHeaders pattern: Trips, CommonIncomeHeaders, CommonTravelHeaders, VisitFirst, VisitLast
    [JsonPropertyName("trips")]
    [ColumnOrder(SheetsConfig.HeaderNames.Trips)]
    public int Trips { get; set; }

    // CommonIncomeHeaders
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

    // CommonTravelHeaders
    [ColumnOrder(SheetsConfig.HeaderNames.AmountPerTrip)]
    public decimal AmountPerTrip { get; set; }

    [JsonPropertyName("distance")]
    [ColumnOrder(SheetsConfig.HeaderNames.Distance)]
    public int Distance { get; set; }

    [ColumnOrder(SheetsConfig.HeaderNames.AmountPerDistance)]
    public decimal AmountPerDistance { get; set; }

    // Visit properties
    [JsonPropertyName("firstTrip")]
    [ColumnOrder(SheetsConfig.HeaderNames.VisitFirst)]
    public string FirstTrip { get; set; } = "";

    [JsonPropertyName("lastTrip")]
    [ColumnOrder(SheetsConfig.HeaderNames.VisitLast)]
    public string LastTrip { get; set; } = "";

    [JsonPropertyName("saved")]
    public bool Saved { get; set; }
}