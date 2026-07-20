namespace RaptorSheets.Core.Entities;

/// <summary>
/// Minimal shape every domain's top-level SheetEntity (Gig, Stock, and future domains) already
/// shares. Lets Core provide generic orchestration (see RaptorSheets.Core.Registries.SheetRegistry)
/// over the common Properties/Messages fields without knowing anything about a domain's own
/// strongly-typed row collections (Trips, Accounts, etc.).
/// </summary>
public interface ISheetEntity
{
    PropertyEntity Properties { get; set; }
    List<MessageEntity> Messages { get; set; }
}
