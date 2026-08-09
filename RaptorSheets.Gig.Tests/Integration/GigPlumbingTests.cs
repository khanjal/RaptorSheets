using RaptorSheets.Core.Services;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Managers;
using RaptorSheets.Gig.Tests.Data.Attributes;
using RaptorSheets.Test.Common.Integration;
using Xunit;

namespace RaptorSheets.Gig.Tests.Integration;

/// <summary>
/// Gig's concrete adapter for the shared, generic plumbing scenarios in
/// <see cref="SheetPlumbingTestsBase{TEntity, TManager}"/> - the third domain wired up after Core and
/// Stock (see #100). Uses Shifts (a real writable sheet) and Daily (its own dependent rollup, keyed
/// off Shifts' Date column - see DailySheet.GetSheet()).
///
/// Joins the same "GigSheetsIntegration" collection - and reuses the same GigCleanSlateFixture
/// instance - as GoogleSheetsIntegrationTests, so nothing runs concurrently against the shared
/// spreadsheet. Uses a throwaway IGoogleSheetService built from the fixture's own (public)
/// Credential/SpreadsheetId for the raw-batch-update escape hatch - production GigSheetManager stays
/// untouched.
/// </summary>
[Collection("GigSheetsIntegration")]
public class GigPlumbingTests : SheetPlumbingTestsBase<SheetEntity, SheetManager>
{
    private const string TestService = "PlumbingTest";
    private const decimal TestPay = 42.42m;

    private readonly GigCleanSlateFixture _fixture;

    public GigPlumbingTests(GigCleanSlateFixture fixture)
    {
        _fixture = fixture;
        Config = BuildConfig(fixture);
    }

    protected override SheetManager? Manager => _fixture.Manager;

    protected override PlumbingTestConfig<SheetEntity> Config { get; }

    private static PlumbingTestConfig<SheetEntity> BuildConfig(GigCleanSlateFixture fixture) => new()
    {
        InputSheetName = SheetsConfig.SheetNames.Shifts,
        TestColumnName = "Pay",
        DependentSheetName = SheetsConfig.SheetNames.Daily,
        BuildTestRow = rowId => new SheetEntity
        {
            Sheets = { Shifts = { new ShiftEntity { RowId = rowId, Date = "2026-01-15", Service = TestService, Pay = TestPay } } }
        },
        ContainsTestRow = (entity, rowId) => entity.Sheets.Shifts.Any(s =>
            s.RowId == rowId && s.Service == TestService && s.Pay == TestPay),
        ExecuteRawBatchUpdateAsync = async (request, ct) =>
        {
            var rawService = new GoogleSheetService(fixture.Credential, fixture.SpreadsheetId);
            return await rawService.BatchUpdateSpreadsheet(request, ct) != null;
        },
        SettleDelay = TimeSpan.FromSeconds(2),
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
