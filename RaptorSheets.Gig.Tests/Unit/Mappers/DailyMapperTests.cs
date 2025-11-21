using RaptorSheets.Gig.Mappers;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Core.Mappers;

namespace RaptorSheets.Gig.Tests.Unit.Mappers;

public class DailyMapperTests
{
    [Fact]
    public void MapFromRangeData_WithValidData_ShouldReturnDailyEntities()
    {
        // Arrange
        var values = new List<IList<object>>
        {
            new List<object> { "Date", "Total", "Trips" },
            new List<object> { "2024-01-01", "100.50", "5" },
            new List<object> { "2024-01-02", "200.75", "10" }
        };

        // Act
        var result = GenericSheetMapper<DailyEntity>.MapFromRangeData(values);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("2024-01-01", result[0].Date);
        Assert.Equal(100.50m, result[0].Total);
        Assert.Equal(5, result[0].Trips);
    }

    [Fact]
    public void GetSheet_ShouldReturnCorrectConfiguration()
    {
        // Act
        var sheet = DailyMapper.GetSheet();

        // Assert
        Assert.NotNull(sheet);
        Assert.Equal("Daily", sheet.Name);
        Assert.NotEmpty(sheet.Headers);
    }
}