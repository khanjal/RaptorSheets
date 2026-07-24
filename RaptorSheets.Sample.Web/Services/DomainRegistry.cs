namespace RaptorSheets.Sample.Web.Services;

/// <summary>Every domain's ISheetOperations, looked up by route-segment name - NavMenu links to
/// "sheet/{domain}/{sheetName}", Sheet.razor resolves {domain} back through here.</summary>
public sealed class DomainRegistry(IEnumerable<ISheetOperations> operations)
{
    public IReadOnlyList<ISheetOperations> Domains { get; } = operations.ToList();

    public ISheetOperations? TryGet(string domainName) =>
        Domains.FirstOrDefault(d => string.Equals(d.DomainName, domainName, StringComparison.OrdinalIgnoreCase));
}
