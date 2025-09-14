using System.Text.Json.Serialization;
using RaptorSheets.Core.Attributes;
using RaptorSheets.Gig.Constants;

namespace RaptorSheets.Gig.Entities;

public class TripEntity : AmountEntity
{
    [JsonPropertyName("rowId")]
    public int RowId { get; set; }

    [JsonPropertyName("action")]
    public string Action { get; set; } = "";

    [JsonPropertyName("date")]
    [SheetOrder(SheetsConfig.HeaderNames.Date)]
    public string Date { get; set; } = "";

    [JsonPropertyName("service")]
    [SheetOrder(SheetsConfig.HeaderNames.Service)]
    public string Service { get; set; } = "";

    [JsonPropertyName("number")]
    [SheetOrder(SheetsConfig.HeaderNames.Number)]
    public int? Number { get; set; }

    [JsonPropertyName("exclude")]
    [SheetOrder(SheetsConfig.HeaderNames.Exclude)]
    public bool Exclude { get; set; } = false;

    [JsonPropertyName("type")]
    [SheetOrder(SheetsConfig.HeaderNames.Type)]
    public string Type { get; set; } = "";

    [JsonPropertyName("place")]
    [SheetOrder(SheetsConfig.HeaderNames.Place)]
    public string Place { get; set; } = "";

    [JsonPropertyName("pickupTime")]
    [SheetOrder(SheetsConfig.HeaderNames.Pickup)]
    public string Pickup { get; set; } = "";

    [JsonPropertyName("dropoffTime")]
    [SheetOrder(SheetsConfig.HeaderNames.Dropoff)]
    public string Dropoff { get; set; } = "";

    [JsonPropertyName("duration")]
    [SheetOrder(SheetsConfig.HeaderNames.Duration)]
    public string Duration { get; set; } = "";

    // AmountEntity properties: Pay, Tips, Bonus, Total, Cash (inherited)

    [JsonPropertyName("startOdometer")]
    [SheetOrder(SheetsConfig.HeaderNames.OdometerStart)]
    public decimal? OdometerStart { get; set; }

    [JsonPropertyName("endOdometer")]
    [SheetOrder(SheetsConfig.HeaderNames.OdometerEnd)]
    public decimal? OdometerEnd { get; set; }

    [JsonPropertyName("distance")]
    [SheetOrder(SheetsConfig.HeaderNames.Distance)]
    public decimal? Distance { get; set; }

    [JsonPropertyName("name")]
    [SheetOrder(SheetsConfig.HeaderNames.Name)]
    public string Name { get; set; } = "";

    [JsonPropertyName("startAddress")]
    [SheetOrder(SheetsConfig.HeaderNames.AddressStart)]
    public string StartAddress { get; set; } = "";

    [JsonPropertyName("endAddress")]
    [SheetOrder(SheetsConfig.HeaderNames.AddressEnd)]
    public string EndAddress { get; set; } = "";

    [JsonPropertyName("endUnit")]
    [SheetOrder(SheetsConfig.HeaderNames.UnitEnd)]
    public string EndUnit { get; set; } = "";

    [JsonPropertyName("orderNumber")]
    [SheetOrder(SheetsConfig.HeaderNames.OrderNumber)]
    public string OrderNumber { get; set; } = "";

    [JsonPropertyName("region")]
    [SheetOrder(SheetsConfig.HeaderNames.Region)]
    public string Region { get; set; } = "";

    [JsonPropertyName("note")]
    [SheetOrder(SheetsConfig.HeaderNames.Note)]
    public string Note { get; set; } = "";

    [JsonPropertyName("key")]
    [SheetOrder(SheetsConfig.HeaderNames.Key)]
    public string Key { get; set; } = "";

    [SheetOrder(SheetsConfig.HeaderNames.Day)]
    public string Day { get; set; } = "";

    [SheetOrder(SheetsConfig.HeaderNames.Month)]
    public string Month { get; set; } = "";

    [SheetOrder(SheetsConfig.HeaderNames.Year)]
    public string Year { get; set; } = "";

    [JsonPropertyName("amountPerTime")]
    [SheetOrder(SheetsConfig.HeaderNames.AmountPerTime)]
    public decimal? AmountPerTime { get; set; }

    [JsonPropertyName("amountPerDistance")]
    [SheetOrder(SheetsConfig.HeaderNames.AmountPerDistance)]
    public decimal? AmountPerDistance { get; set; }

    [JsonPropertyName("saved")]
    public bool Saved { get; set; }
}