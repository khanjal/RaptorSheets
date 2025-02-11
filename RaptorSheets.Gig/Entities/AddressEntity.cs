using System.Text.Json.Serialization;

namespace RaptorSheets.Gig.Entities;

public class AddressEntity : AmountEntity
{
    [JsonPropertyName("rowId")]
    public int RowId { get; set; }

    [JsonPropertyName("address")]
    public string Address { get; set; } = "";

    [JsonPropertyName("visits")]
    public int Visits { get; set; }

    [JsonPropertyName("distance")]
    public decimal Distance { get; set; }
}