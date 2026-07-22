using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Helpers;

namespace RaptorSheets.Gig.Tests.Unit.Helpers;

public class GigSheetConfigurationHelpersTests
{
    [Theory]
    [InlineData(Header.DATE, Format.DATE)]
    [InlineData(Header.DATE_BEGIN, Format.DATE)]
    [InlineData(Header.DATE_END, Format.DATE)]
    [InlineData(Header.VISIT_FIRST, Format.DATE)]
    [InlineData(Header.VISIT_LAST, Format.DATE)]
    [InlineData(Header.TIME_START, Format.TIME)]
    [InlineData(Header.TIME_END, Format.TIME)]
    [InlineData(Header.DURATION, Format.DURATION)]
    [InlineData(Header.TIME_TOTAL, Format.DURATION)]
    [InlineData(Header.TIME_ACTIVE, Format.DURATION)]
    [InlineData(Header.TOTAL_TIME, Format.DURATION)]
    [InlineData(Header.TOTAL_TIME_ACTIVE, Format.DURATION)]
    [InlineData(Header.AMOUNT, Format.ACCOUNTING)]
    [InlineData(Header.PAY, Format.ACCOUNTING)]
    [InlineData(Header.TIP, Format.ACCOUNTING)]
    [InlineData(Header.TIPS, Format.ACCOUNTING)]
    [InlineData(Header.BONUS, Format.ACCOUNTING)]
    [InlineData(Header.CASH, Format.ACCOUNTING)]
    [InlineData(Header.TOTAL, Format.ACCOUNTING)]
    [InlineData(Header.TOTAL_BONUS, Format.ACCOUNTING)]
    [InlineData(Header.TOTAL_CASH, Format.ACCOUNTING)]
    [InlineData(Header.TOTAL_PAY, Format.ACCOUNTING)]
    [InlineData(Header.TOTAL_TIPS, Format.ACCOUNTING)]
    [InlineData(Header.TOTAL_GRAND, Format.ACCOUNTING)]
    [InlineData(Header.AVERAGE, Format.ACCOUNTING)]
    [InlineData(Header.DISTANCE, Format.DISTANCE)]
    [InlineData(Header.TOTAL_DISTANCE, Format.DISTANCE)]
    [InlineData(Header.NUMBER, Format.NUMBER)]
    [InlineData(Header.ORDER_NUMBER, Format.NUMBER)]
    [InlineData(Header.TRIPS, Format.NUMBER)]
    [InlineData(Header.TRIPS_PER_DAY, Format.NUMBER)]
    [InlineData(Header.TRIPS_PER_HOUR, Format.NUMBER)]
    [InlineData(Header.TOTAL_TRIPS, Format.NUMBER)]
    [InlineData(Header.VISITS, Format.NUMBER)]
    [InlineData(Header.DAYS, Format.NUMBER)]
    [InlineData(Header.NUMBER_OF_DAYS, Format.NUMBER)]
    [InlineData(Header.DAYS_PER_VISIT, Format.NUMBER)]
    [InlineData(Header.DAYS_SINCE_VISIT, Format.NUMBER)]
    [InlineData(Header.ODOMETER_START, Format.NUMBER)]
    [InlineData(Header.ODOMETER_END, Format.NUMBER)]
    [InlineData(Header.WEEKDAY, Format.WEEKDAY)]
    [InlineData(Header.ADDRESS, Format.TEXT)]
    [InlineData(Header.NAME, Format.TEXT)]
    [InlineData(Header.PLACE, Format.TEXT)]
    [InlineData(Header.REGION, Format.TEXT)]
    [InlineData(Header.SERVICE, Format.TEXT)]
    [InlineData(Header.TYPE, Format.TEXT)]
    [InlineData(Header.CATEGORY, Format.TEXT)]
    [InlineData(Header.DESCRIPTION, Format.TEXT)]
    [InlineData(Header.NOTE, Format.TEXT)]
    public void ApplyFormatsByHeaderEnum_ShouldApplyCorrectFormat(Header headerEnum, Format expectedFormat)
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
        var unknownHeader = Header.KEY; // This doesn't have specific formatting

        // Act
        GigSheetConfigurationHelpers.ApplyFormatsByHeaderEnum(header, unknownHeader);

        // Assert
        Assert.Equal(Format.DEFAULT, header.Format);
    }

    [Fact]
    public void ApplyCommonFormats_WithValidHeaderEnum_ShouldUseTypeSafeFormatting()
    {
        // Arrange
        var header = new SheetCellModel { Name = "TestHeader" };
        var headerName = SheetsConfig.HeaderNames.Date; // This maps to Header.DATE

        // Act
        GigSheetConfigurationHelpers.ApplyCommonFormats(header, headerName);

        // Assert
        Assert.Equal(Format.DATE, header.Format);
    }

    [Fact]
    public void ApplyCommonFormats_WithInvalidHeaderEnum_ShouldLeaveUnchanged()
    {
        // Arrange
        var header = new SheetCellModel { Name = "TestHeader", Format = null };
        var headerName = "Custom Date Field"; // This doesn't map to a Header

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
        // Should leave header unchanged since it doesn't match a known Header
        Assert.Null(header.Format);
    }
}