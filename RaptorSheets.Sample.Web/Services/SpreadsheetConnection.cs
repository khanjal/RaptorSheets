using System.Text.Json.Serialization;

namespace RaptorSheets.Sample.Web.Services;

/// <summary>
/// One user-managed spreadsheet reference: a domain <see cref="Type"/> ("gig"/"stock"/"job"/"home",
/// matching <see cref="ISheetOperations.DomainName"/>, or "generic" for a non-strongly-typed sheet),
/// a display <see cref="Label"/> (needed since <see cref="Type"/> alone no longer identifies a single
/// connection - multiple of the same type are allowed), and the spreadsheet itself. Persisted via
/// <see cref="LocalConnectionsStore"/>, never in user secrets.
/// </summary>
public sealed record SpreadsheetConnection(string Id, string Type, string Label, string SpreadsheetId)
{
    /// <summary>
    /// True for a connection synthesized by <see cref="ConnectionRegistry"/> from
    /// spreadsheets:test:{domain} (id "test:{domain}") rather than a real entry in
    /// <see cref="LocalConnectionsStore"/> - never persisted, so this is derived from the id
    /// convention rather than stored, and can't drift out of sync with how it was created.
    /// </summary>
    [JsonIgnore]
    public bool IsTestFallback => Id.StartsWith("test:", StringComparison.Ordinal);

    /// <summary>
    /// Extra tab names Sheet Inspector offers for this connection beyond whatever's actually
    /// live on the spreadsheet right now - either a hand-typed custom name, or one borrowed from
    /// another domain's known sheet names (e.g. trying "Trips" on a Generic connection before it
    /// exists there). Purely a Sheet Inspector convenience list - never consulted by typed CRUD,
    /// which only ever looks at ITypedConnectedSheet's fixed Sheets container. A settable property
    /// rather than a positional constructor parameter so every existing call site that builds a
    /// SpreadsheetConnection positionally (throwaway connections, the test-fallback synthesis in
    /// ConnectionRegistry) keeps compiling unchanged and gets the empty-list default; System.Text.Json
    /// deserializes older connections.json entries that predate this field the same way.
    /// </summary>
    public IReadOnlyList<string> CustomSheetNames { get; init; } = [];
}
