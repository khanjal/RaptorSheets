using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Mappers;
using System.ComponentModel;

namespace RaptorSheets.Gig.Tests.Unit.Mappers;

[Category("Unit Tests")]
public class TripMapperTests
{
    #region Core Mapping Tests (Essential)
    
    [Fact]
    public void MapFromRangeData_WithValidData_ShouldReturnTrips()
    {
        // Arrange
        var values = new List<IList<object>>
        {
            new List<object> { "Date", "Service", "#", "Type", "Place", "Pay", "Tips", "Bonus", "Name" },
            new List<object> { "2024-01-15", "Uber", "1", "UberX", "Restaurant", "25.50", "5.00", "2.00", "John Doe" },
            new List<object> { "2024-01-16", "Lyft", "2", "Standard", "Airport", "45.75", "10.00", "", "Jane Smith" }
        };

        // Act
        var result = GenericSheetMapper<TripEntity>.MapFromRangeData(values);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        
        var firstTrip = result[0];
        Assert.Equal(2, firstTrip.RowId);
        Assert.Equal("2024-01-15", firstTrip.Date);
        Assert.Equal("Uber", firstTrip.Service);
        Assert.Equal(1, firstTrip.Number);
        Assert.Equal("UberX", firstTrip.Type);
        Assert.Equal("Restaurant", firstTrip.Place);
        Assert.Equal(25.50m, firstTrip.Pay);
        Assert.Equal(5.00m, firstTrip.Tip);
        Assert.Equal(2.00m, firstTrip.Bonus);
        Assert.Equal("John Doe", firstTrip.Name);
        
        var secondTrip = result[1];
        Assert.Equal(3, secondTrip.RowId);
        Assert.Equal("Lyft", secondTrip.Service);
        Assert.Equal(45.75m, secondTrip.Pay);
        Assert.Null(secondTrip.Bonus); // Empty string should be null for decimal
    }

    [Fact]
    public void MapFromRangeData_WithEmptyRows_ShouldFilterOutEmptyRows()
    {
        // Arrange
        var values = new List<IList<object>>
        {
            new List<object> { "Date", "Service" },
            new List<object> { "2024-01-15", "Uber" },
            new List<object> { "", "" }, // Empty row
            new List<object> { "2024-01-16", "Lyft" }
        };

        // Act
        var result = GenericSheetMapper<TripEntity>.MapFromRangeData(values);

        // Assert
        Assert.Equal(2, result.Count); // Empty row filtered out
        Assert.Equal("Uber", result[0].Service);
        Assert.Equal("Lyft", result[1].Service);
    }

    [Fact]
    public void MapToRangeData_WithValidTrips_ShouldReturnCorrectData()
    {
        // Arrange
        var trips = new List<TripEntity>
        {
            new()
            {
                Date = "2024-01-15",
                Service = "Uber",
                Number = 1,
                Type = "UberX",
                Pay = 25.50m,
                Tip = 5.00m,
                Name = "John Doe"
            }
        };
        var headers = new List<object> { "Date", "Service", "#", "Type", "Pay", "Tips", "Name" };

        // Act
        var result = GenericSheetMapper<TripEntity>.MapToRangeData(trips, headers);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var row = result[0];
        Assert.Equal("2024-01-15", row[0]);
        Assert.Equal("Uber", row[1]);
        Assert.Equal("1", row[2]);
        Assert.Equal("UberX", row[3]);
        Assert.Equal("25.50", row[4]);
        Assert.Equal("5.00", row[5]);
        Assert.Equal("John Doe", row[6]);
    }
    
    #endregion

    #region Sheet Configuration Tests (Core Structure)
    
    [Fact]
    public void GetSheet_ShouldReturnCorrectSheetConfiguration()
    {
        // Act
        var result = TripMapper.GetSheet();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Trips", result.Name);
        Assert.NotNull(result.Headers);
        Assert.True(result.Headers.Count > 10, "Trip sheet should have many columns");
        
        // Verify essential headers exist with proper configuration
        var dateHeader = result.Headers.FirstOrDefault(h => h.Name == HeaderEnum.DATE.GetDescription());
        Assert.NotNull(dateHeader);
        Assert.Equal(FormatEnum.DATE, dateHeader.Format);
        
        var payHeader = result.Headers.FirstOrDefault(h => h.Name == HeaderEnum.PAY.GetDescription());
        Assert.NotNull(payHeader);
        // TripEntity uses FieldTypeEnum.Currency which maps to ACCOUNTING format
        Assert.Equal(FormatEnum.ACCOUNTING, payHeader.Format);
        
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
        var sheet = TripMapper.GetSheet();
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

    [Theory]
    [InlineData("Date", "2024-01-15")]  // Representative test cases only
    [InlineData("Service", "Uber")]
    [InlineData("Pay", "25.50")]
    public void MapFromRangeData_WithSingleColumn_ShouldMapCorrectly(string headerName, string testValue)
    {
        // Arrange
        var values = new List<IList<object>>
        {
            new List<object> { headerName },
            new List<object> { testValue }
        };

        // Act
        var result = GenericSheetMapper<TripEntity>.MapFromRangeData(values);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var trip = result[0];
        switch (headerName)
        {
            case "Date":
                Assert.Equal(testValue, trip.Date);
                break;
            case "Service":
                Assert.Equal(testValue, trip.Service);
                break;
            case "Pay":
                Assert.Equal(25.50m, trip.Pay);
                break;
        }
    }
    
    #endregion
}