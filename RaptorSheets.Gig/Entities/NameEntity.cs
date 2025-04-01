using System.Text.Json.Serialization;

namespace RaptorSheets.Gig.Entities;

public class NameEntity : VisitEntity
{
    [JsonPropertyName("rowId")]
    public int RowId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("distance")]
    public int Distance { get; set; }

    [JsonPropertyName("saved")]
    public bool Saved { get; set; }
}