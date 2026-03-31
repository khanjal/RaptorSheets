using RaptorSheets.Job.Constants;

namespace RaptorSheets.Job.Tests.Unit.Constants;

public class SheetsConfigTests
{
    [Fact]
    public void SheetNames_ShouldHaveApplications()
    {
        // Assert
        SheetsConfig.SheetNames.Applications.Should().Be("Applications");
    }

    [Fact]
    public void SheetNames_ShouldHaveInterviews()
    {
        // Assert
        SheetsConfig.SheetNames.Interviews.Should().Be("Interviews");
    }

    [Fact]
    public void HeaderNames_ShouldHaveCommonHeaders()
    {
        // Assert
        SheetsConfig.HeaderNames.Date.Should().Be("Date");
        SheetsConfig.HeaderNames.Company.Should().Be("Company");
        SheetsConfig.HeaderNames.JobTitle.Should().Be("Job Title");
    }

    [Fact]
    public void GetAllSheetNames_ShouldReturnOrderedList()
    {
        // Act
        var sheetNames = SheetsConfig.SheetUtilities.GetAllSheetNames();

        // Assert
        sheetNames.Should().NotBeEmpty();
        sheetNames.First().Should().Be(SheetsConfig.SheetNames.Applications);
        sheetNames.Should().Contain(SheetsConfig.SheetNames.Interviews);
        sheetNames.Last().Should().Be(SheetsConfig.SheetNames.Setup);
    }

    [Fact]
    public void GetSheetIndex_ShouldReturnCorrectIndex()
    {
        // Act
        var index = SheetsConfig.SheetUtilities.GetSheetIndex(SheetsConfig.SheetNames.Applications);

        // Assert
        index.Should().Be(0);
    }

    [Fact]
    public void GetSheetIndex_ShouldBeCaseInsensitive()
    {
        // Act
        var index = SheetsConfig.SheetUtilities.GetSheetIndex("applications");

        // Assert
        index.Should().Be(0);
    }

    [Fact]
    public void GetSheetIndex_InvalidName_ShouldThrow()
    {
        // Act
        var act = () => SheetsConfig.SheetUtilities.GetSheetIndex("InvalidSheet");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*InvalidSheet*not found*");
    }

    [Fact]
    public void IsValidSheetName_ValidName_ShouldReturnTrue()
    {
        // Act
        var result = SheetsConfig.SheetUtilities.IsValidSheetName(SheetsConfig.SheetNames.Applications);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidSheetName_InvalidName_ShouldReturnFalse()
    {
        // Act
        var result = SheetsConfig.SheetUtilities.IsValidSheetName("InvalidSheet");

        // Assert
        result.Should().BeFalse();
    }
}
