using RaptorSheets.Core.Services;
using RaptorSheets.Home.Constants;
using RaptorSheets.Home.Entities;
using RaptorSheets.Home.Helpers;
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

    /// <summary>
    /// These are the destructive scenarios - they delete sheets, drop columns and reorder them - so
    /// they are both the most likely to leave damage and the most likely to inherit it. The clean
    /// slate runs once per collection, so a test that fails before restoring what it removed hands
    /// the wreckage to everything after it (#130).
    ///
    /// Checking here means a test states its own precondition instead of trusting the previous one,
    /// and damage is named where it is found rather than wherever it eventually causes a failure.
    /// Mirrors Gig's and Core's adoption of this - Home was left opt-in when
    /// CleanSlateSheetFixture first gained the capability.
    /// </summary>
    private async Task VerifyPreconditionsAsync()
    {
        if (_fixture.Manager == null)
        {
            return;
        }

        var (repaired, drift) = await _fixture.VerifyPreconditionsAsync(HomeSheetHelpers.GetSheetNames());

        if (repaired.Count > 0)
        {
            // Console, not Debug.WriteLine: Debug.WriteLine is [Conditional("DEBUG")] and CI builds
            // Release, so every diagnostic written that way is absent from the one run anybody reads
            // after the fact.
            Console.WriteLine(
                $"WARNING: repaired {repaired.Count} sheet(s) missing before this test ran: {string.Join(", ", repaired)}. " +
                "An earlier test removed them without restoring them - see #130.");
        }

        if (drift.Count > 0)
        {
            Console.WriteLine(
                $"WARNING: {drift.Count} sheet(s) have drifted columns before this test ran: {string.Join(" | ", drift)}. " +
                "An earlier test changed them without restoring them - see #130.");
        }
    }

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
