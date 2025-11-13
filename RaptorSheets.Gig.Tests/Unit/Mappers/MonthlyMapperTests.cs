using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Mappers;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Core.Extensions;
using System.ComponentModel;

namespace RaptorSheets.Gig.Tests.Unit.Mappers;

[Category("Unit Tests")]
public class MonthlyMapperTests
{
    #region Core Mapping Tests

    [Fact]
    public void MapFromRangeData_WithValidData_ShouldReturnMonthlyData()
    {
        // Arrange
        var values = new List<IList<object>>
        {
            new List<object> { "Month", "Total" },
            new List<object> { "2024-01", "10000.00" },
            new List<object> { "2024-02", "15000.00" }
        };

        // Act
        var result = GenericSheetMapper<MonthlyEntity>.MapFromRangeData(values);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        var firstMonth = result[0];
        Assert.Equal("2024-01", firstMonth.Month);
        Assert.Equal(10000.00m, firstMonth.Total);

        var secondMonth = result[1];
        Assert.Equal("2024-02", secondMonth.Month);
        Assert.Equal(15000.00m, secondMonth.Total);
    }

    [Fact]
    public void MapFromRangeData_WithEmptyRows_ShouldFilterOutEmptyRows()
    {
        // Arrange
        var values = new List<IList<object>>
        {
            new List<object> { "Month", "Total" },
            new List<object> { "2024-01", "10000.00" },
            new List<object> { "", "" }, // Empty row
            new List<object> { "2024-02", "15000.00" }
        };

        // Act
        var result = GenericSheetMapper<MonthlyEntity>.MapFromRangeData(values);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("2024-01", result[0].Month);
        Assert.Equal("2024-02", result[1].Month);
    }

    #endregion

    #region Sheet Configuration Tests

    [Fact]
    public void GetSheet_ShouldReturnCorrectSheetConfiguration()
    {
        // Act
        var result = MonthlyMapper.GetSheet();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Monthly", result.Name);
        Assert.NotNull(result.Headers);
        Assert.True(result.Headers.Count > 0, "Monthly sheet should have headers");

        // Verify essential headers exist
        var monthHeader = result.Headers.FirstOrDefault(h => h.Name == HeaderEnum.MONTH.GetDescription());
        Assert.NotNull(monthHeader);

        var averageHeader = result.Headers.FirstOrDefault(h => h.Name == HeaderEnum.AVERAGE.GetDescription());
        Assert.NotNull(averageHeader);
        Assert.Equal(FormatEnum.ACCOUNTING, averageHeader.Format);
    }

    #endregion
}