using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Sheets;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Core.Extensions;
using System.ComponentModel;

namespace RaptorSheets.Gig.Tests.Unit.Sheets;

[Category("Unit Tests")]
public class WeeklySheetTests
{
    #region Core Mapping Tests

    [Fact]
    public void MapFromRangeData_WithValidData_ShouldReturnWeeklyData()
    {
        // Arrange
        var values = new List<IList<object>>
        {
            new List<object> { "Week", "Total" },
            new List<object> { "2024-W01", "1000.00" },
            new List<object> { "2024-W02", "1500.00" }
        };

        // Act
        var result = GenericSheetMapper<WeeklyEntity>.MapFromRangeData(values);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        var firstWeek = result[0];
        Assert.Equal("2024-W01", firstWeek.Week);
        Assert.Equal(1000.00m, firstWeek.Total);

        var secondWeek = result[1];
        Assert.Equal("2024-W02", secondWeek.Week);
        Assert.Equal(1500.00m, secondWeek.Total);
    }

    [Fact]
    public void MapFromRangeData_WithEmptyRows_ShouldFilterOutEmptyRows()
    {
        // Arrange
        var values = new List<IList<object>>
        {
            new List<object> { "Week", "Total" },
            new List<object> { "2024-W01", "1000.00" },
            new List<object> { "", "" }, // Empty row
            new List<object> { "2024-W02", "1500.00" }
        };

        // Act
        var result = GenericSheetMapper<WeeklyEntity>.MapFromRangeData(values);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("2024-W01", result[0].Week);
        Assert.Equal("2024-W02", result[1].Week);
    }

    #endregion

    #region Sheet Configuration Tests

    [Fact]
    public void GetSheet_ShouldReturnCorrectSheetConfiguration()
    {
        // Act
        var result = WeeklySheet.GetSheet();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Weekly", result.Name);
        Assert.NotNull(result.Headers);
        Assert.True(result.Headers.Count > 0, "Weekly sheet should have headers");

        // Verify essential headers exist
        var weekHeader = result.Headers.FirstOrDefault(h => h.Name == Header.WEEK.GetDescription());
        Assert.NotNull(weekHeader);

        var averageHeader = result.Headers.FirstOrDefault(h => h.Name == Header.AVERAGE.GetDescription());
        Assert.NotNull(averageHeader);
        Assert.Equal(Format.ACCOUNTING, averageHeader.Format);
    }

    #endregion
}