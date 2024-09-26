using System.Text.Json.Serialization;

namespace RLE.Gig.Entities;

public class ShiftEntity : AmountEntity
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("key")]
    public string Key { get; set; } = "";

    [JsonPropertyName("date")]
    public string Date { get; set; } = "";

    [JsonPropertyName("start")]
    public string Start { get; set; } = "";

    [JsonPropertyName("finish")]
    public string Finish { get; set; } = "";

    [JsonPropertyName("service")]
    public string Service { get; set; } = "";

    [JsonPropertyName("number")]
    public int? Number { get; set; }

    [JsonPropertyName("active")]
    public string Active { get; set; } = "";

    [JsonPropertyName("trips")]
    public int Trips { get; set; }

    [JsonPropertyName("distance")]
    public decimal? Distance { get; set; }

    [JsonPropertyName("time")]
    public string Time { get; set; } = "";

    [JsonPropertyName("omit")]
    public bool? Omit { get; set; }

    [JsonPropertyName("region")]
    public string Region { get; set; } = "";

    [JsonPropertyName("note")]
    public string Note { get; set; } = "";

    [JsonPropertyName("totalTrips")]
    public int TotalTrips { get; set; }

    [JsonPropertyName("totalDistance")]
    public decimal? TotalDistance { get; set; }

    [JsonPropertyName("totalPay")]
    public decimal? TotalPay { get; set; }

    [JsonPropertyName("totalTips")]
    public decimal? TotalTips { get; set; }

    [JsonPropertyName("totalBonus")]
    public decimal? TotalBonus { get; set; }

    [JsonPropertyName("grandTotal")]
    public decimal? GrandTotal { get; set; }

    [JsonPropertyName("totalCash")]
    public decimal? TotalCash { get; set; }

    [JsonPropertyName("amountPerTime")]
    public decimal? AmountPerTime { get; set; }

    [JsonPropertyName("amountPerDistance")]
    public decimal? AmountPerDistance { get; set; }

    [JsonPropertyName("amountPerTrip")]
    public decimal? AmountPerTrip { get; set; }

    [JsonPropertyName("saved")]
    public bool Saved { get; set; }
}