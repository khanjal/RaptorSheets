using System.Text.Json.Serialization;

namespace GigRaptorLib.Entities;

public class AmountEntity
{
    [JsonPropertyName("pay")]
    public decimal? Pay { get; set; }

    [JsonPropertyName("tip")]
    public decimal? Tip { get; set; }

    [JsonPropertyName("bonus")]
    public decimal? Bonus { get; set; }

    [JsonPropertyName("total")]
    public decimal? Total { get; set; }

    [JsonPropertyName("cash")]
    public decimal? Cash { get; set; }

}