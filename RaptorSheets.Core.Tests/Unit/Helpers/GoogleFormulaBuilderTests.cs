using RaptorSheets.Core.Constants;
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
        Assert.Contains("SORT(UNIQUE(FILTER(", result);
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
}