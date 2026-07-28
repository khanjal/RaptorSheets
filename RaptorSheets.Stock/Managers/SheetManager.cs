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
    private static List<string> CanonicalSheetNames()
        => Enum.GetValues<SheetName>().Select(e => e.GetDescription()).ToList();

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

    // Only the Stocks sheet is genuinely user-writable today (Ticker/Account/Shares - see
    // StockSheet.MapToRowData) - Accounts and Tickers are fully formula/GOOGLEFINANCE-driven
    // rollups, so they get no accessor entry (same as Gig's read-only summary sheets -
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
    /// by their own formulas, so only the Stocks sheet needs to be written.
    /// </summary>
    public async Task<SheetEntity> PopulateDemoData(int? seed = null, CancellationToken cancellationToken = default)
    {
        var demoData = GenerateDemoData(seed);
        await ChangeSheetData([SheetName.STOCKS.GetDescription()], demoData, cancellationToken);

        // Inserting new tickers triggers Tickers' GOOGLEFINANCE-driven columns to recompute at the
        // same moment Stocks' own formulas (which read from Tickers) re-evaluate. GOOGLEFINANCE
        // resolves asynchronously, so both sheets can latch onto a transient #N/A mid-flight and
        // never self-recover on their own - Tickers' own MaxHigh/MinLow (a GOOGLEFINANCE historical
        // daily-range call, evaluated once per ticker via MAP/LAMBDA) is the slowest to settle, so a
        // single short delay isn't reliably enough. Re-apply both sheets' header formulas (inherited
        // from SheetManagerBase) twice, with increasing delays, to force a clean re-evaluation
        // against settled data.
        var sheetNames = new[] { SheetName.TICKERS.GetDescription(), SheetName.STOCKS.GetDescription() };

        await Task.Delay(5000, cancellationToken);
        await RefreshHeaderFormulasAsync(sheetNames, cancellationToken: cancellationToken);

        await Task.Delay(15000, cancellationToken);
        await RefreshHeaderFormulasAsync(sheetNames, cancellationToken: cancellationToken);

        return demoData;
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
