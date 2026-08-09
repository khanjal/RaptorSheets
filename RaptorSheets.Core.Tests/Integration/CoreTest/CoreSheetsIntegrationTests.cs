using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
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
/// This file holds only the genuinely Core-schema-specific tests - Summary's actual computed totals
/// and the large seeded dataset's own shape. Every structural/mechanical scenario (delete/recreate a
/// sheet, self-heal, column reorder, extra columns, reapply-formatting) was generalized into
/// <see cref="SheetPlumbingTestsBase{TEntity, TManager}"/> (RaptorSheets.Test.Common) so Gig/Job/Home/
/// Stock can reuse it against their own real schemas instead of each re-authoring the same raw
/// batch-update orchestration - see <see cref="CorePlumbingTests"/> for Core's own adapter.
///
/// Skipped automatically unless credentials and "spreadsheets:test:core" are configured in user
/// secrets (RaptorSheets.Test.Common's shared secrets store - never spreadsheets:live:*). Collection
/// fixture (<see cref="CoreCleanSlateFixture"/>) deletes/recreates every sheet before tests run.
/// </summary>
[Collection("CoreSheetsIntegration")]
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
    public async Task Summary_Total_HasFormulaReferencingItems()
    {
        SkipIfNoCredentials();

        var structures = await Manager!.GetAllLiveSheetStructures();

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
