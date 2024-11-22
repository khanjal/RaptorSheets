using System.Text.Json.Serialization;

namespace RLE.Stock.Entities;

public class TickerEntity : PriceEntity
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("ticker")]
    public string Ticker { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("accounts")]
    public string Accounts { get; set; } = "";
}
