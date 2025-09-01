using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Gig.Helpers;
using RaptorSheets.Gig.Mappers;
using Xunit;

namespace RaptorSheets.Gig.Tests.Integration.Helpers;

/// <summary>
/// Integration tests to verify formula builders work correctly with actual mappers
/// </summary>
public class FormulaBuilderIntegrationTests
{
    [Fact]
    public void GoogleFormulaBuilder_WithActualMapperData_ShouldGenerateValidFormulas()
    {
        // Arrange - Use actual mapper configuration
        var tripSheet = TripMapper.GetSheet();
        var placeSheet = PlaceMapper.GetSheet();
        
        // Get actual ranges from real sheet configuration
        var placeRange = placeSheet.GetLocalRange("Place");
        var tripPlaceRange = tripSheet.GetRange("Place");
        var tripPayRange = tripSheet.GetRange("Pay");

        // Act - Generate formulas using real mapper data
        var uniqueFormula = GoogleFormulaBuilder.BuildArrayFormulaUnique(placeRange, "Place", tripPlaceRange);
        var sumIfFormula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(placeRange, "Pay", tripPlaceRange, tripPayRange);
        var countIfFormula = GoogleFormulaBuilder.BuildArrayFormulaCountIf(placeRange, "Trips", tripPlaceRange);

        // Assert - Verify formulas contain expected Google Sheets functions
        Assert.Contains("=ARRAYFORMULA(", uniqueFormula);
        Assert.Contains("SORT(UNIQUE(", uniqueFormula);
        
        Assert.Contains("=ARRAYFORMULA(", sumIfFormula);
        Assert.Contains("SUMIF(", sumIfFormula);
        
        Assert.Contains("=ARRAYFORMULA(", countIfFormula);
        Assert.Contains("COUNTIF(", countIfFormula);

        // Verify actual range references are included
        Assert.Contains("Place", uniqueFormula);
        Assert.Contains("Pay", sumIfFormula);
        Assert.Contains("Trips", countIfFormula);
    }

    [Fact]
    public void GigFormulaBuilder_WithActualMapperData_ShouldGenerateValidGigFormulas()
    {
        // Arrange - Use actual mapper configuration
        var placeSheet = PlaceMapper.GetSheet();
        var placeRange = placeSheet.GetLocalRange("Place");
        
        // Simulate real range references
        var payRange = placeSheet.GetLocalRange("Pay");
        var tipsRange = placeSheet.GetLocalRange("Tips");
        var bonusRange = placeSheet.GetLocalRange("Bonus");
        var totalRange = placeSheet.GetLocalRange("Total");
        var tripsRange = placeSheet.GetLocalRange("Trips");

        // Act - Generate gig-specific formulas
        var totalFormula = GigFormulaBuilder.BuildArrayFormulaTotal(placeRange, "Total", payRange, tipsRange, bonusRange);
        var amountPerTripFormula = GigFormulaBuilder.BuildArrayFormulaAmountPerTrip(placeRange, "Amount Per Trip", totalRange, tripsRange);

        // Assert - Verify gig business logic is correctly applied
        Assert.Contains("=ARRAYFORMULA(", totalFormula);
        Assert.Contains(payRange, totalFormula);
        Assert.Contains(tipsRange, totalFormula);
        Assert.Contains(bonusRange, totalFormula);
        Assert.Contains("+", totalFormula); // Addition of income components

        Assert.Contains("=ARRAYFORMULA(", amountPerTripFormula);
        Assert.Contains(totalRange, amountPerTripFormula);
        Assert.Contains(tripsRange, amountPerTripFormula);
        Assert.Contains("/IF(", amountPerTripFormula); // Zero-safe division
    }

    [Fact]
    public void FormulaBuilders_ComparedToLegacyHelpers_ShouldProduceSimilarResults()
    {
        // Arrange
        var keyRange = "$A:$A";
        var header = "Test Header";
        var lookupRange = "$B:$B";
        var sumRange = "$C:$C";

        // Act - Compare legacy vs new approaches
        #pragma warning disable CS0618 // Type or member is obsolete
        var legacyFormula = ArrayFormulaHelpers.ArrayFormulaSumIf(keyRange, header, lookupRange, sumRange);
        #pragma warning restore CS0618
        var newFormula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, header, lookupRange, sumRange);

        // Assert - Both should generate valid ARRAYFORMULA with SUMIF
        Assert.Contains("=ARRAYFORMULA(", legacyFormula);
        Assert.Contains("=ARRAYFORMULA(", newFormula);
        Assert.Contains("SUMIF(", legacyFormula);
        Assert.Contains("SUMIF(", newFormula);
        
        // Both should contain the same range references
        Assert.Contains(keyRange, legacyFormula);
        Assert.Contains(keyRange, newFormula);
        Assert.Contains(header, legacyFormula);
        Assert.Contains(header, newFormula);
        Assert.Contains(lookupRange, legacyFormula);
        Assert.Contains(lookupRange, newFormula);
        Assert.Contains(sumRange, legacyFormula);
        Assert.Contains(sumRange, newFormula);
    }

    [Fact]
    public void FormulaBuilders_WithComplexRanges_ShouldHandleCorrectly()
    {
        // Arrange - Test with complex range references similar to real usage
        var complexKeyRange = "Places!$A$2:$A";
        var complexHeader = "Complex Header With Spaces";
        var complexLookupRange = "Trips!$D$2:$D";
        var complexSumRange = "Trips!$E$2:$E";

        // Act
        var formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(complexKeyRange, complexHeader, complexLookupRange, complexSumRange);

        // Assert
        Assert.Contains(complexKeyRange, formula);
        Assert.Contains(complexHeader, formula);
        Assert.Contains(complexLookupRange, formula);
        Assert.Contains(complexSumRange, formula);
        Assert.Contains("=ARRAYFORMULA(", formula);
        Assert.Contains("SUMIF(", formula);
    }

    [Fact]
    public void GigFormulaBuilder_WithWeekdayAnalysis_ShouldGenerateComplexFormulas()
    {
        // Arrange - Test weekday analysis formulas that are used in WeekdayMapper
        var dayRange = "$A:$A";
        var header = "Current Amount";
        var dailySheet = "Daily";
        var dateColumn = "$A:$A";
        var totalColumn = "$E:$E";
        var totalIndex = "5";

        // Act
        var currentAmountFormula = GigFormulaBuilder.BuildArrayFormulaCurrentAmount(
            dayRange, header, dayRange, dailySheet, dateColumn, totalColumn, totalIndex);

        // Assert
        Assert.Contains("=ARRAYFORMULA(", currentAmountFormula);
        Assert.Contains("TODAY()-WEEKDAY(TODAY(),2)", currentAmountFormula);
        Assert.Contains(dailySheet, currentAmountFormula);
        Assert.Contains(totalIndex, currentAmountFormula);
        Assert.Contains("VLOOKUP(", currentAmountFormula);
    }

    [Fact]
    public void FormulaConstantsAndBuilders_ShouldBeConsistent()
    {
        // This test verifies that builders properly use the constants
        
        // Arrange
        var keyRange = "$A:$A";
        var header = "Test";
        var totalRange = "$B:$B";
        var tripsRange = "$C:$C";

        // Act
        var formula = GigFormulaBuilder.BuildArrayFormulaAmountPerTrip(keyRange, header, totalRange, tripsRange);

        // Assert - Verify the formula contains elements from both GigFormulas constants and GoogleFormulas base
        Assert.Contains("=ARRAYFORMULA(IFS(", formula); // From GoogleFormulas.ArrayFormulaBase
        Assert.Contains("/IF(", formula); // From GigFormulas.AmountPerTripFormula
        Assert.Contains("=0,1,", formula); // Zero protection from gig formula
        Assert.Contains(totalRange, formula);
        Assert.Contains(tripsRange, formula);
    }

    [Fact]
    public void FormulaBuildersWithActualSheetConfiguration_ShouldIntegrateCorrectly()
    {
        // Arrange - Get actual sheet configurations
        var tripSheet = TripMapper.GetSheet();
        var shiftSheet = ShiftMapper.GetSheet();
        var dailySheet = DailyMapper.GetSheet();

        // Act - Verify UpdateColumns works correctly with formula builders
        tripSheet.Headers.UpdateColumns();
        shiftSheet.Headers.UpdateColumns();
        dailySheet.Headers.UpdateColumns();

        // Assert - Verify column indexes are set correctly for formula building
        Assert.All(tripSheet.Headers, header => Assert.True(header.Index >= 0));
        Assert.All(shiftSheet.Headers, header => Assert.True(header.Index >= 0));
        Assert.All(dailySheet.Headers, header => Assert.True(header.Index >= 0));

        // Verify range references can be built from these sheets
        var tripPlaceRange = tripSheet.GetRange("Place");
        var dailyTotalRange = dailySheet.GetRange("Total");

        Assert.NotNull(tripPlaceRange);
        Assert.NotNull(dailyTotalRange);
        Assert.NotEmpty(tripPlaceRange);
        Assert.NotEmpty(dailyTotalRange);
    }

    [Fact]
    public void MapperFormulas_ShouldGenerateValidGoogleSheetsFormulas()
    {
        // This test verifies that the refactored mappers generate valid formulas
        
        // Arrange - Get actual configured sheets
        var placeSheet = PlaceMapper.GetSheet();
        var nameSheet = NameMapper.GetSheet();
        var dailySheet = DailyMapper.GetSheet();

        // Act - Get formulas from actual headers
        var placeFormulas = placeSheet.Headers.Where(h => !string.IsNullOrEmpty(h.Formula)).Select(h => h.Formula).ToList();
        var nameFormulas = nameSheet.Headers.Where(h => !string.IsNullOrEmpty(h.Formula)).Select(h => h.Formula).ToList();
        var dailyFormulas = dailySheet.Headers.Where(h => !string.IsNullOrEmpty(h.Formula)).Select(h => h.Formula).ToList();

        // Assert - All formulas should be valid
        Assert.NotEmpty(placeFormulas);
        Assert.NotEmpty(nameFormulas);
        Assert.NotEmpty(dailyFormulas);
        
        // All formulas should start with =
        Assert.All(placeFormulas, formula => Assert.StartsWith("=", formula));
        Assert.All(nameFormulas, formula => Assert.StartsWith("=", formula));
        Assert.All(dailyFormulas, formula => Assert.StartsWith("=", formula));
        
        // Most formulas should contain ARRAYFORMULA
        var arrayFormulaCount = placeFormulas.Count(f => f.Contains("ARRAYFORMULA")) +
                              nameFormulas.Count(f => f.Contains("ARRAYFORMULA")) +
                              dailyFormulas.Count(f => f.Contains("ARRAYFORMULA"));
        
        Assert.True(arrayFormulaCount > 0);
    }
}