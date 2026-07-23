using System.ComponentModel;
using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Services;
using RaptorSheets.Stock.Entities;
using RaptorSheets.Stock.Enums;
using RaptorSheets.Stock.Managers;
using RaptorSheets.Stock.Tests.Data.Attributes;
using RaptorSheets.Stock.Tests.Integration.Base;
using RaptorSheets.Test.Common.Fixtures;
using RaptorSheets.Test.Common.Helpers;
using Xunit;

namespace RaptorSheets.Stock.Tests.Integration;

/// <summary>
/// Integration tests that read back from a live Stock Google Sheet - a dedicated blank test
/// spreadsheet that's safe to delete and recreate freely (distinct from any real portfolio
/// spreadsheet). Sheet creation and demo-data seeding happen once in <see cref="StockCleanSlateFixture"/>
/// before this collection runs. Skipped automatically unless credentials and a Stock spreadsheet
/// ID are configured in user secrets.
/// </summary>
[Collection("StockSheetsIntegration")]
[Category("Integration")]
public class StockSheetsIntegrationTests : IntegrationTestBase
{
    public StockSheetsIntegrationTests(StockCleanSlateFixture fixture) : base(fixture)
    {
    }

    [FactCheckUserSecrets]
    public async Task Environment_ShouldHaveSeededHoldings()
    {
        SkipIfNoCredentials();

        var readBack = await GoogleSheetManager!.GetSheets(TestSheets);

        Assert.NotEmpty(readBack.Sheets.Stocks);
        Assert.All(readBack.Sheets.Stocks, s =>
        {
            Assert.False(string.IsNullOrWhiteSpace(s.Account));
            Assert.False(string.IsNullOrWhiteSpace(s.Ticker));
            // Name is GOOGLEFINANCE-derived from this row's own Ticker - should resolve for real tickers
            Assert.False(string.IsNullOrWhiteSpace(s.Name));
        });

        // Accounts/Tickers reference sheets are SORT/UNIQUE-derived from Stocks
        Assert.NotEmpty(readBack.Sheets.Accounts);
        Assert.NotEmpty(readBack.Sheets.Tickers);

        var seededAccounts = readBack.Sheets.Stocks.Select(s => s.Account).Distinct().ToList();
        Assert.All(seededAccounts, account =>
            Assert.Contains(readBack.Sheets.Accounts, a => string.Equals(a.Account, account, StringComparison.OrdinalIgnoreCase)));

        var seededTickers = readBack.Sheets.Stocks.Select(s => s.Ticker).Distinct().ToList();
        Assert.All(seededTickers, ticker =>
            Assert.Contains(readBack.Sheets.Tickers, t => string.Equals(t.Ticker, ticker, StringComparison.OrdinalIgnoreCase)));
    }
}

/// <summary>
/// Collection definition for Stock Google Sheets integration tests. Every Stock integration test
/// class - including the MapFromRangeData mapper tests, which consume <see cref="StockCleanSlateFixture.ValueRanges"/> -
/// belongs to this collection so nothing runs concurrently with the fixture's delete/recreate/seed
/// sequence.
/// </summary>
[CollectionDefinition("StockSheetsIntegration")]
public class StockSheetsIntegrationCollection : ICollectionFixture<StockCleanSlateFixture>
{
}

/// <summary>
/// Stock's clean-slate integration fixture (see <see cref="CleanSlateSheetFixture{TEntity,TManager}"/>).
/// Deletes and recreates every canonical sheet, seeds realistic demo holdings, then captures a
/// batch-data snapshot for the MapFromRangeData mapper tests to map against directly without each
/// doing their own live fetch. Safe because spreadsheets:stock is configured to point at a dedicated
/// blank test spreadsheet, not a real portfolio.
/// </summary>
public class StockCleanSlateFixture : CleanSlateSheetFixture<SheetEntity, GoogleSheetManager>
{
    /// <summary>
    /// Raw batch-get value ranges captured after seeding, for MapFromRangeData tests to map
    /// against directly without each doing their own live fetch.
    /// </summary>
    public IList<MatchedValueRange>? ValueRanges { get; private set; }

    public StockCleanSlateFixture() : base(
        TestConfigurationHelpers.GetStockSpreadsheet(),
        (credential, spreadsheetId) => new GoogleSheetManager(credential, spreadsheetId),
        manager => manager.PopulateDemoData(seed: 42))
    {
    }

    protected override async Task AfterSetupAsync()
    {
        var googleSheetService = new GoogleSheetService(Credential, SpreadsheetId);
        var sheetNames = Enum.GetValues(typeof(SheetName)).Cast<SheetName>().Select(x => x.GetDescription()).ToList();
        var result = await googleSheetService.GetBatchData(sheetNames);
        ValueRanges = result?.ValueRanges;
    }
}
