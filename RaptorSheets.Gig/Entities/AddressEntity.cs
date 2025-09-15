using System.Text.Json.Serialization;
using RaptorSheets.Core.Attributes;
using RaptorSheets.Gig.Constants;

namespace RaptorSheets.Gig.Entities;

public class AddressEntity
{
    [JsonPropertyName("rowId")]
    public int RowId { get; set; }

    [JsonPropertyName("address")]
    [SheetOrder(SheetsConfig.HeaderNames.Address)]
    public string Address { get; set; } = "";

    [JsonPropertyName("trips")]
    [SheetOrder(SheetsConfig.HeaderNames.Trips)]
    public int Trips { get; set; }

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

    [JsonPropertyName("first trip")]
    [SheetOrder(SheetsConfig.HeaderNames.VisitFirst)]
    public string FirstTrip { get; set; } = "";

    [JsonPropertyName("last trip")]
    [SheetOrder(SheetsConfig.HeaderNames.VisitLast)]
    public string LastTrip { get; set; } = "";

    [JsonPropertyName("saved")]
    public bool Saved { get; set; }
}