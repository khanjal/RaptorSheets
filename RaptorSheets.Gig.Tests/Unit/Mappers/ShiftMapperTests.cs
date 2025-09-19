using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Mappers;
using HeaderEnum = RaptorSheets.Gig.Enums.HeaderEnum;

namespace RaptorSheets.Gig.Tests.Unit.Mappers;

public class ShiftMapperTests
{
    [Fact]
    public void MapFromRangeData_WithValidData_ShouldReturnShifts()
    {
        // Arrange
        var values = new List<IList<object>>
        {
            new List<object> { HeaderEnum.DATE.GetDescription(), HeaderEnum.TIME_START.GetDescription(), HeaderEnum.TIME_END.GetDescription(), HeaderEnum.SERVICE.GetDescription(), HeaderEnum.NUMBER.GetDescription(), HeaderEnum.REGION.GetDescription(), HeaderEnum.ODOMETER_START.GetDescription(), HeaderEnum.ODOMETER_END.GetDescription(), HeaderEnum.DISTANCE.GetDescription() },
            new List<object> { "2024-01-15", "09:00:00", "17:00:00", "Uber", "123", "Downtown", "100.0", "150.0", "50.0" },
            new List<object> { "2024-01-16", "10:00:00", "18:00:00", "Lyft", "124", "Suburbs", "200.0", "260.0", "60.0" }
        };

        // Act
        var result = ShiftMapper.MapFromRangeData(values);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        
        var firstShift = result[0];
        Assert.Equal(2, firstShift.RowId); // Row 2 (after headers)
        Assert.Equal("2024-01-15", firstShift.Date);
        Assert.Equal("09:00:00", firstShift.Start);
        Assert.Equal("17:00:00", firstShift.Finish);
        Assert.Equal("Uber", firstShift.Service);
        Assert.Equal(123, firstShift.Number);
        Assert.Equal("Downtown", firstShift.Region);
        Assert.Equal(100.0m, firstShift.OdometerStart);
        Assert.Equal(150.0m, firstShift.OdometerEnd);
        Assert.Equal(50.0m, firstShift.Distance); // Distance matches odometer difference
        
        var secondShift = result[1];
        Assert.Equal(3, secondShift.RowId);
        Assert.Equal("2024-01-16", secondShift.Date);
        Assert.Equal("Lyft", secondShift.Service);
        Assert.Equal("Suburbs", secondShift.Region);
        Assert.Equal(200.0m, secondShift.OdometerStart);
        Assert.Equal(260.0m, secondShift.OdometerEnd);
        Assert.Equal(60.0m, secondShift.Distance); // Distance matches odometer difference
    }

    [Fact]
    public void MapFromRangeData_WithEmptyRows_ShouldFilterOutEmptyRows()
    {
        // Arrange
        var values = new List<IList<object>>
        {
            new List<object> { HeaderEnum.DATE.GetDescription(), HeaderEnum.SERVICE.GetDescription() }, // Headers
            new List<object> { "2024-01-15", "Uber" },
            new List<object> { "", "" }, // Empty row - should be filtered
            new List<object> { "2024-01-16", "Lyft" }
        };

        // Act
        var result = ShiftMapper.MapFromRangeData(values);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count); // Empty row should be filtered out
        Assert.Equal("Uber", result[0].Service);
        Assert.Equal("Lyft", result[1].Service);
    }

    [Fact]
    public void MapToRangeData_WithValidShifts_ShouldReturnCorrectData()
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
                Note = "Good day"
            },
            new()
            {
                Date = "2024-01-16",
                Start = "10:00:00", 
                Finish = "18:00:00",
                Service = "Lyft",
                Number = 124,
                Region = "Suburbs"
            }
        };

        var headers = new List<object> { HeaderEnum.DATE.GetDescription(), HeaderEnum.TIME_START.GetDescription(), HeaderEnum.TIME_END.GetDescription(), HeaderEnum.SERVICE.GetDescription(), HeaderEnum.NUMBER.GetDescription(), HeaderEnum.REGION.GetDescription(), HeaderEnum.NOTE.GetDescription() };

        // Act
        var result = ShiftMapper.MapToRangeData(shifts, headers);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        
        var firstRow = result[0];
        Assert.Equal("2024-01-15", firstRow[0]);
        Assert.Equal("09:00:00", firstRow[1]);
        Assert.Equal("17:00:00", firstRow[2]);
        Assert.Equal("Uber", firstRow[3]);
        Assert.Equal("123", firstRow[4]);
        Assert.Equal("Downtown", firstRow[5]);
        Assert.Equal("Good day", firstRow[6]);
        
        var secondRow = result[1];
        Assert.Equal("2024-01-16", secondRow[0]);
        Assert.Equal("10:00:00", secondRow[1]);
        Assert.Equal("18:00:00", secondRow[2]);
        Assert.Equal("Lyft", secondRow[3]);
        Assert.Equal("124", secondRow[4]);
        Assert.Equal("Suburbs", secondRow[5]);
        Assert.Equal("", secondRow[6]); // Note is empty string, not null for second shift
    }

    [Fact]
    public void MapToRowData_WithValidShifts_ShouldReturnRowData()
    {
        // Arrange
        var shifts = new List<ShiftEntity>
        {
            new()
            {
                Date = "2024-01-15",
                Start = "09:00:00",
                Service = "Uber",
                Number = 123,
                Pay = 50.0m,
                Tip = 10.0m,
                Omit = false,
                Region = "Downtown"
            }
        };
        var headers = new List<object>
        {
            HeaderEnum.DATE.GetDescription(),
            HeaderEnum.TIME_START.GetDescription(),
            HeaderEnum.SERVICE.GetDescription(),
            HeaderEnum.NUMBER.GetDescription(),
            HeaderEnum.PAY.GetDescription(),
            HeaderEnum.TIPS.GetDescription(),
            HeaderEnum.TIME_OMIT.GetDescription(),
            HeaderEnum.REGION.GetDescription()
        };

        // Act
        var result = ShiftMapper.MapToRowData(shifts, headers);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var rowData = result[0];
        Assert.Equal(8, rowData.Values.Count); // 8 columns
        
        // Check specific cell types
        Assert.NotNull(rowData.Values[0].UserEnteredValue.NumberValue); // Date as serial
        Assert.NotNull(rowData.Values[1].UserEnteredValue.NumberValue); // Time as serial
        Assert.Equal("Uber", rowData.Values[2].UserEnteredValue.StringValue); // Service as string
        Assert.Equal(123, rowData.Values[3].UserEnteredValue.NumberValue); // Number
        Assert.Equal(50.0, rowData.Values[4].UserEnteredValue.NumberValue); // Pay
        Assert.Equal(10.0, rowData.Values[5].UserEnteredValue.NumberValue); // Tips
        Assert.False(rowData.Values[6].UserEnteredValue.BoolValue); // Omit as boolean
        Assert.Equal("Downtown", rowData.Values[7].UserEnteredValue.StringValue); // Region
    }

    [Fact]
    public void MapToRowData_WithNullValues_ShouldHandleGracefully()
    {
        // Arrange
        var shifts = new List<ShiftEntity>
        {
            new()
            {
                Date = "2024-01-15",
                Service = string.Empty, // Use empty string instead of null
                Pay = null,
                Region = string.Empty // Use empty string instead of null
            }
        };
        var headers = new List<object>
        {
            HeaderEnum.DATE.GetDescription(),
            HeaderEnum.SERVICE.GetDescription(),
            HeaderEnum.PAY.GetDescription(),
            HeaderEnum.REGION.GetDescription()
        };

        // Act
        var result = ShiftMapper.MapToRowData(shifts, headers);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var rowData = result[0];
        Assert.Equal(4, rowData.Values.Count);
        
        // Check null handling
        Assert.NotNull(rowData.Values[0].UserEnteredValue.NumberValue); // Date should still work
        Assert.Equal(string.Empty, rowData.Values[1].UserEnteredValue.StringValue); // Empty service
        Assert.Null(rowData.Values[2].UserEnteredValue.NumberValue); // Null pay
        Assert.Equal(string.Empty, rowData.Values[3].UserEnteredValue.StringValue); // Empty region
    }

    [Fact]
    public void GetSheet_ShouldReturnCorrectSheetConfiguration()
    {
        // Act
        var result = ShiftMapper.GetSheet();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Headers);
        Assert.True(result.Headers.Count > 20); // Should have many columns due to complex formulas
        
        // Check key headers exist
        var dateHeader = result.Headers.FirstOrDefault(h => h.Name == HeaderEnum.DATE.GetDescription());
        Assert.NotNull(dateHeader);
        Assert.Equal(FormatEnum.DATE, dateHeader.Format);
        
        var serviceHeader = result.Headers.FirstOrDefault(h => h.Name == HeaderEnum.SERVICE.GetDescription());
        Assert.NotNull(serviceHeader);
        Assert.Equal(ValidationEnum.RANGE_SERVICE.GetDescription(), serviceHeader.Validation);
        
        var payHeader = result.Headers.FirstOrDefault(h => h.Name == HeaderEnum.PAY.GetDescription());
        Assert.NotNull(payHeader);
        Assert.Equal(FormatEnum.ACCOUNTING, payHeader.Format);
        
        // Check that formula columns exist
        var keyHeader = result.Headers.FirstOrDefault(h => h.Name == HeaderEnum.KEY.GetDescription());
        Assert.NotNull(keyHeader);
        Assert.NotNull(keyHeader.Formula);
        Assert.Contains("ARRAYFORMULA", keyHeader.Formula);
        
        var totalPayHeader = result.Headers.FirstOrDefault(h => h.Name == HeaderEnum.TOTAL_PAY.GetDescription());
        Assert.NotNull(totalPayHeader);
        Assert.NotNull(totalPayHeader.Formula);
        Assert.Contains("SUMIF", totalPayHeader.Formula);
    }

    [Theory]
    [InlineData(HeaderEnum.DATE)]
    [InlineData(HeaderEnum.SERVICE)]
    [InlineData(HeaderEnum.NUMBER)]
    [InlineData(HeaderEnum.PAY)]
    [InlineData(HeaderEnum.REGION)]
    public void MapFromRangeData_WithSingleColumn_ShouldMapCorrectly(HeaderEnum headerType)
    {
        // Arrange
        var headerName = headerType.GetDescription();
        var values = new List<IList<object>>
        {
            new List<object> { headerName }, // Single header
            new List<object> { GetTestValueForHeader(headerType) } // Single value
        };

        // Act
        var result = ShiftMapper.MapFromRangeData(values);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var shift = result[0];
        switch (headerType)
        {
            case HeaderEnum.DATE:
                Assert.Equal("2024-01-15", shift.Date);
                break;
            case HeaderEnum.SERVICE:
                Assert.Equal("Test Service", shift.Service);
                break;
            case HeaderEnum.NUMBER:
                Assert.Equal(123, shift.Number);
                break;
            case HeaderEnum.PAY:
                Assert.Equal(50.0m, shift.Pay);
                break;
            case HeaderEnum.REGION:
                Assert.Equal("Test Region", shift.Region);
                break;
        }
    }

    private static object GetTestValueForHeader(HeaderEnum headerType)
    {
        return headerType switch
        {
            HeaderEnum.DATE => "2024-01-15",
            HeaderEnum.SERVICE => "Test Service",
            HeaderEnum.NUMBER => "123",
            HeaderEnum.PAY => "50.00",
            HeaderEnum.REGION => "Test Region",
            _ => "Test Value"
        };
    }

    [Fact]
    public void MapFromRangeData_WithOdometer_ShouldMapCorrectly()
    {
        var values = new List<IList<object>>
        {
            new List<object> { "Odo Start", "Odo End" },
            new List<object> { "100.5", "150.75" }
        };

        var result = ShiftMapper.MapFromRangeData(values);

        Assert.Single(result);
        var shift = result[0];
        Assert.Equal(100.5m, shift.OdometerStart);
        Assert.Equal(150.75m, shift.OdometerEnd);
    }

    [Fact]
    public void MapToRangeData_WithOdometer_ShouldReturnCorrectData()
    {
        var shifts = new List<ShiftEntity>
        {
            new() { OdometerStart = 100.5m, OdometerEnd = 150.75m }
        };
        var headers = new List<object> { "Odo Start", "Odo End" };

        var result = ShiftMapper.MapToRangeData(shifts, headers);

        Assert.Equal("100.5", result[0][0]);
        Assert.Equal("150.75", result[0][1]);
    }

    [Fact]
    public void MapToRowData_WithOdometer_ShouldReturnRowData()
    {
        var shifts = new List<ShiftEntity>
        {
            new() { OdometerStart = 100.5m, OdometerEnd = 150.75m }
        };
        var headers = new List<object> { "Odo Start", "Odo End" };

        var result = ShiftMapper.MapToRowData(shifts, headers);

        Assert.Equal(100.5, result[0].Values[0].UserEnteredValue.NumberValue);
        Assert.Equal(150.75, result[0].Values[1].UserEnteredValue.NumberValue);
    }
}