using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Helpers;
using Xunit;

namespace RaptorSheets.Gig.Tests.Unit.Helpers;

public class GigFormulaBuilderTests
{
    private const string TestKeyRange = "$A:$A";
    private const string TestHeader = "Test Header";
    private const string TestLookupRange = "$B:$B";
    private const string TestSumRange = "$C:$C";
    private const string TestPayRange = "$B:$B";
    private const string TestTipsRange = "$C:$C";
    private const string TestBonusRange = "$D:$D";
    private const string TestTotalRange = "$E:$E";
    private const string TestTripsRange = "$F:$F";
    private const string TestDistanceRange = "$G:$G";

    [Fact]
    public void BuildArrayFormulaTotal_ShouldGenerateGigTotalFormula()
    {
        // Act
        var result = GigFormulaBuilder.BuildArrayFormulaTotal(TestKeyRange, TestHeader, TestPayRange, TestTipsRange, TestBonusRange);

        // Assert
        Assert.Contains("=ARRAYFORMULA(", result);
        Assert.Contains(TestPayRange, result);
        Assert.Contains(TestTipsRange, result);
        Assert.Contains(TestBonusRange, result);
        Assert.Contains("+", result); // Should have addition operators
        Assert.Contains(TestKeyRange, result);
        Assert.Contains(TestHeader, result);
    }

    [Fact]
    public void BuildArrayFormulaAmountPerTrip_ShouldGeneratePerTripCalculation()
    {
        // Act
        var result = GigFormulaBuilder.BuildArrayFormulaAmountPerTrip(TestKeyRange, TestHeader, TestTotalRange, TestTripsRange);

        // Assert
        Assert.Contains("=ARRAYFORMULA(", result);
        Assert.Contains(TestTotalRange, result);
        Assert.Contains(TestTripsRange, result);
        Assert.Contains("/IF(", result);
        Assert.Contains("=0,1,", result); // Zero protection
        Assert.Contains(TestKeyRange, result);
        Assert.Contains(TestHeader, result);
    }

    [Fact]
    public void BuildArrayFormulaAmountPerDistance_ShouldGeneratePerDistanceCalculation()
    {
        // Act
        var result = GigFormulaBuilder.BuildArrayFormulaAmountPerDistance(TestKeyRange, TestHeader, TestTotalRange, TestDistanceRange);

        // Assert
        Assert.Contains("=ARRAYFORMULA(", result);
        Assert.Contains(TestTotalRange, result);
        Assert.Contains(TestDistanceRange, result);
        Assert.Contains("/IF(", result);
        Assert.Contains("=0,1,", result); // Zero protection
    }

    [Fact]
    public void BuildArrayFormulaAmountPerTime_ShouldGenerateHourlyRateCalculation()
    {
        // Arrange
        var timeRange = "$H:$H";

        // Act
        var result = GigFormulaBuilder.BuildArrayFormulaAmountPerTime(TestKeyRange, TestHeader, TestTotalRange, timeRange);

        // Assert
        Assert.Contains("=ARRAYFORMULA(", result);
        Assert.Contains(TestTotalRange, result);
        Assert.Contains(timeRange, result);
        Assert.Contains("*24", result); // Time conversion to hours
        Assert.Contains("/IF(", result);
    }

    [Fact]
    public void BuildArrayFormulaWeekNumber_ShouldGenerateWeekWithYear()
    {
        // Arrange
        var dateRange = "$B:$B";

        // Act
        var result = GigFormulaBuilder.BuildArrayFormulaWeekNumber(TestKeyRange, TestHeader, dateRange);

        // Assert
        Assert.Contains("=ARRAYFORMULA(", result);
        Assert.Contains("WEEKNUM(", result);
        Assert.Contains("YEAR(", result);
        Assert.Contains("&\"-\"&", result); // Week-Year separator
        Assert.Contains(dateRange, result);
    }

    [Fact]
    public void BuildArrayFormulaMonthNumber_ShouldGenerateMonthWithYear()
    {
        // Arrange
        var dateRange = "$B:$B";

        // Act
        var result = GigFormulaBuilder.BuildArrayFormulaMonthNumber(TestKeyRange, TestHeader, dateRange);

        // Assert
        Assert.Contains("=ARRAYFORMULA(", result);
        Assert.Contains("MONTH(", result);
        Assert.Contains("YEAR(", result);
        Assert.Contains("&\"-\"&", result); // Month-Year separator
        Assert.Contains(dateRange, result);
    }

    [Fact]
    public void BuildArrayFormulaWeekBegin_ShouldGenerateWeekStartCalculation()
    {
        // Arrange
        var yearRange = "$C:$C";
        var weekRange = "$D:$D";

        // Act
        var result = GigFormulaBuilder.BuildArrayFormulaWeekBegin(TestKeyRange, TestHeader, yearRange, weekRange);

        // Assert
        Assert.Contains("=ARRAYFORMULA(", result);
        Assert.Contains("DATE(", result);
        Assert.Contains("WEEKDAY(", result);
        Assert.Contains(yearRange, result);
        Assert.Contains(weekRange, result);
        Assert.Contains("*7", result); // Week calculation
    }

    [Fact]
    public void BuildArrayFormulaWeekEnd_ShouldGenerateWeekEndCalculation()
    {
        // Arrange
        var yearRange = "$C:$C";
        var weekRange = "$D:$D";

        // Act
        var result = GigFormulaBuilder.BuildArrayFormulaWeekEnd(TestKeyRange, TestHeader, yearRange, weekRange);

        // Assert
        Assert.Contains("=ARRAYFORMULA(", result);
        Assert.Contains("DATE(", result);
        Assert.Contains(",1,7)", result); // Week end calculation
        Assert.Contains(yearRange, result);
        Assert.Contains(weekRange, result);
    }

    [Fact]
    public void BuildArrayFormulaCurrentAmount_ShouldGenerateWeekdayLookup()
    {
        // Arrange
        var dayRange = "$B:$B";
        var dailySheet = "Daily";
        var dateColumn = "$A:$A";
        var totalColumn = "$E:$E";
        var totalIndex = "5";

        // Act
        var result = GigFormulaBuilder.BuildArrayFormulaCurrentAmount(TestKeyRange, TestHeader, dayRange, dailySheet, dateColumn, totalColumn, totalIndex);

        // Assert
        Assert.Contains("=ARRAYFORMULA(", result);
        Assert.Contains("IFERROR(VLOOKUP(", result);
        Assert.Contains("TODAY()-WEEKDAY(TODAY(),2)", result);
        Assert.Contains(dayRange, result);
        Assert.Contains(dailySheet, result);
        Assert.Contains(dateColumn, result);
        Assert.Contains(totalColumn, result);
        Assert.Contains(totalIndex, result);
    }

    [Fact]
    public void BuildArrayFormulaPreviousAmount_ShouldGeneratePreviousWeekLookup()
    {
        // Arrange
        var dayRange = "$B:$B";
        var dailySheet = "Daily";
        var dateColumn = "$A:$A";
        var totalColumn = "$E:$E";
        var totalIndex = "5";

        // Act
        var result = GigFormulaBuilder.BuildArrayFormulaPreviousAmount(TestKeyRange, TestHeader, dayRange, dailySheet, dateColumn, totalColumn, totalIndex);

        // Assert
        Assert.Contains("=ARRAYFORMULA(", result);
        Assert.Contains("TODAY()-WEEKDAY(TODAY(),2)", result);
        Assert.Contains("-7", result); // Previous week offset
        Assert.Contains(dayRange, result);
        Assert.Contains(dailySheet, result);
    }

    [Fact]
    public void BuildArrayFormulaPreviousDayAverage_ShouldGenerateAverageCalculation()
    {
        // Arrange
        var previousRange = "$C:$C";
        var daysRange = "$D:$D";

        // Act
        var result = GigFormulaBuilder.BuildArrayFormulaPreviousDayAverage(TestKeyRange, TestHeader, TestTotalRange, previousRange, daysRange);

        // Assert
        Assert.Contains("=ARRAYFORMULA(", result);
        Assert.Contains("(" + TestTotalRange + "-" + previousRange + ")", result);
        Assert.Contains("/IF(" + daysRange + "=0,1,", result);
        Assert.Contains("-IF(" + previousRange + "=0,0,-1)", result);
    }

    [Fact]
    public void BuildArrayFormulaMultipleFieldVisit_WithFirstVisit_ShouldUseMinFunction()
    {
        // Arrange
        var sourceSheet = "Trips";
        var dateColumn = "$A:$A";
        var keyColumn1 = "$B:$B";
        var keyColumn2 = "$C:$C";
        var isFirst = true;

        // Act
        var result = GigFormulaBuilder.BuildArrayFormulaMultipleFieldVisit(TestKeyRange, TestHeader, sourceSheet, dateColumn, keyColumn1, keyColumn2, isFirst);

        // Assert
        Assert.Contains("=ARRAYFORMULA(", result);
        Assert.Contains("IFERROR(MIN(IF(", result);
        Assert.Contains(sourceSheet, result);
        Assert.Contains(dateColumn, result);
        Assert.Contains(keyColumn1, result);
        Assert.Contains(keyColumn2, result);
    }

    [Fact]
    public void BuildArrayFormulaMultipleFieldVisit_WithLastVisit_ShouldUseMaxFunction()
    {
        // Arrange
        var sourceSheet = "Trips";
        var dateColumn = "$A:$A";
        var keyColumn1 = "$B:$B";
        var keyColumn2 = "$C:$C";
        var isFirst = false;

        // Act
        var result = GigFormulaBuilder.BuildArrayFormulaMultipleFieldVisit(TestKeyRange, TestHeader, sourceSheet, dateColumn, keyColumn1, keyColumn2, isFirst);

        // Assert
        Assert.Contains("=ARRAYFORMULA(", result);
        Assert.Contains("IFERROR(MAX(IF(", result);
        Assert.Contains(sourceSheet, result);
    }

    [Fact]
    public void CommonBuildSumAggregation_ShouldUseGenericBuilder()
    {
        // Act
        var result = GigFormulaBuilder.Common.BuildSumAggregation(TestKeyRange, TestHeader, TestLookupRange, TestSumRange);

        // Assert
        Assert.Contains("=ARRAYFORMULA(", result);
        Assert.Contains("SUMIF", result);
        Assert.Contains(TestKeyRange, result);
        Assert.Contains(TestHeader, result);
    }

    [Fact]
    public void CommonBuildCountAggregation_ShouldUseGenericBuilder()
    {
        // Act
        var result = GigFormulaBuilder.Common.BuildCountAggregation(TestKeyRange, TestHeader, TestLookupRange);

        // Assert
        Assert.Contains("=ARRAYFORMULA(", result);
        Assert.Contains("COUNTIF", result);
        Assert.Contains(TestKeyRange, result);
        Assert.Contains(TestHeader, result);
    }

    [Fact]
    public void CommonBuildVisitDateLookup_ShouldUseGenericSortedLookup()
    {
        // Arrange
        var sourceSheet = "Trips";
        var dateColumn = "$A:$A";
        var keyColumn = "$B:$B";
        var isFirst = true;

        // Act
        var result = GigFormulaBuilder.Common.BuildVisitDateLookup(TestKeyRange, TestHeader, sourceSheet, dateColumn, keyColumn, isFirst);

        // Assert
        Assert.Contains("=ARRAYFORMULA(", result);
        Assert.Contains("IFERROR(VLOOKUP(", result);
        Assert.Contains("SORT(QUERY(", result);
        Assert.Contains(sourceSheet, result);
    }

    [Theory]
    [InlineData("$A:$A", "Pay", "$B:$B", "$C:$C", "$D:$D")]
    [InlineData("Range1", "Total Income", "Range2", "Range3", "Range4")]
    public void BuildArrayFormulaTotal_WithVariousInputs_ShouldGenerateValidFormula(string keyRange, string header, string payRange, string tipsRange, string bonusRange)
    {
        // Act
        var result = GigFormulaBuilder.BuildArrayFormulaTotal(keyRange, header, payRange, tipsRange, bonusRange);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("=ARRAYFORMULA(", result);
        Assert.Contains(payRange, result);
        Assert.Contains(tipsRange, result);
        Assert.Contains(bonusRange, result);
        Assert.Contains("+", result);
    }
}