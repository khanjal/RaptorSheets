namespace RaptorSheets.Sample.Web.Services;

/// <summary>
/// Every live connection a page can route to, by id or by domain type - real ones from
/// LocalConnectionsStore, plus a synthesized one per typed domain (gig/stock/job/home) that has zero
/// real connections yet, sourced from spreadsheets:test:{domain} in user secrets. That synthesis is
/// what preserves the app's original "always something to look at" behavior (the shared test dataset,
/// with a warning) now that a real connection is something you add explicitly rather than something
/// that's there by default - without it, a fresh clone with only test IDs configured would show
/// nothing until Settings had a real connection added. The synthesized connection's id always starts
/// with "test:" (see SpreadsheetConnection.IsTestFallback) and is never written to connections.json.
/// </summary>
public sealed class ConnectionRegistry(IConfiguration configuration, DomainRegistry domains)
{
    private static readonly string[] TypedDomainTypes = ["gig", "stock", "job", "home"];

    public IReadOnlyList<SpreadsheetConnection> GetAll() => Resolve();

    public SpreadsheetConnection? TryGet(string id) => Resolve().FirstOrDefault(c => c.Id == id);

    public IReadOnlyList<SpreadsheetConnection> GetByType(string type) =>
        Resolve().Where(c => string.Equals(c.Type, type, StringComparison.OrdinalIgnoreCase)).ToList();

    private List<SpreadsheetConnection> Resolve()
    {
        var result = new List<SpreadsheetConnection>(LocalConnectionsStore.GetAll());

        foreach (var type in TypedDomainTypes)
        {
            if (result.Any(c => string.Equals(c.Type, type, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            var testId = configuration[$"spreadsheets:test:{type}"];
            if (string.IsNullOrWhiteSpace(testId))
            {
                continue;
            }

            var label = domains.TryGet(type)?.DomainLabel ?? type;
            result.Add(new SpreadsheetConnection($"test:{type}", type, $"{label} (shared test dataset)", testId));
        }

        return result;
    }
}
