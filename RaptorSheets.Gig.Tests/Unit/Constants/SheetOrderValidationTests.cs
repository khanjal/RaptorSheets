using RaptorSheets.Gig.Constants;
using System.Reflection;

namespace RaptorSheets.Gig.Tests.Unit.Constants;

/// <summary>
/// Tests for the validation functionality that ensures the explicit array stays synchronized with constants
/// </summary>
public class SheetOrderValidationTests
{
    [Fact]
    public void ValidateSheetOrderCompleteness_WithCurrentConfiguration_PassesValidation()
    {
        // This is the most important test - ensures our current configuration is valid
        
        // Act
        var validationErrors = SheetsConfig.SheetUtilities.ValidateSheetOrderCompleteness();

        // Assert
        Assert.Empty(validationErrors);
        
        // If this fails, the explicit array needs to be updated to match the constants
        if (validationErrors.Any())
        {
            var errorDetails = string.Join("\n", validationErrors.Select(e => $"  - {e}"));
            Assert.Fail($"Sheet order synchronization issues found:\n{errorDetails}");
        }
    }

    [Fact]
    public void ValidateSheetOrderCompleteness_DetectsAllConstants()
    {
        // Arrange - Get all constants using reflection (for validation)
        var constantFields = typeof(SheetsConfig.SheetNames)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
            .ToList();

        var explicitOrder = SheetsConfig.SheetUtilities.GetAllSheetNames();

        // Assert - Should have same count and all constants should be included
        Assert.Equal(constantFields.Count, explicitOrder.Count);

        foreach (var field in constantFields)
        {
            var constantValue = field.GetValue(null)?.ToString();
            Assert.NotNull(constantValue);
            Assert.Contains(constantValue, explicitOrder);
        }
    }

    [Fact]
    public void ExplicitOrderArray_MaintainsBusinessLogic()
    {
        // Act
        var sheetOrder = SheetsConfig.SheetUtilities.GetAllSheetNames();

        // Assert - Primary data sheets should be first, admin sheets last
        var tripsIndex = sheetOrder.IndexOf("Trips");
        var shiftsIndex = sheetOrder.IndexOf("Shifts");
        var setupIndex = sheetOrder.IndexOf("Setup");

        Assert.True(tripsIndex < 5, "Trips should be early for easy access");
        Assert.True(shiftsIndex < 5, "Shifts should be early for easy access");
        Assert.Equal(sheetOrder.Count - 1, setupIndex); // Setup should be last
    }
}