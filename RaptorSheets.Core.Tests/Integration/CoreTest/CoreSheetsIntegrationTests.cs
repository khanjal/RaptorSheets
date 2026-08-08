using System.ComponentModel;
using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Managers;
using RaptorSheets.Test.Common.Attributes;
using RaptorSheets.Test.Common.Fixtures;
using RaptorSheets.Test.Common.Helpers;
using Xunit;

namespace RaptorSheets.Core.Tests.Integration.CoreTest;

/// <summary>
/// Live integration tests for RaptorSheets.Core's own domain-agnostic plumbing (SheetManagerBase,
/// ColumnInsertionHelper, SheetRegistry) - see GitHub issue #100 for why this exists: the four
/// domain-owned test spreadsheets only ever exercised this code by accident, and a real round trip
/// is the only thing that actually proves a Google API field mask doesn't silently clear data (see
/// #99's Copilot-caught bug in GenerateColumnFormatRequest).
///
/// Skipped automatically unless credentials and "spreadsheets:test:core" are configured in user
/// secrets (RaptorSheets.Test.Common's shared secrets store - never spreadsheets:live:*). Collection
/// fixture (<see cref="CoreCleanSlateFixture"/>) deletes/recreates every sheet before tests run.
///
/// Each test is self-contained (writes/deletes/recreates whatever it needs) rather than relying on
/// another test's side effects, since xUnit doesn't guarantee method execution order within a class.
/// </summary>
[Collection("CoreSheetsIntegration")]
[Category("Integration")]
public class CoreSheetsIntegrationTests
{
    private readonly CoreTestManager? Manager;

    public CoreSheetsIntegrationTests(CoreCleanSlateFixture fixture)
    {
        Manager = fixture.Manager;
    }

    private void SkipIfNoCredentials()
    {
        if (Manager == null)
        {
            Assert.Fail("Google Sheets credentials not available. Configure user secrets to run integration tests.");
        }
    }

    private static List<MessageEntity> CriticalErrors(CoreTestSheetEntity result) =>
        result.Messages
            .Where(m => m.Level == MessageLevel.ERROR.GetDescription() && !IsExpectedError(m.Message))
            .ToList();

    private static bool IsExpectedError(string message) =>
        message.Contains("not supported") ||
        message.Contains("already exists") ||
        message.Contains("header issue") ||
        message.Contains("No data to change");

    [FactCheckUserSecrets]
    public async Task CreateAllSheets_ThenReadStructure_HasExpectedHeadersAndFormulas()
    {
        SkipIfNoCredentials();

        var structures = await Manager!.GetAllLiveSheetStructures();

        Assert.True(structures.ContainsKey(CoreTestSheetNames.Items));
        Assert.True(structures.ContainsKey(CoreTestSheetNames.Log));
        Assert.True(structures.ContainsKey(CoreTestSheetNames.Summary));

        var summaryTotal = structures[CoreTestSheetNames.Summary].Headers.FirstOrDefault(h => h.Name == "Total");
        Assert.NotNull(summaryTotal);
        Assert.Contains("Items", summaryTotal!.Formula, StringComparison.OrdinalIgnoreCase);
    }

    [FactCheckUserSecrets]
    public async Task ChangeSheetData_ThenGetSheets_WriteThenReadRoundTrips_AndSummaryComputes()
    {
        SkipIfNoCredentials();

        var data = new CoreTestSheetEntity();
        data.Sheets.Items.Add(new ItemEntity { RowId = 2, Name = "Widget", Category = "Hardware", Amount = 25.50m, Active = true });
        data.Sheets.Log.Add(new LogEntity { RowId = 2, Date = "2026-01-15", Description = "First entry" });

        var writeResult = await Manager!.ChangeSheetData([CoreTestSheetNames.Items, CoreTestSheetNames.Log], data);
        Assert.Empty(CriticalErrors(writeResult));

        await Task.Delay(2500);

        var readResult = await Manager!.GetSheets([CoreTestSheetNames.Items, CoreTestSheetNames.Log, CoreTestSheetNames.Summary]);

        var widget = readResult.Sheets.Items.FirstOrDefault(i => i.Name == "Widget");
        Assert.NotNull(widget);
        Assert.Equal(25.50m, widget!.Amount);

        Assert.Contains(readResult.Sheets.Log, l => l.Description == "First entry");

        var hardwareRow = Assert.Single(readResult.Sheets.Summary, s => s.Category == "Hardware");
        Assert.Equal(25.50m, hardwareRow.Total);
        Assert.Equal(1, hardwareRow.Count);
    }

    /// <summary>
    /// Direct regression test for the Copilot-caught field-mask bug on #99: GenerateColumnFormatRequest
    /// previously reused GenerateRepeatCellRequest's "*" mask, which would have blanked every value in
    /// the column since the request never set UserEnteredValue. Only a real round trip proves the fix.
    /// </summary>
    [FactCheckUserSecrets]
    public async Task ReapplyFormatting_OnPopulatedColumn_PreservesExistingValues()
    {
        SkipIfNoCredentials();

        var data = new CoreTestSheetEntity();
        data.Sheets.Items.Add(new ItemEntity { RowId = 2, Name = "Gadget", Category = "Electronics", Amount = 99.99m, Active = true });

        var writeResult = await Manager!.ChangeSheetData([CoreTestSheetNames.Items], data);
        Assert.Empty(CriticalErrors(writeResult));
        await Task.Delay(2500);

        var reapplyResult = await Manager!.ReapplyFormatting(CoreTestSheetNames.Items);
        Assert.Empty(CriticalErrors(reapplyResult));
        await Task.Delay(2000);

        var readResult = await Manager!.GetSheets([CoreTestSheetNames.Items]);
        var gadget = readResult.Sheets.Items.FirstOrDefault(i => i.Name == "Gadget");

        Assert.NotNull(gadget);
        Assert.Equal("Electronics", gadget!.Category);
        Assert.Equal(99.99m, gadget.Amount);
        Assert.True(gadget.Active);
    }

    /// <summary>
    /// try/finally throughout this destructive-test group is deliberate: an assertion failure inside
    /// a delete step must never skip the recreate step, or it corrupts the shared spreadsheet for
    /// every other test in this collection (that's exactly what happened during initial live runs -
    /// a wrong assertion aborted a delete-heavy test mid-method and cascaded failures through the
    /// rest of the suite, since C# skips everything after a failed Assert.Contains).
    /// </summary>
    [FactCheckUserSecrets]
    public async Task DeleteSheets_SingleSheet_LeavesOthersIntact_ThenRecreatesIt()
    {
        SkipIfNoCredentials();

        try
        {
            await Manager!.DeleteSheets([CoreTestSheetNames.Log]);

            var tabNamesAfterDelete = await Manager!.GetAllSheetTabNames();
            Assert.DoesNotContain(CoreTestSheetNames.Log, tabNamesAfterDelete);
            Assert.Contains(CoreTestSheetNames.Items, tabNamesAfterDelete);
            Assert.Contains(CoreTestSheetNames.Summary, tabNamesAfterDelete);
        }
        finally
        {
            var createResult = await Manager!.CreateSheets([CoreTestSheetNames.Log]);
            Assert.Empty(CriticalErrors(createResult));
        }

        await Task.Delay(1500);
        var tabNamesAfterRecreate = await Manager!.GetAllSheetTabNames();
        Assert.Contains(CoreTestSheetNames.Log, tabNamesAfterRecreate);
    }

    [FactCheckUserSecrets]
    public async Task DeleteAllSheets_ThenCreateAllSheets_UsesTempSheetSafetyNet()
    {
        SkipIfNoCredentials();

        // Google Sheets always starts a new spreadsheet with a default "Sheet1" tab, and any earlier
        // run of this very test leaves a "TempSheet" behind (both non-canonical, so the library's own
        // DeleteAllSheets/CreateAllSheets never touch either). Remove both here (best-effort, outside
        // the library's normal path) so deleting our 3 canonical sheets genuinely leaves nothing
        // behind and the safety net actually has to create a fresh TempSheet - otherwise either stray
        // tab alone already satisfies "at least one sheet remains" (a leftover TempSheet especially:
        // NeedsTempSheet correctly avoids creating a duplicate of one that already exists - see #100).
        await TryDeleteNonCanonicalSheetAsync("Sheet1");
        await TryDeleteNonCanonicalSheetAsync(SheetManagerBase.TempSheetName);

        try
        {
            var deleteResult = await Manager!.DeleteAllSheets();
            Assert.Contains(deleteResult.Messages, m => m.Message.Contains("safety sheet"));

            await Task.Delay(2000);
        }
        finally
        {
            var createResult = await Manager!.CreateAllSheets();
            Assert.Empty(CriticalErrors(createResult));
        }

        await Task.Delay(2000);

        var tabNames = await Manager!.GetAllSheetTabNames();
        Assert.Contains(CoreTestSheetNames.Items, tabNames);
        Assert.Contains(CoreTestSheetNames.Log, tabNames);
        Assert.Contains(CoreTestSheetNames.Summary, tabNames);
    }

    private async Task TryDeleteNonCanonicalSheetAsync(string sheetName)
    {
        var properties = await Manager!.GetSheetProperties([sheetName]);
        var property = properties.FirstOrDefault(p => !string.IsNullOrEmpty(p.Id));

        if (property == null || !int.TryParse(property.Id, out var sheetId))
        {
            return;
        }

        var deleteRequest = new BatchUpdateSpreadsheetRequest
        {
            Requests = [new Request { DeleteSheet = new DeleteSheetRequest { SheetId = sheetId } }]
        };

        await Manager!.ExecuteRawBatchUpdateAsync(deleteRequest);
        await Task.Delay(1000);
    }

    /// <summary>
    /// Live regression test for #53 gap 1: simulates a column being manually deleted outside the
    /// library (via a raw DeleteDimension request, bypassing every normal write path), then confirms
    /// GetSheets' auto-heal restores it with its Formula and Format intact - not just the header text.
    /// </summary>
    [FactCheckUserSecrets]
    public async Task MissingColumn_OnGetSheets_SelfHealRestoresFormulaAndFormat()
    {
        SkipIfNoCredentials();

        var data = new CoreTestSheetEntity();
        data.Sheets.Items.Add(new ItemEntity { RowId = 2, Name = "Bolt", Category = "Hardware", Amount = 10m, Active = true });
        await Manager!.ChangeSheetData([CoreTestSheetNames.Items], data);
        await Task.Delay(2000);

        var summaryProperty = (await Manager!.GetSheetProperties([CoreTestSheetNames.Summary]))[0];
        var summarySheetId = int.Parse(summaryProperty.Id);

        var summaryModel = SummarySheetDefinition.GetSheet();
        summaryModel.Headers.UpdateColumns();
        var totalIndex = summaryModel.Headers.First(h => h.Name == "Total").Index;

        var deleteColumnRequest = new BatchUpdateSpreadsheetRequest
        {
            Requests =
            [
                new Request
                {
                    DeleteDimension = new DeleteDimensionRequest
                    {
                        Range = new DimensionRange { SheetId = summarySheetId, Dimension = "COLUMNS", StartIndex = totalIndex, EndIndex = totalIndex + 1 }
                    }
                }
            ]
        };

        var deleted = await Manager!.ExecuteRawBatchUpdateAsync(deleteColumnRequest);
        Assert.True(deleted);
        await Task.Delay(2000);

        // The heal (insert + format reapply) is a side effect of this call itself, independent of
        // whether the message assertion below passes - no risk of the missing column lingering into
        // the next test even if this assertion fails.
        var healingRead = await Manager!.GetSheets([CoreTestSheetNames.Summary]);
        Assert.Contains(healingRead.Messages, m => m.Message.Contains("Inserting column") && m.Message.Contains("Total"));

        await Task.Delay(2000);

        // Second read sees the restored, recomputing formula.
        var afterHeal = await Manager!.GetSheets([CoreTestSheetNames.Summary]);
        var hardwareRow = Assert.Single(afterHeal.Sheets.Summary, s => s.Category == "Hardware");
        Assert.True(hardwareRow.Total > 0);
    }

    /// <summary>
    /// The scenario you asked for directly: deletes the sheet a calculated/child sheet depends on,
    /// confirms self-heal recreates it AND that the dependent's formula (SheetRegistry.GetDependents /
    /// SheetManagerBase.RefreshDependentSheetsAsync) still computes correctly afterward, rather than
    /// staying silently broken against a stale sheet reference.
    /// </summary>
    [FactCheckUserSecrets]
    public async Task DeleteAndRecreateItems_ThenGetSummary_DependentFormulaStillComputes()
    {
        SkipIfNoCredentials();

        var data = new CoreTestSheetEntity();
        data.Sheets.Items.Add(new ItemEntity { RowId = 2, Name = "Nut", Category = "Hardware", Amount = 5m, Active = true });
        await Manager!.ChangeSheetData([CoreTestSheetNames.Items], data);
        await Task.Delay(2000);

        // Not asserting on this result: the recreation below is a side effect of the next GetSheets
        // call regardless of what DeleteSheets reports, and must always run - a blocking assertion
        // here would risk leaving Items permanently deleted for every later test if it ever failed.
        await Manager!.DeleteSheets([CoreTestSheetNames.Items]);
        await Task.Delay(2000);

        var healResult = await Manager!.GetSheets([CoreTestSheetNames.Items, CoreTestSheetNames.Summary]);
        Assert.Contains(healResult.Messages, m => m.Message.Contains("Created missing sheets"));
        await Task.Delay(2000);

        var refill = new CoreTestSheetEntity();
        refill.Sheets.Items.Add(new ItemEntity { RowId = 2, Name = "Screw", Category = "Hardware", Amount = 3m, Active = true });
        var refillResult = await Manager!.ChangeSheetData([CoreTestSheetNames.Items], refill);
        Assert.Empty(CriticalErrors(refillResult));
        await Task.Delay(2500);

        var finalRead = await Manager!.GetSheets([CoreTestSheetNames.Summary]);
        var hardwareRow = Assert.Single(finalRead.Sheets.Summary, s => s.Category == "Hardware");
        Assert.True(hardwareRow.Total > 0);
        Assert.True(hardwareRow.Count > 0);
    }

    [FactCheckUserSecrets]
    public async Task GetLiveSheetStructure_ReturnsConfiguredFormatAndFormula()
    {
        SkipIfNoCredentials();

        var structure = await Manager!.GetLiveSheetStructure(CoreTestSheetNames.Summary);

        Assert.NotNull(structure);
        var total = structure!.Headers.FirstOrDefault(h => h.Name == "Total");
        Assert.NotNull(total);
        Assert.False(string.IsNullOrEmpty(total!.Formula));
    }

    [FactCheckUserSecrets]
    public async Task GetLiveSheetRawValues_ReturnsPositionalRowsRegardlessOfSchema()
    {
        SkipIfNoCredentials();

        var rawValues = await Manager!.GetLiveSheetRawValues(CoreTestSheetNames.Items);

        Assert.NotEmpty(rawValues);
        Assert.Contains("Name", rawValues[0]);
    }

    [FactCheckUserSecrets]
    public async Task GetSheetProperties_And_GetAllSheetTabNames_ReturnCurrentMetadata()
    {
        SkipIfNoCredentials();

        var tabNames = await Manager!.GetAllSheetTabNames();
        Assert.Contains(CoreTestSheetNames.Items, tabNames);

        var properties = await Manager!.GetSheetProperties([CoreTestSheetNames.Items]);
        var itemsProperty = Assert.Single(properties);
        Assert.False(string.IsNullOrEmpty(itemsProperty.Id));
    }

    [FactCheckUserSecrets]
    public async Task GetSpreadsheetTitle_ReturnsConfiguredTitle()
    {
        SkipIfNoCredentials();

        var title = await Manager!.GetSpreadsheetTitle();

        Assert.False(string.IsNullOrWhiteSpace(title));
    }
}

/// <summary>
/// Collection definition for Core Google Sheets integration tests.
/// </summary>
[CollectionDefinition("CoreSheetsIntegration")]
public class CoreSheetsIntegrationCollection : ICollectionFixture<CoreCleanSlateFixture>
{
}

/// <summary>
/// Core's clean-slate integration fixture (see <see cref="CleanSlateSheetFixture{TEntity,TManager}"/>).
/// Deletes and recreates every canonical sheet once, before the collection's tests run. Safe because
/// spreadsheets:test:core is configured to point at its own dedicated, empty test spreadsheet - never
/// shared with a domain or with anyone's real data.
/// </summary>
public class CoreCleanSlateFixture : CleanSlateSheetFixture<CoreTestSheetEntity, CoreTestManager>
{
    public CoreCleanSlateFixture() : base(
        TestConfigurationHelpers.GetCoreSpreadsheet(),
        (credential, spreadsheetId) => new CoreTestManager(credential, spreadsheetId))
    {
    }
}
