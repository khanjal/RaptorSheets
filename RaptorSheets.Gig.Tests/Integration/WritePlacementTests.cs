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
/// Deliberately small and order-independent: it measures the extent before and after its own write
/// rather than assuming a clean sheet, so it neither depends on nor contributes to the shared-state
/// coupling tracked in #130. Bulk-volume behaviour belongs in a load test, not here.
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
