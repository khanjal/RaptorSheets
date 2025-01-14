using System.Text.Json.Serialization;

namespace RaptorSheets.Gig.Entities;

public class RegionEntity : AmountEntity
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("region")]
    public string Region { get; set; } = "";

    [JsonPropertyName("visits")]
    public int Trips { get; set; }

    [JsonPropertyName("distance")]
    public decimal Distance { get; set; }
}