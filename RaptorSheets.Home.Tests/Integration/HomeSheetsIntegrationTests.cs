using System.ComponentModel;
using RaptorSheets.Home.Constants;
using RaptorSheets.Home.Entities;
using RaptorSheets.Home.Tests.Data.Attributes;
using RaptorSheets.Home.Tests.Integration.Base;

namespace RaptorSheets.Home.Tests.Integration;

/// <summary>
/// Integration tests that actually write to (and read back from) a live Home Google Sheet.
/// Skipped automatically unless credentials and a Home spreadsheet ID are configured in user secrets
/// (add "spreadsheets:home" alongside the existing "spreadsheets:gig"/"spreadsheets:stock").
/// </summary>
[Category("Integration")]
public class HomeSheetsIntegrationTests : IntegrationTestBase
{
    [FactCheckUserSecrets]
    public async Task WriteThenRead_Rooms_Contacts_Appliances_RoundTrips()
    {
        // Arrange - make sure the sheets we're writing to exist
        SkipIfNoCredentials();
        Assert.True(await EnsureSheetsExist(TestSheets), "Failed to ensure Home sheets exist");

        var data = BuildTestData();

        // Act - write the data
        var writeResult = await GoogleSheetManager!.ChangeSheetData(TestSheets, data);

        // Assert - the write itself had no critical errors
        var writeErrors = CriticalErrors(writeResult);
        Assert.True(writeErrors.Count == 0,
            $"Write had critical errors: {string.Join("; ", writeErrors.Select(e => e.Message))}");

        // Give Google Sheets a moment to recalc the array-formula columns
        await Task.Delay(2000);

        // Act - read the data back
        var readResult = await GoogleSheetManager!.GetSheets(TestSheets);

        // Assert - rows round-tripped
        var livingRoom = readResult.Sheets.Rooms.FirstOrDefault(r => r.Room == "Living Room");
        Assert.NotNull(livingRoom);
        // Sq. Ft is a calculated column: 15 x 12 = 180
        Assert.Equal(180m, livingRoom!.SquareFeet);

        Assert.Contains(readResult.Sheets.Contacts, c => c.Name == "Ace Plumbing");

        var fridge = readResult.Sheets.Appliances.FirstOrDefault(a => a.Type == "Refrigerator");
        Assert.NotNull(fridge);
        // Next Filter is calculated (Filter Date + Rpl. Mth), so it should be populated
        Assert.False(string.IsNullOrWhiteSpace(fridge!.NextFilter));
    }

    private static SheetEntity BuildTestData()
    {
        var data = new SheetEntity();

        // Rooms - RowId starts at 2 (row after the header) so rows are written by position
        data.Sheets.Rooms.Add(new RoomEntity { RowId = 2, Room = "Living Room", Length = 15, Width = 12, Level = "Main" });
        data.Sheets.Rooms.Add(new RoomEntity { RowId = 3, Room = "Kitchen", Length = 12, Width = 10, Level = "Main" });
        data.Sheets.Rooms.Add(new RoomEntity { RowId = 4, Room = "Primary Bedroom", Length = 14, Width = 13, Level = "Upper" });

        // Contacts
        data.Sheets.Contacts.Add(new ContactEntity { RowId = 2, Name = "Ace Plumbing", Number = "555-0100", Description = "Plumber", Retired = false });
        data.Sheets.Contacts.Add(new ContactEntity { RowId = 3, Name = "Bright Spark Electric", Number = "555-0111", Description = "Electrician", Retired = false });

        // Appliances - Location references a Room; Next Filter is derived from Filter Date + Rpl. Mth
        data.Sheets.Appliances.Add(new ApplianceEntity
        {
            RowId = 2,
            Type = "Refrigerator",
            Location = "Kitchen",
            Manufacturer = "LG",
            Model = "LRFVS3006S",
            FilterDate = "2026-01-15",
            ReplacementMonths = 6,
            OriginalPrice = 1899.99m
        });

        return data;
    }
}
