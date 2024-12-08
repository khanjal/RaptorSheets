using System.Text.Json.Serialization;

namespace RaptorSheets.Gig.Entities;

public class DailyEntity : AmountEntity
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("date")]
    public string Date { get; set; } = "";

    [JsonPropertyName("trips")]
    public int Trips { get; set; }

    [JsonPropertyName("amt/trip")]
    public decimal AmountPerTrip { get; set; }

    [JsonPropertyName("distance")]
    public decimal Distance { get; set; }

    [JsonPropertyName("amt/dist")]
    public decimal AmountPerDistance { get; set; }

    [JsonPropertyName("time")]
    public string Time { get; set; } = "";

    [JsonPropertyName("amt/hour")]
    public decimal AmountPerTime { get; set; }

    [JsonPropertyName("day")]
    public string Day { get; set; } = "";

    [JsonPropertyName("week")]
    public string Week { get; set; } = "";

    [JsonPropertyName("month")]
    public string Month { get; set; } = "";
}