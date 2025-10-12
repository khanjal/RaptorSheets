using RaptorSheets.Gig.Helpers;

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

    #region New Formula Tests

    [Fact]
    public void BuildArrayFormulaAmountPerDay_ShouldGenerateDailyAverage()
    {
        // Arrange
        var totalRange = "$B:$B";
        var daysRange = "$C:$C";

        // Act
        var result = GigFormulaBuilder.BuildArrayFormulaAmountPerDay(TestKeyRange, TestHeader, totalRange, daysRange);

        // Assert
        Assert.Contains("=ARRAYFORMULA(", result);
        Assert.Contains(totalRange, result);
        Assert.Contains(daysRange, result);
        Assert.Contains("/IF(", result);
        Assert.Contains("=0,1,", result);
    }

    [Fact]
    public void BuildArrayFormulaShiftKey_ShouldGenerateKeyWithFallback()
    {
        // Arrange
        var dateRange = "$A:$A";
        var serviceRange = "$B:$B";
        var numberRange = "$C:$C";

        // Act
        var result = GigFormulaBuilder.BuildArrayFormulaShiftKey(TestKeyRange, TestHeader, dateRange, serviceRange, numberRange);

        // Assert
        Assert.Contains("=ARRAYFORMULA(", result);
        Assert.Contains("IF(ISBLANK(", result);
        Assert.Contains("\"-0-\"", result); // Default number fallback
        Assert.Contains("\"-\"", result); // Delimiter
        Assert.Contains(dateRange, result);
        Assert.Contains(serviceRange, result);
        Assert.Contains(numberRange, result);
    }

    [Fact]
    public void BuildArrayFormulaTripKey_ShouldGenerateKeyWithExcludeLogic()
    {
        // Arrange
        var dateRange = "$A:$A";
        var serviceRange = "$B:$B";
        var numberRange = "$C:$C";
        var excludeRange = "$D:$D";

        // Act
        var result = GigFormulaBuilder.BuildArrayFormulaTripKey(TestKeyRange, TestHeader, dateRange, serviceRange, numberRange, excludeRange);

        // Assert
        Assert.Contains("=ARRAYFORMULA(", result);
        Assert.Contains("IF(ISBLANK(", result); // Should contain conditional logic
        Assert.Contains("\"-X-\"", result); // Exclude marker
        Assert.Contains("\"-0-\"", result); // Default number fallback
        Assert.Contains(dateRange, result);
        Assert.Contains(serviceRange, result);
        Assert.Contains(numberRange, result);
        Assert.Contains(excludeRange, result);
    }

    [Fact]
    public void BuildArrayFormulaTotalTimeActive_ShouldGenerateTimeWithFallback()
    {
        // Arrange
        var activeTimeRange = "$B:$B";
        var tripKeyRange = "$C:$C";
        var shiftKeyRange = "$D:$D";
        var tripDurationRange = "$E:$E";

        // Act
        var result = GigFormulaBuilder.BuildArrayFormulaTotalTimeActive(TestKeyRange, TestHeader, activeTimeRange, tripKeyRange, shiftKeyRange, tripDurationRange);

        // Assert
        Assert.Contains("=ARRAYFORMULA(", result);
        Assert.Contains("IF(ISBLANK(", result);
        Assert.Contains("SUMIF(", result);
        Assert.Contains(activeTimeRange, result);
        Assert.Contains(tripKeyRange, result);
        Assert.Contains(shiftKeyRange, result);
        Assert.Contains(tripDurationRange, result);
    }

    [Fact]
    public void BuildArrayFormulaTotalTimeWithOmit_ShouldGenerateOmitLogic()
    {
        // Arrange
        var omitRange = "$B:$B";
        var totalTimeRange = "$C:$C";
        var totalActiveRange = "$D:$D";

        // Act
        var result = GigFormulaBuilder.BuildArrayFormulaTotalTimeWithOmit(TestKeyRange, TestHeader, omitRange, totalTimeRange, totalActiveRange);

        // Assert
        Assert.Contains("=ARRAYFORMULA(", result);
        Assert.Contains("IF(ISBLANK(", result); // Should contain conditional logic
        Assert.Contains("=false", result); // Omit logic (resolved, not placeholder)
        Assert.Contains(",0)", result); // Omit returns 0
        Assert.Contains(omitRange, result);
        Assert.Contains(totalTimeRange, result);
        Assert.Contains(totalActiveRange, result);
    }

    [Fact]
    public void BuildArrayFormulaShiftTotalWithTripSum_ShouldGenerateShiftPlusTripTotal()
    {
        // Arrange
        var localRange = "$B:$B";
        var tripKeyRange = "$C:$C";
        var shiftKeyRange = "$D:$D";
        var tripSumRange = "$E:$E";

        // Act
        var result = GigFormulaBuilder.BuildArrayFormulaShiftTotalWithTripSum(TestKeyRange, TestHeader, localRange, tripKeyRange, shiftKeyRange, tripSumRange);

        // Assert
        Assert.Contains("=ARRAYFORMULA(", result);
        Assert.Contains($"{localRange} + SUMIF(", result);
        Assert.Contains(tripKeyRange, result);
        Assert.Contains(shiftKeyRange, result);
        Assert.Contains(tripSumRange, result);
    }

    [Fact]
    public void BuildArrayFormulaShiftTotalTrips_ShouldGenerateTripsCount()
    {
        // Arrange
        var localTripsRange = "$B:$B";
        var tripKeyRange = "$C:$C";
        var shiftKeyRange = "$D:$D";

        // Act
        var result = GigFormulaBuilder.BuildArrayFormulaShiftTotalTrips(TestKeyRange, TestHeader, localTripsRange, tripKeyRange, shiftKeyRange);

        // Assert
        Assert.Contains("=ARRAYFORMULA(", result);
        Assert.Contains($"{localTripsRange} + COUNTIF(", result);
        Assert.Contains(tripKeyRange, result);
        Assert.Contains(shiftKeyRange, result);
    }

    [Fact]
    public void BuildArrayFormulaRollingAverage_ShouldGenerateComplexAverage()
    {
        // Arrange
        var totalRange = "$B:$B";

        // Act
        var result = GigFormulaBuilder.BuildArrayFormulaRollingAverage(TestKeyRange, TestHeader, totalRange);

        // Assert
        Assert.Contains("=ARRAYFORMULA(", result);
        Assert.Contains("DAVERAGE(", result);
        Assert.Contains("transpose(", result);
        Assert.Contains("TRANSPOSE(", result);
        Assert.Contains("sequence(", result);
        Assert.Contains("rows(" + totalRange + ")", result);
        Assert.Contains("ROW(" + totalRange + ")", result);
        Assert.Contains(totalRange, result);
    }

    [Fact]
    public void BuildArrayFormulaDualFieldVisit_WithFirstVisit_ShouldUseTrueSortOrder()
    {
        // Arrange
        var sourceSheet = "Trips";
        var dateColumnLetter = "A";
        var keyColumn1Letter = "S";
        var keyColumn2Letter = "T";
        var dateIndex = "2";
        var isFirst = true;

        // Act
        var result = GigFormulaBuilder.BuildArrayFormulaDualFieldVisit(
            TestKeyRange, TestHeader, sourceSheet, dateColumnLetter, keyColumn1Letter, keyColumn2Letter, dateIndex, isFirst);

        // Assert
        Assert.Contains("=ARRAYFORMULA(", result);
        Assert.Contains("IFERROR(VLOOKUP(", result);
        Assert.Contains($"SORT(QUERY({sourceSheet}!A:{keyColumn1Letter}", result);
        Assert.Contains($"\"SELECT {keyColumn1Letter}, A\"", result);
        Assert.Contains("1,True", result); // First visit: sort by column 1 (key) ascending
        Assert.Contains(",2,0)", result); // VLOOKUP returns 2nd column
        Assert.Contains($"SORT(QUERY({sourceSheet}!A:{keyColumn2Letter}", result);
        Assert.Contains($"\"SELECT {keyColumn2Letter}, A\"", result);
        Assert.Contains(TestKeyRange, result);
        Assert.Contains(TestHeader, result);
    }

    [Fact]
    public void BuildArrayFormulaDualFieldVisit_WithLastVisit_ShouldUseFalseSortOrder()
    {
        // Arrange
        var sourceSheet = "Trips";
        var dateColumnLetter = "A";
        var keyColumn1Letter = "S";
        var keyColumn2Letter = "T";
        var dateIndex = "2";
        var isFirst = false;

        // Act
        var result = GigFormulaBuilder.BuildArrayFormulaDualFieldVisit(
            TestKeyRange, TestHeader, sourceSheet, dateColumnLetter, keyColumn1Letter, keyColumn2Letter, dateIndex, isFirst);

        // Assert
        Assert.Contains("=ARRAYFORMULA(", result);
        Assert.Contains("2,False", result); // Last visit: sort by column 2 (date) descending
        Assert.Contains($"SORT(QUERY({sourceSheet}!A:{keyColumn1Letter}", result);
        Assert.Contains($"SORT(QUERY({sourceSheet}!A:{keyColumn2Letter}", result);
    }

    [Fact]
    public void BuildArrayFormulaDualFieldVisit_ShouldHaveNestedIferrorStructure()
    {
        // Arrange
        var sourceSheet = "Trips";
        var dateColumnLetter = "A";
        var keyColumn1Letter = "S";
        var keyColumn2Letter = "T";
        var dateIndex = "2";
        var isFirst = true;

        // Act
        var result = GigFormulaBuilder.BuildArrayFormulaDualFieldVisit(
            TestKeyRange, TestHeader, sourceSheet, dateColumnLetter, keyColumn1Letter, keyColumn2Letter, dateIndex, isFirst);

        // Assert
        // Should have nested IFERROR for fallback from keyColumn1 to keyColumn2
        var iferrorCount = result.Split("IFERROR").Length - 1;
        Assert.True(iferrorCount >= 2, "Should have at least 2 IFERROR calls (one for each field lookup)");
        
        // Should contain both column lookups
        Assert.Contains(keyColumn1Letter, result);
        Assert.Contains(keyColumn2Letter, result);
        
        // Should have proper VLOOKUP structure
        var vlookupCount = result.Split("VLOOKUP").Length - 1;
        Assert.True(vlookupCount >= 2, "Should have at least 2 VLOOKUP calls");
    }

    [Theory]
    [InlineData("Trips", "A", "B", "C", "2")]
    [InlineData("Shifts", "D", "E", "F", "3")]
    [InlineData("Orders", "G", "H", "I", "1")]
    public void BuildArrayFormulaDualFieldVisit_WithVariousParameters_ShouldGenerateValidFormula(
        string sourceSheet, string dateColumnLetter, string keyColumn1Letter, string keyColumn2Letter, string dateIndex)
    {
        // Act
        var result = GigFormulaBuilder.BuildArrayFormulaDualFieldVisit(
            TestKeyRange, TestHeader, sourceSheet, dateColumnLetter, keyColumn1Letter, keyColumn2Letter, dateIndex, true);

        // Assert
        Assert.Contains("=ARRAYFORMULA(", result);
        Assert.Contains(sourceSheet, result);
        // Note: dateIndex is not used in formula anymore, sortColumn is determined by isFirst
        Assert.Contains(keyColumn1Letter, result);
        Assert.Contains(keyColumn2Letter, result);
        Assert.Contains("1,True", result); // First visit always uses column 1, True
        Assert.Contains("QUERY", result);
        Assert.Contains("SORT", result);
        Assert.Contains("VLOOKUP", result);
    }

    [Fact]
    public void CommonBuildDualFieldVisitLookup_WithFirstVisit_ShouldCallUnderlyingMethod()
    {
        // Arrange
        var sourceSheet = "Trips";
        var dateColumnLetter = "A";
        var keyColumn1Letter = "S";
        var keyColumn2Letter = "T";
        var dateIndex = "2";
        var isFirst = true;

        // Act
        var result = GigFormulaBuilder.Common.BuildDualFieldVisitLookup(
            TestKeyRange, TestHeader, sourceSheet, dateColumnLetter, keyColumn1Letter, keyColumn2Letter, dateIndex, isFirst);

        // Assert
        Assert.Contains("=ARRAYFORMULA(", result);
        Assert.Contains("IFERROR(VLOOKUP(", result);
        Assert.Contains($"SORT(QUERY({sourceSheet}!A:{keyColumn1Letter}", result);
        Assert.Contains($"\"SELECT {keyColumn1Letter}, A\"", result);
        Assert.Contains("1,True", result); // First visit: sort by column 1 ascending
        Assert.Contains(TestKeyRange, result);
        Assert.Contains(TestHeader, result);
    }

    [Fact]
    public void CommonBuildDualFieldVisitLookup_WithLastVisit_ShouldCallUnderlyingMethod()
    {
        // Arrange
        var sourceSheet = "Trips";
        var dateColumnLetter = "A";
        var keyColumn1Letter = "S";
        var keyColumn2Letter = "T";
        var dateIndex = "2";
        var isFirst = false;

        // Act
        var result = GigFormulaBuilder.Common.BuildDualFieldVisitLookup(
            TestKeyRange, TestHeader, sourceSheet, dateColumnLetter, keyColumn1Letter, keyColumn2Letter, dateIndex, isFirst);

        // Assert
        Assert.Contains("=ARRAYFORMULA(", result);
        Assert.Contains("2,False", result); // Last visit: sort by column 2 descending
        Assert.Contains($"SORT(QUERY({sourceSheet}!A:{keyColumn1Letter}", result);
        Assert.Contains($"SORT(QUERY({sourceSheet}!A:{keyColumn2Letter}", result);
    }

    [Fact]
    public void CommonBuildDualFieldVisitLookup_ShouldMatchArrayFormulaBase()
    {
        // Arrange
        var sourceSheet = "Trips";
        var dateColumnLetter = "A";
        var keyColumn1Letter = "S";
        var keyColumn2Letter = "T";
        var dateIndex = "2";
        var isFirst = true;

        // Act
        var result = GigFormulaBuilder.Common.BuildDualFieldVisitLookup(
            TestKeyRange, TestHeader, sourceSheet, dateColumnLetter, keyColumn1Letter, keyColumn2Letter, dateIndex, isFirst);

        // Assert
        // Should follow ARRAYFORMULA pattern with IFS
        Assert.Contains("IFS(", result);
        Assert.Contains($"ROW({TestKeyRange})=1", result);
        Assert.Contains($"\"{TestHeader}\"", result);
        Assert.Contains($"ISBLANK({TestKeyRange})", result);
    }

    [Theory]
    [InlineData(true, "1", "True")]
    [InlineData(false, "2", "False")]
    public void BuildArrayFormulaDualFieldVisit_ShouldUseCorrectSortColumnParameter(bool isFirst, string expectedSortColumn, string expectedSortOrder)
    {
        // Arrange
        var sourceSheet = "Trips";
        var dateColumnLetter = "A";
        var keyColumn1Letter = "S";
        var keyColumn2Letter = "T";
        var dateIndex = "2";

        // Act
        var result = GigFormulaBuilder.BuildArrayFormulaDualFieldVisit(
            TestKeyRange, TestHeader, sourceSheet, dateColumnLetter, keyColumn1Letter, keyColumn2Letter, dateIndex, isFirst);

        // Assert
        // Check for sort column (1 for first, 2 for last) and sort order
        Assert.Contains($"{expectedSortColumn},{expectedSortOrder}", result);
    }

    #endregion
}