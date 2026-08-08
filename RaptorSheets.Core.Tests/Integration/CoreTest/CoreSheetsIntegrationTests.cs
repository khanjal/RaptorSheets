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
/// The three tests that wipe a whole sheet (see their own comments) each repopulate a modest amount
/// of data afterward, so the live sheet stays richly populated regardless of run order - a fixed
/// "run this test last" ordering trick was tried first and worked, but only covered one specific
/// test; self-repopulating is robust to any destructive test running last, not just one.
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

        // "RoundTripCheck" is deliberately not one of CoreTestDataSeeder's category pool - this test
        // asserts an EXACT Summary total, which the fixture's randomized seed data (also aggregated
        // per-category) would otherwise silently pollute if this row shared a category with it.
        const string category = "RoundTripCheck";

        var data = new CoreTestSheetEntity();
        data.Sheets.Items.Add(new ItemEntity { RowId = 2, Name = "Widget", Category = category, Amount = 25.50m, Active = true });
        data.Sheets.Log.Add(new LogEntity { RowId = 2, Date = "2026-01-15", Description = "First entry" });

        var writeResult = await Manager!.ChangeSheetData([CoreTestSheetNames.Items, CoreTestSheetNames.Log], data);
        Assert.Empty(CriticalErrors(writeResult));

        await Task.Delay(2500);

        var readResult = await Manager!.GetSheets([CoreTestSheetNames.Items, CoreTestSheetNames.Log, CoreTestSheetNames.Summary]);

        var widget = readResult.Sheets.Items.FirstOrDefault(i => i.Name == "Widget");
        Assert.NotNull(widget);
        Assert.Equal(25.50m, widget!.Amount);

        Assert.Contains(readResult.Sheets.Log, l => l.Description == "First entry");

        var categoryRow = Assert.Single(readResult.Sheets.Summary, s => s.Category == category);
        Assert.Equal(25.50m, categoryRow.Total);
        Assert.Equal(1, categoryRow.Count);
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
    /// rest of the suite, since C# skips everything after a failed Assert.Contains). Repopulating a
    /// modest amount of data after recreating (here and in the two tests below) is the same idea one
    /// level up: this test deletes the whole Log sheet, including the fixture's own seeded rows - if
    /// it's the last thing to touch Log in a given run (xUnit doesn't guarantee order), leaving it
    /// merely non-empty-but-bare would undo what the fixture's seed step was for.
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

            await Task.Delay(1500);
            var reseed = new CoreTestSheetEntity();
            reseed.Sheets.Log.AddRange(CoreTestDataSeeder.GenerateLogEntries(CoreCleanSlateFixture.SeededLogCount, CoreCleanSlateFixture.SeedStartRowId, new Random()));
            await Manager!.ChangeSheetData([CoreTestSheetNames.Log], reseed);
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

            // This is the most disruptive of the three sheet-wiping tests - deletes everything the
            // fixture seeded. Repopulate both sheets so the live spreadsheet stays richly populated
            // even if this happens to be the last test to touch it in a given run.
            await Task.Delay(2000);
            var random = new Random();
            var reseed = new CoreTestSheetEntity();
            reseed.Sheets.Items.AddRange(CoreTestDataSeeder.GenerateItems(CoreCleanSlateFixture.SeededItemCount, CoreCleanSlateFixture.SeedStartRowId, random));
            reseed.Sheets.Log.AddRange(CoreTestDataSeeder.GenerateLogEntries(CoreCleanSlateFixture.SeededLogCount, CoreCleanSlateFixture.SeedStartRowId, random));
            await Manager!.ChangeSheetData([CoreTestSheetNames.Items, CoreTestSheetNames.Log], reseed);
        }

        await Task.Delay(2000);

        var tabNames = await Manager!.GetAllSheetTabNames();
        Assert.Contains(CoreTestSheetNames.Items, tabNames);
        Assert.Contains(CoreTestSheetNames.Log, tabNames);
        Assert.Contains(CoreTestSheetNames.Summary, tabNames);
    }

    private async Task<int> GetSheetIdAsync(string sheetName)
    {
        var property = (await Manager!.GetSheetProperties([sheetName]))[0];
        return int.Parse(property.Id);
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
    /// Same self-heal, but on Items - a genuine user-INPUT column, not a formula column, and with
    /// real data already in it. Self-heal restores the column's structure (header/format/note) fully,
    /// but deleting a column deletes its cell content along with it - there is nothing left to
    /// recover the row's prior value from. Both halves are asserted explicitly: structure comes back
    /// correctly configured, data for that pre-existing row does not.
    /// </summary>
    [FactCheckUserSecrets]
    public async Task MissingColumn_OnInputSheet_RestoresStructureButNotData()
    {
        SkipIfNoCredentials();

        var data = new CoreTestSheetEntity();
        data.Sheets.Items.Add(new ItemEntity { RowId = 2, Name = "Wrench", Category = "Hardware", Amount = 42m, Active = true });
        await Manager!.ChangeSheetData([CoreTestSheetNames.Items], data);
        await Task.Delay(2000);

        var itemsSheetId = await GetSheetIdAsync(CoreTestSheetNames.Items);
        var itemsModel = ItemSheetDefinition.GetSheet();
        itemsModel.Headers.UpdateColumns();
        var amountIndex = itemsModel.Headers.First(h => h.Name == "Amount").Index;

        var deleteRequest = new BatchUpdateSpreadsheetRequest
        {
            Requests =
            [
                new Request
                {
                    DeleteDimension = new DeleteDimensionRequest
                    {
                        Range = new DimensionRange { SheetId = itemsSheetId, Dimension = "COLUMNS", StartIndex = amountIndex, EndIndex = amountIndex + 1 }
                    }
                }
            ]
        };
        Assert.True(await Manager!.ExecuteRawBatchUpdateAsync(deleteRequest));
        await Task.Delay(2000);

        var healingRead = await Manager!.GetSheets([CoreTestSheetNames.Items]);
        Assert.Contains(healingRead.Messages, m => m.Message.Contains("Inserting column") && m.Message.Contains("Amount"));
        await Task.Delay(2000);

        // Structure (format/note) is fully restored, matching the original entity's [Column] config.
        // EntitySheetConfigHelper appends a "Cell Format: ..." line to the note for any column with a
        // custom number pattern (see its ApplyNotesAndValidation) - Contains, not Equal, on purpose.
        var structure = await Manager!.GetLiveSheetStructure(CoreTestSheetNames.Items);
        var amountHeader = structure!.Headers.First(h => h.Name == "Amount");
        Assert.Equal(Format.ACCOUNTING, amountHeader.Format);
        Assert.Contains("Enter the amount in USD", amountHeader.Note);

        // But the pre-existing row's Amount value is genuinely gone.
        var afterHeal = await Manager!.GetSheets([CoreTestSheetNames.Items]);
        var wrench = afterHeal.Sheets.Items.FirstOrDefault(i => i.Name == "Wrench");
        Assert.NotNull(wrench);
        Assert.Equal(0m, wrench!.Amount);
    }

    /// <summary>
    /// Deletes two of Summary's three columns at once (right-to-left, same convention
    /// ColumnInsertionHelper itself uses for insertion, so the second delete's index isn't shifted by
    /// the first) and confirms self-heal restores both AND puts them back at their canonical
    /// positions - not just that the data is findable by name somewhere.
    /// </summary>
    [FactCheckUserSecrets]
    public async Task MultipleMissingColumns_OnGetSheets_RestoresAllAtCorrectPositions()
    {
        SkipIfNoCredentials();

        var data = new CoreTestSheetEntity();
        data.Sheets.Items.Add(new ItemEntity { RowId = 2, Name = "Hammer", Category = "Hardware", Amount = 15m, Active = true });
        await Manager!.ChangeSheetData([CoreTestSheetNames.Items], data);
        await Task.Delay(2000);

        var summarySheetId = await GetSheetIdAsync(CoreTestSheetNames.Summary);
        var summaryModel = SummarySheetDefinition.GetSheet();
        summaryModel.Headers.UpdateColumns();
        var totalIndex = summaryModel.Headers.First(h => h.Name == "Total").Index;
        var countIndex = summaryModel.Headers.First(h => h.Name == "Count").Index;

        var deleteRequests = new[] { totalIndex, countIndex }
            .OrderByDescending(i => i)
            .Select(index => new Request
            {
                DeleteDimension = new DeleteDimensionRequest
                {
                    Range = new DimensionRange { SheetId = summarySheetId, Dimension = "COLUMNS", StartIndex = index, EndIndex = index + 1 }
                }
            })
            .ToList();

        Assert.True(await Manager!.ExecuteRawBatchUpdateAsync(new BatchUpdateSpreadsheetRequest { Requests = deleteRequests }));
        await Task.Delay(2000);

        var healingRead = await Manager!.GetSheets([CoreTestSheetNames.Summary]);
        Assert.Contains(healingRead.Messages, m => m.Message.Contains("Inserting column") && m.Message.Contains("Total"));
        Assert.Contains(healingRead.Messages, m => m.Message.Contains("Inserting column") && m.Message.Contains("Count"));
        await Task.Delay(2000);

        var structure = await Manager!.GetLiveSheetStructure(CoreTestSheetNames.Summary);
        var liveOrder = structure!.Headers.OrderBy(h => h.Index).Select(h => h.Name).ToList();
        Assert.Equal(new[] { "Category", "Total", "Count" }, liveOrder);

        var afterHeal = await Manager!.GetSheets([CoreTestSheetNames.Summary]);
        var hardwareRow = Assert.Single(afterHeal.Sheets.Summary, s => s.Category == "Hardware");
        Assert.True(hardwareRow.Total > 0);
        Assert.True(hardwareRow.Count > 0);
    }

    /// <summary>
    /// Simulates a user manually dragging a column to a new position (no deletion, nothing missing) -
    /// a scenario the library has never had live coverage for. Confirms reads stay correct (header
    /// matching is name-based, not positional - see HeaderHelpers.BuildHeaderIndex), that the mismatch
    /// is reported as a warning rather than silently ignored, that the library does NOT auto-correct
    /// the live sheet's order on its own (it only ever writes on request, never surprise-mutates on a
    /// read), and that a subsequent write still lands in the correct columns despite the reorder.
    /// </summary>
    [FactCheckUserSecrets]
    public async Task ColumnsReordered_ReadsAndWritesStayCorrect_LibraryDoesNotAutoCorrectOrder()
    {
        SkipIfNoCredentials();

        var data = new CoreTestSheetEntity();
        data.Sheets.Items.Add(new ItemEntity { RowId = 2, Name = "Level", Category = "Hardware", Amount = 20m, Active = true });
        await Manager!.ChangeSheetData([CoreTestSheetNames.Items], data);
        await Task.Delay(2000);

        var itemsSheetId = await GetSheetIdAsync(CoreTestSheetNames.Items);
        var itemsModel = ItemSheetDefinition.GetSheet();
        itemsModel.Headers.UpdateColumns();
        var categoryIndex = itemsModel.Headers.First(h => h.Name == "Category").Index;
        var amountIndex = itemsModel.Headers.First(h => h.Name == "Amount").Index;

        try
        {
            // Swap Category and Amount's physical positions.
            var moveRequest = new BatchUpdateSpreadsheetRequest
            {
                Requests =
                [
                    new Request
                    {
                        MoveDimension = new MoveDimensionRequest
                        {
                            Source = new DimensionRange { SheetId = itemsSheetId, Dimension = "COLUMNS", StartIndex = categoryIndex, EndIndex = categoryIndex + 1 },
                            DestinationIndex = amountIndex + 1
                        }
                    }
                ]
            };
            Assert.True(await Manager!.ExecuteRawBatchUpdateAsync(moveRequest));
            await Task.Delay(2000);

            var readResult = await Manager!.GetSheets([CoreTestSheetNames.Items]);
            var level = readResult.Sheets.Items.FirstOrDefault(i => i.Name == "Level");
            Assert.NotNull(level);
            Assert.Equal("Hardware", level!.Category);
            Assert.Equal(20m, level.Amount);
            Assert.Contains(readResult.Messages, m => m.Message.Contains("should be"));

            var structure = await Manager!.GetLiveSheetStructure(CoreTestSheetNames.Items);
            var liveOrder = structure!.Headers.OrderBy(h => h.Index).Select(h => h.Name).ToList();
            Assert.NotEqual(new[] { "Name", "Category", "Amount", "Active" }, liveOrder);

            var update = new CoreTestSheetEntity();
            update.Sheets.Items.Add(new ItemEntity { RowId = 2, Name = "Level", Category = "Tools", Amount = 33m, Active = false });
            var writeResult = await Manager!.ChangeSheetData([CoreTestSheetNames.Items], update);
            Assert.Empty(CriticalErrors(writeResult));
            await Task.Delay(2000);

            var finalRead = await Manager!.GetSheets([CoreTestSheetNames.Items]);
            var updated = finalRead.Sheets.Items.FirstOrDefault(i => i.Name == "Level");
            Assert.NotNull(updated);
            Assert.Equal("Tools", updated!.Category);
            Assert.Equal(33m, updated.Amount);
            Assert.False(updated.Active);
        }
        finally
        {
            // Restore canonical order. Critical, not cosmetic: Summary's SUMIF/COUNTIF formulas are
            // always (re)computed from the STATIC canonical entity definition (SummarySheetDefinition
            // never reads the live sheet), so a permanently reordered Items would make every later
            // test's Summary assertions silently sum the wrong physical column - exactly what caused
            // a cascading failure across unrelated tests during development of this test. Delete and
            // recreate rather than trying to reverse-engineer MoveDimensionRequest's exact
            // before/after-removal index semantics (genuinely easy to get subtly wrong) - this reuses
            // the delete/self-heal path already proven reliable elsewhere in this suite and guarantees
            // exact canonical order regardless.
            await Manager!.DeleteSheets([CoreTestSheetNames.Items]);
            await Task.Delay(2000);
            await Manager!.GetSheets([CoreTestSheetNames.Items]); // triggers self-heal recreation
            await Task.Delay(2000);

            var restoredStructure = await Manager!.GetLiveSheetStructure(CoreTestSheetNames.Items);
            var restoredOrder = restoredStructure!.Headers.OrderBy(h => h.Index).Select(h => h.Name).ToList();
            Assert.Equal(new[] { "Name", "Category", "Amount", "Active" }, restoredOrder);

            // The delete above also wiped the fixture's seeded Items rows - repopulate for the same
            // reason as this suite's other sheet-wiping tests (see their own comments).
            var reseed = new CoreTestSheetEntity();
            reseed.Sheets.Items.AddRange(CoreTestDataSeeder.GenerateItems(CoreCleanSlateFixture.SeededItemCount, CoreCleanSlateFixture.SeedStartRowId, new Random()));
            await Manager!.ChangeSheetData([CoreTestSheetNames.Items], reseed);
        }
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

        try
        {
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
        finally
        {
            // This test's own DeleteSheets wiped the fixture's seeded Items rows along with the
            // "Nut" row it wrote itself. Repopulate so the live sheet stays richly populated even if
            // this happens to be the last test to touch Items in a given run.
            var reseed = new CoreTestSheetEntity();
            reseed.Sheets.Items.AddRange(CoreTestDataSeeder.GenerateItems(CoreCleanSlateFixture.SeededItemCount, CoreCleanSlateFixture.SeedStartRowId, new Random()));
            await Manager!.ChangeSheetData([CoreTestSheetNames.Items], reseed);
        }
    }

    /// <summary>
    /// Verifies the fixture's own seed step (see CoreCleanSlateFixture.SeedAsync) rather than writing
    /// its own dataset from scratch - the "large batch write" proof (still just ONE
    /// BatchUpdateSpreadsheet call regardless of row count, see GoogleRequestHelpers.
    /// CreateUpdateCellRequests) now lives there instead, so the rest of this suite runs against a
    /// realistically-populated sheet from the start rather than only after this one test finishes.
    /// This is deliberately read-only - the seed's own count/random-amount guarantees are asserted
    /// here, not re-created.
    /// </summary>
    [FactCheckUserSecrets]
    public async Task SeededDataset_ReadsBackCorrectly_AndSummaryAggregatesAcrossManyCategories()
    {
        SkipIfNoCredentials();

        var readStart = DateTime.UtcNow;
        var readResult = await Manager!.GetSheets([CoreTestSheetNames.Items, CoreTestSheetNames.Summary]);
        var readElapsed = DateTime.UtcNow - readStart;

        Assert.True(readElapsed.TotalSeconds < 30,
            $"Reading the seeded dataset should complete within 30s, took {readElapsed.TotalSeconds:F1}s");

        var seededItems = readResult.Sheets.Items.Where(i => i.RowId >= CoreCleanSlateFixture.SeedStartRowId).ToList();
        Assert.True(seededItems.Count >= CoreCleanSlateFixture.SeededItemCount / 2,
            $"Expected at least half the {CoreCleanSlateFixture.SeededItemCount} seeded rows to still be present, found {seededItems.Count}");

        // Amounts are randomized (see CoreTestDataSeeder), not a suspicious linear/increasing
        // sequence - confirm that live, not just by reading the generator's own code.
        var distinctAmounts = seededItems.Select(i => i.Amount).Distinct().Count();
        Assert.True(distinctAmounts > seededItems.Count / 2,
            $"Seeded amounts look too repetitive to be random: {distinctAmounts} distinct values across {seededItems.Count} rows");

        // Summary should aggregate across every category the randomized seed actually used, not just
        // whatever one category individual scenario tests happen to write to RowId 2.
        Assert.True(readResult.Sheets.Summary.Count > 1,
            $"Expected Summary to show more than one category from the seeded dataset, found {readResult.Sheets.Summary.Count}");
        Assert.All(readResult.Sheets.Summary, s => Assert.True(s.Total >= 0));
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

    /// <summary>
    /// Simulates a user manually inserting their own extra column in the middle of Items (with their
    /// own header text and a value on the existing row) - never tested before, live or mocked.
    /// Confirms known columns keep reading/writing correctly despite the shift, and that the unknown
    /// column is flagged (CheckExtraColumns) rather than silently dropped or deleted. What a
    /// subsequent write does to the unknown column's own value is investigated, not assumed - see
    /// the assertion's own comment for what was actually found.
    /// </summary>
    [FactCheckUserSecrets]
    public async Task ExtraUnexpectedColumn_IsFlaggedNotRemoved_KnownColumnsUnaffected()
    {
        SkipIfNoCredentials();

        var data = new CoreTestSheetEntity();
        data.Sheets.Items.Add(new ItemEntity { RowId = 2, Name = "Level", Category = "Hardware", Amount = 20m, Active = true });
        await Manager!.ChangeSheetData([CoreTestSheetNames.Items], data);
        await Task.Delay(2000);

        var itemsSheetId = await GetSheetIdAsync(CoreTestSheetNames.Items);

        // Insert a blank column at index 2 (between Category and Amount), then give it a header and
        // a value on row 2 in one follow-up write - simulating a user adding their own column by hand.
        var insertRequest = new BatchUpdateSpreadsheetRequest
        {
            Requests =
            [
                new Request
                {
                    InsertDimension = new InsertDimensionRequest
                    {
                        Range = new DimensionRange { SheetId = itemsSheetId, Dimension = "COLUMNS", StartIndex = 2, EndIndex = 3 },
                        InheritFromBefore = false
                    }
                }
            ]
        };
        Assert.True(await Manager!.ExecuteRawBatchUpdateAsync(insertRequest));
        await Task.Delay(1000);

        var writeCommentRequest = new BatchUpdateSpreadsheetRequest
        {
            Requests =
            [
                new Request
                {
                    UpdateCells = new UpdateCellsRequest
                    {
                        Fields = "userEnteredValue",
                        Range = new GridRange { SheetId = itemsSheetId, StartRowIndex = 0, EndRowIndex = 2, StartColumnIndex = 2, EndColumnIndex = 3 },
                        Rows =
                        [
                            new RowData { Values = [new CellData { UserEnteredValue = new ExtendedValue { StringValue = "Comments" } }] },
                            new RowData { Values = [new CellData { UserEnteredValue = new ExtendedValue { StringValue = "user note" } }] }
                        ]
                    }
                }
            ]
        };
        Assert.True(await Manager!.ExecuteRawBatchUpdateAsync(writeCommentRequest));
        await Task.Delay(2000);

        try
        {
            // Known columns still read correctly despite the shift; the unknown one is flagged, not
            // erased. Google's ACCOUNTING-adjacent formatting pads raw text with spaces to visually
            // align it with numeric columns - Trim before comparing raw cell values (Amount's own
            // "$ 20.00" is padded the same way, confirming this isn't specific to the new column).
            var readResult = await Manager!.GetSheets([CoreTestSheetNames.Items]);
            var level = readResult.Sheets.Items.FirstOrDefault(i => i.Name == "Level");
            Assert.NotNull(level);
            Assert.Equal("Hardware", level!.Category);
            Assert.Equal(20m, level.Amount);
            Assert.Contains(readResult.Messages, m => m.Message.Contains("Extra column") && m.Message.Contains("Comments"));

            var beforeWrite = await Manager!.GetLiveSheetRawValues(CoreTestSheetNames.Items);
            Assert.Contains(beforeWrite[1], v => v?.Trim() == "user note");

            // FINDING (confirmed live): a normal write DOES clear an unrecognized column's existing
            // value. GenerateUpdateCellsRequest's field mask is "userEnteredValue" only, and the empty
            // CellData GenericSheetMapper.MapToRowData writes for any non-input header has no
            // UserEnteredValue set - Google treats "field is in the mask but absent from the payload"
            // as "clear it", not "leave it alone". The placeholder's own comment ("to preserve column
            // position") is accurate about the column's structural position surviving, but misleading
            // about the CELL'S VALUE - that does not survive. Concretely: if a user manually adds their
            // own column with real data, the next ordinary write to that same row (through any known
            // column) silently wipes out the user's column too, not just formula/output columns.
            var update = new CoreTestSheetEntity();
            update.Sheets.Items.Add(new ItemEntity { RowId = 2, Name = "Level", Category = "Hardware", Amount = 25m, Active = true });
            await Manager!.ChangeSheetData([CoreTestSheetNames.Items], update);
            await Task.Delay(2000);

            var afterWrite = await Manager!.GetLiveSheetRawValues(CoreTestSheetNames.Items);
            Assert.DoesNotContain(afterWrite[1], v => v?.Trim() == "user note");
        }
        finally
        {
            // Clean up the inserted column so it doesn't linger and corrupt every later test's
            // column-position assumptions - this exact kind of oversight (a mutating test not
            // restoring state) caused a previous cascading failure in this suite. The longer delay
            // (matching every other structural-change test here) gives Google's automatic cross-sheet
            // formula-reference adjustment (Summary's SUMIF/COUNTIF ranges into Items) time to
            // resettle before the next sequential test starts - 1000ms wasn't enough and caused a
            // second round of cascading failures during development.
            var deleteCommentColumn = new BatchUpdateSpreadsheetRequest
            {
                Requests = [new Request { DeleteDimension = new DeleteDimensionRequest
                {
                    Range = new DimensionRange { SheetId = itemsSheetId, Dimension = "COLUMNS", StartIndex = 2, EndIndex = 3 }
                }}]
            };
            await Manager!.ExecuteRawBatchUpdateAsync(deleteCommentColumn);
            await Task.Delay(2000);
        }
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
/// Deletes and recreates every canonical sheet, seeds a large randomized baseline dataset, once before
/// the collection's tests run. Safe because spreadsheets:test:core is configured to point at its own
/// dedicated, empty test spreadsheet - never shared with a domain or with anyone's real data.
/// </summary>
public class CoreCleanSlateFixture : CleanSlateSheetFixture<CoreTestSheetEntity, CoreTestManager>
{
    /// <summary>
    /// How many Items/Log rows the fixture seeds - also the "large batch write" proof (one
    /// BatchUpdateSpreadsheet call regardless of row count - see GoogleRequestHelpers.
    /// CreateUpdateCellRequests) that used to be a standalone test. Items stays under the sheet's
    /// default ~1,000-row grid to avoid the append/update classification boundary - see #101.
    /// </summary>
    public const int SeededItemCount = 600;
    public const int SeededLogCount = 40;

    /// <summary>
    /// RowId 2-9 is reserved for individual tests' own scratch usage (only RowId 2 is used today) -
    /// seed data starts at 10 so it never collides with a specific-scenario test's own write.
    /// </summary>
    public const int SeedStartRowId = 10;

    public CoreCleanSlateFixture() : base(
        TestConfigurationHelpers.GetCoreSpreadsheet(),
        (credential, spreadsheetId) => new CoreTestManager(credential, spreadsheetId),
        SeedAsync)
    {
    }

    private static async Task SeedAsync(CoreTestManager manager)
    {
        var random = new Random();
        var seed = new CoreTestSheetEntity();
        seed.Sheets.Items.AddRange(CoreTestDataSeeder.GenerateItems(SeededItemCount, SeedStartRowId, random));
        seed.Sheets.Log.AddRange(CoreTestDataSeeder.GenerateLogEntries(SeededLogCount, SeedStartRowId, random));
        await manager.ChangeSheetData([CoreTestSheetNames.Items, CoreTestSheetNames.Log], seed);
    }
}
