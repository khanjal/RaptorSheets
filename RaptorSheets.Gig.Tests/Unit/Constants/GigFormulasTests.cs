using RaptorSheets.Gig.Constants;
using Xunit;

namespace RaptorSheets.Gig.Tests.Unit.Constants;

public class GigFormulasTests
{
    [Fact]
    public void TotalIncomeFormula_ShouldContainAllIncomeComponents()
    {
        // Act
        var formula = GigFormulas.TotalIncomeFormula;

        // Assert
        Assert.Equal("{payRange}+{tipsRange}+{bonusRange}", formula);
        Assert.Contains("{payRange}", formula);
        Assert.Contains("{tipsRange}", formula);
        Assert.Contains("{bonusRange}", formula);
        Assert.Contains("+", formula);
    }

    [Fact]
    public void AmountPerTripFormula_ShouldContainZeroProtection()
    {
        // Act
        var formula = GigFormulas.AmountPerTripFormula;

        // Assert
        Assert.Contains("{totalRange}", formula);
        Assert.Contains("{tripsRange}", formula);
        Assert.Contains("/IF(", formula);
        Assert.Contains("=0,1,", formula);
    }

    [Fact]
    public void AmountPerDistanceFormula_ShouldContainZeroProtection()
    {
        // Act
        var formula = GigFormulas.AmountPerDistanceFormula;

        // Assert
        Assert.Contains("{totalRange}", formula);
        Assert.Contains("{distanceRange}", formula);
        Assert.Contains("/IF(", formula);
        Assert.Contains("=0,1,", formula);
    }

    [Fact]
    public void AmountPerTimeFormula_ShouldContainTimeConversion()
    {
        // Act
        var formula = GigFormulas.AmountPerTimeFormula;

        // Assert
        Assert.Contains("{totalRange}", formula);
        Assert.Contains("{timeRange}", formula);
        Assert.Contains("*24", formula); // Time conversion to hours
        Assert.Contains("/IF(", formula);
    }

    [Fact]
    public void WeekNumberWithYear_ShouldContainWeekAndYearFunctions()
    {
        // Act
        var formula = GigFormulas.WeekNumberWithYear;

        // Assert
        Assert.Contains("WEEKNUM(", formula);
        Assert.Contains("YEAR(", formula);
        Assert.Contains("&\"-\"&", formula);
        Assert.Contains("{dateRange}", formula);
    }

    [Fact]
    public void MonthNumberWithYear_ShouldContainMonthAndYearFunctions()
    {
        // Act
        var formula = GigFormulas.MonthNumberWithYear;

        // Assert
        Assert.Contains("MONTH(", formula);
        Assert.Contains("YEAR(", formula);
        Assert.Contains("&\"-\"&", formula);
        Assert.Contains("{dateRange}", formula);
    }

    [Fact]
    public void WeekBeginDate_ShouldContainWeekCalculationLogic()
    {
        // Act
        var formula = GigFormulas.WeekBeginDate;

        // Assert
        Assert.Contains("DATE({yearRange},1,1)", formula);
        Assert.Contains("({weekRange}-1)*7", formula);
        Assert.Contains("WEEKDAY(DATE({yearRange},1,1),3)", formula);
        Assert.Contains("{yearRange}", formula);
        Assert.Contains("{weekRange}", formula);
    }

    [Fact]
    public void WeekEndDate_ShouldContainWeekEndCalculationLogic()
    {
        // Act
        var formula = GigFormulas.WeekEndDate;

        // Assert
        Assert.Contains("DATE({yearRange},1,7)", formula); // Week end uses day 7
        Assert.Contains("({weekRange}-1)*7", formula);
        Assert.Contains("WEEKDAY(DATE({yearRange},1,1),3)", formula);
    }

    [Fact]
    public void CurrentAmountLookup_ShouldContainTodayLogic()
    {
        // Act
        var formula = GigFormulas.CurrentAmountLookup;

        // Assert
        Assert.Contains("TODAY()-WEEKDAY(TODAY(),2)", formula);
        Assert.Contains("{dayRange}", formula);
        Assert.Contains("{dailySheet}", formula);
        Assert.Contains("{dateColumn}", formula);
        Assert.Contains("{totalColumn}", formula);
        Assert.Contains("{totalIndex}", formula);
        Assert.Contains("IFERROR(VLOOKUP(", formula);
    }

    [Fact]
    public void PreviousAmountLookup_ShouldContainPreviousWeekLogic()
    {
        // Act
        var formula = GigFormulas.PreviousAmountLookup;

        // Assert
        Assert.Contains("TODAY()-WEEKDAY(TODAY(),2)", formula);
        Assert.Contains("-7", formula); // Previous week offset
        Assert.Contains("IFERROR(VLOOKUP(", formula);
    }

    [Fact]
    public void PreviousDayAverage_ShouldContainAverageCalculationLogic()
    {
        // Act
        var formula = GigFormulas.PreviousDayAverage;

        // Assert
        Assert.Contains("({totalRange}-{previousRange})", formula);
        Assert.Contains("/IF({daysRange}=0,1,", formula);
        Assert.Contains("-IF({previousRange}=0,0,-1)", formula);
        Assert.Contains("{totalRange}", formula);
        Assert.Contains("{previousRange}", formula);
        Assert.Contains("{daysRange}", formula);
    }

    [Fact]
    public void MultipleFieldVisitLookup_ShouldContainMultipleFieldLogic()
    {
        // Act
        var formula = GigFormulas.MultipleFieldVisitLookup;

        // Assert
        Assert.Contains("IFERROR(MIN(IF(", formula);
        Assert.Contains("{sourceSheet}!{keyColumn1}:{keyColumn1}={keyRange}", formula);
        Assert.Contains("{sourceSheet}!{keyColumn2}:{keyColumn2}={keyRange}", formula);
        Assert.Contains("{sourceSheet}!{dateColumn}:{dateColumn}", formula);
        Assert.Contains("{keyRange}", formula);
        Assert.Contains("{sourceSheet}", formula);
        Assert.Contains("{keyColumn1}", formula);
        Assert.Contains("{keyColumn2}", formula);
        Assert.Contains("{dateColumn}", formula);
    }

    [Theory]
    [InlineData("TotalIncomeFormula")]
    [InlineData("AmountPerTripFormula")]
    [InlineData("WeekNumberWithYear")]
    [InlineData("CurrentAmountLookup")]
    public void AllGigFormulas_ShouldNotBeNullOrEmpty(string formulaName)
    {
        // Act
        var formula = typeof(GigFormulas).GetField(formulaName)?.GetValue(null) as string;

        // Assert
        Assert.NotNull(formula);
        Assert.NotEmpty(formula);
    }

    [Fact]
    public void AllGigFormulas_ShouldContainValidGoogleSheetsFunction()
    {
        // Arrange
        var expectedFunctions = new[] { "WEEKNUM", "MONTH", "YEAR", "DATE", "WEEKDAY", "TODAY", "VLOOKUP", "MIN", "MAX", "IF" };
        var allFormulas = new[]
        {
            GigFormulas.TotalIncomeFormula,
            GigFormulas.AmountPerTripFormula,
            GigFormulas.AmountPerDistanceFormula,
            GigFormulas.AmountPerTimeFormula,
            GigFormulas.WeekNumberWithYear,
            GigFormulas.MonthNumberWithYear,
            GigFormulas.WeekBeginDate,
            GigFormulas.WeekEndDate,
            GigFormulas.CurrentAmountLookup,
            GigFormulas.PreviousAmountLookup,
            GigFormulas.PreviousDayAverage,
            GigFormulas.MultipleFieldVisitLookup
        };

        // Act & Assert
        foreach (var formula in allFormulas)
        {
            Assert.NotNull(formula);
            Assert.NotEmpty(formula);
            
            // Each formula should contain at least one valid Google Sheets function or valid operators
            var containsValidFunction = expectedFunctions.Any(func => formula.Contains(func)) || 
                                      formula.Contains("+") || formula.Contains("/") || formula.Contains("-");
            
            Assert.True(containsValidFunction, $"Formula '{formula}' should contain at least one valid Google Sheets function or operator");
        }
    }
}