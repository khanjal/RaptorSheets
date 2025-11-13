using RaptorSheets.Core.Mappers;
using RaptorSheets.Gig.Entities;
using System.ComponentModel;

namespace RaptorSheets.Gig.Tests.Unit.Mappers;

[Category("Unit Tests")]
public class MapToRangeDataTests
{
    #region Core Mapping Tests
    
    [Fact]
    public void ShiftMapper_MapToRangeData_ShouldReturnCorrectStructure()
    {
        // Arrange
        var shifts = new List<ShiftEntity>
        {
            new()
            {
                Date = "2024-01-15",
                Start = "09:00:00",
                Finish = "17:00:00",
                Service = "Uber",
                Number = 123,
                Region = "Downtown",
                Note = "Test shift"
            }
        };
        var headers = new List<object> { "Date", "Start", "Finish", "Service", "#", "Region", "Note" };

        // Act
        var result = GenericSheetMapper<ShiftEntity>.MapToRangeData(shifts, headers);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var row = result[0];
        Assert.Equal(7, row.Count); // Should match header count
        
        // Verify key fields are mapped correctly (representative sample)
        Assert.Equal("2024-01-15", row[0]);
        Assert.Equal("09:00:00", row[1]);
        Assert.Equal("Uber", row[3]);
        Assert.Equal("123", row[4]);
        Assert.Equal("Downtown", row[5]);
    }

    [Fact]
    public void TripMapper_MapToRangeData_ShouldReturnCorrectStructure()
    {
        // Arrange
        var trips = new List<TripEntity>
        {
            new()
            {
                Date = "2024-01-15",
                Service = "Uber",
                StartAddress = "123 Main St",
                EndAddress = "456 Oak Ave",
                Pay = 25.50m,
                Tip = 5.00m,
                Distance = 10.5m
            }
        };
        var headers = new List<object> { "Date", "Service", "Start Address", "End Address", "Pay", "Tip", "Distance" };

        // Act
        var result = GenericSheetMapper<TripEntity>.MapToRangeData(trips, headers);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var row = result[0];
        Assert.Equal(7, row.Count); // Should match header count
        
        // Verify key fields are mapped correctly (representative sample)
        Assert.Equal("2024-01-15", row[0]);
        Assert.Equal("Uber", row[1]);
        Assert.Equal("123 Main St", row[2]);
        Assert.Equal("456 Oak Ave", row[3]);
    }

    [Fact]
    public void MapToRangeData_WithNullValues_ShouldHandleGracefully()
    {
        // Arrange
        var shifts = new List<ShiftEntity>
        {
            new()
            {
                Date = "2024-01-15",
                Service = "Uber",
                Number = null, // Test null handling
                Pay = null,
                Note = null
            }
        };
        var headers = new List<object> { "Date", "Service", "#", "Pay", "Note" };

        // Act
        var result = GenericSheetMapper<ShiftEntity>.MapToRangeData(shifts, headers);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var row = result[0];
        Assert.Equal(5, row.Count);
        
        // Verify non-null values are present
        Assert.Equal("2024-01-15", row[0]);
        Assert.Equal("Uber", row[1]);
        // Null values should be handled appropriately (exact handling is implementation detail)
    }

    [Fact]
    public void MapToRangeData_WithEmptyList_ShouldReturnEmptyResult()
    {
        // Arrange
        var emptyShifts = new List<ShiftEntity>();
        var headers = new List<object> { "Date", "Service" };

        // Act
        var result = GenericSheetMapper<ShiftEntity>.MapToRangeData(emptyShifts, headers);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void MapToRangeData_WithMultipleEntities_ShouldReturnAllRows()
    {
        // Arrange
        var shifts = new List<ShiftEntity>
        {
            new() { Date = "2024-01-15", Service = "Uber" },
            new() { Date = "2024-01-16", Service = "Lyft" },
            new() { Date = "2024-01-17", Service = "DoorDash" }
        };
        var headers = new List<object> { "Date", "Service" };

        // Act
        var result = GenericSheetMapper<ShiftEntity>.MapToRangeData(shifts, headers);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        
        // Verify each row has correct structure
        Assert.All(result, row => Assert.Equal(2, row.Count));
        
        // Spot check first and last rows
        Assert.Equal("2024-01-15", result[0][0]);
        Assert.Equal("Uber", result[0][1]);
        Assert.Equal("2024-01-17", result[2][0]);
        Assert.Equal("DoorDash", result[2][1]);
    }
    
    #endregion
}
