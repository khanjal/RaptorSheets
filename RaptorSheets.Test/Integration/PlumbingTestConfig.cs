using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Entities;

namespace RaptorSheets.Test.Common.Integration;

/// <summary>
/// The small "feed it the sheets and data" adapter each domain supplies to
/// <see cref="SheetPlumbingTestsBase{TEntity, TManager}"/>. Everything the shared plumbing scenarios
/// need that can't be derived generically from <see cref="RaptorSheets.Core.Managers.ISheetManager{TEntity}"/>
/// lives here - which sheet/column is safe to delete or corrupt, how to build one recognizable test
/// row, and how long this domain's formulas take to settle after a write (Core's plain SUMIF/COUNTIF
/// settle in ~2s; Stock's GOOGLEFINANCE-driven columns need far longer - see SheetManager.PopulateDemoData).
/// </summary>
public class PlumbingTestConfig<TEntity> where TEntity : class, ISheetEntity, new()
{
    /// <summary>A real, genuinely user-writable sheet (e.g. Gig's Trips, Stock's Stocks, Core's Items).</summary>
    public required string InputSheetName { get; init; }

    /// <summary>
    /// A plain input column on <see cref="InputSheetName"/> safe to delete and let self-heal restore -
    /// ideally one with a custom Format and/or Note so restoration is actually verifiable.
    /// </summary>
    public required string TestColumnName { get; init; }

    /// <summary>
    /// Optional formula sheet that references <see cref="InputSheetName"/> (e.g. Core's Summary,
    /// Stock's Accounts). When set, its canonical headers are assumed to follow this codebase's
    /// established rollup convention: the first header (index 0) is the key/category column (SORT/
    /// UNIQUE-driven), every other header is a computed formula column. Dependent-sheet scenarios are
    /// skipped entirely when this is null.
    /// </summary>
    public string? DependentSheetName { get; init; }

    /// <summary>Builds one recognizable, valid row (at the given RowId) for <see cref="InputSheetName"/>.</summary>
    public required Func<int, TEntity> BuildTestRow { get; init; }

    /// <summary>Whether a round-tripped entity still contains the row built by <see cref="BuildTestRow"/> for the given RowId.</summary>
    public required Func<TEntity, int, bool> ContainsTestRow { get; init; }

    /// <summary>
    /// Test-only escape hatch to issue a raw batch update directly against the spreadsheet, bypassing
    /// every normal write path - used to simulate a column/sheet being manually deleted, reordered, or
    /// added to outside the library entirely. Each domain backs this with a thin test-only manager
    /// subclass reaching its own protected IGoogleSheetService (see CoreTestManager.ExecuteRawBatchUpdateAsync
    /// for the pattern); production domain managers are never touched.
    /// </summary>
    public required Func<BatchUpdateSpreadsheetRequest, CancellationToken, Task<bool>> ExecuteRawBatchUpdateAsync { get; init; }

    /// <summary>
    /// Optional richer reseed a domain can plug in for after a test that wipes the whole spreadsheet
    /// (e.g. Core's own large randomized Items/Log dataset). When null, the shared base falls back to
    /// writing a handful of rows via <see cref="BuildTestRow"/> so the sheet is left non-trivially
    /// populated rather than richly seeded.
    /// </summary>
    public Func<CancellationToken, Task>? BulkReseedAsync { get; init; }

    /// <summary>How long to wait after a write for this domain's formulas to settle before reading back.</summary>
    /// <summary>
    /// How long to wait after a write before reading values back. This is the expensive one: Stock
    /// sets it to 20s because its GOOGLEFINANCE-driven columns recompute far more slowly than plain
    /// SUMIF/COUNTIF, and a read issued too early sees stale or empty values.
    /// </summary>
    public TimeSpan SettleDelay { get; init; } = TimeSpan.FromSeconds(2);

    /// <summary>
    /// How long to wait after a purely structural change - creating or deleting a sheet, inserting,
    /// moving or deleting a column, or cleaning one up. Google only has to make metadata consistent
    /// here; nothing is recomputed, so the long GOOGLEFINANCE settle buys nothing.
    ///
    /// Kept separate because applying <see cref="SettleDelay"/> everywhere made Stock's suite take
    /// 11 minutes of the ~18 the whole live suite needed, most of it spent waiting for a
    /// recalculation that no following assertion actually read.
    /// </summary>
    public TimeSpan StructureSettleDelay { get; init; } = TimeSpan.FromSeconds(2);
}
