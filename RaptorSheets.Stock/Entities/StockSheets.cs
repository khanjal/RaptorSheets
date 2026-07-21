using System.Text.Json.Serialization;

namespace RaptorSheets.Stock.Entities;

/// <summary>
/// Holds every strongly-typed row collection for the Stock domain, nested under
/// <see cref="SheetEntity.Sheets"/> so domain sheet data can never collide with the reserved
/// top-level <c>Properties</c>/<c>Messages</c> members.
/// </summary>
public class StockSheets
{
    [JsonPropertyName("accounts")]
    public List<AccountEntity> Accounts { get; set; } = [];

    [JsonPropertyName("stocks")]
    public List<StockEntity> Stocks { get; set; } = [];

    [JsonPropertyName("tickers")]
    public List<TickerEntity> Tickers { get; set; } = [];
}
