using RaptorSheets.Core.Services;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Helpers;
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

    /// <summary>
    /// These are the destructive scenarios - they delete sheets, drop columns and reorder them - so
    /// they are both the most likely to leave damage and the most likely to inherit it. The clean
    /// slate runs once per collection, so a test that fails before restoring what it removed hands
    /// the wreckage to everything after it (#130).
    ///
    /// Checking here means a test states its own precondition instead of trusting the previous one,
    /// and damage is named where it is found rather than wherever it eventually causes a failure.
    /// </summary>
    private async Task VerifyPreconditionsAsync()
    {
        if (_fixture.Manager == null)
        {
            return;
        }

        var repaired = await _fixture.VerifyAndRepairAsync(GigSheetHelpers.GetSheetNames());

        if (repaired.Count > 0)
        {
            System.Diagnostics.Debug.WriteLine(
                $"⚠️  Repaired {repaired.Count} sheet(s) missing before this test ran: {string.Join(", ", repaired)}. " +
                "An earlier test removed them without restoring them - see #130.");
        }
    }

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
