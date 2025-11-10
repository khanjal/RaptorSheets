using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Mappers;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Core.Extensions;
using System.ComponentModel;

namespace RaptorSheets.Gig.Tests.Unit.Mappers;

[Category("Unit Tests")]
public class TypeMapperTests
{
    #region Core Mapping Tests

    [Fact]
    public void MapFromRangeData_WithValidData_ShouldReturnTypes()
    {
        // Arrange
        var values = new List<IList<object>>
        {
            new List<object> { "Type", "Trips" },
            new List<object> { "UberX", "10" },
            new List<object> { "Standard", "5" }
        };

        // Act
        var result = GenericSheetMapper<TypeEntity>.MapFromRangeData(values);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        var firstType = result[0];
        Assert.Equal("UberX", firstType.Type);
        Assert.Equal(10, firstType.Trips);

        var secondType = result[1];
        Assert.Equal("Standard", secondType.Type);
        Assert.Equal(5, secondType.Trips);
    }

    [Fact]
    public void MapFromRangeData_WithEmptyRows_ShouldFilterOutEmptyRows()
    {
        // Arrange
        var values = new List<IList<object>>
        {
            new List<object> { "Type", "Trips" },
            new List<object> { "UberX", "10" },
            new List<object> { "", "" }, // Empty row
            new List<object> { "Standard", "5" }
        };

        // Act
        var result = GenericSheetMapper<TypeEntity>.MapFromRangeData(values);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("UberX", result[0].Type);
        Assert.Equal("Standard", result[1].Type);
    }

    #endregion

    #region Sheet Configuration Tests

    [Fact]
    public void GetSheet_ShouldReturnCorrectSheetConfiguration()
    {
        // Act
        var result = TypeMapper.GetSheet();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Types", result.Name);
        Assert.NotNull(result.Headers);
        Assert.True(result.Headers.Count > 0, "Type sheet should have headers");

        // Verify essential headers exist
        var typeHeader = result.Headers.FirstOrDefault(h => h.Name == HeaderEnum.TYPE.GetDescription());
        Assert.NotNull(typeHeader);

        var visitFirstHeader = result.Headers.FirstOrDefault(h => h.Name == HeaderEnum.VISIT_FIRST.GetDescription());
        Assert.NotNull(visitFirstHeader);
        Assert.Equal(FormatEnum.DATE, visitFirstHeader.Format);

        var visitLastHeader = result.Headers.FirstOrDefault(h => h.Name == HeaderEnum.VISIT_LAST.GetDescription());
        Assert.NotNull(visitLastHeader);
        Assert.Equal(FormatEnum.DATE, visitLastHeader.Format);
    }

    #endregion
}