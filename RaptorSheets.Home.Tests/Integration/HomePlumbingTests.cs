using RaptorSheets.Core.Services;
using RaptorSheets.Home.Constants;
using RaptorSheets.Home.Entities;
using RaptorSheets.Home.Managers;
using RaptorSheets.Home.Tests.Data.Attributes;
using RaptorSheets.Test.Common.Integration;
using Xunit;

namespace RaptorSheets.Home.Tests.Integration;

/// <summary>
/// Home's concrete adapter for the shared, generic plumbing scenarios in
/// <see cref="SheetPlumbingTestsBase{TEntity, TManager}"/> - the fifth and final domain wired up
/// after Core, Stock, Gig, and Job (see #100). Home's 9 sheets are all independent catalog sheets -
/// none of them cross-reference another sheet via a formula (confirmed: zero GetRange calls across
/// every RaptorSheets.Home/Sheets/*.cs), so there's no dependent/rollup sheet to configure here.
/// DependentSheetName is left null - the shared base skips every dependent-sheet scenario in that case.
///
/// Joins the same "HomeSheetsIntegration" collection - and reuses the same HomeCleanSlateFixture
/// instance - as HomeSheetsIntegrationTests, so nothing runs concurrently against the shared
/// spreadsheet. Uses a throwaway IGoogleSheetService built from the fixture's own (public)
/// Credential/SpreadsheetId for the raw-batch-update escape hatch - production HomeSheetManager stays
/// untouched.
/// </summary>
[Collection("HomeSheetsIntegration")]
public class HomePlumbingTests : SheetPlumbingTestsBase<SheetEntity, SheetManager>
{
    private const string TestManufacturer = "PlumbingTest";
    private const string TestType = "Plumbing Fixture";

    private readonly HomeCleanSlateFixture _fixture;

    public HomePlumbingTests(HomeCleanSlateFixture fixture)
    {
        _fixture = fixture;
        Config = BuildConfig(fixture);
    }

    protected override SheetManager? Manager => _fixture.Manager;

    protected override PlumbingTestConfig<SheetEntity> Config { get; }

    private static PlumbingTestConfig<SheetEntity> BuildConfig(HomeCleanSlateFixture fixture) => new()
    {
        InputSheetName = SheetsConfig.SheetNames.Appliances,
        TestColumnName = "Manufacturer",
        DependentSheetName = null,
        BuildTestRow = rowId => new SheetEntity
        {
            Sheets = { Appliances = { new ApplianceEntity { RowId = rowId, Type = TestType, Manufacturer = TestManufacturer } } }
        },
        ContainsTestRow = (entity, rowId) => entity.Sheets.Appliances.Any(a =>
            a.RowId == rowId && a.Type == TestType && a.Manufacturer == TestManufacturer),
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
