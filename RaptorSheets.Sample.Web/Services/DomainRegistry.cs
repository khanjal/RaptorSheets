namespace RaptorSheets.Sample.Web.Services;

/// <summary>Every domain's ISheetOperations, looked up by route-segment name - NavMenu links to
/// "sheet/{domain}/{sheetName}", Sheet.razor resolves {domain} back through here.</summary>
public sealed class DomainRegistry(IEnumerable<ISheetOperations> operations)
{
    // Alphabetical by display label, not DI registration order - so the nav lists domains
    // predictably regardless of the order Program.cs happens to register them in.
    public IReadOnlyList<ISheetOperations> Domains { get; } =
        operations.OrderBy(o => o.DomainLabel, StringComparer.OrdinalIgnoreCase).ToList();

    public ISheetOperations? TryGet(string domainName) =>
        Domains.FirstOrDefault(d => string.Equals(d.DomainName, domainName, StringComparison.OrdinalIgnoreCase));
}
