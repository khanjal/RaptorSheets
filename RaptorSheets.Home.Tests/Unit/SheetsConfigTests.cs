using RaptorSheets.Home.Constants;

namespace RaptorSheets.Home.Tests.Unit;

public class SheetsConfigTests
{
    [Fact]
    public void GetAllSheetNames_ReturnsAllNineSheets()
    {
        var names = SheetsConfig.SheetUtilities.GetAllSheetNames();

        Assert.Equal(9, names.Count);
        Assert.Equal(SheetsConfig.SheetNames.Appliances, names.First());
        Assert.Equal(SheetsConfig.SheetNames.Stats, names.Last());
    }

    [Fact]
    public void SheetOrder_IsSynchronizedWithConstants()
    {
        var errors = SheetsConfig.SheetUtilities.ValidateSheetOrderCompleteness();

        Assert.Empty(errors);
    }

    [Theory]
    [InlineData("Rooms")]
    [InlineData("appliances & electronics")]
    [InlineData("STATS")]
    public void IsValidSheetName_IsCaseInsensitive(string name)
    {
        Assert.True(SheetsConfig.SheetUtilities.IsValidSheetName(name));
    }

    [Fact]
    public void IsValidSheetName_ReturnsFalseForUnknown()
    {
        Assert.False(SheetsConfig.SheetUtilities.IsValidSheetName("Cameras"));
    }
}
