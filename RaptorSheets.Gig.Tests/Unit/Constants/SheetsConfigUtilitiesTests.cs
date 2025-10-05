using RaptorSheets.Gig.Constants;

namespace RaptorSheets.Gig.Tests.Unit.Constants;

/// <summary>
/// Essential tests for the explicit array-based sheet ordering functionality
/// </summary>
public class SheetsConfigUtilitiesTests
{
    [Fact]
    public void GetAllSheetNames_ReturnsExpectedOrder()
    {
        // Act
        var sheetOrder = SheetsConfig.SheetUtilities.GetAllSheetNames();

        // Assert
        Assert.Equal(15, sheetOrder.Count);
        Assert.Equal(SheetsConfig.SheetNames.Trips, sheetOrder[0]);      // First
        Assert.Equal(SheetsConfig.SheetNames.Shifts, sheetOrder[1]);     // Second
        Assert.Equal(SheetsConfig.SheetNames.Setup, sheetOrder.Last());  // Last
    }

    [Fact]
    public void GetAllSheetNames_ReturnsNewCopyEachTime()
    {
        // Act
        var list1 = SheetsConfig.SheetUtilities.GetAllSheetNames();
        var list2 = SheetsConfig.SheetUtilities.GetAllSheetNames();

        // Assert
        Assert.NotSame(list1, list2); // Different instances
        Assert.Equal(list1, list2);   // Same content
    }

    [Fact]
    public void IsValidSheetName_WorksCorrectly()
    {
        // Assert - Valid cases
        Assert.True(SheetsConfig.SheetUtilities.IsValidSheetName("Trips"));
        Assert.True(SheetsConfig.SheetUtilities.IsValidSheetName("trips")); // Case insensitive
        Assert.True(SheetsConfig.SheetUtilities.IsValidSheetName("Setup"));
        
        // Invalid cases
        Assert.False(SheetsConfig.SheetUtilities.IsValidSheetName("NonExistent"));
        Assert.False(SheetsConfig.SheetUtilities.IsValidSheetName(""));
        Assert.False(SheetsConfig.SheetUtilities.IsValidSheetName(null!));
    }

    [Fact]
    public void GetSheetIndex_ReturnsCorrectPositions()
    {
        // Act & Assert
        Assert.Equal(0, SheetsConfig.SheetUtilities.GetSheetIndex("Trips"));
        Assert.Equal(0, SheetsConfig.SheetUtilities.GetSheetIndex("trips")); // Case insensitive
        Assert.Equal(14, SheetsConfig.SheetUtilities.GetSheetIndex("Setup"));
        Assert.Equal(-1, SheetsConfig.SheetUtilities.GetSheetIndex("NotFound"));
    }

    [Fact]
    public void ValidateSheetNames_WorksCorrectly()
    {
        // Arrange
        var validSheets = new[] { "Trips", "Shifts", "Expenses" };
        var mixedSheets = new[] { "Trips", "InvalidSheet", "Setup" };

        // Act
        var validErrors = SheetsConfig.SheetUtilities.ValidateSheetNames(validSheets);
        var invalidErrors = SheetsConfig.SheetUtilities.ValidateSheetNames(mixedSheets);

        // Assert
        Assert.Empty(validErrors);
        Assert.Single(invalidErrors);
        Assert.Contains("InvalidSheet", invalidErrors[0]);
    }
}