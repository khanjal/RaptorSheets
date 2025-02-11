using System.Text.Json.Serialization;

namespace RaptorSheets.Stock.Entities;

public class TickerEntity : PriceEntity
{
    [JsonPropertyName("rowId")]
    public int RowId { get; set; }

    [JsonPropertyName("ticker")]
    public string Ticker { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("accounts")]
    public int Accounts { get; set; }
}
