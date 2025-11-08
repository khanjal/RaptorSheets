using RaptorSheets.Gig.Mappers;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Core.Mappers;
using Xunit;

namespace RaptorSheets.Gig.Tests.Unit.Mappers;

public class PlaceMapperTests
{
    [Fact]
    public void MapFromRangeData_WithValidData_ShouldReturnPlaceEntities()
    {
        // Arrange
        var values = new List<IList<object>>
        {
            new List<object> { "Place", "Trips", "Total" },
            new List<object> { "Restaurant", "10", "500.00" },
            new List<object> { "Airport", "5", "250.00" }
        };

        // Act
        var result = GenericSheetMapper<PlaceEntity>.MapFromRangeData(values);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Restaurant", result[0].Place);
        Assert.Equal(10, result[0].Trips);
        Assert.Equal(500.00m, result[0].Total);
    }

    [Fact]
    public void GetSheet_ShouldReturnCorrectConfiguration()
    {
        // Act
        var sheet = PlaceMapper.GetSheet();

        // Assert
        Assert.NotNull(sheet);
        Assert.Equal("Places", sheet.Name);
        Assert.NotEmpty(sheet.Headers);
    }
}