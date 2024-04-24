using System.Text.Json.Serialization;

namespace GigRaptorLib.Entities;

public class YearlyEntity : AmountEntity
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("year")]
    public int Year { get; set; }

    [JsonPropertyName("trips")]
    public int Trips { get; set; }

    [JsonPropertyName("days")]
    public int Days { get; set; }

    [JsonPropertyName("amt/trip")]
    public decimal AmountPerTrip { get; set; }

    [JsonPropertyName("distance")]
    public decimal Distance { get; set; }

    [JsonPropertyName("amt/dist")]
    public decimal AmountPerDistance { get; set; }

    [JsonPropertyName("time")]
    public string Time { get; set; }

    [JsonPropertyName("amt/hour")]
    public decimal AmountPerTime { get; set; }

    [JsonPropertyName("average")]
    public decimal Average { get; set; }

    [JsonPropertyName("amt/day")]
    public decimal AmountPerDay { get; set; }
}