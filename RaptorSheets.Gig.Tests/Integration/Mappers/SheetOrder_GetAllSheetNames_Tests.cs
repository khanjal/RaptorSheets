using RaptorSheets.Gig.Constants;

namespace RaptorSheets.Gig.Tests.Integration.Mappers;

public class SheetOrder_GetAllSheetNames_Tests
{
    [Fact]
    public void GetAllSheetNames_ReturnsExpectedOrder()
    {
        // Arrange: expected order matches SheetEntity property order
        var expected = new List<string>
        {
            SheetsConfig.SheetNames.Trips,
            SheetsConfig.SheetNames.Shifts,
            SheetsConfig.SheetNames.Expenses,
            SheetsConfig.SheetNames.Addresses,
            SheetsConfig.SheetNames.Names,
            SheetsConfig.SheetNames.Places,
            SheetsConfig.SheetNames.Regions,
            SheetsConfig.SheetNames.Services,
            SheetsConfig.SheetNames.Types,
            SheetsConfig.SheetNames.Daily,
            SheetsConfig.SheetNames.Weekdays,
            SheetsConfig.SheetNames.Weekly,
            SheetsConfig.SheetNames.Monthly,
            SheetsConfig.SheetNames.Yearly,
            SheetsConfig.SheetNames.Setup
        };

        // Act
        var actual = SheetsConfig.SheetUtilities.GetAllSheetNames();

        // Assert
        Assert.Equal(expected, actual);
    }
}
