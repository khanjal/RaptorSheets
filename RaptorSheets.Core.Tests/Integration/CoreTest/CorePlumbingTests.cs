using RaptorSheets.Core.Entities;
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

    private readonly CoreTestManager? _manager;

    public CorePlumbingTests(CoreCleanSlateFixture fixture)
    {
        _manager = fixture.Manager;
        Config = BuildConfig(_manager);
    }

    protected override CoreTestManager? Manager => _manager;

    protected override PlumbingTestConfig<CoreTestSheetEntity> Config { get; }

    private static PlumbingTestConfig<CoreTestSheetEntity> BuildConfig(CoreTestManager? manager) => new()
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
        ExecuteRawBatchUpdateAsync = (request, ct) => manager!.ExecuteRawBatchUpdateAsync(request, ct),
        BulkReseedAsync = async ct =>
        {
            var random = new Random();
            var reseed = new CoreTestSheetEntity();
            reseed.Sheets.Items.AddRange(CoreTestDataSeeder.GenerateItems(CoreCleanSlateFixture.SeededItemCount, CoreCleanSlateFixture.SeedStartRowId, random));
            await manager!.ChangeSheetData([CoreTestSheetNames.Items], reseed, ct);
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
