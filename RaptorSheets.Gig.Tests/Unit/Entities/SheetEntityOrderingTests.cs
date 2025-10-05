using RaptorSheets.Core.Helpers;
using RaptorSheets.Gig.Constants;

namespace RaptorSheets.Gig.Tests.Unit.Entities;

public class SheetEntityOrderingTests
{
    [Fact]
    public void SheetEntity_ConstantsBasedOrdering_ReturnsCorrectOrder()
    {
        // Act
        var sheetOrder = SheetsConfig.SheetUtilities.GetAllSheetNames();

        // Assert
        Assert.Equal(15, sheetOrder.Count);
        
        // Verify order based on SheetsConfig.SheetNames declaration order
        Assert.Equal(SheetsConfig.SheetNames.Trips, sheetOrder[0]);      // First declared
        Assert.Equal(SheetsConfig.SheetNames.Shifts, sheetOrder[1]);     // Second declared
        Assert.Equal(SheetsConfig.SheetNames.Expenses, sheetOrder[2]);   // Third declared
        Assert.Equal(SheetsConfig.SheetNames.Addresses, sheetOrder[3]);  // Fourth declared
        Assert.Equal(SheetsConfig.SheetNames.Names, sheetOrder[4]);      // Fifth declared
        Assert.Equal(SheetsConfig.SheetNames.Places, sheetOrder[5]);     // Sixth declared
        Assert.Equal(SheetsConfig.SheetNames.Regions, sheetOrder[6]);    // Seventh declared
        Assert.Equal(SheetsConfig.SheetNames.Services, sheetOrder[7]);   // Eighth declared
        Assert.Equal(SheetsConfig.SheetNames.Types, sheetOrder[8]);      // Ninth declared
        Assert.Equal(SheetsConfig.SheetNames.Daily, sheetOrder[9]);      // Tenth declared
        Assert.Equal(SheetsConfig.SheetNames.Weekdays, sheetOrder[10]);  // Eleventh declared
        Assert.Equal(SheetsConfig.SheetNames.Weekly, sheetOrder[11]);    // Twelfth declared
        Assert.Equal(SheetsConfig.SheetNames.Monthly, sheetOrder[12]);   // Thirteenth declared
        Assert.Equal(SheetsConfig.SheetNames.Yearly, sheetOrder[13]);    // Fourteenth declared
        Assert.Equal(SheetsConfig.SheetNames.Setup, sheetOrder[14]);     // Last declared
    }

    [Fact]
    public void SheetsConfig_ValidateSheetNames_WorksCorrectly()
    {
        // Arrange
        var validSheets = new[]
        {
            SheetsConfig.SheetNames.Trips,
            SheetsConfig.SheetNames.Shifts,
            SheetsConfig.SheetNames.Expenses
        };
        
        var invalidSheets = new[]
        {
            SheetsConfig.SheetNames.Trips,
            "InvalidSheet",
            SheetsConfig.SheetNames.Setup
        };

        // Act
        var validErrors = SheetsConfig.SheetUtilities.ValidateSheetNames(validSheets);
        var invalidErrors = SheetsConfig.SheetUtilities.ValidateSheetNames(invalidSheets);

        // Assert
        Assert.Empty(validErrors);
        Assert.Single(invalidErrors);
        Assert.Contains("InvalidSheet", invalidErrors[0]);
    }

    [Fact]
    public void SheetsConfig_GetSheetIndex_ReturnsCorrectPositions()
    {
        // Act & Assert
        Assert.Equal(0, SheetsConfig.SheetUtilities.GetSheetIndex(SheetsConfig.SheetNames.Trips));
        Assert.Equal(1, SheetsConfig.SheetUtilities.GetSheetIndex(SheetsConfig.SheetNames.Shifts));
        Assert.Equal(2, SheetsConfig.SheetUtilities.GetSheetIndex(SheetsConfig.SheetNames.Expenses));
        Assert.Equal(14, SheetsConfig.SheetUtilities.GetSheetIndex(SheetsConfig.SheetNames.Setup));
        Assert.Equal(-1, SheetsConfig.SheetUtilities.GetSheetIndex("NonExistentSheet"));
    }

    [Fact]
    public void SheetsConfig_IsValidSheetName_WorksCorrectly()
    {
        // Act & Assert
        Assert.True(SheetsConfig.SheetUtilities.IsValidSheetName(SheetsConfig.SheetNames.Trips));
        Assert.True(SheetsConfig.SheetUtilities.IsValidSheetName(SheetsConfig.SheetNames.Setup));
        Assert.True(SheetsConfig.SheetUtilities.IsValidSheetName("trips")); // Case insensitive
        Assert.False(SheetsConfig.SheetUtilities.IsValidSheetName("InvalidSheet"));
    }

    [Fact]
    public void SheetEntity_BusinessLogicalOrder_MakesSense()
    {
        // Act
        var sheetOrder = SheetsConfig.SheetUtilities.GetAllSheetNames();

        // Assert - Verify logical business ordering
        var tripsIndex = sheetOrder.IndexOf(SheetsConfig.SheetNames.Trips);
        var shiftsIndex = sheetOrder.IndexOf(SheetsConfig.SheetNames.Shifts);
        var expensesIndex = sheetOrder.IndexOf(SheetsConfig.SheetNames.Expenses);
        var setupIndex = sheetOrder.IndexOf(SheetsConfig.SheetNames.Setup);

        // Primary data entry should be early (defined first in constants)
        Assert.True(tripsIndex < 5, "Trips should be early for easy access");
        Assert.True(shiftsIndex < 5, "Shifts should be early for easy access");
        Assert.True(expensesIndex < 5, "Expenses should be early for easy access");

        // Administrative sheets should be last (defined last in constants)
        Assert.True(setupIndex > 10, "Setup should be toward the end as it's administrative");
        Assert.Equal(sheetOrder.Count - 1, setupIndex); // Should be last

        // Log the order for debugging
        System.Diagnostics.Debug.WriteLine("Constants-based sheet order:");
        for (int i = 0; i < sheetOrder.Count; i++)
        {
            System.Diagnostics.Debug.WriteLine($"  {i}: {sheetOrder[i]}");
        }
    }

    [Fact]
    public void ConstantsOrderHelper_ExtractsOrderCorrectly()
    {
        // Act
        var constantsOrder = ConstantsOrderHelper.GetOrderFromConstants(typeof(SheetsConfig.SheetNames));
        var configOrder = SheetsConfig.SheetUtilities.GetAllSheetNames();

        // Assert - Should be identical
        Assert.Equal(configOrder.Count, constantsOrder.Count);
        for (int i = 0; i < constantsOrder.Count; i++)
        {
            Assert.Equal(configOrder[i], constantsOrder[i]);
        }
    }
}