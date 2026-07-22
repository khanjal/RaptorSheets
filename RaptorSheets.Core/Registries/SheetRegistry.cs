using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models;
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
    /// Sheet -> sheets that read from it (reverse of the dependsOn edges passed to <see cref="Register"/>).
    /// Backs <see cref="GetDependents"/>, which drives automatic header-formula refresh of dependent
    /// sheets whenever a sheet they cross-reference is created/healed/changed - see
    /// <see cref="Managers.GoogleSheetManagerBase{TEntity}.RefreshDependentSheetsAsync"/>.
    /// </summary>
    private readonly Dictionary<string, List<string>> _dependents = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Registers a sheet by name with its SheetModel factory (used for header/layout definitions
    /// and missing-sheet creation) and a processor that maps raw row values onto the entity. The
    /// processor is fully domain-owned - it can call RaptorSheets.Core.Mappers.GenericSheetMapper&lt;T&gt;
    /// (see <see cref="SheetRegistryExtensions.RegisterGeneric{TEntity, TRow}"/> for that common case)
    /// or a domain's own hand-rolled mapper; the registry doesn't need to know which.
    /// </summary>
    /// <param name="dependsOn">
    /// Names of sheets whose headers this sheet's formulas cross-reference (e.g. via
    /// <c>otherSheet.GetRange(...)</c> in its mapper's <c>GetSheet()</c>). Sheets named here don't
    /// need to be registered yet/at all - edges are just names until resolved by <see cref="GetDependents"/>.
    /// </param>
    public void Register(string sheetName, Func<SheetModel> sheetModelFactory, Action<TEntity, IList<IList<object>>> processor, IEnumerable<string>? dependsOn = null)
    {
        ArgumentNullException.ThrowIfNull(sheetModelFactory);
        ArgumentNullException.ThrowIfNull(processor);

        _factories[sheetName] = sheetModelFactory;
        _processors[sheetName] = processor;

        foreach (var dependency in dependsOn ?? [])
        {
            if (!_dependents.TryGetValue(dependency, out var dependents))
            {
                dependents = [];
                _dependents[dependency] = dependents;
            }

            if (!dependents.Contains(sheetName, StringComparer.OrdinalIgnoreCase))
            {
                dependents.Add(sheetName);
            }
        }
    }

    public bool IsRegistered(string sheetName) => _factories.ContainsKey(sheetName);

    public IReadOnlyDictionary<string, Func<SheetModel>> Factories => _factories;

    /// <summary>
    /// Returns every sheet that transitively depends on any of <paramref name="changedSheetNames"/>
    /// (per the <c>dependsOn</c> edges passed to <see cref="Register"/>), e.g. if Weekday depends on
    /// Daily and Daily depends on Shift, a changed Shift returns both Daily and Weekday. BFS order;
    /// a changed sheet is never included in its own result even if a cycle exists.
    /// </summary>
    public List<string> GetDependents(IEnumerable<string> changedSheetNames)
    {
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var result = new List<string>();
        var queue = new Queue<string>();

        foreach (var name in changedSheetNames.Where(visited.Add))
        {
            queue.Enqueue(name);
        }

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (!_dependents.TryGetValue(current, out var directDependents))
            {
                continue;
            }

            foreach (var dependent in directDependents.Where(visited.Add))
            {
                result.Add(dependent);
                queue.Enqueue(dependent);
            }
        }

        return result;
    }

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
        foreach (var title in spreadsheet.Sheets.Select(sheet => sheet.Properties.Title))
        {
            if (sheetValues.TryGetValue(title, out var values))
            {
                Process(entity, title, values);
            }
        }

        return entity;
    }

    /// <summary>
    /// Detects columns missing entirely from a batchGet response, reusing the header row already
    /// present in each range - no extra API call needed. SheetId is left at 0 on each result; the
    /// caller fills it in from spreadsheet metadata it already has (e.g. the cheap, no-ranges
    /// GetSheetInfo() call already used for unknown-sheet detection) before acting on it.
    /// </summary>
    public Dictionary<string, List<ColumnInsertionInfo>> DetectMissingColumns(BatchGetValuesByDataFilterResponse? response)
    {
        var missingColumns = new Dictionary<string, List<ColumnInsertionInfo>>();

        if (response?.ValueRanges == null)
        {
            return missingColumns;
        }

        foreach (var matchedValue in response.ValueRanges)
        {
            var sheetName = matchedValue.DataFilters[0].A1Range;
            var values = matchedValue.ValueRange.Values;

            if (values == null || values.Count == 0 || !_factories.TryGetValue(sheetName, out var factory))
            {
                continue;
            }

            HeaderHelpers.CheckSheetHeaders(values[0], factory(), out var insertionInfo);

            if (insertionInfo.Count > 0)
            {
                missingColumns[sheetName] = insertionInfo;
            }
        }

        return missingColumns;
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

    /// <summary>
    /// Checks a spreadsheet's tab names for sheets that don't correspond to any registered sheet.
    /// Unlike <see cref="CheckSheetHeaders"/>, this only needs sheet tab metadata (no grid/cell
    /// data), so it's safe to call with a cheap metadata-only spreadsheet fetch.
    /// </summary>
    public List<MessageEntity> CheckUnknownSheets(Spreadsheet spreadsheet)
    {
        var messages = new List<MessageEntity>();

        if (spreadsheet == null)
        {
            messages.Add(MessageHelpers.CreateErrorMessage("Unable to retrieve sheet(s)", MessageType.GENERAL));
            return messages;
        }

        var sheets = spreadsheet.Sheets ?? [];
        var unknownSheets = sheets.Where(s => !_factories.ContainsKey(s.Properties.Title));

        messages.AddRange(UnknownSheetWarnings(unknownSheets));

        return messages;
    }

    /// <summary>
    /// Full header validation: for every known sheet present in <paramref name="spreadsheet"/>,
    /// checks its actual header row (from grid data) against the registered SheetModel's expected
    /// headers (missing/renamed/reordered columns); for every unrecognized tab, adds a warning.
    /// Requires grid data (IncludeGridData=true) on <paramref name="spreadsheet"/>.
    /// </summary>
    public List<MessageEntity> CheckSheetHeaders(Spreadsheet spreadsheet)
    {
        return CheckSheetHeaders(spreadsheet, out _);
    }

    /// <summary>
    /// Same as <see cref="CheckSheetHeaders(Spreadsheet)"/>, but also reports which columns are
    /// missing entirely and where they should be inserted (per sheet), so a caller can act on it
    /// via <see cref="ColumnInsertionHelper.InsertMissingColumnsAsync{TEntity}"/>.
    /// </summary>
    /// <param name="spreadsheet">Spreadsheet with grid data (IncludeGridData=true)</param>
    /// <param name="missingColumns">Sheet name -> missing columns for that sheet (empty if none found)</param>
    public List<MessageEntity> CheckSheetHeaders(Spreadsheet spreadsheet, out Dictionary<string, List<ColumnInsertionInfo>> missingColumns)
    {
        var messages = new List<MessageEntity>();
        missingColumns = [];

        if (spreadsheet == null)
        {
            messages.Add(MessageHelpers.CreateErrorMessage("Unable to retrieve sheet(s)", MessageType.GENERAL));
            return messages;
        }

        var sheets = spreadsheet.Sheets ?? [];
        var knownSheets = sheets.Where(s => _factories.ContainsKey(s.Properties.Title)).ToList();
        var unknownSheets = sheets.Except(knownSheets);

        var headerMessages = new List<MessageEntity>();

        foreach (var sheet in knownSheets)
        {
            var sheetHeader = HeaderHelpers.GetHeadersFromCellData(sheet.Data?[0]?.RowData?[0]?.Values);

            if (!_factories.TryGetValue(sheet.Properties.Title, out var factory))
            {
                continue;
            }

            headerMessages.AddRange(HeaderHelpers.CheckSheetHeaders(sheetHeader, factory(), out var insertionInfo));

            if (insertionInfo.Count > 0)
            {
                foreach (var info in insertionInfo)
                {
                    info.SheetId = sheet.Properties.SheetId ?? 0;
                }

                missingColumns[sheet.Properties.Title] = insertionInfo;
            }
        }

        messages.AddRange(UnknownSheetWarnings(unknownSheets));

        if (headerMessages.Count > 0)
        {
            messages.Add(MessageHelpers.CreateWarningMessage("Found sheet header issue(s)", MessageType.CHECK_SHEET));
            messages.AddRange(headerMessages);
        }
        else
        {
            messages.Add(MessageHelpers.CreateInfoMessage("No sheet header issues found", MessageType.CHECK_SHEET));
        }

        return messages;
    }

    private static IEnumerable<MessageEntity> UnknownSheetWarnings(IEnumerable<Sheet> unknownSheets)
    {
        foreach (var sheet in unknownSheets)
        {
            yield return MessageHelpers.CreateWarningMessage($"Sheet {sheet.Properties.Title} does not match any known sheet name", MessageType.CHECK_SHEET);
        }
    }

    /// <summary>
    /// Gets the SheetModel (headers/formulas/formats) for a registered sheet by name, or null if unknown.
    /// </summary>
    public SheetModel? GetSheetLayout(string sheetName)
    {
        if (string.IsNullOrEmpty(sheetName))
        {
            return null;
        }

        return _factories.TryGetValue(sheetName, out var factory) ? factory() : null;
    }

    /// <summary>
    /// Gets SheetModels for every requested name that's registered, silently skipping unknown ones.
    /// </summary>
    public List<SheetModel> GetSheetLayouts(IEnumerable<string> sheetNames)
    {
        var models = new List<SheetModel>();

        foreach (var name in sheetNames)
        {
            var model = GetSheetLayout(name);
            if (model != null)
            {
                models.Add(model);
            }
        }

        return models;
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
        Action<TEntity, List<TRow>> assign,
        IEnumerable<string>? dependsOn = null)
        where TEntity : class, ISheetEntity, new()
        where TRow : class, new()
    {
        registry.Register(sheetName, sheetModelFactory, (entity, values) =>
        {
            var headers = values[0];
            entity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headers, sheetModelFactory()));
            assign(entity, Mappers.GenericSheetMapper<TRow>.MapFromRangeData(values));
        }, dependsOn);
    }
}
