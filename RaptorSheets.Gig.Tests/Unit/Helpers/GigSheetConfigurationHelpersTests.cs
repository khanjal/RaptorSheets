using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Helpers;

namespace RaptorSheets.Gig.Tests.Unit.Helpers;

public class GigSheetConfigurationHelpersTests
{
    [Fact]
    public void ConfigureSheet_ShouldUpdateColumnsAndApplyConfigurator()
    {
        // Arrange
        var baseSheet = new SheetModel
        {
            Name = "TestSheet",
            Headers = new List<SheetCellModel>
            {
                new() { Name = SheetsConfig.HeaderNames.Date },
                new() { Name = SheetsConfig.HeaderNames.Amount },
                new() { Name = SheetsConfig.HeaderNames.Name }
            }
        };

        var configuratorCallCount = 0;

        // Act
        var result = GigSheetConfigurationHelpers.ConfigureSheet(baseSheet, (header, index) =>
        {
            configuratorCallCount++;
            header.Note = $"Configured at index {index}";
        });

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, configuratorCallCount);
        Assert.Equal("Configured at index 0", result.Headers[0].Note);
        Assert.Equal("Configured at index 1", result.Headers[1].Note);
        Assert.Equal("Configured at index 2", result.Headers[2].Note);
        
        // Verify columns were updated
        Assert.Equal("A", result.Headers[0].Column);
        Assert.Equal("B", result.Headers[1].Column);
        Assert.Equal("C", result.Headers[2].Column);
    }

    [Fact]
    public void CreateArrayFormula_ShouldGenerateCorrectFormula()
    {
        // Arrange
        var headerText = "Total Amount";
        var keyRange = "A2:A";
        var formula = "SUM(B2:B)";

        // Act
        var result = GigSheetConfigurationHelpers.CreateArrayFormula(headerText, keyRange, formula);

        // Assert
        var expected = "=ARRAYFORMULA(IFS(ROW(A2:A)=1,\"Total Amount\",ISBLANK(A2:A), \"\",true,SUM(B2:B)))";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void CreateDatePartFormula_ShouldGenerateCorrectFormula()
    {
        // Arrange
        var headerText = "Day";
        var dateRange = "A2:A";
        var datePart = "DAY";

        // Act
        var result = GigSheetConfigurationHelpers.CreateDatePartFormula(headerText, dateRange, datePart);

        // Assert
        var expected = "=ARRAYFORMULA(IFS(ROW(A2:A)=1,\"Day\",ISBLANK(A2:A), \"\",true,DAY(A2:A)))";
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(HeaderEnum.DATE, FormatEnum.DATE)]
    [InlineData(HeaderEnum.DATE_BEGIN, FormatEnum.DATE)]
    [InlineData(HeaderEnum.DATE_END, FormatEnum.DATE)]
    [InlineData(HeaderEnum.VISIT_FIRST, FormatEnum.DATE)]
    [InlineData(HeaderEnum.VISIT_LAST, FormatEnum.DATE)]
    [InlineData(HeaderEnum.TIME_START, FormatEnum.TIME)]
    [InlineData(HeaderEnum.TIME_END, FormatEnum.TIME)]
    [InlineData(HeaderEnum.DURATION, FormatEnum.DURATION)]
    [InlineData(HeaderEnum.TIME_TOTAL, FormatEnum.DURATION)]
    [InlineData(HeaderEnum.TIME_ACTIVE, FormatEnum.DURATION)]
    [InlineData(HeaderEnum.TOTAL_TIME, FormatEnum.DURATION)]
    [InlineData(HeaderEnum.TOTAL_TIME_ACTIVE, FormatEnum.DURATION)]
    [InlineData(HeaderEnum.AMOUNT, FormatEnum.ACCOUNTING)]
    [InlineData(HeaderEnum.PAY, FormatEnum.ACCOUNTING)]
    [InlineData(HeaderEnum.TIP, FormatEnum.ACCOUNTING)]
    [InlineData(HeaderEnum.TIPS, FormatEnum.ACCOUNTING)]
    [InlineData(HeaderEnum.BONUS, FormatEnum.ACCOUNTING)]
    [InlineData(HeaderEnum.CASH, FormatEnum.ACCOUNTING)]
    [InlineData(HeaderEnum.TOTAL, FormatEnum.ACCOUNTING)]
    [InlineData(HeaderEnum.TOTAL_BONUS, FormatEnum.ACCOUNTING)]
    [InlineData(HeaderEnum.TOTAL_CASH, FormatEnum.ACCOUNTING)]
    [InlineData(HeaderEnum.TOTAL_PAY, FormatEnum.ACCOUNTING)]
    [InlineData(HeaderEnum.TOTAL_TIPS, FormatEnum.ACCOUNTING)]
    [InlineData(HeaderEnum.TOTAL_GRAND, FormatEnum.ACCOUNTING)]
    [InlineData(HeaderEnum.AVERAGE, FormatEnum.ACCOUNTING)]
    [InlineData(HeaderEnum.DISTANCE, FormatEnum.DISTANCE)]
    [InlineData(HeaderEnum.TOTAL_DISTANCE, FormatEnum.DISTANCE)]
    [InlineData(HeaderEnum.NUMBER, FormatEnum.NUMBER)]
    [InlineData(HeaderEnum.ORDER_NUMBER, FormatEnum.NUMBER)]
    [InlineData(HeaderEnum.TRIPS, FormatEnum.NUMBER)]
    [InlineData(HeaderEnum.TRIPS_PER_DAY, FormatEnum.NUMBER)]
    [InlineData(HeaderEnum.TRIPS_PER_HOUR, FormatEnum.NUMBER)]
    [InlineData(HeaderEnum.TOTAL_TRIPS, FormatEnum.NUMBER)]
    [InlineData(HeaderEnum.VISITS, FormatEnum.NUMBER)]
    [InlineData(HeaderEnum.DAYS, FormatEnum.NUMBER)]
    [InlineData(HeaderEnum.NUMBER_OF_DAYS, FormatEnum.NUMBER)]
    [InlineData(HeaderEnum.DAYS_PER_VISIT, FormatEnum.NUMBER)]
    [InlineData(HeaderEnum.DAYS_SINCE_VISIT, FormatEnum.NUMBER)]
    [InlineData(HeaderEnum.ODOMETER_START, FormatEnum.NUMBER)]
    [InlineData(HeaderEnum.ODOMETER_END, FormatEnum.NUMBER)]
    [InlineData(HeaderEnum.WEEKDAY, FormatEnum.WEEKDAY)]
    [InlineData(HeaderEnum.ADDRESS, FormatEnum.TEXT)]
    [InlineData(HeaderEnum.NAME, FormatEnum.TEXT)]
    [InlineData(HeaderEnum.PLACE, FormatEnum.TEXT)]
    [InlineData(HeaderEnum.REGION, FormatEnum.TEXT)]
    [InlineData(HeaderEnum.SERVICE, FormatEnum.TEXT)]
    [InlineData(HeaderEnum.TYPE, FormatEnum.TEXT)]
    [InlineData(HeaderEnum.CATEGORY, FormatEnum.TEXT)]
    [InlineData(HeaderEnum.DESCRIPTION, FormatEnum.TEXT)]
    [InlineData(HeaderEnum.NOTE, FormatEnum.TEXT)]
    public void ApplyFormatsByHeaderEnum_ShouldApplyCorrectFormat(HeaderEnum headerEnum, FormatEnum expectedFormat)
    {
        // Arrange
        var header = new SheetCellModel { Name = "TestHeader" };

        // Act
        GigSheetConfigurationHelpers.ApplyFormatsByHeaderEnum(header, headerEnum);

        // Assert
        Assert.Equal(expectedFormat, header.Format);
    }

    [Fact]
    public void ApplyFormatsByHeaderEnum_WithUnknownHeader_ShouldUseDefaultFormat()
    {
        // Arrange
        var header = new SheetCellModel { Name = "TestHeader" };
        var unknownHeader = HeaderEnum.KEY; // This doesn't have specific formatting

        // Act
        GigSheetConfigurationHelpers.ApplyFormatsByHeaderEnum(header, unknownHeader);

        // Assert
        Assert.Equal(FormatEnum.DEFAULT, header.Format);
    }

    [Fact]
    public void ApplyCommonFormats_WithValidHeaderEnum_ShouldUseTypeSafeFormatting()
    {
        // Arrange
        var header = new SheetCellModel { Name = "TestHeader" };
        var headerName = SheetsConfig.HeaderNames.Date; // This maps to HeaderEnum.DATE

        // Act
        GigSheetConfigurationHelpers.ApplyCommonFormats(header, headerName);

        // Assert
        Assert.Equal(FormatEnum.DATE, header.Format);
    }

    [Fact]
    public void ApplyCommonFormats_WithInvalidHeaderEnum_ShouldLeaveUnchanged()
    {
        // Arrange
        var header = new SheetCellModel { Name = "TestHeader", Format = null };
        var headerName = "Custom Date Field"; // This doesn't map to a HeaderEnum

        // Act
        GigSheetConfigurationHelpers.ApplyCommonFormats(header, headerName);

        // Assert
        // Should leave header formatting unchanged since header not found in enum
        Assert.Null(header.Format);
    }

    [Theory]
    [InlineData("Custom Date Field")]
    [InlineData("Total Amount Field")]
    [InlineData("Pay Rate")]
    [InlineData("Tip Amount")]
    [InlineData("Distance Traveled")]
    [InlineData("Number of Items")]
    [InlineData("Weekday Name")]
    [InlineData("Start Time")]
    [InlineData("Duration Total")]
    [InlineData("Unknown Field")]
    public void ApplyCommonFormats_WithUnknownStrings_ShouldLeaveUnchanged(string headerName)
    {
        // Arrange
        var header = new SheetCellModel { Name = "TestHeader", Format = null };

        // Act
        GigSheetConfigurationHelpers.ApplyCommonFormats(header, headerName);

        // Assert
        // Should leave header formatting unchanged for unknown headers
        Assert.Null(header.Format);
    }

    [Fact]
    public void ApplyCommonFormats_WithUnknownHeader_ShouldLeaveUnchanged()
    {
        // Arrange
        var header = new SheetCellModel { Name = "TestHeader", Format = null };
        var headerName = "Active Time"; // Unknown header should be left unchanged

        // Act
        GigSheetConfigurationHelpers.ApplyCommonFormats(header, headerName);

        // Assert
        // Should leave header formatting unchanged
        Assert.Null(header.Format);
    }

    [Fact]
    public void ApplyCommonFormats_WithTimeActiveException_ShouldLeaveUnchanged()
    {
        // Arrange
        var header = new SheetCellModel { Name = "TestHeader", Format = null };
        var headerName = "Active Time"; // Unknown header

        // Act
        GigSheetConfigurationHelpers.ApplyCommonFormats(header, headerName);

        // Assert
        // Should leave header unchanged since it doesn't match a known HeaderEnum
        Assert.Null(header.Format);
    }
}