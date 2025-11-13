using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Mappers;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Core.Extensions;
using System.ComponentModel;

namespace RaptorSheets.Gig.Tests.Unit.Mappers;

[Category("Unit Tests")]
public class YearlyMapperTests
{
    #region Core Mapping Tests

    [Fact]
    public void MapFromRangeData_WithValidData_ShouldReturnYearlyData()
    {
        // Arrange
        var values = new List<IList<object>>
        {
            new List<object> { "Year", "Total" },
            new List<object> { 2024, "12000.00" },
            new List<object> { 2025, "15000.00" }
        };

        // Act
        var result = GenericSheetMapper<YearlyEntity>.MapFromRangeData(values);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        var firstYear = result[0];
        Assert.Equal(2024, firstYear.Year);
        Assert.Equal(12000.00m, firstYear.Total);

        var secondYear = result[1];
        Assert.Equal(2025, secondYear.Year);
        Assert.Equal(15000.00m, secondYear.Total);
    }

    [Fact]
    public void MapFromRangeData_WithEmptyRows_ShouldFilterOutEmptyRows()
    {
        // Arrange
        var values = new List<IList<object>>
        {
            new List<object> { "Year", "Total" },
            new List<object> { 2024, "12000.00" },
            new List<object> { "", "" }, // Empty row
            new List<object> { 2025, "15000.00" }
        };

        // Act
        var result = GenericSheetMapper<YearlyEntity>.MapFromRangeData(values);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(2024, result[0].Year);
        Assert.Equal(2025, result[1].Year);
    }

    #endregion

    #region Sheet Configuration Tests

    [Fact]
    public void GetSheet_ShouldReturnCorrectSheetConfiguration()
    {
        // Act
        var result = YearlyMapper.GetSheet();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Yearly", result.Name);
        Assert.NotNull(result.Headers);
        Assert.True(result.Headers.Count > 0, "Yearly sheet should have headers");

        // Verify essential headers exist
        var yearHeader = result.Headers.FirstOrDefault(h => h.Name == HeaderEnum.YEAR.GetDescription());
        Assert.NotNull(yearHeader);

        var daysHeader = result.Headers.FirstOrDefault(h => h.Name == HeaderEnum.DAYS.GetDescription());
        Assert.NotNull(daysHeader);
        Assert.Equal(FormatEnum.NUMBER, daysHeader.Format);

        var averageHeader = result.Headers.FirstOrDefault(h => h.Name == HeaderEnum.AVERAGE.GetDescription());
        Assert.NotNull(averageHeader);
        Assert.Equal(FormatEnum.ACCOUNTING, averageHeader.Format);
    }

    #endregion
}