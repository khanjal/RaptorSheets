using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Mappers;
using System.ComponentModel;
using Xunit;

namespace RaptorSheets.Gig.Tests.Unit.Mappers;

/// <summary>
/// Comprehensive validation that GenericSheetMapper preserves column positions for output columns
/// in a realistic TripEntity scenario matching actual Google Sheets usage.
/// </summary>
[Category("Unit Tests")]
public class TripMapperColumnPositionTests
{
    [Fact]
    public void MapToRangeData_WithRealisticTripData_ShouldPreserveAllColumnPositions()
    {
        // Arrange - Create a trip with all common fields populated
        var trip = new TripEntity
        {
            // Row 1: Input columns
            Date = "2024-01-15",
            Service = "Uber",
            Number = 1,
            Exclude = false,
            Type = "UberX",
            Place = "Restaurant",
            Pickup = "18:30:00",
            Dropoff = "19:15:00",
            Duration = "00:45:00",
            
            // Row 2: Financial - mix of input and output
            Pay = 25.50m,
            Tip = 5.00m,
            Bonus = 2.00m,
            Total = 32.50m,  // OUTPUT (formula: Pay + Tips + Bonus)
            Cash = 3.00m,
            
            // Row 3: Travel data - input
            OdometerStart = 12345.0m,
            OdometerEnd = 12360.5m,
            Distance = 15.5m,
            
            // Row 4: Location - input
            Name = "John D.",
            StartAddress = "123 Main St",
            EndAddress = "456 Oak Ave",
            EndUnit = "Apt 5",
            OrderNumber = "ABC123",
            Region = "Downtown",
            Note = "Left at door",
            
            // Row 5: Calculated fields - all OUTPUT (formulas)
            Key = "2024-01-15-1-Uber",
            Day = "Monday",
            Month = "January",
            Year = "2024",
            AmountPerTime = 43.33m,
            AmountPerDistance = 2.10m
        };

        // Create headers in the same order as TripEntity properties (matches Google Sheets column order)
        var headers = new List<object>
        {
            // Input columns from TripEntity
            SheetsConfig.HeaderNames.Date,
            SheetsConfig.HeaderNames.Service,
            SheetsConfig.HeaderNames.Number,
            SheetsConfig.HeaderNames.Exclude,
            SheetsConfig.HeaderNames.Type,
            SheetsConfig.HeaderNames.Place,
            SheetsConfig.HeaderNames.Pickup,
            SheetsConfig.HeaderNames.Dropoff,
            SheetsConfig.HeaderNames.Duration,
            SheetsConfig.HeaderNames.Pay,
            SheetsConfig.HeaderNames.Tips,
            SheetsConfig.HeaderNames.Bonus,
            SheetsConfig.HeaderNames.Total,  // OUTPUT - array formula
            SheetsConfig.HeaderNames.Cash,
            SheetsConfig.HeaderNames.OdometerStart,
            SheetsConfig.HeaderNames.OdometerEnd,
            SheetsConfig.HeaderNames.Distance,
            SheetsConfig.HeaderNames.Name,
            SheetsConfig.HeaderNames.AddressStart,
            SheetsConfig.HeaderNames.AddressEnd,
            SheetsConfig.HeaderNames.UnitEnd,
            SheetsConfig.HeaderNames.OrderNumber,
            SheetsConfig.HeaderNames.Region,
            SheetsConfig.HeaderNames.Note,
            SheetsConfig.HeaderNames.Key,           // OUTPUT - array formula
            SheetsConfig.HeaderNames.Day,           // OUTPUT - array formula
            SheetsConfig.HeaderNames.Month,         // OUTPUT - array formula
            SheetsConfig.HeaderNames.Year,          // OUTPUT - array formula
            SheetsConfig.HeaderNames.AmountPerTime, // OUTPUT - array formula
            SheetsConfig.HeaderNames.AmountPerDistance // OUTPUT - array formula
        };

        // Act
        var result = TripMapper.MapToRangeData(new List<TripEntity> { trip }, headers);

        // Assert
        Assert.Single(result);
        var row = result[0];
        Assert.Equal(30, row.Count); // All 30 columns present

        // Verify INPUT columns have values
        Assert.Equal("2024-01-15", row[0]);  // Date
        Assert.Equal("Uber", row[1]);        // Service
        Assert.Equal("1", row[2]);           // Number
        Assert.Equal("False", row[3]);       // Exclude
        Assert.Equal("UberX", row[4]);       // Type
        Assert.Equal("Restaurant", row[5]);  // Place
        Assert.Equal("18:30:00", row[6]);    // Pickup
        Assert.Equal("19:15:00", row[7]);    // Dropoff
        Assert.Equal("00:45:00", row[8]);    // Duration
        Assert.Equal("25.50", row[9]);       // Pay
        Assert.Equal("5.00", row[10]);       // Tips
        Assert.Equal("2.00", row[11]);       // Bonus
        
        // **CRITICAL**: Verify Total (OUTPUT) is NULL but reserves position
        Assert.Null(row[12]); // Total - OUTPUT (formula column)
        
        // Verify subsequent INPUT columns are in correct positions (not shifted)
        Assert.Equal("3.00", row[13]);       // Cash (would be position 12 if Total didn't reserve!)
        Assert.Equal("12345.0", row[14]);    // OdometerStart
        Assert.Equal("12360.5", row[15]);    // OdometerEnd
        Assert.Equal("15.5", row[16]);       // Distance
        Assert.Equal("John D.", row[17]);    // Name
        Assert.Equal("123 Main St", row[18]); // StartAddress
        Assert.Equal("456 Oak Ave", row[19]); // EndAddress
        Assert.Equal("Apt 5", row[20]);      // EndUnit
        Assert.Equal("ABC123", row[21]);     // OrderNumber
        Assert.Equal("Downtown", row[22]);   // Region
        Assert.Equal("Left at door", row[23]); // Note

        // Verify all OUTPUT formula columns are NULL
        Assert.Null(row[24]); // Key - OUTPUT
        Assert.Null(row[25]); // Day - OUTPUT
        Assert.Null(row[26]); // Month - OUTPUT
        Assert.Null(row[27]); // Year - OUTPUT
        Assert.Null(row[28]); // AmountPerTime - OUTPUT
        Assert.Null(row[29]); // AmountPerDistance - OUTPUT
    }

    [Fact]
    public void MapToRowData_WithRealisticTripData_ShouldPreserveAllColumnPositions()
    {
        // Arrange - Same trip as above
        var trip = new TripEntity
        {
            Date = "2024-01-15",
            Service = "Uber",
            Pay = 25.50m,
            Tip = 5.00m,
            Bonus = 2.00m,
            Total = 32.50m,  // OUTPUT
            Cash = 3.00m,
            Key = "2024-01-15-1-Uber", // OUTPUT
            AmountPerTime = 43.33m // OUTPUT
        };

        var headers = new List<object>
        {
            SheetsConfig.HeaderNames.Date,
            SheetsConfig.HeaderNames.Service,
            SheetsConfig.HeaderNames.Pay,
            SheetsConfig.HeaderNames.Tips,
            SheetsConfig.HeaderNames.Bonus,
            SheetsConfig.HeaderNames.Total,       // OUTPUT
            SheetsConfig.HeaderNames.Cash,
            SheetsConfig.HeaderNames.Key,         // OUTPUT
            SheetsConfig.HeaderNames.AmountPerTime // OUTPUT
        };

        // Act
        var result = TripMapper.MapToRowData(new List<TripEntity> { trip }, headers);

        // Assert
        Assert.Single(result);
        var row = result[0];
        Assert.NotNull(row.Values);
        Assert.Equal(9, row.Values.Count);

        // Verify INPUT columns have CellData with values
        Assert.NotNull(row.Values[0]); // Date
        Assert.NotNull(row.Values[1]); // Service
        Assert.NotNull(row.Values[2]); // Pay
        Assert.NotNull(row.Values[3]); // Tips
        Assert.NotNull(row.Values[4]); // Bonus

        // Verify OUTPUT column is NULL but position reserved
        Assert.Null(row.Values[5]); // Total - OUTPUT

        // Verify subsequent INPUT column is in correct position
        Assert.NotNull(row.Values[6]); // Cash
        Assert.Equal(3.00, row.Values[6]!.UserEnteredValue?.NumberValue);

        // Verify remaining OUTPUT columns are NULL
        Assert.Null(row.Values[7]); // Key - OUTPUT
        Assert.Null(row.Values[8]); // AmountPerTime - OUTPUT
    }

    [Fact]
    public void MapToRangeData_MultipleTrips_AllOutputColumnsPreservePositions()
    {
        // Arrange - Multiple trips to ensure consistency
        var trips = new List<TripEntity>
        {
            new() 
            { 
                Pay = 10.00m, 
                Tip = 2.00m, 
                Total = 12.00m, 
                Cash = 1.00m,
                Key = "key1" 
            },
            new() 
            { 
                Pay = 20.00m, 
                Tip = 4.00m, 
                Total = 24.00m, 
                Cash = 2.00m,
                Key = "key2" 
            },
            new() 
            { 
                Pay = 30.00m, 
                Tip = 6.00m, 
                Total = 36.00m, 
                Cash = 3.00m,
                Key = "key3" 
            }
        };

        var headers = new List<object>
        {
            SheetsConfig.HeaderNames.Pay,
            SheetsConfig.HeaderNames.Tips,
            SheetsConfig.HeaderNames.Total,  // OUTPUT
            SheetsConfig.HeaderNames.Cash,
            SheetsConfig.HeaderNames.Key     // OUTPUT
        };

        // Act
        var result = TripMapper.MapToRangeData(trips, headers);

        // Assert
        Assert.Equal(3, result.Count);

        // Verify each row maintains position alignment
        for (int i = 0; i < 3; i++)
        {
            var row = result[i];
            Assert.Equal(5, row.Count);

            // INPUT columns have values
            Assert.NotNull(row[0]); // Pay
            Assert.NotNull(row[1]); // Tips

            // OUTPUT column is NULL
            Assert.Null(row[2]); // Total - OUTPUT

            // INPUT column after OUTPUT
            Assert.NotNull(row[3]); // Cash

            // OUTPUT column at end
            Assert.Null(row[4]); // Key - OUTPUT
        }
    }

    [Fact]
    public void MapToRangeData_WithAllOutputColumns_ReturnsAllNulls()
    {
        // Arrange - Headers with ONLY output columns
        var trips = new List<TripEntity>
        {
            new()
            {
                Total = 100.00m,
                Key = "test-key",
                Day = "Monday",
                Month = "January",
                Year = "2024",
                AmountPerTime = 50.00m,
                AmountPerDistance = 10.00m
            }
        };

        var headers = new List<object>
        {
            SheetsConfig.HeaderNames.Total,
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
        Assert.Equal(7, row.Count);
        
        // ALL should be null (output columns)
        Assert.All(row, value => Assert.Null(value));
    }
}
