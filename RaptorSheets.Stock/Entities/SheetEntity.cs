using RaptorSheets.Core.Entities;
using System.Text.Json.Serialization;

namespace RaptorSheets.Stock.Entities;

public class SheetEntity : ISheetEntity
{
    [JsonPropertyName("properties")]
    public PropertyEntity Properties { get; set; } = new();

    [JsonPropertyName("sheets")]
    public StockSheets Sheets { get; set; } = new();

    [JsonPropertyName("messages")]
    public List<MessageEntity> Messages { get; set; } = [];
}
