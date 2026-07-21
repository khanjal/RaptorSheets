using System.Text.Json.Serialization;

namespace RaptorSheets.Stock.Entities;

public class AccountEntity : CostEntity
{
    [JsonPropertyName("account")]
    public string Account { get; set; } = "";

    [JsonPropertyName("stocks")]
    public decimal Stocks { get; set; }
}
