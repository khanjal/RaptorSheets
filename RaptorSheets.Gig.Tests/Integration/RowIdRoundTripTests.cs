using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Services;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Helpers;
using RaptorSheets.Gig.Tests.Data.Attributes;
using RaptorSheets.Gig.Tests.Integration.Base;
using Xunit;

namespace RaptorSheets.Gig.Tests.Integration;

/// <summary>
/// Round-trip coverage: write, read back, edit what was read, write again, read again.
///
/// Every integration test in this suite so far checks one direction. That is how a coordinate-system
/// disagreement survives - it only shows when a value produced by the read path is fed back into the
/// write path, and nothing did that.
///
/// The specific question here: <c>GenericSheetMapper</c> filters blank rows out *before* numbering,
/// so RowId is a position in the non-empty sequence, not a physical sheet row. Writes are positional
/// (<c>GenerateUpdateCellsRequest(sheetId, entity.RowId - 1, ...)</c>). On a sheet whose data sits
/// below a gap - which every sheet written between #104 and #133 has - those two used to disagree,
/// and an edit landed on the wrong physical row, duplicating the record instead of updating it.
///
/// Fixed in #134 by numbering blank rows rather than filtering them out before numbering. This test
/// builds the gapped layout deliberately rather than waiting to encounter one, and failed with
/// "the collection contained 2 items" against the unfixed mapper.
/// </summary>
[Collection("GigSheetsIntegration")]
public class RowIdRoundTripTests : IntegrationTestBase
{
    private readonly GigCleanSlateFixture _fixture;

    public RowIdRoundTripTests(GigCleanSlateFixture fixture) : base(fixture)
    {
        _fixture = fixture;
    }

    [FactCheckUserSecrets]
    public async Task EditingARowReadBackFromBelowAGap_UpdatesItRatherThanDuplicatingIt()
    {
        SkipIfNoCredentials();

        var sheet = SheetsConfig.SheetNames.Trips;
        var marker = $"RoundTrip-{Guid.NewGuid():N}"[..18];

        // A modest gap is enough to separate physical from logical position; it does not need to be
        // the full 1,000 rows the original bug produced.
        const int gapRows = 25;

        // Write the row through the normal path, then push it down by inserting blank rows above it.
        // Building the gap this way rather than retargeting a generated request keeps the test
        // independent of how appends happen to be issued.
        var seed = new SheetEntity();
        seed.Sheets.Trips.Add(new TripEntity
        {
            RowId = int.MaxValue, // any row past the extent - the write path decides where it lands
            Date = DateTime.Today.ToString("yyyy-MM-dd"),
            Service = marker,
            Name = "before"
        });

        await SheetManager!.ChangeSheetData([sheet], seed);
        await Task.Delay(2000);

        var placed = Assert.Single((await SheetManager.GetSheets([sheet])).Sheets.Trips, t => t.Service == marker);
        var rowBeforeGap = placed.RowId;

        var sheetId = int.Parse((await SheetManager.GetSheetProperties([sheet]))[0].Id);
        var insertGap = new Request
        {
            InsertDimension = new InsertDimensionRequest
            {
                Range = new DimensionRange
                {
                    SheetId = sheetId,
                    Dimension = Dimension.ROWS.GetDescription(),
                    StartIndex = rowBeforeGap - 1,
                    EndIndex = rowBeforeGap - 1 + gapRows
                }
            }
        };

        var rawService = new GoogleSheetService(_fixture.Credential, _fixture.SpreadsheetId);
        Assert.NotNull(await rawService.BatchUpdateSpreadsheet(new BatchUpdateSpreadsheetRequest { Requests = [insertGap] }, default));
        await Task.Delay(2000);

        var physicalRow = rowBeforeGap + gapRows;

        // Read: RowId must be the row's physical position, gap or no gap. This is the invariant the
        // write path depends on - before #134 the mapper renumbered past the gap and under-reported it.
        var readBack = Assert.Single((await SheetManager.GetSheets([sheet])).Sheets.Trips, t => t.Service == marker);

        Assert.Equal(physicalRow, readBack.RowId);

        // Edit exactly what was read, the way the app does, and write it back.
        readBack.Name = "after";
        var edit = new SheetEntity();
        edit.Sheets.Trips.Add(readBack);

        await SheetManager.ChangeSheetData([sheet], edit);
        await Task.Delay(2000);

        // The record should have been updated in place. If the write followed the logical RowId to a
        // different physical row, the original is still sitting below the gap and there are now two.
        var afterEdit = await SheetManager.GetSheets([sheet]);
        var updated = Assert.Single(afterEdit.Sheets.Trips, t => t.Service == marker);

        Assert.Equal("after", updated.Name);
        Assert.Equal(physicalRow, updated.RowId);
    }
}
