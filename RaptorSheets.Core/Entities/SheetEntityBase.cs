using System.Text.Json.Serialization;

namespace RaptorSheets.Core.Entities;

/// <summary>
/// Common shape for every domain's top-level SheetEntity (Gig, Stock, and future domains):
/// Properties/Messages (the pieces Core's generic orchestration - see
/// RaptorSheets.Core.Registries.SheetRegistry - reasons about via <see cref="ISheetEntity"/>) plus
/// a domain-typed <see cref="Sheets"/> container holding the actual row collections
/// (Trips/Accounts/etc.). <typeparamref name="TSheets"/> is domain-specific and unknown to Core -
/// this base class exists purely so each domain doesn't have to re-declare this boilerplate.
/// Nesting row collections under <see cref="Sheets"/> rather than flattening them onto the entity
/// itself means a domain sheet can never collide with the reserved Properties/Messages members.
/// </summary>
public abstract class SheetEntityBase<TSheets> : ISheetEntity where TSheets : new()
{
    [JsonPropertyName("properties")]
    public PropertyEntity Properties { get; set; } = new();

    [JsonPropertyName("sheets")]
    public TSheets Sheets { get; set; } = new();

    [JsonPropertyName("messages")]
    public List<MessageEntity> Messages { get; set; } = [];
}
