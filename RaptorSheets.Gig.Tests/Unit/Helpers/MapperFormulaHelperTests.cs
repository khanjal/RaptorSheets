using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Helpers;
using RaptorSheets.Gig.Mappers;

namespace RaptorSheets.Gig.Tests.Unit.Helpers;

public class MapperFormulaHelperTests
{
    #region ConfigureCommonAggregationHeaders Tests

    [Theory]
    [InlineData("Pay", "SUMIF(", FormatEnum.ACCOUNTING)]
    [InlineData("Tips", "SUMIF(", FormatEnum.ACCOUNTING)]
    [InlineData("Bonus", "SUMIF(", FormatEnum.ACCOUNTING)]
    [InlineData("Cash", "SUMIF(", FormatEnum.ACCOUNTING)]
    [InlineData("Dist", "SUMIF(", FormatEnum.DISTANCE)]
    [InlineData("Time", "SUMIF(", FormatEnum.DURATION)]
    [InlineData("Days", "COUNTIF(", FormatEnum.NUMBER)]
    public void ConfigureCommonAggregationHeaders_WithStandardHeaders_ShouldSetFormulaAndFormat(
        string headerName, string expectedFormula, FormatEnum expectedFormat)
    {
        // Arrange
        var sheet = CreateTestSheet(headerName);
        var sourceSheet = CreateSourceSheet();
        var keyRange = "$A:$A";
        var sourceKeyRange = "$B:$B";

        // Act
        MapperFormulaHelper.ConfigureCommonAggregationHeaders(sheet, keyRange, sourceSheet, sourceKeyRange);

        // Assert
        var header = sheet.Headers.First(h => h.Name == headerName);
        Assert.NotEmpty(header.Formula);
        Assert.StartsWith("=ARRAYFORMULA(", header.Formula);
        Assert.Contains(expectedFormula, header.Formula);
        Assert.Equal(expectedFormat, header.Format);
    }

    [Fact]
    public void ConfigureCommonAggregationHeaders_WithTotalHeader_ShouldAddPayTipsBonus()
    {
        // Arrange
        var sheet = CreateTestSheetWithMultipleHeaders(
            HeaderEnum.PAY.GetDescription(),
            HeaderEnum.TIPS.GetDescription(),
            HeaderEnum.BONUS.GetDescription(),
            HeaderEnum.TOTAL.GetDescription());
        sheet.Headers.UpdateColumns();
        var sourceSheet = CreateSourceSheet();

        // Act
        MapperFormulaHelper.ConfigureCommonAggregationHeaders(sheet, "$A:$A", sourceSheet, "$B:$B");

        // Assert
        var totalHeader = sheet.Headers.First(h => h.Name == HeaderEnum.TOTAL.GetDescription());
        Assert.Contains("+", totalHeader.Formula);
        Assert.Equal(FormatEnum.ACCOUNTING, totalHeader.Format);
    }

    [Theory]
    [InlineData(true, "SUMIF(")]
    [InlineData(false, "SUMIF(")]
    public void ConfigureCommonAggregationHeaders_WithTripsHeader_ShouldUseCorrectFormula(
        bool useShiftTotals, string expectedFormula)
    {
        // Arrange
        var sheet = CreateTestSheet(HeaderEnum.TRIPS.GetDescription());
        var sourceSheet = CreateSourceSheet();

        // Act
        MapperFormulaHelper.ConfigureCommonAggregationHeaders(sheet, "$A:$A", sourceSheet, "$B:$B", useShiftTotals);

        // Assert
        var header = sheet.Headers.First();
        Assert.Contains(expectedFormula, header.Formula);
        Assert.Equal(FormatEnum.NUMBER, header.Format);
    }

    #endregion

    #region ConfigureCommonRatioHeaders Tests

    [Theory]
    [InlineData("Amt/Trip", "Trips")]
    [InlineData("Amt/Dist", "Dist")]
    [InlineData("Amt/Hour", "Time")]
    [InlineData("Amt/Day", "Days")]
    public void ConfigureCommonRatioHeaders_WithRatioHeaders_ShouldSetZeroSafeDivision(
        string ratioHeader, string denominatorHeader)
    {
        // Arrange
        var sheet = CreateTestSheetWithMultipleHeaders("Total", denominatorHeader, ratioHeader);
        sheet.Headers.UpdateColumns();

        // Act
        MapperFormulaHelper.ConfigureCommonRatioHeaders(sheet, "$A:$A");

        // Assert
        var header = sheet.Headers.First(h => h.Name == ratioHeader);
        Assert.NotEmpty(header.Formula);
        Assert.Contains("/IF(", header.Formula); // Zero-safe division
        Assert.Equal(FormatEnum.ACCOUNTING, header.Format);
    }

    #endregion

    #region ConfigureUniqueValueHeader Tests

    [Theory]
    [InlineData("Service", "RANGE_SERVICE")]
    [InlineData("Region", "RANGE_REGION")]
    [InlineData("Place", "RANGE_PLACE")]
    public void ConfigureUniqueValueHeader_WithKnownHeaders_ShouldSetValidation(
        string headerName, string expectedValidation)
    {
        // Arrange
        var header = new SheetCellModel { Name = headerName };

        // Act
        MapperFormulaHelper.ConfigureUniqueValueHeader(header, "Source!$A:$A");

        // Assert
        Assert.Contains("SORT(UNIQUE(", header.Formula);
        Assert.Equal(expectedValidation, header.Validation);
    }

    [Fact]
    public void ConfigureUniqueValueHeader_WithUnknownHeader_ShouldNotSetValidation()
    {
        // Arrange
        var header = new SheetCellModel { Name = "CustomField" };

        // Act
        MapperFormulaHelper.ConfigureUniqueValueHeader(header, "Source!$A:$A");

        // Assert
        Assert.Contains("SORT(UNIQUE(", header.Formula);
        Assert.True(string.IsNullOrEmpty(header.Validation));
    }

    #endregion

    #region ConfigureCombinedUniqueValueHeader Tests

    [Fact]
    public void ConfigureCombinedUniqueValueHeader_WithTwoRanges_ShouldCombineWithSemicolon()
    {
        // Arrange
        var header = new SheetCellModel { Name = "Address" };

        // Act
        MapperFormulaHelper.ConfigureCombinedUniqueValueHeader(header, "A:A", "B:B");

        // Assert
        Assert.Contains("SORT(UNIQUE({", header.Formula);
        Assert.Contains(";", header.Formula); // Array separator
        Assert.Contains("A:A", header.Formula);
        Assert.Contains("B:B", header.Formula);
    }

    #endregion

    #region ConfigureDualCountHeader Tests

    [Fact]
    public void ConfigureDualCountHeader_ShouldAddTwoCounts()
    {
        // Arrange
        var header = new SheetCellModel { Name = "Trips" };

        // Act
        MapperFormulaHelper.ConfigureDualCountHeader(header, "$A:$A", "Range1", "Range2");

        // Assert
        Assert.Contains("COUNTIF(", header.Formula);
        Assert.Contains("+COUNTIF(", header.Formula);
        Assert.Equal(FormatEnum.NUMBER, header.Format);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void MapperFormulaHelper_WithRealMapper_ShouldGenerateValidFormulas()
    {
        // Act
        var placeSheet = PlaceMapper.GetSheet();
        var dailySheet = DailyMapper.GetSheet();

        // Assert - Verify formulas are generated and valid
        var placeFormulas = placeSheet.Headers.Where(h => !string.IsNullOrEmpty(h.Formula)).ToList();
        var dailyFormulas = dailySheet.Headers.Where(h => !string.IsNullOrEmpty(h.Formula)).ToList();

        Assert.NotEmpty(placeFormulas);
        Assert.NotEmpty(dailyFormulas);
        Assert.All(placeFormulas, h => Assert.StartsWith("=", h.Formula));
        Assert.All(dailyFormulas, h => Assert.StartsWith("=", h.Formula));
    }

    #endregion

    #region Helper Methods

    private static SheetModel CreateTestSheet(string headerName)
    {
        return new SheetModel
        {
            Name = "TestSheet",
            Headers = new List<SheetCellModel>
            {
                new SheetCellModel { Name = headerName, Index = 0, Column = "A" }
            }
        };
    }

    private static SheetModel CreateTestSheetWithMultipleHeaders(params string[] headerNames)
    {
        var sheet = new SheetModel
        {
            Name = "TestSheet",
            Headers = new List<SheetCellModel>()
        };

        for (int i = 0; i < headerNames.Length; i++)
        {
            sheet.Headers.Add(new SheetCellModel { Name = headerNames[i], Index = i });
        }

        return sheet;
    }

    private static SheetModel CreateSourceSheet()
    {
        return new SheetModel
        {
            Name = "SourceSheet",
            Headers = new List<SheetCellModel>
            {
                new SheetCellModel { Name = HeaderEnum.PAY.GetDescription(), Index = 0, Column = "A" },
                new SheetCellModel { Name = HeaderEnum.TIPS.GetDescription(), Index = 1, Column = "B" },
                new SheetCellModel { Name = HeaderEnum.BONUS.GetDescription(), Index = 2, Column = "C" },
                new SheetCellModel { Name = HeaderEnum.CASH.GetDescription(), Index = 3, Column = "D" },
                new SheetCellModel { Name = HeaderEnum.DISTANCE.GetDescription(), Index = 4, Column = "E" },
                new SheetCellModel { Name = HeaderEnum.TIME_TOTAL.GetDescription(), Index = 5, Column = "F" },
                new SheetCellModel { Name = HeaderEnum.TOTAL_PAY.GetDescription(), Index = 6, Column = "G" },
                new SheetCellModel { Name = HeaderEnum.TOTAL_TIPS.GetDescription(), Index = 7, Column = "H" },
                new SheetCellModel { Name = HeaderEnum.TOTAL_BONUS.GetDescription(), Index = 8, Column = "I" },
                new SheetCellModel { Name = HeaderEnum.TOTAL_CASH.GetDescription(), Index = 9, Column = "J" },
                new SheetCellModel { Name = HeaderEnum.TOTAL_DISTANCE.GetDescription(), Index = 10, Column = "K" },
                new SheetCellModel { Name = HeaderEnum.TOTAL_TIME.GetDescription(), Index = 11, Column = "L" },
                new SheetCellModel { Name = HeaderEnum.TOTAL_TRIPS.GetDescription(), Index = 12, Column = "M" }
            }
        };
    }

    #endregion
}
