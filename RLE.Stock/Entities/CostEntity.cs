using System.Text.Json.Serialization;

namespace RLE.Stock.Entities;

public class CostEntity
{
    [JsonPropertyName("shares")]
    public decimal Shares { get; set; }

    [JsonPropertyName("averageCost")]
    public decimal AverageCost { get; set; }

    [JsonPropertyName("costTotal")]
    public decimal CostTotal { get; set; }

    [JsonPropertyName("currentPrice")]
    public decimal CurrentPrice { get; set; }

    [JsonPropertyName("currentTotal")]
    public decimal CurrentTotal { get; set; }

    [JsonPropertyName("return")]
    public decimal Return { get; set; }
}
