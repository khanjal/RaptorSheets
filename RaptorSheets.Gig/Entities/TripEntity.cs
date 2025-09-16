using System.Text.Json.Serialization;
using RaptorSheets.Core.Attributes;
using RaptorSheets.Gig.Constants;

namespace RaptorSheets.Gig.Entities;

public class TripEntity : EntityBase
{
    [JsonPropertyName("rowId")]
    public int RowId { get; set; }

    [JsonPropertyName("action")]
    public string Action { get; set; } = "";

    [JsonPropertyName("date")]
    [ColumnOrder(SheetsConfig.HeaderNames.Date)]
    public string Date { get; set; } = "";

    [JsonPropertyName("service")]
    [ColumnOrder(SheetsConfig.HeaderNames.Service)]
    public string Service { get; set; } = "";

    [JsonPropertyName("number")]
    [ColumnOrder(SheetsConfig.HeaderNames.Number)]
    public int? Number { get; set; }

    [JsonPropertyName("exclude")]
    [ColumnOrder(SheetsConfig.HeaderNames.Exclude)]
    public bool Exclude { get; set; } = false;

    [JsonPropertyName("type")]
    [ColumnOrder(SheetsConfig.HeaderNames.Type)]
    public string Type { get; set; } = "";

    [JsonPropertyName("place")]
    [ColumnOrder(SheetsConfig.HeaderNames.Place)]
    public string Place { get; set; } = "";

    [JsonPropertyName("pickupTime")]
    [ColumnOrder(SheetsConfig.HeaderNames.Pickup)]
    public string Pickup { get; set; } = "";

    [JsonPropertyName("dropoffTime")]
    [ColumnOrder(SheetsConfig.HeaderNames.Dropoff)]
    public string Dropoff { get; set; } = "";

    [JsonPropertyName("duration")]
    [ColumnOrder(SheetsConfig.HeaderNames.Duration)]
    public string Duration { get; set; } = "";

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

    [JsonPropertyName("startOdometer")]
    [ColumnOrder(SheetsConfig.HeaderNames.OdometerStart)]
    public decimal? OdometerStart { get; set; }

    [JsonPropertyName("endOdometer")]
    [ColumnOrder(SheetsConfig.HeaderNames.OdometerEnd)]
    public decimal? OdometerEnd { get; set; }

    [JsonPropertyName("distance")]
    [ColumnOrder(SheetsConfig.HeaderNames.Distance)]
    public decimal? Distance { get; set; }

    [JsonPropertyName("name")]
    [ColumnOrder(SheetsConfig.HeaderNames.Name)]
    public string Name { get; set; } = "";

    [JsonPropertyName("startAddress")]
    [ColumnOrder(SheetsConfig.HeaderNames.AddressStart)]
    public string StartAddress { get; set; } = "";

    [JsonPropertyName("endAddress")]
    [ColumnOrder(SheetsConfig.HeaderNames.AddressEnd)]
    public string EndAddress { get; set; } = "";

    [JsonPropertyName("endUnit")]
    [ColumnOrder(SheetsConfig.HeaderNames.UnitEnd)]
    public string EndUnit { get; set; } = "";

    [JsonPropertyName("orderNumber")]
    [ColumnOrder(SheetsConfig.HeaderNames.OrderNumber)]
    public string OrderNumber { get; set; } = "";

    [JsonPropertyName("region")]
    [ColumnOrder(SheetsConfig.HeaderNames.Region)]
    public string Region { get; set; } = "";

    [JsonPropertyName("note")]
    [ColumnOrder(SheetsConfig.HeaderNames.Note)]
    public string Note { get; set; } = "";

    [JsonPropertyName("key")]
    [ColumnOrder(SheetsConfig.HeaderNames.Key)]
    public string Key { get; set; } = "";

    [ColumnOrder(SheetsConfig.HeaderNames.Day)]
    public string Day { get; set; } = "";

    [ColumnOrder(SheetsConfig.HeaderNames.Month)]
    public string Month { get; set; } = "";

    [ColumnOrder(SheetsConfig.HeaderNames.Year)]
    public string Year { get; set; } = "";

    [JsonPropertyName("amountPerTime")]
    [ColumnOrder(SheetsConfig.HeaderNames.AmountPerTime)]
    public decimal? AmountPerTime { get; set; }

    [JsonPropertyName("amountPerDistance")]
    [ColumnOrder(SheetsConfig.HeaderNames.AmountPerDistance)]
    public decimal? AmountPerDistance { get; set; }

    [JsonPropertyName("saved")]
    public bool Saved { get; set; }
}