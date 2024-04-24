using System.Text.Json.Serialization;

namespace GigRaptorLib.Entities;

public class PlaceEntity : AmountEntity
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("place")]
    public string Place { get; set; }

    [JsonPropertyName("visits")]
    public int Trips { get; set; }

    [JsonPropertyName("distance")]
    public decimal Distance { get; set; }
}