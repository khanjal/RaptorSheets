using RaptorSheets.Core.Services;
using RaptorSheets.Job.Constants;
using RaptorSheets.Job.Entities;
using RaptorSheets.Job.Helpers;
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

    /// <summary>
    /// Each live test states its own precondition rather than trusting whatever the previous
    /// one left behind (#130). The check and its warning reporting live on
    /// CleanSlateSheetFixture.VerifyAndReportPreconditionsAsync, shared by all five domains.
    /// </summary>
    private Task VerifyPreconditionsAsync()
        => _fixture.VerifyAndReportPreconditionsAsync(JobSheetHelpers.GetSheetNames());

    // SonarQube S2699: each override below is pure attribute-application delegation (xUnit needs
    // [FactCheckUserSecrets] on the concrete method for discovery/skip - the attribute type is
    // domain-specific and can't live on the shared abstract base). The actual assertions live in the
    // base class method each one calls.
#pragma warning disable S2699

    [FactCheckUserSecrets]
    public override async Task CreateAllSheets_ThenReadStructure_HasExpectedHeaders()
    {
        await VerifyPreconditionsAsync();
        await base.CreateAllSheets_ThenReadStructure_HasExpectedHeaders();
    }

    [FactCheckUserSecrets]
    public override async Task ReapplyFormatting_OnPopulatedColumn_PreservesExistingValues()
    {
        await VerifyPreconditionsAsync();
        await base.ReapplyFormatting_OnPopulatedColumn_PreservesExistingValues();
    }

    [FactCheckUserSecrets]
    public override async Task DeleteDependentSheet_LeavesInputSheetIntact_ThenRecreatesIt()
    {
        await VerifyPreconditionsAsync();
        await base.DeleteDependentSheet_LeavesInputSheetIntact_ThenRecreatesIt();
    }

    [FactCheckUserSecrets]
    public override async Task DeleteInputSheet_ThenRecreate_DependentFormulaStillComputes()
    {
        await VerifyPreconditionsAsync();
        await base.DeleteInputSheet_ThenRecreate_DependentFormulaStillComputes();
    }

    [FactCheckUserSecrets]
    public override async Task DeleteAllSheets_ThenCreateAllSheets_UsesTempSheetSafetyNet()
    {
        await VerifyPreconditionsAsync();
        await base.DeleteAllSheets_ThenCreateAllSheets_UsesTempSheetSafetyNet();
    }

    [FactCheckUserSecrets]
    public override async Task MissingColumn_OnDependentSheet_SelfHealRestoresFormula()
    {
        await VerifyPreconditionsAsync();
        await base.MissingColumn_OnDependentSheet_SelfHealRestoresFormula();
    }

    [FactCheckUserSecrets]
    public override async Task MissingColumn_OnInputSheet_RestoresStructureButNotData()
    {
        await VerifyPreconditionsAsync();
        await base.MissingColumn_OnInputSheet_RestoresStructureButNotData();
    }

    [FactCheckUserSecrets]
    public override async Task MultipleMissingColumns_OnDependentSheet_RestoresAllAtCorrectPositions()
    {
        await VerifyPreconditionsAsync();
        await base.MultipleMissingColumns_OnDependentSheet_RestoresAllAtCorrectPositions();
    }

    [FactCheckUserSecrets]
    public override async Task ColumnsReordered_ReadsAndWritesStayCorrect_LibraryDoesNotAutoCorrectOrder()
    {
        await VerifyPreconditionsAsync();
        await base.ColumnsReordered_ReadsAndWritesStayCorrect_LibraryDoesNotAutoCorrectOrder();
    }

    [FactCheckUserSecrets]
    public override async Task ExtraUnexpectedColumn_IsFlaggedNotRemoved_KnownColumnsUnaffected()
    {
        await VerifyPreconditionsAsync();
        await base.ExtraUnexpectedColumn_IsFlaggedNotRemoved_KnownColumnsUnaffected();
    }

    [FactCheckUserSecrets]
    public override async Task GetLiveSheetStructure_ReturnsConfiguredHeaders()
    {
        await VerifyPreconditionsAsync();
        await base.GetLiveSheetStructure_ReturnsConfiguredHeaders();
    }

    [FactCheckUserSecrets]
    public override async Task GetLiveSheetRawValues_ReturnsPositionalRows()
    {
        await VerifyPreconditionsAsync();
        await base.GetLiveSheetRawValues_ReturnsPositionalRows();
    }

    [FactCheckUserSecrets]
    public override async Task GetSheetProperties_And_GetAllSheetTabNames_ReturnCurrentMetadata()
    {
        await VerifyPreconditionsAsync();
        await base.GetSheetProperties_And_GetAllSheetTabNames_ReturnCurrentMetadata();
    }

    [FactCheckUserSecrets]
    public override async Task GetSpreadsheetTitle_ReturnsConfiguredTitle()
    {
        await VerifyPreconditionsAsync();
        await base.GetSpreadsheetTitle_ReturnsConfiguredTitle();
    }

#pragma warning restore S2699
}
