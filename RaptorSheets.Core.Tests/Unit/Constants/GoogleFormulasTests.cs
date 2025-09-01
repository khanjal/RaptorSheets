using RaptorSheets.Core.Constants;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Constants;

public class GoogleFormulasTests
{
    [Fact]
    public void ArrayFormulaBase_ShouldContainRequiredPlaceholders()
    {
        // Act
        var formula = GoogleFormulas.ArrayFormulaBase;

        // Assert
        Assert.Contains("{keyRange}", formula);
        Assert.Contains("{header}", formula);
        Assert.Contains("{formula}", formula);
        Assert.StartsWith("=ARRAYFORMULA(", formula);
        Assert.Contains("IFS(ROW(", formula);
        Assert.Contains("ISBLANK(", formula);
    }

    [Fact]
    public void ArrayFormulaUnique_ShouldContainUniqueLogic()
    {
        // Act
        var formula = GoogleFormulas.ArrayFormulaUnique;

        // Assert
        Assert.Contains("SORT(UNIQUE(", formula);
        Assert.Contains("{sourceRange}", formula);
        Assert.Contains("{keyRange}", formula);
        Assert.Contains("{header}", formula);
    }

    [Fact]
    public void ArrayFormulaUniqueFiltered_ShouldContainFilterLogic()
    {
        // Act
        var formula = GoogleFormulas.ArrayFormulaUniqueFiltered;

        // Assert
        Assert.Contains("SORT(UNIQUE(IFERROR(FILTER(", formula);
        Assert.Contains("{sourceRange}", formula);
        Assert.Contains("<>\"\"", formula); // Filter condition
    }

    [Fact]
    public void SumIfAggregation_ShouldContainSumIfStructure()
    {
        // Act
        var formula = GoogleFormulas.SumIfAggregation;

        // Assert
        Assert.Equal("SUMIF({lookupRange},{keyRange},{sumRange})", formula);
        Assert.Contains("{lookupRange}", formula);
        Assert.Contains("{keyRange}", formula);
        Assert.Contains("{sumRange}", formula);
    }

    [Fact]
    public void CountIfAggregation_ShouldContainCountIfStructure()
    {
        // Act
        var formula = GoogleFormulas.CountIfAggregation;

        // Assert
        Assert.Equal("COUNTIF({lookupRange},{keyRange})", formula);
        Assert.Contains("{lookupRange}", formula);
        Assert.Contains("{keyRange}", formula);
    }

    [Fact]
    public void SafeDivisionFormula_ShouldContainZeroProtection()
    {
        // Act
        var formula = GoogleFormulas.SafeDivisionFormula;

        // Assert
        Assert.Contains("{numerator}", formula);
        Assert.Contains("{denominator}", formula);
        Assert.Contains("/IF(", formula);
        Assert.Contains("=0,1,", formula);
    }

    [Fact]
    public void SafeVLookup_ShouldContainErrorHandling()
    {
        // Act
        var formula = GoogleFormulas.SafeVLookup;

        // Assert
        Assert.Contains("IFERROR(VLOOKUP(", formula);
        Assert.Contains("{searchKey}", formula);
        Assert.Contains("{searchRange}", formula);
        Assert.Contains("{columnIndex}", formula);
        Assert.Contains("false)", formula);
    }

    [Fact]
    public void SortedVLookup_ShouldContainQueryAndSort()
    {
        // Act
        var formula = GoogleFormulas.SortedVLookup;

        // Assert
        Assert.Contains("SORT(QUERY(", formula);
        Assert.Contains("SELECT", formula);
        Assert.Contains("{sourceSheet}", formula);
        Assert.Contains("{dateColumn}", formula);
        Assert.Contains("{keyColumn}", formula);
        Assert.Contains("{isFirst}", formula);
    }

    [Fact]
    public void WeekdayNumber_ShouldContainWeekdayFunction()
    {
        // Act
        var formula = GoogleFormulas.WeekdayNumber;

        // Assert
        Assert.Equal("WEEKDAY({dateRange},2)", formula);
        Assert.Contains("{dateRange}", formula);
        Assert.Contains(",2)", formula); // Monday = 1 format
    }

    [Fact]
    public void SplitStringByIndex_ShouldContainSplitLogic()
    {
        // Act
        var formula = GoogleFormulas.SplitStringByIndex;

        // Assert
        Assert.Contains("IFERROR(INDEX(SPLIT(", formula);
        Assert.Contains("{sourceRange}", formula);
        Assert.Contains("{delimiter}", formula);
        Assert.Contains("{index}", formula);
    }

    [Fact]
    public void ZeroDivisionProtection_ShouldContainIfLogic()
    {
        // Act
        var formula = GoogleFormulas.ZeroDivisionProtection;

        // Assert
        Assert.Equal("IF({divisorRange}=0,1,{divisorRange})", formula);
        Assert.Contains("IF(", formula);
        Assert.Contains("=0,1,", formula);
    }

    [Fact]
    public void ZeroCheckCondition_ShouldContainConditionalLogic()
    {
        // Act
        var formula = GoogleFormulas.ZeroCheckCondition;

        // Assert
        Assert.Contains("{valueRange} = 0, 0, true, {calculation}", formula);
        Assert.Contains("{valueRange}", formula);
        Assert.Contains("{calculation}", formula);
    }

    #region Array Literal Formula Tests

    [Fact]
    public void ArrayLiteralUnique_ShouldHaveProperFormat()
    {
        // Act
        var formula = GoogleFormulas.ArrayLiteralUnique;

        // Assert
        Assert.Contains("{\"{header}\";SORT(UNIQUE({sourceRange}))}", formula);
        Assert.Contains("{header}", formula);
        Assert.Contains("{sourceRange}", formula);
    }

    [Fact]
    public void ArrayLiteralUniqueCombined_ShouldCombineRanges()
    {
        // Act
        var formula = GoogleFormulas.ArrayLiteralUniqueCombined;

        // Assert
        Assert.Contains("{\"{header}\";SORT(UNIQUE({{range1};{range", GoogleFormulas.ArrayLiteralUniqueCombined); // Update to match actual template structure
        Assert.Contains("{header}", formula);
        Assert.Contains("{range1}", formula);
        Assert.Contains("{range2}", formula);
    }

    [Fact]
    public void ArrayLiteralUniqueFiltered_ShouldFilterEmptyValues()
    {
        // Act
        var formula = GoogleFormulas.ArrayLiteralUniqueFiltered;

        // Assert
        Assert.Contains("{\"{header}\";SORT(UNIQUE(IFERROR(FILTER({sourceRange}", GoogleFormulas.ArrayLiteralUniqueFiltered); // Updated to include IFERROR
        Assert.Contains("{header}", formula);
        Assert.Contains("{sourceRange}", formula);
        Assert.Contains("<>\"\"", formula);
    }

    #endregion

    [Theory]
    [InlineData("ArrayFormulaBase")]
    [InlineData("ArrayFormulaUnique")]
    [InlineData("SumIfAggregation")]
    [InlineData("SafeVLookup")]
    public void AllFormulas_ShouldNotBeNullOrEmpty(string formulaName)
    {
        // Act
        var formula = typeof(GoogleFormulas).GetField(formulaName)?.GetValue(null) as string;

        // Assert
        Assert.NotNull(formula);
        Assert.NotEmpty(formula);
    }
}