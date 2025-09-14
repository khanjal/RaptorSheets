using System.Text.Json.Serialization;
using RaptorSheets.Core.Attributes;
using RaptorSheets.Gig.Constants;

namespace RaptorSheets.Gig.Entities;

public class ShiftEntity : AmountEntity
{
    [JsonPropertyName("rowId")]
    public int RowId { get; set; }

    [JsonPropertyName("action")]
    public string Action { get; set; } = "";

    [JsonPropertyName("date")]
    [SheetOrder(SheetsConfig.HeaderNames.Date)]
    public string Date { get; set; } = "";

    [JsonPropertyName("start")]
    [SheetOrder(SheetsConfig.HeaderNames.TimeStart)]
    public string Start { get; set; } = "";

    [JsonPropertyName("finish")]
    [SheetOrder(SheetsConfig.HeaderNames.TimeEnd)]
    public string Finish { get; set; } = "";

    [JsonPropertyName("service")]
    [SheetOrder(SheetsConfig.HeaderNames.Service)]
    public string Service { get; set; } = "";

    [JsonPropertyName("number")]
    [SheetOrder(SheetsConfig.HeaderNames.Number)]
    public int? Number { get; set; }

    [JsonPropertyName("active")]
    [SheetOrder(SheetsConfig.HeaderNames.TimeActive)]
    public string Active { get; set; } = "";

    [JsonPropertyName("time")]
    [SheetOrder(SheetsConfig.HeaderNames.TimeTotal)]
    public string Time { get; set; } = "";

    [JsonPropertyName("omit")]
    [SheetOrder(SheetsConfig.HeaderNames.TimeOmit)]
    public bool? Omit { get; set; }

    [JsonPropertyName("trips")]
    [SheetOrder(SheetsConfig.HeaderNames.Trips)]
    public int Trips { get; set; }

    // AmountEntity properties: Pay, Tips, Bonus, Cash (inherited)

    [JsonPropertyName("startOdometer")]
    [SheetOrder(SheetsConfig.HeaderNames.OdometerStart)]
    public decimal? OdometerStart { get; set; }

    [JsonPropertyName("endOdometer")]
    [SheetOrder(SheetsConfig.HeaderNames.OdometerEnd)]
    public decimal? OdometerEnd { get; set; }

    [JsonPropertyName("distance")]
    [SheetOrder(SheetsConfig.HeaderNames.Distance)]
    public decimal? Distance { get; set; }

    [JsonPropertyName("region")]
    [SheetOrder(SheetsConfig.HeaderNames.Region)]
    public string Region { get; set; } = "";

    [JsonPropertyName("note")]
    [SheetOrder(SheetsConfig.HeaderNames.Note)]
    public string Note { get; set; } = "";

    [JsonPropertyName("key")]
    [SheetOrder(SheetsConfig.HeaderNames.Key)]
    public string Key { get; set; } = "";

    [JsonPropertyName("totalActive")]
    [SheetOrder(SheetsConfig.HeaderNames.TotalTimeActive)]
    public string TotalActive { get; set; } = "";

    [JsonPropertyName("totalTime")]
    [SheetOrder(SheetsConfig.HeaderNames.TotalTime)]
    public string TotalTime { get; set; } = "";

    [JsonPropertyName("totalTrips")]
    [SheetOrder(SheetsConfig.HeaderNames.TotalTrips)]
    public int TotalTrips { get; set; }

    [JsonPropertyName("totalPay")]
    [SheetOrder(SheetsConfig.HeaderNames.TotalPay)]
    public decimal? TotalPay { get; set; }

    [JsonPropertyName("totalTips")]
    [SheetOrder(SheetsConfig.HeaderNames.TotalTips)]
    public decimal? TotalTips { get; set; }

    [JsonPropertyName("totalBonus")]
    [SheetOrder(SheetsConfig.HeaderNames.TotalBonus)]
    public decimal? TotalBonus { get; set; }

    [JsonPropertyName("grandTotal")]
    [SheetOrder(SheetsConfig.HeaderNames.TotalGrand)]
    public decimal? GrandTotal { get; set; }

    [JsonPropertyName("totalCash")]
    [SheetOrder(SheetsConfig.HeaderNames.TotalCash)]
    public decimal? TotalCash { get; set; }

    [JsonPropertyName("amountPerTrip")]
    [SheetOrder(SheetsConfig.HeaderNames.AmountPerTrip)]
    public decimal? AmountPerTrip { get; set; }

    [JsonPropertyName("amountPerTime")]
    [SheetOrder(SheetsConfig.HeaderNames.AmountPerTime)]
    public decimal? AmountPerTime { get; set; }

    [JsonPropertyName("totalDistance")]
    [SheetOrder(SheetsConfig.HeaderNames.TotalDistance)]
    public decimal? TotalDistance { get; set; }

    [JsonPropertyName("amountPerDistance")]
    [SheetOrder(SheetsConfig.HeaderNames.AmountPerDistance)]
    public decimal? AmountPerDistance { get; set; }

    [SheetOrder(SheetsConfig.HeaderNames.TripsPerHour)]
    public decimal? TripsPerHour { get; set; }

    [SheetOrder(SheetsConfig.HeaderNames.Day)]
    public string Day { get; set; } = "";

    [SheetOrder(SheetsConfig.HeaderNames.Month)]
    public string Month { get; set; } = "";

    [SheetOrder(SheetsConfig.HeaderNames.Year)]
    public string Year { get; set; } = "";

    [JsonPropertyName("saved")]
    public bool Saved { get; set; }
}