using RLE.Core.Entities;
using System.Text.Json.Serialization;

namespace RLE.Stock.Entities;

public class SheetEntity
{
    [JsonPropertyName("properties")]
    public PropertyEntity Properties { get; set; } = new();

    [JsonPropertyName("accounts")]
    public List<AccountEntity> Accounts { get; set; } = [];

    [JsonPropertyName("stocks")]
    public List<StockEntity> Stocks { get; set; } = [];

    [JsonPropertyName("tickers")]
    public List<TickerEntity> Tickers { get; set; } = [];

   [JsonPropertyName("messages")]
    public List<MessageEntity> Messages { get; set; } = [];
}