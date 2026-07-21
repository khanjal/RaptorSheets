using System.Text.Json.Serialization;

namespace RaptorSheets.Stock.Entities;

public class StockEntity : PriceEntity
{
    [JsonPropertyName("account")]
    public string Account { get; set; } = "";

    [JsonPropertyName("ticker")]
    public string Ticker { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
}