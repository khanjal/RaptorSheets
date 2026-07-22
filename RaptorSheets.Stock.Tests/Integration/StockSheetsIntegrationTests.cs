using System.ComponentModel;
using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Services;
using RaptorSheets.Stock.Enums;
using RaptorSheets.Stock.Managers;
using RaptorSheets.Stock.Tests.Data.Attributes;
using RaptorSheets.Stock.Tests.Integration.Base;
using RaptorSheets.Test.Common.Helpers;
using Xunit;

namespace RaptorSheets.Stock.Tests.Integration;

/// <summary>
/// Integration tests that read back from a live Stock Google Sheet - a dedicated blank test
/// spreadsheet that's safe to delete and recreate freely (distinct from any real portfolio
/// spreadsheet). Sheet creation and demo-data seeding happen once in <see cref="StockSheetsIntegrationFixture"/>
/// before this collection runs. Skipped automatically unless credentials and a Stock spreadsheet
/// ID are configured in user secrets.
/// </summary>
[Collection("StockSheetsIntegration")]
[Category("Integration")]
public class StockSheetsIntegrationTests : IntegrationTestBase
{
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
/// class - including the MapFromRangeData mapper tests, which consume <see cref="StockSheetsIntegrationFixture.ValueRanges"/> -
/// belongs to this collection so nothing runs concurrently with the fixture's delete/recreate/seed
/// sequence.
/// </summary>
[CollectionDefinition("StockSheetsIntegration")]
public class StockSheetsIntegrationCollection : ICollectionFixture<StockSheetsIntegrationFixture>
{
}

/// <summary>
/// Fixture for Stock Google Sheets integration tests. Deletes and recreates every canonical sheet,
/// seeds realistic demo holdings, and captures a batch-data snapshot for the MapFromRangeData
/// mapper tests - all once, before the collection runs. Safe because spreadsheets:stock is
/// configured to point at a dedicated blank test spreadsheet, not a real portfolio.
/// </summary>
public class StockSheetsIntegrationFixture : IAsyncLifetime
{
    private GoogleSheetManager? _manager;

    /// <summary>
    /// Raw batch-get value ranges captured after seeding, for MapFromRangeData tests to map
    /// against directly without each doing their own live fetch.
    /// </summary>
    public IList<MatchedValueRange>? ValueRanges { get; private set; }

    public async Task InitializeAsync()
    {
        System.Diagnostics.Debug.WriteLine("🚀 Initializing Stock Google Sheets integration test environment (Clean Slate)");

        var spreadsheetId = TestConfigurationHelpers.GetStockSpreadsheet();
        var credential = TestConfigurationHelpers.GetJsonCredential();

        if (!GoogleCredentialHelpers.IsCredentialFilled(credential))
        {
            System.Diagnostics.Debug.WriteLine("⚠️  No credentials - skipping environment setup");
            return;
        }

        _manager = new GoogleSheetManager(credential, spreadsheetId);

        try
        {
            System.Diagnostics.Debug.WriteLine("  🗑️  Deleting all existing sheets to ensure clean slate...");

            var deleteResult = await _manager.DeleteAllSheets();
            var deleteErrors = deleteResult.Messages.Where(m =>
                m.Level == MessageLevel.ERROR.GetDescription()).ToList();

            if (deleteErrors.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"  ⚠️  Sheet deletion had errors: {string.Join(", ", deleteErrors.Select(e => e.Message))}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("  ✓ All sheets deleted successfully");
            }

            await Task.Delay(3000); // Allow deletion to complete

            System.Diagnostics.Debug.WriteLine("  📌 Creating all sheets fresh to validate creation process...");
            var createResult = await _manager.CreateAllSheets();
            var createErrors = createResult.Messages.Where(m =>
                m.Level == MessageLevel.ERROR.GetDescription()).ToList();

            if (createErrors.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"  ⚠️  Sheet creation had errors: {string.Join(", ", createErrors.Select(e => e.Message))}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("  ✓ All sheets created successfully");
            }

            await Task.Delay(3000); // Allow creation to complete

            System.Diagnostics.Debug.WriteLine("  🔍 Verifying sheet creation...");
            var allProperties = await _manager.GetAllSheetProperties();

            System.Diagnostics.Debug.WriteLine($"  📊 Found {allProperties.Count} sheet tabs");
            System.Diagnostics.Debug.WriteLine($"  📋 Tabs: {string.Join(", ", allProperties.Select(p => p.Name))}");

            var spreadsheetInfo = await _manager.GetSpreadsheetInfo(
                allProperties.Select(p => $"{p.Name}!1:1").ToList());

            if (spreadsheetInfo != null)
            {
                var headerValidation = GoogleSheetManager.CheckSheetHeaders(spreadsheetInfo);
                var headerErrors = headerValidation.Where(m =>
                    m.Level == MessageLevel.ERROR.GetDescription() ||
                    m.Level == MessageLevel.WARNING.GetDescription()).ToList();

                if (headerErrors.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"  ⚠️  Header validation warnings/errors:");
                    foreach (var error in headerErrors)
                    {
                        System.Diagnostics.Debug.WriteLine($"     {error.Level}: {error.Message}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("  ✓ All sheet headers validated successfully");
                }
            }

            System.Diagnostics.Debug.WriteLine("  🌱 Seeding demo holdings...");
            await _manager.PopulateDemoData(seed: 42);
            await Task.Delay(3000); // Let GOOGLEFINANCE/cross-sheet formulas resolve

            System.Diagnostics.Debug.WriteLine("  📥 Capturing batch-data snapshot for mapper tests...");
            var googleSheetService = new GoogleSheetService(credential, spreadsheetId);
            var sheetNames = Enum.GetValues(typeof(SheetName)).Cast<SheetName>().Select(x => x.GetDescription()).ToList();
            var result = await googleSheetService.GetBatchData(sheetNames);
            ValueRanges = result?.ValueRanges;

            System.Diagnostics.Debug.WriteLine("✅ Integration test environment ready (Clean Slate Validated)");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"⚠️  Setup failed: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"     Stack: {ex.StackTrace}");
            // Don't fail the fixture - let individual tests handle issues
        }
    }

    public async Task DisposeAsync()
    {
        System.Diagnostics.Debug.WriteLine("✅ Stock Google Sheets integration tests completed");
        await Task.CompletedTask;
    }
}
