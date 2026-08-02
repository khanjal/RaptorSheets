using RaptorSheets.Core.Models.Google;

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

    /// <summary>
    /// Live-read sheet structure (headers, formats, validation, notes, ...), keyed by sheet name.
    /// Only populated when a caller opts in (e.g. <c>GetSheets(sheets, includeStructure: true, ...)</c>
    /// or <c>GetLiveSheetStructure(s)</c>) - empty otherwise.
    /// </summary>
    Dictionary<string, SheetModel> Structures { get; set; }
}
