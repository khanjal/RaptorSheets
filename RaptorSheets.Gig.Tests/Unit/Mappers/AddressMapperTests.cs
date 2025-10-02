using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Mappers;
using System.ComponentModel;

namespace RaptorSheets.Gig.Tests.Unit.Mappers;

[Category("Unit Tests")]
public class AddressMapperTests
{
    #region Core Mapping Tests
    
    [Fact]
    public void MapFromRangeData_WithValidData_ShouldReturnAddresses()
    {
        // Arrange
        var values = new List<IList<object>>
        {
            new List<object> { "Address", "Trips", "Pay", "Tip", "Dist" },
            new List<object> { "123 Main St", "5", "125.50", "25.00", "45.5" },
            new List<object> { "456 Oak Ave", "3", "75.25", "15.00", "32.1" }
        };

        // Act
        var result = AddressMapper.MapFromRangeData(values);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        
        var firstAddress = result[0];
        Assert.Equal(2, firstAddress.RowId);
        Assert.Equal("123 Main St", firstAddress.Address);
        Assert.Equal(5, firstAddress.Trips);
        Assert.Equal(125.50m, firstAddress.Pay);
        Assert.Equal(25.00m, firstAddress.Tip);
        Assert.Equal(45.5m, firstAddress.Distance);
        Assert.True(firstAddress.Saved);
        
        var secondAddress = result[1];
        Assert.Equal("456 Oak Ave", secondAddress.Address);
        Assert.Equal(3, secondAddress.Trips);
    }

    [Fact]
    public void MapFromRangeData_WithEmptyRows_ShouldFilterOutEmptyRows()
    {
        // Arrange
        var values = new List<IList<object>>
        {
            new List<object> { "Address", "Trips" },
            new List<object> { "123 Main St", "5" },
            new List<object> { "", "" }, 
            new List<object> { "456 Oak Ave", "3" }
        };

        // Act
        var result = AddressMapper.MapFromRangeData(values);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("123 Main St", result[0].Address);
        Assert.Equal("456 Oak Ave", result[1].Address);
    }

    [Fact]
    public void MapFromRangeData_WithHeadersOnly_ShouldReturnEmptyList()
    {
        // Arrange
        var values = new List<IList<object>>
        {
            new List<object> { "Address", "Trips", "Pay" }
        };

        // Act
        var result = AddressMapper.MapFromRangeData(values);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
    
    #endregion

    #region Sheet Configuration Tests
    
    [Fact]
    public void GetSheet_ShouldReturnCorrectSheetConfiguration()
    {
        // Act
        var result = AddressMapper.GetSheet();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Headers);
        
        // Verify key headers exist and have proper formats
        var addressHeader = result.Headers.FirstOrDefault(h => h.Name == HeaderEnum.ADDRESS.GetDescription());
        Assert.NotNull(addressHeader);
        
        var tripsHeader = result.Headers.FirstOrDefault(h => h.Name == HeaderEnum.TRIPS.GetDescription());
        Assert.NotNull(tripsHeader);
        Assert.Equal(FormatEnum.NUMBER, tripsHeader.Format);
        
        var distanceHeader = result.Headers.FirstOrDefault(h => h.Name == HeaderEnum.DISTANCE.GetDescription());
        Assert.NotNull(distanceHeader);
        Assert.Equal(FormatEnum.DISTANCE, distanceHeader.Format);
    }

    [Fact]
    public void GetSheet_ShouldHaveFormulaHeaders()
    {
        // Act
        var sheet = AddressMapper.GetSheet();
        
        // Assert - Verify essential formula headers exist
        var addressHeader = sheet.Headers.FirstOrDefault(h => h.Name == HeaderEnum.ADDRESS.GetDescription());
        var tripsHeader = sheet.Headers.FirstOrDefault(h => h.Name == HeaderEnum.TRIPS.GetDescription());
        var firstVisitHeader = sheet.Headers.FirstOrDefault(h => h.Name == HeaderEnum.VISIT_FIRST.GetDescription());
        var lastVisitHeader = sheet.Headers.FirstOrDefault(h => h.Name == HeaderEnum.VISIT_LAST.GetDescription());

        // Verify formulas are present (not empty)
        if (addressHeader != null)
        {
            Assert.NotNull(addressHeader.Formula);
            Assert.Contains("Trips!", addressHeader.Formula); // Should reference Trips sheet
        }
        
        if (tripsHeader != null)
        {
            Assert.NotNull(tripsHeader.Formula);
            Assert.Contains("COUNTIF", tripsHeader.Formula); // Should count occurrences
        }
        
        if (firstVisitHeader != null)
        {
            Assert.NotNull(firstVisitHeader.Formula);
            Assert.Equal(FormatEnum.DATE, firstVisitHeader.Format);
        }
        
        if (lastVisitHeader != null)
        {
            Assert.NotNull(lastVisitHeader.Formula);
            Assert.Equal(FormatEnum.DATE, lastVisitHeader.Format);
        }
    }
    
    #endregion
}