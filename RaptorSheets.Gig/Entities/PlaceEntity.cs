using System.Text.Json.Serialization;

namespace RaptorSheets.Gig.Entities;

public class PlaceEntity : AmountEntity
{
    [JsonPropertyName("rowId")]
    public int RowId { get; set; }

    [JsonPropertyName("place")]
    public string Place { get; set; } = "";

    [JsonPropertyName("visits")]
    public int Trips { get; set; }

    [JsonPropertyName("distance")]
    public decimal Distance { get; set; }
}