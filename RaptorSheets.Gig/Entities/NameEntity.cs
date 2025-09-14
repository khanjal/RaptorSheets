using System.Text.Json.Serialization;
using RaptorSheets.Core.Attributes;
using RaptorSheets.Gig.Constants;

namespace RaptorSheets.Gig.Entities;

public class NameEntity : VisitEntity
{
    [JsonPropertyName("rowId")]
    public int RowId { get; set; }

    [JsonPropertyName("name")]
    [SheetOrder(SheetsConfig.HeaderNames.Name)]
    public string Name { get; set; } = "";

    [JsonPropertyName("distance")]
    [SheetOrder(SheetsConfig.HeaderNames.Distance)]
    public int Distance { get; set; }

    [JsonPropertyName("saved")]
    public bool Saved { get; set; }
}