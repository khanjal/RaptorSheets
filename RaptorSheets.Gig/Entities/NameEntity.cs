using System.Text.Json.Serialization;

namespace RaptorSheets.Gig.Entities;

public class NameEntity : AmountEntity
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("visits")]
    public int Visits { get; set; }

    [JsonPropertyName("distance")]
    public int Distance { get; set; }
}