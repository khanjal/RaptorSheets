using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Mappers;
using HeaderEnum = RaptorSheets.Gig.Enums.HeaderEnum;

namespace RaptorSheets.Gig.Tests.Unit.Mappers;

public class ShiftMapperTests
{
    #region Core Mapping Tests (Essential)
    
    [Fact]
    public void MapFromRangeData_WithValidData_ShouldReturnShifts()
    {
        // Arrange
        var values = new List<IList<object>>
        {
            new List<object> { "Date", "Start", "Finish", "Service", "#", "Region" },
            new List<object> { "2024-01-15", "09:00:00", "17:00:00", "Uber", "123", "Downtown" },
            new List<object> { "2024-01-16", "10:00:00", "18:00:00", "Lyft", "124", "Suburbs" }
        };

        // Act
        var result = GenericSheetMapper<ShiftEntity>.MapFromRangeData(values);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        
        var firstShift = result[0];
        Assert.Equal(2, firstShift.RowId);
        Assert.Equal("2024-01-15", firstShift.Date);
        Assert.Equal("09:00:00", firstShift.Start);
        Assert.Equal("17:00:00", firstShift.Finish);
        Assert.Equal("Uber", firstShift.Service);
        Assert.Equal(123, firstShift.Number);
        Assert.Equal("Downtown", firstShift.Region);
        
        var secondShift = result[1];
        Assert.Equal(3, secondShift.RowId);
        Assert.Equal("2024-01-16", secondShift.Date);
        Assert.Equal("Lyft", secondShift.Service);
        Assert.Equal("Suburbs", secondShift.Region);
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
        var result = GenericSheetMapper<ShiftEntity>.MapFromRangeData(values);

        // Assert
        Assert.Equal(2, result.Count); // Empty row filtered out
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
                Region = "Downtown"
            }
        };
        var headers = new List<object> { "Date", "Start", "Finish", "Service", "#", "Region" };

        // Act
        var result = GenericSheetMapper<ShiftEntity>.MapToRangeData(shifts, headers);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var row = result[0];
        Assert.Equal("2024-01-15", row[0]);
        Assert.Equal("09:00:00", row[1]);
        Assert.Equal("17:00:00", row[2]);
        Assert.Equal("Uber", row[3]);
        Assert.Equal("123", row[4]);
        Assert.Equal("Downtown", row[5]);
    }
    
    #endregion

    #region Sheet Configuration Tests (Core Structure)
    
    [Fact]
    public void GetSheet_ShouldReturnCorrectSheetConfiguration()
    {
        // Act
        var result = ShiftMapper.GetSheet();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Shifts", result.Name);
        Assert.NotNull(result.Headers);
        Assert.True(result.Headers.Count > 10, "Shift sheet should have many columns");
        
        // Verify essential headers exist with proper configuration
        var dateHeader = result.Headers.FirstOrDefault(h => h.Name == HeaderEnum.DATE.GetDescription());
        Assert.NotNull(dateHeader);
        Assert.Equal(FormatEnum.DATE, dateHeader.Format);
        
        var payHeader = result.Headers.FirstOrDefault(h => h.Name == HeaderEnum.PAY.GetDescription());
        Assert.NotNull(payHeader);
        Assert.Equal(FormatEnum.CURRENCY, payHeader.Format);
        
        // Verify all headers have proper column assignments
        Assert.All(result.Headers, header => 
        {
            Assert.True(header.Index >= 0, "All headers should have valid indexes");
            Assert.False(string.IsNullOrEmpty(header.Column), "All headers should have column letters");
        });
    }
    
    #endregion

    #region Basic Validation Tests (High-Level Only)
    
    [Theory]
    [InlineData("Date", "2024-01-15")]  // Representative test cases only
    [InlineData("Service", "Uber")]
    [InlineData("Pay", "50.00")]
    public void MapFromRangeData_WithSingleColumn_ShouldMapCorrectly(string headerName, string testValue)
    {
        // Arrange
        var values = new List<IList<object>>
        {
            new List<object> { headerName },
            new List<object> { testValue }
        };

        // Act
        var result = GenericSheetMapper<ShiftEntity>.MapFromRangeData(values);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var shift = result[0];
        switch (headerName)
        {
            case "Date":
                Assert.Equal(testValue, shift.Date);
                break;
            case "Service":
                Assert.Equal(testValue, shift.Service);
                break;
            case "Pay":
                Assert.Equal(50.00m, shift.Pay);
                break;
        }
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
                Number = 123
            }
        };
        var headers = new List<object> { "Date", "Start", "Service", "#" };

        // Act
        var result = GenericSheetMapper<ShiftEntity>.MapToRowData(shifts, headers);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var rowData = result[0];
        Assert.Equal(4, rowData.Values.Count);
        
        // Basic validation - should have proper cell values (don't test implementation details)
        Assert.NotNull(rowData.Values[0].UserEnteredValue); // Date
        Assert.NotNull(rowData.Values[1].UserEnteredValue); // Start time
        Assert.Equal("Uber", rowData.Values[2].UserEnteredValue.StringValue); // Service
        Assert.Equal(123, rowData.Values[3].UserEnteredValue.NumberValue); // Number
    }
    
    #endregion
}
