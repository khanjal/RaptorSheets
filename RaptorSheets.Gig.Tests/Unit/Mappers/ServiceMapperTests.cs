using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Mappers;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Core.Extensions;
using System.ComponentModel;

namespace RaptorSheets.Gig.Tests.Unit.Mappers;

[Category("Unit Tests")]
public class ServiceMapperTests
{
    #region Core Mapping Tests

    [Fact]
    public void MapFromRangeData_WithValidData_ShouldReturnServices()
    {
        // Arrange
        var values = new List<IList<object>>
        {
            new List<object> { "Service", "Trips" },
            new List<object> { "Uber", "10" },
            new List<object> { "Lyft", "5" }
        };

        // Act
        var result = GenericSheetMapper<ServiceEntity>.MapFromRangeData(values);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        var firstService = result[0];
        Assert.Equal("Uber", firstService.Service);
        Assert.Equal(10, firstService.Trips);

        var secondService = result[1];
        Assert.Equal("Lyft", secondService.Service);
        Assert.Equal(5, secondService.Trips);
    }

    [Fact]
    public void MapFromRangeData_WithEmptyRows_ShouldFilterOutEmptyRows()
    {
        // Arrange
        var values = new List<IList<object>>
        {
            new List<object> { "Service", "Trips" },
            new List<object> { "Uber", "10" },
            new List<object> { "", "" }, // Empty row
            new List<object> { "Lyft", "5" }
        };

        // Act
        var result = GenericSheetMapper<ServiceEntity>.MapFromRangeData(values);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Uber", result[0].Service);
        Assert.Equal("Lyft", result[1].Service);
    }

    #endregion

    #region Sheet Configuration Tests

    [Fact]
    public void GetSheet_ShouldReturnCorrectSheetConfiguration()
    {
        // Act
        var result = ServiceMapper.GetSheet();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Services", result.Name);
        Assert.NotNull(result.Headers);
        Assert.True(result.Headers.Count > 0, "Service sheet should have headers");

        // Verify essential headers exist
        var serviceHeader = result.Headers.FirstOrDefault(h => h.Name == HeaderEnum.SERVICE.GetDescription());
        Assert.NotNull(serviceHeader);

        var visitFirstHeader = result.Headers.FirstOrDefault(h => h.Name == HeaderEnum.VISIT_FIRST.GetDescription());
        Assert.NotNull(visitFirstHeader);
        Assert.Equal(FormatEnum.DATE, visitFirstHeader.Format);

        var visitLastHeader = result.Headers.FirstOrDefault(h => h.Name == HeaderEnum.VISIT_LAST.GetDescription());
        Assert.NotNull(visitLastHeader);
        Assert.Equal(FormatEnum.DATE, visitLastHeader.Format);
    }

    #endregion
}