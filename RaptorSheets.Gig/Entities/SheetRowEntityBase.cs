using System.Text.Json.Serialization;

namespace RaptorSheets.Gig.Entities;

/// <summary>
/// Base class for sheet row entities that support update/save logic.
/// Properties are serialized to JSON for API communication.
/// </summary>
public abstract class SheetRowEntityBase
{
    [JsonPropertyName("rowId")]
    public int RowId { get; set; }

    [JsonPropertyName("action")]
    public string Action { get; set; } = string.Empty;

    [JsonPropertyName("saved")]
    public bool Saved { get; set; }
}
