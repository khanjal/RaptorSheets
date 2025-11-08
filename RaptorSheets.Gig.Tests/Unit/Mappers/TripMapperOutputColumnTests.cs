using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Mappers;
using System.ComponentModel;
using Xunit;

namespace RaptorSheets.Gig.Tests.Unit.Mappers;

[Category("Unit Tests")]
public class TripMapperOutputColumnTests
{
    [Fact]
    public void MapToRangeData_WithTotalColumn_ShouldReserveSpotWithNull()
    {
        // Arrange
        var trips = new List<TripEntity>
        {
            new()
            {
                Date = "2024-01-15",
                Service = "Uber",
                Pay = 25.50m,
                Tip = 5.00m,
                Bonus = 2.00m,
                Total = 32.50m, // This is an OUTPUT column (formula), should be null in write
                Cash = 3.00m
            }
        };
        // Headers with Total column between input columns - using correct header names
        var headers = new List<object> 
        { 
            SheetsConfig.HeaderNames.Date,
            SheetsConfig.HeaderNames.Service,
            SheetsConfig.HeaderNames.Pay,
            SheetsConfig.HeaderNames.Tips, // Note: "Tips" not "Tip"
            SheetsConfig.HeaderNames.Bonus,
            SheetsConfig.HeaderNames.Total,
            SheetsConfig.HeaderNames.Cash
        };

        // Act
        var result = TripMapper.MapToRangeData(trips, headers);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);

        var row = result[0];
        Assert.Equal(7, row.Count); // Should have 7 positions (all headers)

        // Input columns should have values
        Assert.Equal("2024-01-15", row[0]); // Date
        Assert.Equal("Uber", row[1]);       // Service
        Assert.Equal("25.50", row[2]);      // Pay
        Assert.Equal("5.00", row[3]);       // Tips
        Assert.Equal("2.00", row[4]);       // Bonus

        // Total is output column - should be NULL to preserve array formula
        Assert.Null(row[5]); // Total (OUTPUT - formula column)

        // Cash is input - should have value
        Assert.Equal("3.00", row[6]); // Cash
    }

    [Fact]
    public void MapToRowData_WithTotalColumn_ShouldReserveSpotWithEmptyCellData()
    {
        // Arrange
        var trips = new List<TripEntity>
        {
            new()
            {
                Date = "2024-01-15",
                Service = "Uber",
                Pay = 25.50m,
                Tip = 5.00m,
                Bonus = 2.00m,
                Total = 32.50m, // OUTPUT column
                Cash = 3.00m
            }
        };
        var headers = new List<object> 
        { 
            SheetsConfig.HeaderNames.Date,
            SheetsConfig.HeaderNames.Service,
            SheetsConfig.HeaderNames.Pay,
            SheetsConfig.HeaderNames.Tips,
            SheetsConfig.HeaderNames.Bonus,
            SheetsConfig.HeaderNames.Total,
            SheetsConfig.HeaderNames.Cash
        };

        // Act
        var result = TripMapper.MapToRowData(trips, headers);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);

        var row = result[0];
        Assert.NotNull(row.Values);
        Assert.Equal(7, row.Values.Count);

        // Input columns should have cell data
        Assert.NotNull(row.Values[0]); // Date
        Assert.NotNull(row.Values[1]); // Service
        Assert.NotNull(row.Values[2]); // Pay
        Assert.NotNull(row.Values[3]); // Tips
        Assert.NotNull(row.Values[4]); // Bonus

        // Total is output - should be empty CellData (not null) to preserve position
        Assert.NotNull(row.Values[5]); // Total (OUTPUT) - empty CellData
        Assert.Null(row.Values[5].UserEnteredValue); // But no value

        // Cash is input
        Assert.NotNull(row.Values[6]); // Cash
        Assert.NotNull(row.Values[6].UserEnteredValue); // With value
    }

    [Fact]
    public void MapToRangeData_WithAllTripOutputColumns_ShouldReservePositions()
    {
        // Arrange
        var trips = new List<TripEntity>
        {
            new()
            {
                Date = "2024-01-15",
                Service = "Uber",
                Pay = 25.50m,
                Tip = 5.00m,
                Bonus = 2.00m,
                Total = 32.50m,        // OUTPUT (Pay + Tip + Bonus)
                Cash = 3.00m,
                Key = "2024-01-15-0-Uber", // OUTPUT (formula)
                Day = "Monday",        // OUTPUT (formula)
                Month = "January",     // OUTPUT (formula)
                Year = "2024",         // OUTPUT (formula)
                AmountPerTime = 15.75m,    // OUTPUT (formula)
                AmountPerDistance = 3.10m   // OUTPUT (formula)
            }
        };

        // Create headers list using correct header names
        var headers = new List<object> 
        { 
            SheetsConfig.HeaderNames.Date,
            SheetsConfig.HeaderNames.Service,
            SheetsConfig.HeaderNames.Pay,
            SheetsConfig.HeaderNames.Tips,
            SheetsConfig.HeaderNames.Bonus,
            SheetsConfig.HeaderNames.Total,
            SheetsConfig.HeaderNames.Cash,
            SheetsConfig.HeaderNames.Key,
            SheetsConfig.HeaderNames.Day,
            SheetsConfig.HeaderNames.Month,
            SheetsConfig.HeaderNames.Year,
            SheetsConfig.HeaderNames.AmountPerTime,
            SheetsConfig.HeaderNames.AmountPerDistance
        };

        // Act
        var result = TripMapper.MapToRangeData(trips, headers);

        // Assert
        var row = result[0];
        Assert.Equal(13, row.Count);

        // Check input columns have values
        Assert.NotNull(row[0]); // Date (input)
        Assert.NotNull(row[1]); // Service (input)
        Assert.NotNull(row[2]); // Pay (input)
        Assert.NotNull(row[3]); // Tips (input)
        Assert.NotNull(row[4]); // Bonus (input)
        
        // Check output columns are null
        Assert.Null(row[5]);  // Total (OUTPUT)
        
        // Cash is input
        Assert.NotNull(row[6]); // Cash (input)
        
        // All formula columns should be null
        Assert.Null(row[7]);  // Key (OUTPUT)
        Assert.Null(row[8]);  // Day (OUTPUT)
        Assert.Null(row[9]);  // Month (OUTPUT)
        Assert.Null(row[10]); // Year (OUTPUT)
        Assert.Null(row[11]); // Amount Per Time (OUTPUT)
        Assert.Null(row[12]); // Amount Per Distance (OUTPUT)
    }

    [Fact]
    public void MapToRangeData_VerifyTotalPositionBetweenBonusAndCash()
    {
        // Arrange - This test specifically checks that Total maintains its position
        var trips = new List<TripEntity>
        {
            new()
            {
                Bonus = 2.00m,
                Total = 999.99m, // Should be ignored (output column)
                Cash = 3.00m
            }
        };
        var headers = new List<object> 
        { 
            SheetsConfig.HeaderNames.Bonus,
            SheetsConfig.HeaderNames.Total,
            SheetsConfig.HeaderNames.Cash
        };

        // Act
        var result = TripMapper.MapToRangeData(trips, headers);

        // Assert
        var row = result[0];
        Assert.Equal(3, row.Count);
        
        Assert.Equal("2.00", row[0]); // Bonus (input)
        Assert.Null(row[1]);          // Total (output) - MUST be null
        Assert.Equal("3.00", row[2]); // Cash (input)
        
        // If Total didn't reserve its spot, Cash would be in position [1] instead of [2]
        // This would break the column alignment in Google Sheets
    }
}
