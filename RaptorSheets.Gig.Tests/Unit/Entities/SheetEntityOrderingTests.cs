using RaptorSheets.Gig.Constants;

namespace RaptorSheets.Gig.Tests.Unit.Entities;

public class SheetEntityOrderingTests
{
    [Fact]
    public void SheetEntity_ExplicitOrdering_ReturnsCorrectOrder()
    {
        // Act
        var sheetOrder = SheetsConfig.SheetUtilities.GetAllSheetNames();

        // Assert
        Assert.Equal(15, sheetOrder.Count);
        
        // Verify key positions
        Assert.Equal(SheetsConfig.SheetNames.Trips, sheetOrder[0]);      // First
        Assert.Equal(SheetsConfig.SheetNames.Shifts, sheetOrder[1]);     // Second
        Assert.Equal(SheetsConfig.SheetNames.Setup, sheetOrder[14]);     // Last
    }

    [Fact]
    public void SheetsConfig_ExplicitOrderArray_IsSynchronizedWithConstants()
    {
        // Act
        var validationErrors = SheetsConfig.SheetUtilities.ValidateSheetOrderCompleteness();

        // Assert
        Assert.Empty(validationErrors);
    }

    [Fact]
    public void ExplicitOrderArray_IsLibrarySafe()
    {
        // This test documents that we no longer use reflection for ordering
        
        // Act
        var sheetOrder1 = SheetsConfig.SheetUtilities.GetAllSheetNames();
        var sheetOrder2 = SheetsConfig.SheetUtilities.GetAllSheetNames();

        // Assert - Should be identical every time (no reflection variability)
        Assert.Equal(sheetOrder1.Count, sheetOrder2.Count);
        for (int i = 0; i < sheetOrder1.Count; i++)
        {
            Assert.Equal(sheetOrder1[i], sheetOrder2[i]);
        }

        // Should work in any compilation context
        Assert.Equal(SheetsConfig.SheetNames.Trips, sheetOrder1[0]); // Always first
        Assert.Equal(SheetsConfig.SheetNames.Setup, sheetOrder1.Last()); // Always last
    }
}