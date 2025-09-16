using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using RaptorSheets.Core.Attributes;
using RaptorSheets.Gig.Constants;

namespace RaptorSheets.Gig.Entities;

[SuppressMessage("Major Code Smell", "S4144:Properties should not be duplicated", Justification = "Intentional duplication for sheet mapping")]
public class TypeEntity
{
    [JsonPropertyName("rowId")]
    public int RowId { get; set; }

    // Entity-specific property first
    [JsonPropertyName("type")]
    [ColumnOrder(SheetsConfig.HeaderNames.Type)]
    public string Type { get; set; } = "";

    // CommonTripSheetHeaders pattern
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
    public decimal Distance { get; set; }

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
