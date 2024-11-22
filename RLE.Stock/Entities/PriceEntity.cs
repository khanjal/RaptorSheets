using System.Text.Json.Serialization;

namespace RLE.Stock.Entities;

public class PriceEntity : CostEntity
{
    [JsonPropertyName("peRatio")]
    public decimal PeRatio { get; set; }

    [JsonPropertyName("52WeekHigh")]
    public decimal WeekHigh52 { get; set; }

    [JsonPropertyName("52WeekLow")]
    public decimal WeekLow52 { get; set; }

    [JsonPropertyName("MaxHigh")]
    public decimal MaxHigh { get; set; }

    [JsonPropertyName("MinLow")]
    public decimal MinLow { get; set; }
}
