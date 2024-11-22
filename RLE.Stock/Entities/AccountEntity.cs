using System.Text.Json.Serialization;

namespace RLE.Stock.Entities;

public class AccountEntity : CostEntity
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("account")]
    public string Account { get; set; } = "";

    [JsonPropertyName("stocks")]
    public string Stocks { get; set; } = "";
}
