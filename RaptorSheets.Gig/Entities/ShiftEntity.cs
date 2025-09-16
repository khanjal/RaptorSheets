using System.Text.Json.Serialization;
using RaptorSheets.Core.Attributes;
using RaptorSheets.Gig.Constants;

namespace RaptorSheets.Gig.Entities;

public class ShiftEntity : EntityBase
{
    [JsonPropertyName("rowId")]
    public int RowId { get; set; }

    [JsonPropertyName("action")]
    public string Action { get; set; } = "";

    [JsonPropertyName("date")]
    [ColumnOrder(SheetsConfig.HeaderNames.Date)]
    public string Date { get; set; } = "";

    [JsonPropertyName("start")]
    [ColumnOrder(SheetsConfig.HeaderNames.TimeStart)]
    public string Start { get; set; } = "";

    [JsonPropertyName("finish")]
    [ColumnOrder(SheetsConfig.HeaderNames.TimeEnd)]
    public string Finish { get; set; } = "";

    [JsonPropertyName("service")]
    [ColumnOrder(SheetsConfig.HeaderNames.Service)]
    public string Service { get; set; } = "";

    [JsonPropertyName("number")]
    [ColumnOrder(SheetsConfig.HeaderNames.Number)]
    public int? Number { get; set; }

    [JsonPropertyName("active")]
    [ColumnOrder(SheetsConfig.HeaderNames.TimeActive)]
    public string Active { get; set; } = "";

    [JsonPropertyName("time")]
    [ColumnOrder(SheetsConfig.HeaderNames.TimeTotal)]
    public string Time { get; set; } = "";

    [JsonPropertyName("omit")]
    [ColumnOrder(SheetsConfig.HeaderNames.TimeOmit)]
    public bool? Omit { get; set; }

    [JsonPropertyName("trips")]
    [ColumnOrder(SheetsConfig.HeaderNames.Trips)]
    public int Trips { get; set; }

    // Financial properties from AmountEntity in correct position
    [JsonPropertyName("pay")]
    [ColumnOrder(SheetsConfig.HeaderNames.Pay)]
    public decimal? Pay { get; set; }

    [JsonPropertyName("tip")]
    [ColumnOrder(SheetsConfig.HeaderNames.Tips)]
    public decimal? Tip { get; set; }

    [JsonPropertyName("bonus")]
    [ColumnOrder(SheetsConfig.HeaderNames.Bonus)]
    public decimal? Bonus { get; set; }

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

    [JsonPropertyName("region")]
    [ColumnOrder(SheetsConfig.HeaderNames.Region)]
    public string Region { get; set; } = "";

    [JsonPropertyName("note")]
    [ColumnOrder(SheetsConfig.HeaderNames.Note)]
    public string Note { get; set; } = "";

    [JsonPropertyName("key")]
    [ColumnOrder(SheetsConfig.HeaderNames.Key)]
    public string Key { get; set; } = "";

    [JsonPropertyName("totalActive")]
    [ColumnOrder(SheetsConfig.HeaderNames.TotalTimeActive)]
    public string TotalActive { get; set; } = "";

    [JsonPropertyName("totalTime")]
    [ColumnOrder(SheetsConfig.HeaderNames.TotalTime)]
    public string TotalTime { get; set; } = "";

    [JsonPropertyName("totalTrips")]
    [ColumnOrder(SheetsConfig.HeaderNames.TotalTrips)]
    public int TotalTrips { get; set; }

    [JsonPropertyName("totalPay")]
    [ColumnOrder(SheetsConfig.HeaderNames.TotalPay)]
    public decimal? TotalPay { get; set; }

    [JsonPropertyName("totalTips")]
    [ColumnOrder(SheetsConfig.HeaderNames.TotalTips)]
    public decimal? TotalTips { get; set; }

    [JsonPropertyName("totalBonus")]
    [ColumnOrder(SheetsConfig.HeaderNames.TotalBonus)]
    public decimal? TotalBonus { get; set; }

    [JsonPropertyName("grandTotal")]
    [ColumnOrder(SheetsConfig.HeaderNames.TotalGrand)]
    public decimal? GrandTotal { get; set; }

    [JsonPropertyName("totalCash")]
    [ColumnOrder(SheetsConfig.HeaderNames.TotalCash)]
    public decimal? TotalCash { get; set; }

    [JsonPropertyName("amountPerTrip")]
    [ColumnOrder(SheetsConfig.HeaderNames.AmountPerTrip)]
    public decimal? AmountPerTrip { get; set; }

    [JsonPropertyName("amountPerTime")]
    [ColumnOrder(SheetsConfig.HeaderNames.AmountPerTime)]
    public decimal? AmountPerTime { get; set; }

    [JsonPropertyName("totalDistance")]
    [ColumnOrder(SheetsConfig.HeaderNames.TotalDistance)]
    public decimal? TotalDistance { get; set; }

    [JsonPropertyName("amountPerDistance")]
    [ColumnOrder(SheetsConfig.HeaderNames.AmountPerDistance)]
    public decimal? AmountPerDistance { get; set; }

    [ColumnOrder(SheetsConfig.HeaderNames.TripsPerHour)]
    public decimal? TripsPerHour { get; set; }

    [ColumnOrder(SheetsConfig.HeaderNames.Day)]
    public string Day { get; set; } = "";

    [ColumnOrder(SheetsConfig.HeaderNames.Month)]
    public string Month { get; set; } = "";

    [ColumnOrder(SheetsConfig.HeaderNames.Year)]
    public string Year { get; set; } = "";

    [JsonPropertyName("saved")]
    public bool Saved { get; set; }
}
