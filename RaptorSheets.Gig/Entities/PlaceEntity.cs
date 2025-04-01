using System.Text.Json.Serialization;

namespace RaptorSheets.Gig.Entities;

public class PlaceEntity : VisitEntity
{
    [JsonPropertyName("rowId")]
    public int RowId { get; set; }

    [JsonPropertyName("place")]
    public string Place { get; set; } = "";

    [JsonPropertyName("distance")]
    public decimal Distance { get; set; }

    [JsonPropertyName("saved")]
    public bool Saved { get; set; }
}