using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Mappers;
using System.ComponentModel;

namespace RaptorSheets.Gig.Tests.Unit.Mappers;

[Category("Unit Tests")]
public class TripMapperTests
{
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
        var result = TripMapper.MapFromRangeData(values);

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
    public void GetSheet_ShouldReturnCorrectSheetConfiguration()
    {
        // Act
        var result = TripMapper.GetSheet();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Headers);
        Assert.True(result.Headers.Count > 15); // Should have many columns
        
        // Check key headers exist and have proper configuration
        var dateHeader = result.Headers.FirstOrDefault(h => h.Name == HeaderEnum.DATE.GetDescription());
        Assert.NotNull(dateHeader);
        Assert.Equal(FormatEnum.DATE, dateHeader.Format);
        
        var serviceHeader = result.Headers.FirstOrDefault(h => h.Name == HeaderEnum.SERVICE.GetDescription());
        Assert.NotNull(serviceHeader);
        Assert.Equal(ValidationEnum.RANGE_SERVICE.GetDescription(), serviceHeader.Validation);
        
        var payHeader = result.Headers.FirstOrDefault(h => h.Name == HeaderEnum.PAY.GetDescription());
        Assert.NotNull(payHeader);
        Assert.Equal(FormatEnum.ACCOUNTING, payHeader.Format);
    }

    [Fact]
    public void GetSheet_KeyHeader_ShouldGenerateTripKeyFormula()
    {
        // Act
        var sheet = TripMapper.GetSheet();
        var keyHeader = sheet.Headers.First(h => h.Name.ToString() == HeaderEnum.KEY.GetDescription());

        // Assert
        Assert.NotNull(keyHeader.Formula);
        Assert.StartsWith("=ARRAYFORMULA(", keyHeader.Formula);
        Assert.Contains("IF(ISBLANK(", keyHeader.Formula); // Should contain conditional logic
        Assert.Contains("\"-X-\"", keyHeader.Formula); // Exclude marker
        Assert.Contains("\"-0-\"", keyHeader.Formula); // Default number fallback
        Assert.Contains("\"-\"", keyHeader.Formula); // Normal delimiter
        // Note: The actual range references will be resolved, not placeholder tokens
    }

    [Fact]
    public void GetSheet_TotalHeader_ShouldGenerateIncomeAddition()
    {
        // Act
        var sheet = TripMapper.GetSheet();
        var totalHeader = sheet.Headers.First(h => h.Name.ToString() == HeaderEnum.TOTAL.GetDescription());

        // Assert
        Assert.NotNull(totalHeader.Formula);
        Assert.Contains("=ARRAYFORMULA(", totalHeader.Formula);
        Assert.Contains("+", totalHeader.Formula); // Addition of pay + tips + bonus
        Assert.Equal(FormatEnum.ACCOUNTING, totalHeader.Format);
    }

    [Fact]
    public void GetSheet_DateComponentHeaders_ShouldGenerateDateExtractionFormulas()
    {
        // Act
        var sheet = TripMapper.GetSheet();
        var dayHeader = sheet.Headers.First(h => h.Name.ToString() == HeaderEnum.DAY.GetDescription());
        var monthHeader = sheet.Headers.First(h => h.Name.ToString() == HeaderEnum.MONTH.GetDescription());
        var yearHeader = sheet.Headers.First(h => h.Name.ToString() == HeaderEnum.YEAR.GetDescription());

        // Assert
        Assert.NotNull(dayHeader.Formula);
        Assert.Contains("DAY(", dayHeader.Formula);
        
        Assert.NotNull(monthHeader.Formula);
        Assert.Contains("MONTH(", monthHeader.Formula);
        
        Assert.NotNull(yearHeader.Formula);
        Assert.Contains("YEAR(", yearHeader.Formula);
    }

    [Fact]
    public void GetSheet_AmountPerHeaders_ShouldGenerateCalculationFormulas()
    {
        // Act
        var sheet = TripMapper.GetSheet();
        var amountPerTimeHeader = sheet.Headers.First(h => h.Name.ToString() == HeaderEnum.AMOUNT_PER_TIME.GetDescription());
        var amountPerDistanceHeader = sheet.Headers.First(h => h.Name.ToString() == HeaderEnum.AMOUNT_PER_DISTANCE.GetDescription());

        // Assert
        Assert.NotNull(amountPerTimeHeader.Formula);
        Assert.Contains("/IF(", amountPerTimeHeader.Formula); // Zero-safe division
        Assert.Contains("*24", amountPerTimeHeader.Formula); // Time conversion to hours
        Assert.Equal(FormatEnum.ACCOUNTING, amountPerTimeHeader.Format);
        
        Assert.NotNull(amountPerDistanceHeader.Formula);
        Assert.Contains("/IF(", amountPerDistanceHeader.Formula);
        Assert.Equal(FormatEnum.ACCOUNTING, amountPerDistanceHeader.Format);
    }

    [Theory]
    [InlineData(HeaderEnum.DATE, "2024-01-15")]
    [InlineData(HeaderEnum.SERVICE, "Uber")]
    [InlineData(HeaderEnum.NUMBER, "123")]
    [InlineData(HeaderEnum.TYPE, "UberX")]
    [InlineData(HeaderEnum.PLACE, "Restaurant")]
    [InlineData(HeaderEnum.PAY, "25.50")]
    [InlineData(HeaderEnum.NAME, "John Doe")]
    public void MapFromRangeData_WithSingleColumn_ShouldMapCorrectly(HeaderEnum headerType, string testValue)
    {
        // Arrange
        var headerName = headerType.GetDescription();
        var values = new List<IList<object>>
        {
            new List<object> { headerName },
            new List<object> { testValue }
        };

        // Act
        var result = TripMapper.MapFromRangeData(values);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var trip = result[0];
        switch (headerType)
        {
            case HeaderEnum.DATE:
                Assert.Equal(testValue, trip.Date);
                break;
            case HeaderEnum.SERVICE:
                Assert.Equal(testValue, trip.Service);
                break;
            case HeaderEnum.NUMBER:
                Assert.Equal(123, trip.Number);
                break;
            case HeaderEnum.TYPE:
                Assert.Equal(testValue, trip.Type);
                break;
            case HeaderEnum.PLACE:
                Assert.Equal(testValue, trip.Place);
                break;
            case HeaderEnum.PAY:
                Assert.Equal(25.50m, trip.Pay);
                break;
            case HeaderEnum.NAME:
                Assert.Equal(testValue, trip.Name);
                break;
        }
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
        var result = TripMapper.MapToRangeData(trips, headers);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var row = result[0];
        Assert.Equal("2024-01-15", row[0]);
        Assert.Equal("Uber", row[1]);
        Assert.Equal("1", row[2]);
        Assert.Equal("UberX", row[3]);
        Assert.Equal("25.50", row[4]); // Fix decimal comparison
        Assert.Equal("5.00", row[5]);   // Fix decimal comparison
        Assert.Equal("John Doe", row[6]);
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
        var result = TripMapper.MapFromRangeData(values);

        // Assert
        Assert.Equal(2, result.Count); // Empty row filtered out
        Assert.Equal("Uber", result[0].Service);
        Assert.Equal("Lyft", result[1].Service);
    }
}