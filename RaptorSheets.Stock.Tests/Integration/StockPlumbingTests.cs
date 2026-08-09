using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Services;
using RaptorSheets.Stock.Entities;
using RaptorSheets.Stock.Enums;
using RaptorSheets.Stock.Managers;
using RaptorSheets.Stock.Tests.Data.Attributes;
using RaptorSheets.Test.Common.Integration;
using Xunit;

namespace RaptorSheets.Stock.Tests.Integration;

/// <summary>
/// Stock's concrete adapter for the shared, generic plumbing scenarios in
/// <see cref="SheetPlumbingTestsBase{TEntity, TManager}"/> - the second domain to wire this up after
/// Core.Tests proved it live (see #100), and the one domain with a genuinely different SettleDelay
/// need (GOOGLEFINANCE-driven columns on Stocks/Accounts settle far slower than plain SUMIF/COUNTIF -
/// see SheetManager.PopulateDemoData's own comment on this).
///
/// Joins the same "StockSheetsIntegration" collection - and reuses the same StockCleanSlateFixture
/// instance - as StockSheetsIntegrationTests, so nothing runs concurrently against the shared
/// spreadsheet. Uses a throwaway IGoogleSheetService built from the fixture's own (now public)
/// Credential/SpreadsheetId for the raw-batch-update escape hatch, rather than a test-only manager
/// subclass - production StockSheetManager stays untouched.
/// </summary>
[Collection("StockSheetsIntegration")]
public class StockPlumbingTests : SheetPlumbingTestsBase<SheetEntity, SheetManager>
{
    private const string TestAccount = "PlumbingTest";
    private const string TestTicker = "AAPL";
    private const decimal TestShares = 1.5m;

    private readonly StockCleanSlateFixture _fixture;

    public StockPlumbingTests(StockCleanSlateFixture fixture)
    {
        _fixture = fixture;
        Config = BuildConfig(fixture);
    }

    protected override SheetManager? Manager => _fixture.Manager;

    protected override PlumbingTestConfig<SheetEntity> Config { get; }

    private static PlumbingTestConfig<SheetEntity> BuildConfig(StockCleanSlateFixture fixture) => new()
    {
        InputSheetName = SheetName.STOCKS.GetDescription(),
        TestColumnName = "Shares",
        DependentSheetName = SheetName.ACCOUNTS.GetDescription(),
        BuildTestRow = rowId => new SheetEntity
        {
            Sheets = { Stocks = { new StockEntity { RowId = rowId, Account = TestAccount, Ticker = TestTicker, Shares = TestShares } } }
        },
        ContainsTestRow = (entity, rowId) => entity.Sheets.Stocks.Any(s =>
            s.RowId == rowId && s.Account == TestAccount && s.Ticker == TestTicker && s.Shares == TestShares),
        ExecuteRawBatchUpdateAsync = async (request, ct) =>
        {
            var rawService = new GoogleSheetService(fixture.Credential, fixture.SpreadsheetId);
            return await rawService.BatchUpdateSpreadsheet(request, ct) != null;
        },
        // BulkReseedAsync intentionally omitted - the fallback of a handful of BuildTestRow rows is
        // enough here; Stock's own StockSheetsIntegrationTests re-seeds full demo holdings independently.
        SettleDelay = TimeSpan.FromSeconds(20),
    };

    // SonarQube S2699: each override below is pure attribute-application delegation (xUnit needs
    // [FactCheckUserSecrets] on the concrete method for discovery/skip - the attribute type is
    // domain-specific and can't live on the shared abstract base). The actual assertions live in the
    // base class method each one calls.
#pragma warning disable S2699

    [FactCheckUserSecrets]
    public override Task CreateAllSheets_ThenReadStructure_HasExpectedHeaders() => base.CreateAllSheets_ThenReadStructure_HasExpectedHeaders();

    [FactCheckUserSecrets]
    public override Task ReapplyFormatting_OnPopulatedColumn_PreservesExistingValues() => base.ReapplyFormatting_OnPopulatedColumn_PreservesExistingValues();

    [FactCheckUserSecrets]
    public override Task DeleteDependentSheet_LeavesInputSheetIntact_ThenRecreatesIt() => base.DeleteDependentSheet_LeavesInputSheetIntact_ThenRecreatesIt();

    [FactCheckUserSecrets]
    public override Task DeleteInputSheet_ThenRecreate_DependentFormulaStillComputes() => base.DeleteInputSheet_ThenRecreate_DependentFormulaStillComputes();

    [FactCheckUserSecrets]
    public override Task DeleteAllSheets_ThenCreateAllSheets_UsesTempSheetSafetyNet() => base.DeleteAllSheets_ThenCreateAllSheets_UsesTempSheetSafetyNet();

    [FactCheckUserSecrets]
    public override Task MissingColumn_OnDependentSheet_SelfHealRestoresFormula() => base.MissingColumn_OnDependentSheet_SelfHealRestoresFormula();

    [FactCheckUserSecrets]
    public override Task MissingColumn_OnInputSheet_RestoresStructureButNotData() => base.MissingColumn_OnInputSheet_RestoresStructureButNotData();

    [FactCheckUserSecrets]
    public override Task MultipleMissingColumns_OnDependentSheet_RestoresAllAtCorrectPositions() => base.MultipleMissingColumns_OnDependentSheet_RestoresAllAtCorrectPositions();

    [FactCheckUserSecrets]
    public override Task ColumnsReordered_ReadsAndWritesStayCorrect_LibraryDoesNotAutoCorrectOrder() => base.ColumnsReordered_ReadsAndWritesStayCorrect_LibraryDoesNotAutoCorrectOrder();

    [FactCheckUserSecrets]
    public override Task ExtraUnexpectedColumn_IsFlaggedNotRemoved_KnownColumnsUnaffected() => base.ExtraUnexpectedColumn_IsFlaggedNotRemoved_KnownColumnsUnaffected();

    [FactCheckUserSecrets]
    public override Task GetLiveSheetStructure_ReturnsConfiguredHeaders() => base.GetLiveSheetStructure_ReturnsConfiguredHeaders();

    [FactCheckUserSecrets]
    public override Task GetLiveSheetRawValues_ReturnsPositionalRows() => base.GetLiveSheetRawValues_ReturnsPositionalRows();

    [FactCheckUserSecrets]
    public override Task GetSheetProperties_And_GetAllSheetTabNames_ReturnCurrentMetadata() => base.GetSheetProperties_And_GetAllSheetTabNames_ReturnCurrentMetadata();

    [FactCheckUserSecrets]
    public override Task GetSpreadsheetTitle_ReturnsConfiguredTitle() => base.GetSpreadsheetTitle_ReturnsConfiguredTitle();

#pragma warning restore S2699
}
