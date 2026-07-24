using RaptorSheets.Core.Utilities;
using RaptorSheets.Gig.Managers;

namespace RaptorSheets.Sample.Web.Services;

/// <summary>
/// Short-TTL cache for reference-sheet identity values (the lists dropdowns are built from).
/// These sheets are read-only in the app and change rarely, so re-fetching them on every page
/// navigation is wasted API calls - this only ever caches reads; writes always go straight to the
/// live spreadsheet. Registered as a singleton so the cache is shared across every page/circuit
/// rather than re-fetched per user session.
/// </summary>
public class ReferenceSheetCache
{
    private static readonly TimeSpan Ttl = TimeSpan.FromSeconds(60);

    private readonly Dictionary<string, (IReadOnlyList<string> Values, DateTime ExpiresAt)> _cache = [];
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<Dictionary<string, IReadOnlyList<string>>> GetIdentityValuesAsync(
        IGoogleSheetManager manager,
        IReadOnlyList<SheetDescriptor> referenceDescriptors,
        CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<string, IReadOnlyList<string>>();
        List<SheetDescriptor>? stale = null;

        await _lock.WaitAsync(cancellationToken);
        try
        {
            var now = DateTime.UtcNow;
            foreach (var descriptor in referenceDescriptors)
            {
                if (_cache.TryGetValue(descriptor.Name, out var entry) && entry.ExpiresAt > now)
                {
                    result[descriptor.Name] = entry.Values;
                }
                else
                {
                    (stale ??= []).Add(descriptor);
                }
            }
        }
        finally
        {
            _lock.Release();
        }

        if (stale is null)
        {
            return result;
        }

        var sheetEntity = await manager.GetSheets(stale.Select(d => d.Name).ToList(), cancellationToken);
        var expiresAt = DateTime.UtcNow + Ttl;

        await _lock.WaitAsync(cancellationToken);
        try
        {
            foreach (var descriptor in stale)
            {
                var columns = TypedFieldUtils.GetColumnProperties(descriptor.RowType);
                if (columns.Count == 0)
                {
                    continue;
                }

                // Every reference entity declares its identity value as its first column
                // (e.g. ServiceEntity.Service, AddressEntity.Address) - see the entities themselves.
                var identityProperty = columns[0].Property;
                var values = descriptor.GetRows(sheetEntity.Sheets)
                    .Cast<object>()
                    .Select(r => identityProperty.GetValue(r)?.ToString())
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .Distinct()
                    .OrderBy(v => v, StringComparer.OrdinalIgnoreCase)
                    .Cast<string>()
                    .ToList();

                _cache[descriptor.Name] = (values, expiresAt);
                result[descriptor.Name] = values;
            }
        }
        finally
        {
            _lock.Release();
        }

        return result;
    }
}
