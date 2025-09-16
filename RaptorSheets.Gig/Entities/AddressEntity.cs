using System.Diagnostics.CodeAnalysis;
using RaptorSheets.Core.Attributes;
using RaptorSheets.Gig.Constants;
using System.Text.Json.Serialization;

namespace RaptorSheets.Gig.Entities;

[SuppressMessage("Major Code Smell", "S4144:Properties should not be duplicated", Justification = "Intentional duplication for sheet mapping")]
public class AddressEntity 
{
    [JsonPropertyName("rowId")]
    public int RowId { get; set; }

    [JsonPropertyName("address")]
    [ColumnOrder(SheetsConfig.HeaderNames.Address)]
    public string Address { get; set; } = "";

    [JsonPropertyName("trips")]
    [ColumnOrder(SheetsConfig.HeaderNames.Trips)]
    public int Trips { get; set; }

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

    [JsonPropertyName("first trip")]
    [ColumnOrder(SheetsConfig.HeaderNames.VisitFirst)]
    public string FirstTrip { get; set; } = "";

    [JsonPropertyName("last trip")]
    [ColumnOrder(SheetsConfig.HeaderNames.VisitLast)]
    public string LastTrip { get; set; } = "";

    [JsonPropertyName("saved")]
    public bool Saved { get; set; }
}