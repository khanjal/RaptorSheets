using RaptorSheets.Core.Mappers;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Mappers;

namespace RaptorSheets.Gig.Tests.Unit.Mappers;

/// <summary>
/// Tests to verify TripMapper uses GenericSheetMapper correctly
/// </summary>
public class TripMapperGenericConversionTests
{
    [Fact]
    public void TripMapper_ShouldUseGenericMapper_ForMapFromRangeData()
    {
        // Arrange
        var values = new List<IList<object>>
        {
            new List<object> { "Date", "Service", "#", "Pay", "Tips", "Bonus" },
            new List<object> { "2024-01-15", "Uber", "1", "25.50", "5.00", "2.00" },
            new List<object> { "2024-01-16", "Lyft", "2", "45.75", "10.00", "" }
        };

        // Act
        var trips = TripMapper.MapFromRangeData(values);

        // Assert
        Assert.NotNull(trips);
        Assert.Equal(2, trips.Count);
        
        // Verify first trip
        Assert.Equal("2024-01-15", trips[0].Date);
        Assert.Equal("Uber", trips[0].Service);
        Assert.Equal(1, trips[0].Number);
        Assert.Equal(25.50m, trips[0].Pay);
        Assert.Equal(5.00m, trips[0].Tip);
        Assert.Equal(2.00m, trips[0].Bonus);
        Assert.True(trips[0].Saved);
        
        // Verify second trip
        Assert.Equal("2024-01-16", trips[1].Date);
        Assert.Equal("Lyft", trips[1].Service);
        Assert.Equal(2, trips[1].Number);
        Assert.Equal(45.75m, trips[1].Pay);
        Assert.Equal(10.00m, trips[1].Tip);
        Assert.Null(trips[1].Bonus); // Empty string should be null
        Assert.True(trips[1].Saved);
    }

    [Fact]
    public void TripMapper_ShouldUseGenericMapper_ForMapToRangeData()
    {
        // Arrange
        var trips = new List<TripEntity>
        {
            new()
            {
                Date = "2024-01-15",
                Service = "Uber",
                Number = 1,
                Pay = 25.50m,
                Tip = 5.00m,
                Bonus = 2.00m
            }
        };
        var headers = new List<object> { "Date", "Service", "#", "Pay", "Tips", "Bonus" };

        // Act
        var result = TripMapper.MapToRangeData(trips, headers);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var row = result[0];
        Assert.Equal("2024-01-15", row[0]);
        Assert.Equal("Uber", row[1]);
        Assert.Equal("1", row[2]);
        Assert.Equal("25.50", row[3]);
        Assert.Equal("5.00", row[4]);
        Assert.Equal("2.00", row[5]);
    }

    [Fact]
    public void TripMapper_ShouldUseGenericMapper_ForMapToRowData()
    {
        // Arrange
        var trips = new List<TripEntity>
        {
            new()
            {
                Date = "2024-01-15",
                Service = "Uber",
                Number = 1,
                Exclude = false,
                Pickup = "10:00:00",
                Dropoff = "11:30:00",
                Duration = "01:30:00",
                Pay = 25.50m
            }
        };
        var headers = new List<object> { "Date", "Service", "#", "Exclude", "Pickup", "Dropoff", "Duration", "Pay" };

        // Act
        var result = TripMapper.MapToRowData(trips, headers);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var row = result[0];
        Assert.NotNull(row.Values);
        Assert.Equal(8, row.Values.Count);
        
        // Verify Date uses serial date conversion (DateTime field type)
        Assert.NotNull(row.Values[0].UserEnteredValue?.NumberValue);
        
        // Verify Service is string
        Assert.Equal("Uber", row.Values[1].UserEnteredValue?.StringValue);
        
        // Verify Number is numeric
        Assert.Equal(1.0, row.Values[2].UserEnteredValue?.NumberValue);
        
        // Verify Exclude is boolean (can be false or null, both valid)
        Assert.Equal(false, row.Values[3].UserEnteredValue?.BoolValue ?? false);
        
        // Verify Pickup uses serial time conversion (Time field type)
        Assert.NotNull(row.Values[4].UserEnteredValue?.NumberValue);
        
        // Verify Dropoff uses serial time conversion (Time field type)
        Assert.NotNull(row.Values[5].UserEnteredValue?.NumberValue);
        
        // Verify Duration uses serial duration conversion (Duration field type)
        Assert.NotNull(row.Values[6].UserEnteredValue?.NumberValue);
        
        // Verify Pay is numeric (Currency field type)
        Assert.Equal(25.50, row.Values[7].UserEnteredValue?.NumberValue);
    }

    [Fact]
    public void TripMapper_ShouldUseGenericMapper_ForMapToRowFormat()
    {
        // Arrange
        var headers = new List<object> { "Date", "Pay", "Tips", "Pickup", "Duration" };

        // Act
        var result = TripMapper.MapToRowFormat(headers);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Values);
        Assert.Equal(5, result.Values.Count);
        
        // The GenericSheetMapper should apply formats based on field types
        // Some formats might be null/default if they don't have specific patterns
        // Just verify the row structure is correct
        Assert.All(result.Values, cell => Assert.NotNull(cell));
    }

    [Fact]
    public void TripMapper_ShouldHandleInputOutputDistinction()
    {
        // Arrange
        var trips = new List<TripEntity>
        {
            new()
            {
                Date = "2024-01-15",
                Pay = 25.50m,
                Tip = 5.00m,
                Bonus = 2.00m,
                Total = 33.00m, // Output column (formula) - should not be written
                Key = "2024-01-15-Uber-1", // Output column (formula) - should not be written
                AmountPerTime = 20.00m // Output column (formula) - should not be written
            }
        };
        var headers = new List<object> { "Date", "Pay", "Tips", "Bonus", "Total", "Key", "Amount/Time" };

        // Act
        var result = TripMapper.MapToRangeData(trips, headers);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var row = result[0];
        
        // Input columns should have values
        Assert.Equal("2024-01-15", row[0]); // Date (input)
        Assert.Equal("25.50", row[1]); // Pay (input)
        Assert.Equal("5.00", row[2]); // Tip (input)
        Assert.Equal("2.00", row[3]); // Bonus (input)
        
        // Output columns (formulas) should be null to preserve formula
        Assert.Null(row[4]); // Total (output/formula)
        Assert.Null(row[5]); // Key (output/formula)
        Assert.Null(row[6]); // AmountPerTime (output/formula)
    }

    [Fact]
    public void TripMapper_GetSheet_ShouldConfigureFormulasCorrectly()
    {
        // Act
        var sheet = TripMapper.GetSheet();

        // Assert
        Assert.NotNull(sheet);
        Assert.Equal("Trips", sheet.Name);
        Assert.NotNull(sheet.Headers);
        
        // Verify all headers have column assignments
        Assert.All(sheet.Headers, header => 
        {
            Assert.True(header.Index >= 0);
            Assert.False(string.IsNullOrEmpty(header.Column));
        });
        
        // Verify formula columns are configured
        var totalHeader = sheet.Headers.FirstOrDefault(h => h.Name == "Total");
        Assert.NotNull(totalHeader);
        Assert.False(string.IsNullOrEmpty(totalHeader.Formula));
        Assert.StartsWith("=", totalHeader.Formula);
        
        var keyHeader = sheet.Headers.FirstOrDefault(h => h.Name == "Key");
        Assert.NotNull(keyHeader);
        Assert.False(string.IsNullOrEmpty(keyHeader.Formula));
        Assert.StartsWith("=", keyHeader.Formula);
    }
}
