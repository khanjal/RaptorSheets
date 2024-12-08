using System.Text.Json.Serialization;

namespace RaptorSheets.Stock.Entities;

public class TickerEntity : PriceEntity
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("ticker")]
    public string Ticker { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("accounts")]
    public int Accounts { get; set; }
}
