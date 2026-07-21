using RaptorSheets.Job.Constants;

namespace RaptorSheets.Job.Tests.Unit;

public class SheetsConfigTests
{
    [Fact]
    public void SheetNames_HaveExpectedValues()
    {
        Assert.Equal("Applications", SheetsConfig.SheetNames.Applications);
        Assert.Equal("Interviews", SheetsConfig.SheetNames.Interviews);
        Assert.Equal("Job Title", SheetsConfig.HeaderNames.JobTitle);
    }

    [Fact]
    public void GetAllSheetNames_ReturnsOrderedList()
    {
        var names = SheetsConfig.SheetUtilities.GetAllSheetNames();

        Assert.NotEmpty(names);
        Assert.Equal(SheetsConfig.SheetNames.Applications, names.First());
        Assert.Contains(SheetsConfig.SheetNames.Interviews, names);
        Assert.Equal(SheetsConfig.SheetNames.Setup, names.Last());
    }

    [Fact]
    public void SheetOrder_IsSynchronizedWithConstants()
    {
        Assert.Empty(SheetsConfig.SheetUtilities.ValidateSheetOrderCompleteness());
    }

    [Fact]
    public void GetSheetIndex_ReturnsZeroForApplications_CaseInsensitive()
    {
        Assert.Equal(0, SheetsConfig.SheetUtilities.GetSheetIndex("applications"));
    }

    [Fact]
    public void GetSheetIndex_InvalidName_Throws()
    {
        Assert.Throws<ArgumentException>(() => SheetsConfig.SheetUtilities.GetSheetIndex("InvalidSheet"));
    }

    [Fact]
    public void IsValidSheetName_WorksForKnownAndUnknown()
    {
        Assert.True(SheetsConfig.SheetUtilities.IsValidSheetName(SheetsConfig.SheetNames.Applications));
        Assert.False(SheetsConfig.SheetUtilities.IsValidSheetName("InvalidSheet"));
    }
}
