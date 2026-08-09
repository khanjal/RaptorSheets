using RaptorSheets.Core.Services;
using RaptorSheets.Job.Constants;
using RaptorSheets.Job.Entities;
using RaptorSheets.Job.Managers;
using RaptorSheets.Job.Tests.Data.Attributes;
using RaptorSheets.Test.Common.Integration;
using Xunit;

namespace RaptorSheets.Job.Tests.Integration;

/// <summary>
/// Job's concrete adapter for the shared, generic plumbing scenarios in
/// <see cref="SheetPlumbingTestsBase{TEntity, TManager}"/> - the fourth domain wired up after Core,
/// Stock, and Gig (see #100). Uses Applications (a real writable sheet) and Companies (its own
/// dependent rollup, keyed off Applications' Company column - see CompanySheet.GetSheet()).
///
/// Joins the same "JobSheetsIntegration" collection - and reuses the same JobCleanSlateFixture
/// instance - as JobSheetsIntegrationTests, so nothing runs concurrently against the shared
/// spreadsheet. Uses a throwaway IGoogleSheetService built from the fixture's own (public)
/// Credential/SpreadsheetId for the raw-batch-update escape hatch - production JobSheetManager stays
/// untouched.
/// </summary>
[Collection("JobSheetsIntegration")]
public class JobPlumbingTests : SheetPlumbingTestsBase<SheetEntity, SheetManager>
{
    private const string TestCompany = "PlumbingTest";
    private const decimal TestPayLow = 42000m;

    private readonly JobCleanSlateFixture _fixture;

    public JobPlumbingTests(JobCleanSlateFixture fixture)
    {
        _fixture = fixture;
        Config = BuildConfig(fixture);
    }

    protected override SheetManager? Manager => _fixture.Manager;

    protected override PlumbingTestConfig<SheetEntity> Config { get; }

    private static PlumbingTestConfig<SheetEntity> BuildConfig(JobCleanSlateFixture fixture) => new()
    {
        InputSheetName = SheetsConfig.SheetNames.Applications,
        TestColumnName = "Pay Low",
        DependentSheetName = SheetsConfig.SheetNames.Companies,
        BuildTestRow = rowId => new SheetEntity
        {
            Sheets = { Applications = { new ApplicationEntity { RowId = rowId, Date = "2026-01-15", Company = TestCompany, JobTitle = "Plumbing Tester", PayLow = TestPayLow } } }
        },
        ContainsTestRow = (entity, rowId) => entity.Sheets.Applications.Any(a =>
            a.RowId == rowId && a.Company == TestCompany && a.PayLow == TestPayLow),
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
