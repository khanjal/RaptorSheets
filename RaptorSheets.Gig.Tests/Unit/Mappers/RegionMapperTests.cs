using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Mappers;
using System.ComponentModel;
using Xunit;

namespace RaptorSheets.Gig.Tests.Unit.Mappers;

[Category("Unit Tests")]
public class RegionMapperTests
{
    #region Core Mapping Tests (Essential)

    [Fact]
    public void MapFromRangeData_WithValidData_ShouldReturnRegions()
    {
        // Arrange
        var values = new List<IList<object>>
        {
            new List<object> { "Region", "Trips", "Pay" },
            new List<object> { "Downtown", "15", "1500.00" },
            new List<object> { "Suburbs", "10", "1000.00" }
        };

        // Act
        var result = GenericSheetMapper<RegionEntity>.MapFromRangeData(values);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        var firstRegion = result[0];
        Assert.Equal("Downtown", firstRegion.Region);
        Assert.Equal(15, firstRegion.Trips);
        Assert.Equal(1500.00m, firstRegion.Pay);

        var secondRegion = result[1];
        Assert.Equal("Suburbs", secondRegion.Region);
        Assert.Equal(10, secondRegion.Trips);
        Assert.Equal(1000.00m, secondRegion.Pay);
    }

    [Fact]
    public void MapFromRangeData_WithEmptyRows_ShouldFilterOutEmptyRows()
    {
        // Arrange
        var values = new List<IList<object>>
        {
            new List<object> { "Region", "Trips" },
            new List<object> { "Downtown", "15" },
            new List<object> { "", "" }, // Empty row
            new List<object> { "Suburbs", "10" }
        };

        // Act
        var result = GenericSheetMapper<RegionEntity>.MapFromRangeData(values);

        // Assert
        Assert.Equal(2, result.Count); // Empty row filtered out
        Assert.Equal("Downtown", result[0].Region);
        Assert.Equal("Suburbs", result[1].Region);
    }

    #endregion

    #region Sheet Configuration Tests (Core Structure)

    [Fact]
    public void GetSheet_ShouldReturnCorrectSheetConfiguration()
    {
        // Act
        var result = RegionMapper.GetSheet();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Regions", result.Name);
        Assert.NotNull(result.Headers);
        Assert.True(result.Headers.Count > 2, "Region sheet should have multiple columns");

        // Verify essential headers exist with proper configuration
        var regionHeader = result.Headers.FirstOrDefault(h => h.Name == HeaderEnum.REGION.GetDescription());
        Assert.NotNull(regionHeader);

        var tripsHeader = result.Headers.FirstOrDefault(h => h.Name == HeaderEnum.TRIPS.GetDescription());
        Assert.NotNull(tripsHeader);
        Assert.Equal(FormatEnum.NUMBER, tripsHeader.Format);

        // Verify all headers have proper column assignments
        Assert.All(result.Headers, header => 
        {
            Assert.True(header.Index >= 0, "All headers should have valid indexes");
            Assert.False(string.IsNullOrEmpty(header.Column), "All headers should have column letters");
        });
    }

    #endregion

    #region Formula Tests (High-Level Validation Only)

    [Fact]
    public void GetSheet_ShouldGenerateValidFormulas()
    {
        // Act
        var sheet = RegionMapper.GetSheet();
        var formulaHeaders = sheet.Headers.Where(h => !string.IsNullOrEmpty(h.Formula)).ToList();

        // Assert - High-level validation only (don't test formula internals)
        if (formulaHeaders.Any())
        {
            // All formulas should start with =
            Assert.All(formulaHeaders, header => Assert.StartsWith("=", header.Formula));

            // Should not have unresolved placeholders
            Assert.All(formulaHeaders, header => 
            {
                Assert.DoesNotContain("{keyRange}", header.Formula);
                Assert.DoesNotContain("{header}", header.Formula);
            });
        }
    }

    #endregion
}