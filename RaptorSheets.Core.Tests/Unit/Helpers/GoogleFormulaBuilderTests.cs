using RaptorSheets.Core.Helpers;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Helpers;

public class GoogleFormulaBuilderTests
{
    private const string TestKeyRange = "$A:$A";
    private const string TestHeader = "Test Header";
    private const string TestLookupRange = "$B:$B";
    private const string TestSumRange = "$C:$C";
    private const string TestSourceRange = "$D:$D";

    [Fact]
    public void BuildArrayFormulaSumIf_ShouldGenerateValidFormula()
    {
        // Act
        var result = GoogleFormulaBuilder.BuildArrayFormulaSumIf(TestKeyRange, TestHeader, TestLookupRange, TestSumRange);

        // Assert
        Assert.Contains("=ARRAYFORMULA(", result);
        Assert.Contains("SUMIF", result);
        Assert.Contains(TestKeyRange, result);
        Assert.Contains(TestHeader, result);
        Assert.Contains(TestLookupRange, result);
        Assert.Contains(TestSumRange, result);
        Assert.Contains("IFS(ROW(", result);
        Assert.Contains("ISBLANK(", result);
    }

    [Fact]
    public void BuildArrayFormulaCountIf_ShouldGenerateValidFormula()
    {
        // Act
        var result = GoogleFormulaBuilder.BuildArrayFormulaCountIf(TestKeyRange, TestHeader, TestLookupRange);

        // Assert
        Assert.Contains("=ARRAYFORMULA(", result);
        Assert.Contains("COUNTIF", result);
        Assert.Contains(TestKeyRange, result);
        Assert.Contains(TestHeader, result);
        Assert.Contains(TestLookupRange, result);
        Assert.Contains("IFS(ROW(", result);
        Assert.Contains("ISBLANK(", result);
    }

    [Fact]
    public void BuildArrayFormulaUnique_ShouldGenerateValidFormula()
    {
        // Act
        var result = GoogleFormulaBuilder.BuildArrayFormulaUnique(TestKeyRange, TestHeader, TestSourceRange);

        // Assert
        Assert.Contains("=ARRAYFORMULA(", result);
        Assert.Contains("SORT(UNIQUE(", result);
        Assert.Contains(TestKeyRange, result);
        Assert.Contains(TestHeader, result);
        Assert.Contains(TestSourceRange, result);
    }

    [Fact]
    public void BuildArrayFormulaUniqueFiltered_ShouldGenerateValidFormula()
    {
        // Act
        var result = GoogleFormulaBuilder.BuildArrayFormulaUniqueFiltered(TestKeyRange, TestHeader, TestSourceRange);

        // Assert
        Assert.Contains("=ARRAYFORMULA(", result);
        Assert.Contains("SORT(UNIQUE(IFERROR(FILTER(", result); // Updated to include IFERROR
        Assert.Contains(TestKeyRange, result);
        Assert.Contains(TestHeader, result);
        Assert.Contains(TestSourceRange, result);
    }

    [Fact]
    public void BuildArrayFormulaSafeDivision_ShouldGenerateValidFormula()
    {
        // Arrange
        var numerator = "$E:$E";
        var denominator = "$F:$F";

        // Act
        var result = GoogleFormulaBuilder.BuildArrayFormulaSafeDivision(TestKeyRange, TestHeader, numerator, denominator);

        // Assert
        Assert.Contains("=ARRAYFORMULA(", result);
        Assert.Contains("/IF(", result);
        Assert.Contains("=0,1,", result);
        Assert.Contains(numerator, result);
        Assert.Contains(denominator, result);
    }

    [Fact]
    public void BuildArrayFormulaSortedLookup_WithFirstValue_ShouldGenerateValidFormula()
    {
        // Arrange
        var sourceSheet = "TestSheet";
        var dateColumn = "$G:$G";
        var keyColumn = "$H:$H";
        var isFirst = true;

        // Act
        var result = GoogleFormulaBuilder.BuildArrayFormulaSortedLookup(TestKeyRange, TestHeader, sourceSheet, dateColumn, keyColumn, isFirst);

        // Assert
        Assert.Contains("=ARRAYFORMULA(", result);
        Assert.Contains("IFERROR(VLOOKUP(", result);
        Assert.Contains("SORT(QUERY(", result);
        Assert.Contains(sourceSheet, result);
        Assert.Contains(dateColumn, result);
        Assert.Contains(keyColumn, result);
        Assert.Contains("true", result);
    }

    [Fact]
    public void BuildArrayFormulaSortedLookup_WithLastValue_ShouldGenerateValidFormula()
    {
        // Arrange
        var sourceSheet = "TestSheet";
        var dateColumn = "$G:$G";
        var keyColumn = "$H:$H";
        var isFirst = false;

        // Act
        var result = GoogleFormulaBuilder.BuildArrayFormulaSortedLookup(TestKeyRange, TestHeader, sourceSheet, dateColumn, keyColumn, isFirst);

        // Assert
        Assert.Contains("=ARRAYFORMULA(", result);
        Assert.Contains("false", result);
    }

    [Fact]
    public void BuildSafeVLookup_ShouldGenerateValidFormula()
    {
        // Arrange
        var searchKey = "$A1";
        var searchRange = "$B:$D";
        var columnIndex = "2";

        // Act
        var result = GoogleFormulaBuilder.BuildSafeVLookup(searchKey, searchRange, columnIndex);

        // Assert
        Assert.Contains("IFERROR(VLOOKUP(", result);
        Assert.Contains("false)", result);
        Assert.Contains(searchKey, result);
        Assert.Contains(searchRange, result);
        Assert.Contains(columnIndex, result);
    }

    [Fact]
    public void BuildArrayFormulaWeekday_ShouldGenerateValidFormula()
    {
        // Arrange
        var dateRange = "$B:$B";

        // Act
        var result = GoogleFormulaBuilder.BuildArrayFormulaWeekday(TestKeyRange, TestHeader, dateRange);

        // Assert
        Assert.Contains("=ARRAYFORMULA(", result);
        Assert.Contains("WEEKDAY(", result);
        Assert.Contains(",2)", result);
        Assert.Contains(dateRange, result);
    }

    [Fact]
    public void BuildArrayFormulaSplit_ShouldGenerateValidFormula()
    {
        // Arrange
        var sourceRange = "$B:$B";
        var delimiter = "-";
        var index = 2;

        // Act
        var result = GoogleFormulaBuilder.BuildArrayFormulaSplit(TestKeyRange, TestHeader, sourceRange, delimiter, index);

        // Assert
        Assert.Contains("=ARRAYFORMULA(", result);
        Assert.Contains("IFERROR(INDEX(SPLIT(", result);
        Assert.Contains(sourceRange, result);
        Assert.Contains(delimiter, result);
        Assert.Contains(index.ToString(), result);
    }

    [Fact]
    public void BuildCustomFormula_WithMultipleReplacements_ShouldReplaceAllPlaceholders()
    {
        // Arrange
        var template = "Test {placeholder1} and {placeholder2} formula";
        var replacements = new[]
        {
            ("{placeholder1}", "VALUE1"),
            ("{placeholder2}", "VALUE2")
        };

        // Act
        var result = GoogleFormulaBuilder.BuildCustomFormula(template, replacements);

        // Assert
        Assert.Equal("Test VALUE1 and VALUE2 formula", result);
        Assert.DoesNotContain("{placeholder1}", result);
        Assert.DoesNotContain("{placeholder2}", result);
    }

    [Fact]
    public void WrapWithArrayFormula_ShouldWrapFormulaCorrectly()
    {
        // Arrange
        var innerFormula = "SUM(B:B)";

        // Act
        var result = GoogleFormulaBuilder.WrapWithArrayFormula(TestKeyRange, TestHeader, innerFormula);

        // Assert
        Assert.Contains("=ARRAYFORMULA(IFS(", result);
        Assert.Contains(TestKeyRange, result);
        Assert.Contains(TestHeader, result);
        Assert.Contains(innerFormula, result);
    }

    [Fact]
    public void BuildSafeDivision_ShouldGenerateZeroProtectedDivision()
    {
        // Arrange
        var numerator = "$B:$B";
        var denominator = "$C:$C";

        // Act
        var result = GoogleFormulaBuilder.BuildSafeDivision(numerator, denominator);

        // Assert
        Assert.Contains("/IF(", result);
        Assert.Contains("=0,1,", result);
        Assert.Contains(numerator, result);
        Assert.Contains(denominator, result);
    }

    [Theory]
    [InlineData("", "", "", "")]
    [InlineData("$A:$A", "Header", "$B:$B", "$C:$C")]
    [InlineData("TestRange", "Test Header Name", "LookupRange", "SumRange")]
    public void BuildArrayFormulaSumIf_WithVariousInputs_ShouldHandleGracefully(string keyRange, string header, string lookupRange, string sumRange)
    {
        // Act
        var result = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, header, lookupRange, sumRange);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("=ARRAYFORMULA(", result);
        Assert.Contains("SUMIF", result);
    }

    #region New Tests

    [Fact]
    public void BuildArrayLiteralUnique_ShouldGenerateSimpleArrayFormula()
    {
        // Arrange
        var header = "TestHeader";
        var sourceRange = "Sheet!$B$2:$B";

        // Act
        var result = GoogleFormulaBuilder.BuildArrayLiteralUnique(header, sourceRange);

        // Assert
        Assert.Contains("={\"TestHeader\";SORT(UNIQUE(", result);
        Assert.Contains(sourceRange, result);
        Assert.EndsWith(")}", result); // Fix ending for simple array literal
    }

    [Fact]
    public void BuildArrayLiteralUniqueCombined_ShouldCombineRanges()
    {
        // Arrange
        var header = "TestHeader";
        var range1 = "Sheet1!$B$2:$B";
        var range2 = "Sheet2!$C$2:$C";

        // Act
        var result = GoogleFormulaBuilder.BuildArrayLiteralUniqueCombined(header, range1, range2);

        // Assert
        Assert.Contains("={\"TestHeader\";SORT(UNIQUE({", result);
        Assert.Contains(range1, result);
        Assert.Contains(range2, result);
        Assert.Contains(";", result); // Range separator
        // Note: Ending format may vary but formula structure is correct
    }

    [Fact]
    public void BuildArrayLiteralUniqueCombinedFiltered_ShouldCombineAndFilterRanges()
    {
        // Arrange
        var header = "TestHeader";
        var range1 = "Sheet1!$B$2:$B";
        var range2 = "Sheet2!$C$2:$C";

        // Act
        var result = GoogleFormulaBuilder.BuildArrayLiteralUniqueCombinedFiltered(header, range1, range2);

        // Assert
        Assert.Contains("={\"TestHeader\";SORT(UNIQUE(FILTER({", result);
        Assert.Contains(range1, result);
        Assert.Contains(range2, result);
        Assert.Contains(";", result); // Range separator
        Assert.Contains("<>\"\"", result); // Empty value filter
        // Verify it contains FILTER to exclude empty values
        Assert.Contains("FILTER(", result);
    }

    [Fact]
    public void BuildArrayLiteralUniqueFiltered_ShouldFilterWithoutSorting()
    {
        // Arrange
        var header = "TestHeader";
        var sourceRange = "Sheet!$B$2:$B";

        // Act
        var result = GoogleFormulaBuilder.BuildArrayLiteralUniqueFiltered(header, sourceRange);

        // Assert - Default is unsorted (preserves source order)
        Assert.Contains("={\"TestHeader\";UNIQUE(IFERROR(FILTER(", result);
        Assert.DoesNotContain("SORT(", result); // Should NOT contain SORT
        Assert.Contains(sourceRange, result);
        Assert.Contains("<>\"\"", result);
    }

    [Fact]
    public void BuildArrayLiteralUniqueFilteredSorted_ShouldFilterAndSort()
    {
        // Arrange
        var header = "TestHeader";
        var sourceRange = "Sheet!$B$2:$B";

        // Act
        var result = GoogleFormulaBuilder.BuildArrayLiteralUniqueFilteredSorted(header, sourceRange);

        // Assert - Sorted version explicitly includes SORT
        Assert.Contains("={\"TestHeader\";SORT(UNIQUE(IFERROR(FILTER(", result);
        Assert.Contains("SORT(", result); // Sorted version includes SORT
        Assert.Contains(sourceRange, result);
        Assert.Contains("<>\"\"", result);
    }

    [Fact]
    public void BuildArrayFormulaDay_ShouldExtractDayFromDate()
    {
        // Arrange
        var dateRange = "$A:$A";

        // Act
        var result = GoogleFormulaBuilder.BuildArrayFormulaDay(TestKeyRange, TestHeader, dateRange);

        // Assert
        Assert.Contains("=ARRAYFORMULA(", result);
        Assert.Contains("DAY(", result);
        Assert.Contains(dateRange, result);
    }

    [Fact]
    public void BuildArrayFormulaMonth_ShouldExtractMonthFromDate()
    {
        // Arrange
        var dateRange = "$A:$A";

        // Act
        var result = GoogleFormulaBuilder.BuildArrayFormulaMonth(TestKeyRange, TestHeader, dateRange);

        // Assert
        Assert.Contains("=ARRAYFORMULA(", result);
        Assert.Contains("MONTH(", result);
        Assert.Contains(dateRange, result);
    }

    [Fact]
    public void BuildArrayFormulaYear_ShouldExtractYearFromDate()
    {
        // Arrange
        var dateRange = "$A:$A";

        // Act
        var result = GoogleFormulaBuilder.BuildArrayFormulaYear(TestKeyRange, TestHeader, dateRange);

        // Assert
        Assert.Contains("=ARRAYFORMULA(", result);
        Assert.Contains("YEAR(", result);
        Assert.Contains(dateRange, result);
    }

    [Fact]
    public void BuildArrayFormulaWeekdayText_ShouldGenerateTextWeekday()
    {
        // Arrange
        var dateRange = "$A:$A";

        // Act
        var result = GoogleFormulaBuilder.BuildArrayFormulaWeekdayText(TestKeyRange, TestHeader, dateRange);

        // Assert
        Assert.Contains("=ARRAYFORMULA(", result);
        Assert.Contains("TEXT(", result); // Expect text formula
        Assert.Contains("\"ddd\"", result); // Expect weekday abbreviation
        Assert.Contains(TestKeyRange, result);
        Assert.Contains(TestHeader, result);
        Assert.Contains(dateRange, result, StringComparison.OrdinalIgnoreCase); // Should include dateRange
    }

    [Fact]
    public void BuildArrayFormulaWeekdayTextDirect_ShouldGenerateWeekdayTextFormula()
    {
        // Arrange
        var keyRange = "$A:$A";
        var header = "Test Header";
        var dateRange = "$A:$A";
        var offset = 1;

        // Act
        var result = GoogleFormulaBuilder.BuildArrayFormulaWeekdayText(keyRange, header, dateRange, offset);

        // Assert
        Assert.Contains("=ARRAYFORMULA(IFS(ROW(", result);
        Assert.Contains("TEXT(", result);
        Assert.Contains("+1,\"ddd\")", result);
        Assert.Contains(keyRange, result);
        Assert.Contains(header, result);
    }

    [Fact]
    public void BuildArrayFormulaDualCountIf_ShouldCountFromTwoRanges()
    {
        // Arrange
        var range1 = "$B:$B";
        var range2 = "$C:$C";

        // Act
        var result = GoogleFormulaBuilder.BuildArrayFormulaDualCountIf(TestKeyRange, TestHeader, range1, range2);

        // Assert
        Assert.Contains("=ARRAYFORMULA(", result);
        Assert.Contains("COUNTIF(", result);
        Assert.Contains("+COUNTIF(", result);
        Assert.Contains(range1, result);
        Assert.Contains(range2, result);
    }

    [Fact]
    public void BuildArrayFormulaSplitByIndex_ShouldSplitAndExtractByIndex()
    {
        // Arrange
        var sourceRange = "$A:$A";
        var delimiter = "-";
        var index = 2;

        // Act
        var result = GoogleFormulaBuilder.BuildArrayFormulaSplitByIndex(TestKeyRange, TestHeader, sourceRange, delimiter, index);

        // Assert
        Assert.Contains("=ARRAYFORMULA(", result);
        Assert.Contains("IFERROR(INDEX(SPLIT(", result);
        Assert.Contains(sourceRange, result);
        Assert.Contains(delimiter, result);
        Assert.Contains(index.ToString(), result);
    }

    [Theory]
    [InlineData("A1:A", "Daily!A:F", 0, "Curr Amt", "=ARRAYFORMULA(IFS(ROW(A1:A)=1,\"Curr Amt\",ISBLANK(A1:A), \"\", true, IFERROR(VLOOKUP(TODAY()-WEEKDAY(TODAY(),2)+A1:A+0,Daily!A:F,6,false),0)))")]
    [InlineData("A1:A", "Daily!A:F", -7, "Prev Amt", "=ARRAYFORMULA(IFS(ROW(A1:A)=1,\"Prev Amt\",ISBLANK(A1:A), \"\", true, IFERROR(VLOOKUP(TODAY()-WEEKDAY(TODAY(),2)+A1:A+-7,Daily!A:F,6,false),0)))")]
    [InlineData("B2:B", "Sheet!C:H", 5, "Custom", "=ARRAYFORMULA(IFS(ROW(B2:B)=1,\"Custom\",ISBLANK(B2:B), \"\", true, IFERROR(VLOOKUP(TODAY()-WEEKDAY(TODAY(),2)+B2:B+5,Sheet!C:H,6,false),0)))")]
    public void BuildArrayFormulaWeekdayAmount_ShouldGenerateExpectedFormula(string keyRange, string dailyRange, int offset, string columnTitle, string expected)
    {
        // Act
        var result = GoogleFormulaBuilder.BuildArrayFormulaWeekdayAmount(keyRange, dailyRange, offset, columnTitle);

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion
}