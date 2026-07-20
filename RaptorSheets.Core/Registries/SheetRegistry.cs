using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;

namespace RaptorSheets.Core.Registries;

/// <summary>
/// Generic orchestration shared by every domain's sheet-helper class (Gig, Stock, and future
/// domains): "given a sheet name, look up its SheetModel/expected headers and its row-mapping
/// delegate, then walk a batchGet response or Spreadsheet and apply them." Each domain package
/// still owns its own strongly-typed entities and mapper delegates - this only removes the
/// duplicated dictionary-plus-loop plumbing (and the near-identical MapData/GetMissingSheets
/// implementations) that would otherwise be hand-rolled per domain.
/// </summary>
/// <typeparam name="TEntity">The domain's top-level SheetEntity type</typeparam>
public class SheetRegistry<TEntity> where TEntity : class, ISheetEntity, new()
{
    private readonly Dictionary<string, Func<SheetModel>> _factories = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Action<TEntity, IList<IList<object>>>> _processors = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Registers a sheet by name with its SheetModel factory (used for header/layout definitions
    /// and missing-sheet creation) and a processor that maps raw row values onto the entity. The
    /// processor is fully domain-owned - it can call RaptorSheets.Core.Mappers.GenericSheetMapper&lt;T&gt;
    /// (see <see cref="SheetRegistryExtensions.RegisterGeneric{TEntity, TRow}"/> for that common case)
    /// or a domain's own hand-rolled mapper; the registry doesn't need to know which.
    /// </summary>
    public void Register(string sheetName, Func<SheetModel> sheetModelFactory, Action<TEntity, IList<IList<object>>> processor)
    {
        ArgumentNullException.ThrowIfNull(sheetModelFactory);
        ArgumentNullException.ThrowIfNull(processor);

        _factories[sheetName] = sheetModelFactory;
        _processors[sheetName] = processor;
    }

    public bool IsRegistered(string sheetName) => _factories.ContainsKey(sheetName);

    public IReadOnlyDictionary<string, Func<SheetModel>> Factories => _factories;

    /// <summary>
    /// Builds an entity from a batchGet response, dispatching each returned range to its
    /// registered processor by sheet name (taken from the range's DataFilter). Returns null if
    /// the response has no ranges to process (mirrors the historical per-domain behavior).
    /// </summary>
    public TEntity? MapData(BatchGetValuesByDataFilterResponse? response)
    {
        if (response?.ValueRanges == null)
        {
            return null;
        }

        var entity = new TEntity();

        foreach (var matchedValue in response.ValueRanges)
        {
            var sheetName = matchedValue.DataFilters[0].A1Range;
            var values = matchedValue.ValueRange.Values;
            Process(entity, sheetName, values);
        }

        return entity;
    }

    /// <summary>
    /// Builds an entity from a full Spreadsheet (grid-data) response.
    /// </summary>
    public TEntity MapData(Spreadsheet spreadsheet)
    {
        var entity = new TEntity();
        entity.Properties.Name = spreadsheet.Properties.Title;

        var sheetValues = SheetHelpers.GetSheetValues(spreadsheet);
        foreach (var sheet in spreadsheet.Sheets)
        {
            if (sheetValues.TryGetValue(sheet.Properties.Title, out var values))
            {
                Process(entity, sheet.Properties.Title, values);
            }
        }

        return entity;
    }

    private void Process(TEntity entity, string sheetName, IList<IList<object>> values)
    {
        if (values == null || values.Count == 0)
        {
            return;
        }

        if (_processors.TryGetValue(sheetName, out var processor))
        {
            processor(entity, values);
        }
    }

    /// <summary>
    /// Returns SheetModels for every registered sheet in <paramref name="canonicalSheetNames"/> that
    /// isn't already present (by title, case-insensitive) in <paramref name="spreadsheet"/>.
    /// </summary>
    public List<SheetModel> GetMissingSheets(Spreadsheet spreadsheet, IEnumerable<string> canonicalSheetNames)
    {
        var existingTitles = spreadsheet.Sheets.Select(x => x.Properties.Title).ToList();
        var missing = new List<SheetModel>();

        foreach (var name in canonicalSheetNames)
        {
            if (existingTitles.Any(s => string.Equals(s, name, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            if (_factories.TryGetValue(name, out var factory))
            {
                missing.Add(factory());
            }
        }

        return missing;
    }
}

/// <summary>
/// Convenience registration for the common case where a sheet's rows map through the generic,
/// attribute-driven <see cref="RaptorSheets.Core.Mappers.GenericSheetMapper{T}"/> rather than a
/// hand-rolled mapper. Still fully strongly-typed - TRow is a real entity class, not a generic cell/row.
/// </summary>
public static class SheetRegistryExtensions
{
    public static void RegisterGeneric<TEntity, TRow>(
        this SheetRegistry<TEntity> registry,
        string sheetName,
        Func<SheetModel> sheetModelFactory,
        Action<TEntity, List<TRow>> assign)
        where TEntity : class, ISheetEntity, new()
        where TRow : class, new()
    {
        registry.Register(sheetName, sheetModelFactory, (entity, values) =>
        {
            var headers = values[0];
            entity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headers, sheetModelFactory()));
            assign(entity, Mappers.GenericSheetMapper<TRow>.MapFromRangeData(values));
        });
    }
}
