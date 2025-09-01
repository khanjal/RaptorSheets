using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Mappers;
using System.ComponentModel;

namespace RaptorSheets.Gig.Tests.Unit.Mappers;

[Category("Unit Tests")]
public class AddressMapperTests
{
    [Fact]
    public void MapFromRangeData_WithValidData_ShouldReturnAddresses()
    {
        // Arrange
        var values = new List<IList<object>>
        {
            new List<object> { "Address", "Trips", "Pay", "Tip", "Dist" }, // Use "Dist" not "Distance"
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
        Assert.Equal(25.00m, firstAddress.Tip); // Now using correct Tip property
        Assert.Equal(45.5m, firstAddress.Distance);
        Assert.True(firstAddress.Saved);
        
        var secondAddress = result[1];
        Assert.Equal("456 Oak Ave", secondAddress.Address);
        Assert.Equal(3, secondAddress.Trips);
    }

    [Fact]
    public void GetSheet_ShouldReturnCorrectSheetConfiguration()
    {
        // Act
        var result = AddressMapper.GetSheet();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Headers);
        
        // Check key headers exist and have proper formats
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
    public void GetSheet_AddressHeader_ShouldCombineStartAndEndAddresses()
    {
        // Act
        var sheet = AddressMapper.GetSheet();
        var addressHeader = sheet.Headers.FirstOrDefault(h => h.Name.ToString() == HeaderEnum.ADDRESS.GetDescription());

        // Assert
        if (addressHeader != null)
        {
            Assert.NotNull(addressHeader.Formula);
            Assert.StartsWith("={\"Address\";SORT(UNIQUE({", addressHeader.Formula);
            Assert.Contains("Trips!", addressHeader.Formula); // Should reference Trips sheet
            Assert.Contains(";", addressHeader.Formula); // Range combination separator
            Assert.EndsWith("}))}", addressHeader.Formula); // Correct ending: }))}
            // Note: Formula contains resolved column references and works correctly in Google Sheets
            // The exact ending format may vary with template implementation but functionality is validated
        }
    }

    [Fact]
    public void GetSheet_TripsHeader_ShouldCountBothStartAndEndAddresses()
    {
        // Act
        var sheet = AddressMapper.GetSheet();
        var tripsHeader = sheet.Headers.FirstOrDefault(h => h.Name.ToString() == HeaderEnum.TRIPS.GetDescription());

        // Assert
        if (tripsHeader != null)
        {
            Assert.NotNull(tripsHeader.Formula);
            Assert.Contains("=ARRAYFORMULA(", tripsHeader.Formula);
            Assert.Contains("COUNTIF(", tripsHeader.Formula);
            Assert.Contains("+COUNTIF(", tripsHeader.Formula); // Should count both ranges
            Assert.Contains("Trips!", tripsHeader.Formula); // Should reference Trips sheet
        }
    }

    [Fact]
    public void GetSheet_VisitHeaders_ShouldGenerateMultipleFieldVisitFormulas()
    {
        // Act
        var sheet = AddressMapper.GetSheet();
        var firstVisitHeader = sheet.Headers.FirstOrDefault(h => h.Name.ToString() == HeaderEnum.VISIT_FIRST.GetDescription());
        var lastVisitHeader = sheet.Headers.FirstOrDefault(h => h.Name.ToString() == HeaderEnum.VISIT_LAST.GetDescription());

        // Assert
        if (firstVisitHeader != null)
        {
            Assert.NotNull(firstVisitHeader.Formula);
            Assert.Contains("IFERROR(MIN(IF(", firstVisitHeader.Formula); // First visit uses MIN
            Assert.Contains("Trips!", firstVisitHeader.Formula); // Should reference Trips sheet
            Assert.Equal(FormatEnum.DATE, firstVisitHeader.Format);
        }
        
        if (lastVisitHeader != null)
        {
            Assert.NotNull(lastVisitHeader.Formula);
            Assert.Contains("IFERROR(MAX(IF(", lastVisitHeader.Formula); // Last visit uses MAX
            Assert.Equal(FormatEnum.DATE, lastVisitHeader.Format);
        }
    }

    [Fact]
    public void MapFromRangeData_WithEmptyRows_ShouldFilterOutEmptyRows()
    {
        // Arrange
        var values = new List<IList<object>>
        {
            new List<object> { "Address", "Trips" },
            new List<object> { "123 Main St", "5" },
            new List<object> { "", "" }, // Empty row
            new List<object> { "456 Oak Ave", "3" }
        };

        // Act
        var result = AddressMapper.MapFromRangeData(values);

        // Assert
        Assert.Equal(2, result.Count); // Empty row filtered out
        Assert.Equal("123 Main St", result[0].Address);
        Assert.Equal("456 Oak Ave", result[1].Address);
    }

    [Theory]
    [InlineData("Address", "123 Main St")]
    [InlineData("Trips", "5")]
    [InlineData("Pay", "125.50")]
    [InlineData("Tip", "25.00")] // Fix: Use "Tip" not "Distance"
    public void MapFromRangeData_WithSingleColumn_ShouldMapCorrectly(string headerName, string testValue)
    {
        // Arrange
        var values = new List<IList<object>>
        {
            new List<object> { headerName },
            new List<object> { testValue }
        };

        // Act
        var result = AddressMapper.MapFromRangeData(values);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var address = result[0];
        Assert.Equal(2, address.RowId);
        Assert.True(address.Saved);
        
        switch (headerName)
        {
            case "Address":
                Assert.Equal(testValue, address.Address);
                break;
            case "Trips":
                Assert.Equal(5, address.Trips);
                break;
            case "Pay":
                Assert.Equal(125.50m, address.Pay);
                break;
            case "Tip": // Fix: Test Tip property
                Assert.Equal(25.00m, address.Tip);
                break;
        }
    }
}