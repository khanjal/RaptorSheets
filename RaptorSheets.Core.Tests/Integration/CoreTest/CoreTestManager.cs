using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Logging;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Managers;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Core.Registries;

namespace RaptorSheets.Core.Tests.Integration.CoreTest;

/// <summary>
/// Domain-agnostic manager for the Items/Log/Summary schema in CoreTestEntities.cs, wired the same
/// way a real domain's SheetManager is (registry, SheetGenerationHelper, ChangeSheetDataCoreAsync) so
/// this test suite exercises the exact same code path every domain manager uses - see #100. Declares
/// ISheetManager&lt;CoreTestSheetEntity&gt; explicitly (same as every real domain's own SheetManager)
/// so it satisfies SheetPlumbingTestsBase&lt;TEntity,TManager&gt;'s generic constraint.
/// </summary>
public class CoreTestManager : SheetManagerBase<CoreTestSheetEntity>, ISheetManager<CoreTestSheetEntity>
{
    private static readonly SheetRegistry<CoreTestSheetEntity> s_registry = BuildRegistry();

    public CoreTestManager(Dictionary<string, string> parameters, string spreadsheetId, ILogger? logger = null)
        : base(parameters, spreadsheetId, s_registry, GetSheetNames(), logger)
    {
    }

    public static List<string> GetSheetNames() => [CoreTestSheetNames.Items, CoreTestSheetNames.Log, CoreTestSheetNames.Summary];

    private static SheetRegistry<CoreTestSheetEntity> BuildRegistry()
    {
        var registry = new SheetRegistry<CoreTestSheetEntity>();

        registry.RegisterGeneric<CoreTestSheetEntity, ItemEntity>(CoreTestSheetNames.Items, ItemSheetDefinition.GetSheet, (se, rows) => se.Sheets.Items = rows);
        registry.RegisterGeneric<CoreTestSheetEntity, LogEntity>(CoreTestSheetNames.Log, LogSheetDefinition.GetSheet, (se, rows) => se.Sheets.Log = rows);
        registry.RegisterGeneric<CoreTestSheetEntity, SummaryEntity>(CoreTestSheetNames.Summary, SummarySheetDefinition.GetSheet, (se, rows) => se.Sheets.Summary = rows);

        return registry;
    }

    protected override BatchUpdateSpreadsheetRequest GenerateSheetsRequest(List<string> sheetNames)
    {
        // No header uses Validation in this schema, so the getDataValidation delegate is never
        // actually invoked - see SheetGenerationHelper's doc comment on why it's a delegate at all.
        return SheetGenerationHelper.Generate(sheetNames, GetSheetModel, _ => null);
    }

    private static SheetModel GetSheetModel(string sheet) => sheet switch
    {
        CoreTestSheetNames.Items => ItemSheetDefinition.GetSheet(),
        CoreTestSheetNames.Log => LogSheetDefinition.GetSheet(),
        CoreTestSheetNames.Summary => SummarySheetDefinition.GetSheet(),
        SheetManagerBase.TempSheetName => new SheetModel { Name = SheetManagerBase.TempSheetName },
        _ => throw new NotImplementedException($"Sheet model not found for: {sheet}")
    };

    // Summary is fully formula-driven (no isInput: true columns), so only Items/Log are writable -
    // same "only some sheets are genuinely user-writable" pattern as Stock's Accounts/Tickers.
    //
    // Routes through GoogleRequestHelpers.ChangeSheetData<T> (not CreateUpdateCellRequests directly)
    // so entities with Action = ActionType.DELETE.GetDescription() are actually deleted - matching
    // Gig/Job's own accessor pattern. An earlier version of this file called CreateUpdateCellRequests
    // directly (matching Stock's own accessor, which never needs delete support since Stocks has no
    // delete path either) and silently had no delete support at all as a result.
    private static readonly Dictionary<string, GoogleRequestHelpers.SheetChangeAccessor<CoreTestSheetEntity>> _sheetAccessors =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [CoreTestSheetNames.Items] = new(
                entity => entity.Sheets.Items.Count,
                entity => entity.Sheets.Items,
                (data, properties) => GoogleRequestHelpers.ChangeSheetData(
                    data as List<ItemEntity> ?? [],
                    properties,
                    (entities, props) => GoogleRequestHelpers.CreateUpdateCellRequests(entities, props, GenericSheetMapper<ItemEntity>.MapToRowData))),
            [CoreTestSheetNames.Log] = new(
                entity => entity.Sheets.Log.Count,
                entity => entity.Sheets.Log,
                (data, properties) => GoogleRequestHelpers.ChangeSheetData(
                    data as List<LogEntity> ?? [],
                    properties,
                    (entities, props) => GoogleRequestHelpers.CreateUpdateCellRequests(entities, props, GenericSheetMapper<LogEntity>.MapToRowData))),
        };

    public async Task<CoreTestSheetEntity> ChangeSheetData(List<string> sheets, CoreTestSheetEntity sheetEntity, CancellationToken cancellationToken = default)
    {
        return await ChangeSheetDataCoreAsync(sheets, sheetEntity, _sheetAccessors, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Test-only escape hatch: issues an arbitrary batch request directly via the protected
    /// _googleSheetService field this class inherits, bypassing every normal write path. Used to
    /// simulate a column being manually deleted/corrupted outside the library entirely - exactly the
    /// scenario missing-column self-heal (see CoreSheetsIntegrationTests) exists to recover from.
    /// </summary>
    public async Task<bool> ExecuteRawBatchUpdateAsync(BatchUpdateSpreadsheetRequest request, CancellationToken cancellationToken = default)
    {
        return await _googleSheetService.BatchUpdateSpreadsheet(request, cancellationToken) != null;
    }
}
