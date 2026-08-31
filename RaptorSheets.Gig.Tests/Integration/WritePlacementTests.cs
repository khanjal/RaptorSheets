using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Tests.Integration.Base;
using RaptorSheets.Gig.Tests.Data.Attributes;
using Xunit;

namespace RaptorSheets.Gig.Tests.Integration;

/// <summary>
/// Light integration coverage for *where* written rows physically land - the one thing no unit test
/// can answer, because unit tests assert on the requests we build rather than on what Google does
/// with them.
///
/// This exists because of a regression that survived a full green unit suite for three weeks. A
/// header ARRAYFORMULA over an open-ended range (A1:A) writes "" into every row of the default
/// 1000-row grid. An empty string is a value to Google but not to
/// SheetPropertyHelper.FindLastRowWithData, so the library read a fresh sheet's data extent as row 1
/// while Google read it as row 1000 - and AppendCells, which places rows after *Google's* extent,
/// dropped every appended row at 1001. Demo data on a new spreadsheet appeared below a thousand
/// blank rows.
///
/// Deliberately small, and the assertions are deltas - extent measured before and after its own
/// write - so these tests cannot false-fail on whatever a previous test left behind (#130).
///
/// One caveat worth keeping in mind before reordering anything: detecting *this* bug needs the sheet
/// to still be fresh. The disagreement only exists while the library reads the extent as row 1 and
/// Google reads it as 1000; once real rows exist past 1000 the two agree and a regression here would
/// pass unnoticed. The clean-slate fixture supplies that precondition by recreating every sheet
/// before the collection runs. If these ever stop running against a freshly created sheet, they keep
/// passing while covering nothing - so give them their own sheet rather than letting them drift down
/// the run order. Verified failing against the pre-fix code at Actual: 1002 and Actual: 1100.
///
/// Bulk-volume behaviour belongs in a load test, not here.
/// </summary>
[Collection("GigSheetsIntegration")]
public class WritePlacementTests : IntegrationTestBase
{
    public WritePlacementTests(GigCleanSlateFixture fixture) : base(fixture)
    {
    }

    [FactCheckUserSecrets]
    public async Task SavedRows_ShouldLandImmediatelyAfterTheExistingData_NotBelowTheEmptyGrid()
    {
        SkipIfNoCredentials();

        var sheet = SheetsConfig.SheetNames.Trips;
        var extentBefore = await GetDataExtentAsync(sheet);

        // RowIds continue from the current extent, so these are appends whatever ran before us.
        var trips = new List<TripEntity>
        {
            new() { RowId = extentBefore + 1, Date = DateTime.Today.ToString("yyyy-MM-dd"), Service = "PlacementCheck" },
            new() { RowId = extentBefore + 2, Date = DateTime.Today.ToString("yyyy-MM-dd"), Service = "PlacementCheck" }
        };

        var entity = new SheetEntity();
        entity.Sheets.Trips.AddRange(trips);

        await SheetManager!.ChangeSheetData([sheet], entity);
        await Task.Delay(2000); // let the write and dependent formulas settle

        var extentAfter = await GetDataExtentAsync(sheet);

        // Contiguous: two rows written means the extent grows by exactly two. The regression this
        // guards against would land them at 1001+, pushing the extent far past extentBefore + 2.
        Assert.Equal(extentBefore + trips.Count, extentAfter);
    }

    [FactCheckUserSecrets]
    public async Task ARealisticBatch_ShouldLandContiguouslyAndInOrder()
    {
        SkipIfNoCredentials();

        // A realistic batch rather than two hand-built rows: enough to exercise contiguous block
        // placement and row ordering at a shape where alignment can actually go wrong, while staying
        // well under the 1,000-row grid boundary that belongs to the load tier.
        //
        // Rows come from CreateSimpleTestData (the production DemoHelpers generator) so the values
        // are the same shape real data has, rather than a stub that could quietly stop resembling it.
        // Only RowId and two marker fields are overridden - RowId to make these appends whatever ran
        // before, Service to find them again on read-back, and Name to carry the ordinal that proves
        // the block kept its order.
        var sheet = SheetsConfig.SheetNames.Trips;
        var extentBefore = await GetDataExtentAsync(sheet);
        var marker = $"Place-{Guid.NewGuid():N}"[..14];

        var trips = CreateSimpleTestData(shifts: 20, tripsPerShift: 5, expenses: 0)
            .Sheets.Trips
            .Take(100)
            .ToList();

        // The generator's volume varies with its date range, so assert on what it actually produced
        // rather than a hardcoded count - but fail loudly if it is too small to be a realistic batch.
        Assert.True(trips.Count >= 25, $"Expected a realistic batch, generator produced {trips.Count} trips");

        for (var i = 0; i < trips.Count; i++)
        {
            trips[i].RowId = extentBefore + 1 + i;
            trips[i].Service = marker;
            trips[i].Name = i.ToString();
        }

        var entity = new SheetEntity();
        entity.Sheets.Trips.AddRange(trips);

        await SheetManager!.ChangeSheetData([sheet], entity);
        await Task.Delay(3000); // let the write and dependent formulas settle

        var extentAfter = await GetDataExtentAsync(sheet);
        Assert.Equal(extentBefore + trips.Count, extentAfter);

        // Read back and confirm the block is intact and in the order it was written - a contiguous
        // extent alone would not catch rows landing shuffled within the block.
        var readBack = await SheetManager.GetSheets([sheet]);
        var written = readBack.Sheets.Trips.Where(t => t.Service == marker).ToList();

        Assert.Equal(trips.Count, written.Count);
        Assert.Equal(
            Enumerable.Range(0, trips.Count).Select(i => i.ToString()).ToList(),
            written.Select(t => t.Name).ToList());
    }

    /// <summary>
    /// The sheet's real last populated row, as the library computes it (blank formula output does
    /// not count) - the same value the append/update split keys off.
    /// </summary>
    private async Task<int> GetDataExtentAsync(string sheet)
    {
        var properties = await SheetManager!.GetSheetProperties([sheet]);
        var property = properties.Single(p => string.Equals(p.Name, sheet, StringComparison.OrdinalIgnoreCase));

        return int.Parse(property.Attributes[Property.MAX_ROW_VALUE.GetDescription()]);
    }
}
