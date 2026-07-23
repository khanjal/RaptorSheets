using RaptorSheets.Home.Constants;
using RaptorSheets.Home.Helpers;
using RaptorSheets.Home.Sheets;

namespace RaptorSheets.Home.Tests.Unit;

public class SheetLayoutTests
{
    [Fact]
    public void Registry_HasEveryConfiguredSheet()
    {
        foreach (var name in SheetsConfig.SheetUtilities.GetAllSheetNames())
        {
            Assert.True(HomeSheetHelpers.Registry.IsRegistered(name), $"Sheet '{name}' is not registered");
            Assert.NotNull(HomeSheetHelpers.GetSheetLayout(name));
        }
    }

    [Fact]
    public void RoomSheet_SquareFeet_IsCalculatedFromLengthAndWidth()
    {
        var sheet = RoomSheet.GetSheet();
        var squareFeet = sheet.Headers.Single(h => h.Name == SheetsConfig.HeaderNames.SquareFeet);

        Assert.False(string.IsNullOrEmpty(squareFeet.Formula));
        Assert.Contains("ARRAYFORMULA", squareFeet.Formula);
        Assert.Contains("*", squareFeet.Formula);
    }

    [Fact]
    public void ApplianceSheet_NextFilter_IsCalculatedFromFilterDateAndMonths()
    {
        var sheet = ApplianceSheet.GetSheet();
        var nextFilter = sheet.Headers.Single(h => h.Name == SheetsConfig.HeaderNames.NextFilter);

        Assert.False(string.IsNullOrEmpty(nextFilter.Formula));
        Assert.Contains("EDATE", nextFilter.Formula);
    }

    [Fact]
    public void ApplianceSheet_Location_HasRoomDropdownValidation()
    {
        var sheet = ApplianceSheet.GetSheet();
        var location = sheet.Headers.Single(h => h.Name == SheetsConfig.HeaderNames.Location);

        Assert.Equal(SheetsConfig.ValidationNames.RangeRoom, location.Validation);
    }

    [Fact]
    public void MaintenanceSheet_HasSeparateSolutionAndAmountColumns()
    {
        var sheet = HomeSheetHelpers.GetSheetLayout(SheetsConfig.SheetNames.Maintenance)!;
        var names = sheet.Headers.Select(h => h.Name).ToList();

        Assert.Contains(SheetsConfig.HeaderNames.Solution, names);
        Assert.Contains(SheetsConfig.HeaderNames.Amount, names);
    }
}
