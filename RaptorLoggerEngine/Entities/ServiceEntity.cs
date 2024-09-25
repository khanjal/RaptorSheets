using System.Text.Json.Serialization;

namespace RaptorLoggerEngine.Entities;

public class ServiceEntity : AmountEntity
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("service")]
    public string Service { get; set; } = "";

    [JsonPropertyName("visits")]
    public int Trips { get; set; }

    [JsonPropertyName("distance")]
    public decimal Distance { get; set; }
}