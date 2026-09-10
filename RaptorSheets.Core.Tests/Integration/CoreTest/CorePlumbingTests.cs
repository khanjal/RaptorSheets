using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Services;
using RaptorSheets.Test.Common.Attributes;
using RaptorSheets.Test.Common.Integration;
using Xunit;

namespace RaptorSheets.Core.Tests.Integration.CoreTest;

/// <summary>
/// Core's own concrete adapter for the shared, generic plumbing scenarios in
/// <see cref="SheetPlumbingTestsBase{TEntity, TManager}"/> - first proven live here as a bespoke,
/// Core-only suite before being generalized so Gig/Job/Home/Stock don't each re-author the same raw
/// batch-update orchestration against their own real schemas. See #100.
///
/// Row-level/business-value coverage (Summary's actual computed totals, the large seeded dataset)
/// isn't generalizable and stays in <see cref="CoreSheetsIntegrationTests"/> instead.
/// </summary>
[Collection("CoreSheetsIntegration")]
public class CorePlumbingTests : SheetPlumbingTestsBase<CoreTestSheetEntity, CoreTestManager>
{
    private const string TestName = "PlumbingTestItem";
    private const string TestCategory = "PlumbingTest";
    private const decimal TestAmount = 42.42m;

    private readonly CoreCleanSlateFixture _fixture;

    public CorePlumbingTests(CoreCleanSlateFixture fixture)
    {
        _fixture = fixture;
        Config = BuildConfig(fixture);
    }

    protected override CoreTestManager? Manager => _fixture.Manager;

    protected override PlumbingTestConfig<CoreTestSheetEntity> Config { get; }

    private static PlumbingTestConfig<CoreTestSheetEntity> BuildConfig(CoreCleanSlateFixture fixture) => new()
    {
        InputSheetName = CoreTestSheetNames.Items,
        TestColumnName = "Amount",
        DependentSheetName = CoreTestSheetNames.Summary,
        BuildTestRow = rowId => new CoreTestSheetEntity
        {
            Sheets = { Items = { new ItemEntity { RowId = rowId, Name = TestName, Category = TestCategory, Amount = TestAmount, Active = true } } }
        },
        ContainsTestRow = (entity, rowId) => entity.Sheets.Items.Any(i =>
            i.RowId == rowId && i.Name == TestName && i.Category == TestCategory && i.Amount == TestAmount && i.Active),
        ExecuteRawBatchUpdateAsync = async (request, ct) =>
        {
            var rawService = new GoogleSheetService(fixture.Credential, fixture.SpreadsheetId);
            return await rawService.BatchUpdateSpreadsheet(request, ct) != null;
        },
        BulkReseedAsync = async ct =>
        {
            var random = new Random();
            var reseed = new CoreTestSheetEntity();
            reseed.Sheets.Items.AddRange(CoreTestDataSeeder.GenerateItems(CoreCleanSlateFixture.SeededItemCount, CoreCleanSlateFixture.SeedStartRowId, random));
            await fixture.Manager!.ChangeSheetData([CoreTestSheetNames.Items], reseed, ct);
        },
        SettleDelay = TimeSpan.FromSeconds(2),
    };

    /// <summary>
    /// Each live test states its own precondition rather than trusting whatever the previous
    /// one left behind (#130). The check and its warning reporting live on
    /// CleanSlateSheetFixture.VerifyAndReportPreconditionsAsync, shared by all five domains.
    /// </summary>
    private Task VerifyPreconditionsAsync()
        => _fixture.VerifyAndReportPreconditionsAsync(CoreTestManager.GetSheetNames());

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
