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
            new List<object> { "Type", "Trips", "Pay", "Tips", "Bonus" },
            new List<object> { "UberX", "10", "25.50", "5.00", "2.00" }, // Ensure string values for Pay, Tip, Bonus
            new List<object> { "Standard", "5", "15.75", "3.00", "1.00" } // Ensure string values for Pay, Tip, Bonus
        };

        // Act
        var result = GenericSheetMapper<TypeEntity>.MapFromRangeData(values);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        var firstType = result[0];
        Assert.Equal("UberX", firstType.Type);
        Assert.Equal(10, firstType.Trips);
        Assert.Equal(25.50m, firstType.Pay);
        Assert.Equal(5.00m, firstType.Tips);
        Assert.Equal(2.00m, firstType.Bonus);

        var secondType = result[1];
        Assert.Equal("Standard", secondType.Type);
        Assert.Equal(5, secondType.Trips);
        Assert.Equal(15.75m, secondType.Pay);
        Assert.Equal(3.00m, secondType.Tips);
        Assert.Equal(1.00m, secondType.Bonus);
    }

    [Fact]
    public void MapFromRangeData_WithEmptyRows_ShouldFilterOutEmptyRows()
    {
        // Arrange
        var values = new List<IList<object>>
        {
            new List<object> { "Type", "Trips", "Pay", "Tip", "Bonus" },
            new List<object> { "UberX", "10", "25.50", "5.00", "2.00" }, // Ensure string values for Pay, Tip, Bonus
            new List<object> { "", "", "", "", "" }, // Empty row
            new List<object> { "Standard", "5", "15.75", "3.00", "1.00" } // Ensure string values for Pay, Tip, Bonus
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