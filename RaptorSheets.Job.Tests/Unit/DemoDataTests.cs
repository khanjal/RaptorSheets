using RaptorSheets.Job.Managers;

namespace RaptorSheets.Job.Tests.Unit;

/// <summary>
/// Demo generation runs without any Google credentials, so it can be validated in a plain unit test.
/// </summary>
public class DemoDataTests
{
    [Fact]
    public void GenerateDemoData_ProducesApplicationsAndInterviews()
    {
        var manager = new GoogleSheetManager("fake-token", "fake-id");

        var demo = manager.GenerateDemoData(DateTime.Today.AddDays(-30), DateTime.Today, seed: 42);

        Assert.NotEmpty(demo.Sheets.Applications);
        Assert.NotEmpty(demo.Sheets.Interviews);

        // Rows are numbered from 2 (row 1 is the header) so a write lands them below the header.
        Assert.Equal(2, demo.Sheets.Applications.First().RowId);
        Assert.All(demo.Sheets.Applications, a => Assert.False(string.IsNullOrEmpty(a.Company)));
        Assert.All(demo.Sheets.Interviews, i => Assert.False(string.IsNullOrEmpty(i.Company)));
    }

    [Fact]
    public void GenerateDemoData_IsDeterministicForSameSeed()
    {
        var manager = new GoogleSheetManager("fake-token", "fake-id");

        var a = manager.GenerateDemoData(DateTime.Today.AddDays(-30), DateTime.Today, seed: 7);
        var b = manager.GenerateDemoData(DateTime.Today.AddDays(-30), DateTime.Today, seed: 7);

        Assert.Equal(a.Sheets.Applications.Count, b.Sheets.Applications.Count);
        Assert.Equal(a.Sheets.Interviews.Count, b.Sheets.Interviews.Count);
    }
}
