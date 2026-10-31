using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Logging;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Services;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Managers;
using RaptorSheets.Core.Models;
using RaptorSheets.Stock.Entities;
using RaptorSheets.Stock.Enums;
using RaptorSheets.Stock.Helpers;
using RaptorSheets.Core.Models.Google;
using SheetName = RaptorSheets.Stock.Enums.SheetName;

namespace RaptorSheets.Stock.Managers;

/// <summary>
/// Extends the shared <see cref="ISheetManager{TEntity}"/> CRUD/metadata/layout surface with
/// Stock's own demo-data generation (seed only). Stock's concrete manager already implements the
/// metadata members (GetAllSheetProperties, GetSpreadsheetInfo, etc.) via
/// <see cref="SheetManagerBase{TEntity}"/>, same as every other domain - this interface
/// previously just didn't declare them.
/// </summary>
public interface ISheetManager : ISheetManager<SheetEntity>
{
    // Demo Data Generation
    Task<SheetEntity> SetupDemo(int? seed = null, CancellationToken cancellationToken = default);
    Task<SheetEntity> PopulateDemoData(int? seed = null, CancellationToken cancellationToken = default);
    SheetEntity GenerateDemoData(int? seed = null);
}

public class SheetManager : SheetManagerBase<SheetEntity>, ISheetManager
{
    private static List<string> CanonicalSheetNames() => StockSheetHelpers.GetSheetNames();

    public SheetManager(IGoogleSheetService googleSheetService, ILogger? logger = null)
        : base(googleSheetService, StockSheetHelpers.Registry, CanonicalSheetNames(), logger)
    {
    }

    public SheetManager(string accessToken, string spreadsheetId, ILogger? logger = null)
        : base(accessToken, spreadsheetId, StockSheetHelpers.Registry, CanonicalSheetNames(), logger)
    {
    }

    public SheetManager(Dictionary<string, string> parameters, string spreadsheetId, ILogger? logger = null)
        : base(parameters, spreadsheetId, StockSheetHelpers.Registry, CanonicalSheetNames(), logger)
    {
    }

    /// <summary>
    /// Restores sheets found missing entirely during <see cref="SheetManagerBase{TEntity}.GetSheets"/>
    /// self-heal, delegating straight to the base's string-keyed, index-ordered creation.
    /// </summary>
    protected override async Task<SheetEntity> CreateMissingSheetsAsync(Dictionary<string, int> missingIndexMap, CancellationToken cancellationToken = default)
    {
        return await CreateSheets(missingIndexMap.Keys.ToList(), missingIndexMap, cancellationToken);
    }

    /// <summary>
    /// Backs <see cref="SheetManagerBase{TEntity}.CreateSheets"/> and
    /// <see cref="SheetManagerBase{TEntity}.DeleteSheets"/> (for temp-sheet creation) with
    /// Stock's fully-configured AddSheet requests.
    /// </summary>
    protected override BatchUpdateSpreadsheetRequest GenerateSheetsRequest(List<string> sheetNames)
    {
        return GenerateSheetHelpers.Generate(sheetNames);
    }

    /// <summary>
    /// Resolves a self-healed column's raw Validation name into a concrete data validation rule,
    /// restoring dropdowns on a re-inserted column (GitHub issue #103). Stock's resolver takes no
    /// range parameter - none of its validations are self-referential.
    /// </summary>
    protected override DataValidationRule? GetDataValidation(ColumnInsertionInfo column)
    {
        return string.IsNullOrEmpty(column.Validation)
            ? null
            : StockSheetHelpers.GetDataValidation(column.Validation.GetValueFromName<Validation>());
    }

    // Only the Stocks sheet is genuinely user-writable today (Ticker/Account/Shares - the only
    // isInput: true columns on StockEntity) - Accounts and Tickers are fully formula/GOOGLEFINANCE-
    // driven rollups, so they get no accessor entry (same as Gig's read-only summary sheets -
    // Daily/Weekly/Monthly/Yearly - having none).
    private static readonly Dictionary<string, GoogleRequestHelpers.SheetChangeAccessor<SheetEntity>> _sheetAccessors =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [SheetName.STOCKS.GetDescription()] = new(
                entity => entity.Sheets.Stocks.Count,
                entity => entity.Sheets.Stocks,
                (data, properties) => StockRequestHelpers.ChangeStockSheetData(data as List<StockEntity> ?? [], properties))
        };

    public async Task<SheetEntity> ChangeSheetData(List<string> sheets, SheetEntity sheetEntity, CancellationToken cancellationToken = default)
    {
        return await ChangeSheetDataCoreAsync(sheets, sheetEntity, _sheetAccessors, cancellationToken: cancellationToken);
    }

    #region Demo Data Generation

    /// <summary>
    /// Creates all sheets and then fills the Stocks sheet with realistic demo holdings.
    /// </summary>
    public async Task<SheetEntity> SetupDemo(int? seed = null, CancellationToken cancellationToken = default)
    {
        await CreateAllSheets(cancellationToken);
        await Task.Delay(1500, cancellationToken); // let freshly-created sheets become writable
        return await PopulateDemoData(seed, cancellationToken);
    }

    /// <summary>
    /// Writes generated demo holdings into the Stocks sheet. Accounts/Tickers reference sheets and
    /// every financial column (Name, AverageCost, CostTotal, CurrentPrice, ...) are auto-populated
    /// by their own formulas, so only the Stocks sheet needs to be written. Returns once those
    /// formulas have settled, or with a warning in Messages if that couldn't be confirmed in time.
    /// </summary>
    public async Task<SheetEntity> PopulateDemoData(int? seed = null, CancellationToken cancellationToken = default)
    {
        var started = Clock.GetTimestamp();
        var demoData = GenerateDemoData(seed);
        await ChangeSheetData([SheetName.STOCKS.GetDescription()], demoData, cancellationToken);

        // Nothing was written, so there's nothing to settle - the write's own error says why.
        if (demoData.Messages.Exists(m => m.Level == MessageLevel.ERROR.GetDescription()))
        {
            return demoData;
        }

        var lastState = await SettleDemoFormulasAsync(started, cancellationToken);
        if (lastState != SettleState.Settled)
        {
            var seen = lastState == SettleState.Latched ? "some still showed a formula error" : "they hadn't finished loading or couldn't be read";
            demoData.Messages.Add(MessageHelpers.CreateWarningMessage(
                $"Demo data written, but its GOOGLEFINANCE-driven cells couldn't be confirmed settled within {SettleBudget.TotalSeconds:0}s ({seen}) - they may need a manual refresh",
                MessageType.ADD_DATA));
        }

        return demoData;
    }

    // Inserting new tickers makes Tickers' GOOGLEFINANCE columns recompute at the same moment
    // Stocks' own formulas (which read from Tickers) re-evaluate. GOOGLEFINANCE resolves
    // asynchronously, so cells on both sheets latch onto a transient #N/A and never recover on
    // their own - verified live: untouched, they were still #N/A 30s later; one header-formula
    // refresh cleared every one of them within a second. So: wait briefly, refresh, then poll
    // until clean, refreshing again if a refresh came too early and the errors re-latched.
    // Verified live, the common case now returns ~7s after the write instead of a fixed ~21s.
    //
    // Callers run behind a ~30s API Gateway ceiling, and under throttling a single call can spend
    // seconds in retry backoff, so the loop is bounded by wall-clock time from the start of
    // PopulateDemoData (the write included), not by a poll count: nothing new - poll or refresh -
    // starts once its wait would end past SettleBudget. What can still overrun it is the one call
    // already in flight at that moment.
    internal static readonly TimeSpan SettleFirstRefreshDelay = TimeSpan.FromSeconds(3);
    internal static readonly TimeSpan SettlePollInterval = TimeSpan.FromSeconds(2.5);
    internal static readonly TimeSpan SettleBudget = TimeSpan.FromSeconds(18);
    internal const int SettlePollsBeforeRefreshingAgain = 2;

    /// <summary>Test seams: let unit tests run the settle loop without really sleeping.</summary>
    internal Func<TimeSpan, CancellationToken, Task> Delay { get; set; } = Task.Delay;
    internal TimeProvider Clock { get; set; } = TimeProvider.System;

    internal enum SettleState { Settled, Pending, Latched }

    private async Task<SettleState> SettleDemoFormulasAsync(long started, CancellationToken cancellationToken)
    {
        var sheetNames = new List<string> { SheetName.TICKERS.GetDescription(), SheetName.STOCKS.GetDescription() };
        bool Fits(TimeSpan wait) => Clock.GetElapsedTime(started) + wait <= SettleBudget;

        await Delay(SettleFirstRefreshDelay, cancellationToken);

        // Sheet ids don't change between refreshes, so fetch the metadata once rather than per refresh.
        var spreadsheetInfo = await _googleSheetService.GetSheetInfo(sheetNames, cancellationToken);
        await RefreshHeaderFormulasAsync(sheetNames, spreadsheetInfo, cancellationToken);

        var state = SettleState.Pending;
        var pollsSinceRefresh = 0;
        while (Fits(SettlePollInterval))
        {
            await Delay(SettlePollInterval, cancellationToken);
            var result = await _googleSheetService.GetBatchDataResult(sheetNames, cancellationToken: cancellationToken);

            // A failed read tells us nothing either way - keep waiting rather than refreshing blind.
            state = result.Success ? GetSettleState(result.Value, sheetNames) : SettleState.Pending;
            if (state == SettleState.Settled)
            {
                return state;
            }

            pollsSinceRefresh++;

            // A refresh only helps if there's still time to poll after it and see the result.
            if (state == SettleState.Latched && pollsSinceRefresh >= SettlePollsBeforeRefreshingAgain && Fits(SettlePollInterval * 2))
            {
                await RefreshHeaderFormulasAsync(sheetNames, spreadsheetInfo, cancellationToken);
                pollsSinceRefresh = 0;
            }
        }

        return state;
    }

    /// <summary>
    /// Latched if any cell shows a formula error (#N/A, #REF!, ...) - including the header row,
    /// where a whole-column ARRAYFORMULA that failed outright shows its error instead of its title.
    /// Pending if any cell still reads "Loading...", or if any of <paramref name="sheetNames"/> came
    /// back missing or with no data rows yet (Tickers is itself a SORT/UNIQUE pull from Stocks, so
    /// it starts out empty). Otherwise Settled.
    /// </summary>
    internal static SettleState GetSettleState(BatchGetValuesByDataFilterResponse? response, IEnumerable<string> sheetNames)
    {
        var valueRanges = response?.ValueRanges ?? [];
        var state = SettleState.Settled;

        foreach (var sheetName in sheetNames)
        {
            var rows = valueRanges
                .FirstOrDefault(v => v.DataFilters?.Any(f => string.Equals(f.A1Range, sheetName, StringComparison.OrdinalIgnoreCase)) == true)
                ?.ValueRange?.Values;
            if (rows == null || !rows.Skip(1).Any(row => row.Any(cell => !string.IsNullOrWhiteSpace(cell?.ToString()))))
            {
                state = SettleState.Pending;
                continue;
            }

            foreach (var value in rows.SelectMany(row => row).Select(cell => cell?.ToString() ?? ""))
            {
                if (value.StartsWith('#'))
                {
                    return SettleState.Latched;
                }

                if (value.StartsWith("Loading", StringComparison.OrdinalIgnoreCase))
                {
                    state = SettleState.Pending;
                }
            }
        }

        return state;
    }

    /// <summary>
    /// Generates a handful of realistic demo holdings (real, well-known ticker symbols across a
    /// few demo accounts) without writing them to any spreadsheet. RowIds start at 2 so a
    /// subsequent write lands them below the header row. Real tickers are used deliberately so the
    /// Stocks sheet's GOOGLEFINANCE-driven columns (Name, CurrentPrice, 52-week high/low, ...)
    /// resolve to real market data instead of #N/A.
    /// </summary>
    public SheetEntity GenerateDemoData(int? seed = null)
    {
        // SonarQube S2245: Using Random is safe here - this generates demo/sample data, not security-sensitive values
#pragma warning disable S2245
        var random = seed.HasValue ? new Random(seed.Value) : Random.Shared;
#pragma warning restore S2245

        var sheetEntity = new SheetEntity();

        var tickers = new[] { "AAPL", "MSFT", "GOOGL", "AMZN", "NVDA", "TSLA", "META", "JPM", "V", "KO" };
        var accounts = new[] { "Test Brokerage", "Test 401k", "Test Roth IRA" };

        // Pick a handful of distinct Account+Ticker combos so the Accounts/Tickers reference
        // sheets have something meaningful to derive via their SORT/UNIQUE formulas.
        var holdingCount = Math.Min(8, tickers.Length * accounts.Length);
        var combos = new List<(string Account, string Ticker)>();
        while (combos.Count < holdingCount)
        {
            var combo = (Account: accounts[random.Next(accounts.Length)], Ticker: tickers[random.Next(tickers.Length)]);
            if (!combos.Contains(combo))
            {
                combos.Add(combo);
            }
        }

        var rowId = 2; // start at row 2 (row 1 reserved for headers)
        foreach (var (account, ticker) in combos)
        {
            sheetEntity.Sheets.Stocks.Add(new StockEntity
            {
                RowId = rowId++,
                Account = account,
                Ticker = ticker,
                Shares = Math.Round((decimal)(random.NextDouble() * 49 + 1), 2) // 1-50 shares
            });
        }

        return sheetEntity;
    }

    #endregion
}
