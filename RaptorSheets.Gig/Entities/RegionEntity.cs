using System.Text.Json.Serialization;

namespace RaptorSheets.Gig.Entities;

public class RegionEntity : AmountEntity
{
    [JsonPropertyName("rowId")]
    public int RowId { get; set; }

    [JsonPropertyName("region")]
    public string Region { get; set; } = "";

    [JsonPropertyName("visits")]
    public int Trips { get; set; }

    [JsonPropertyName("distance")]
    public decimal Distance { get; set; }

    [JsonPropertyName("saved")]
    public bool Saved { get; set; }
}