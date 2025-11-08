using System.Text.Json;
using RaptorSheets.Gig.Entities;

namespace RaptorSheets.Gig.Tests.Unit.Entities;

public class SheetRowEntityBaseTests
{
    [Fact]
    public void TripEntity_ShouldSerializeBaseProperties_WithCorrectJsonNames()
    {
        // Arrange
        var trip = new TripEntity
        {
            RowId = 42,
            Action = "UPDATE",
            Saved = true,
            Date = "2024-01-15",
            Service = "DoorDash"
        };

        // Act
        var json = JsonSerializer.Serialize(trip);
        var deserialized = JsonSerializer.Deserialize<TripEntity>(json);

        // Assert
        Assert.Contains("\"rowId\":42", json);
        Assert.Contains("\"action\":\"UPDATE\"", json);
        Assert.Contains("\"saved\":true", json);
        Assert.NotNull(deserialized);
        Assert.Equal(42, deserialized.RowId);
        Assert.Equal("UPDATE", deserialized.Action);
        Assert.True(deserialized.Saved);
    }

    [Fact]
    public void ShiftEntity_ShouldSerializeBaseProperties_WithCorrectJsonNames()
    {
        // Arrange
        var shift = new ShiftEntity
        {
            RowId = 10,
            Action = "INSERT",
            Saved = false,
            Date = "2024-01-15",
            Service = "UberEats"
        };

        // Act
        var json = JsonSerializer.Serialize(shift);
        var deserialized = JsonSerializer.Deserialize<ShiftEntity>(json);

        // Assert
        Assert.Contains("\"rowId\":10", json);
        Assert.Contains("\"action\":\"INSERT\"", json);
        Assert.Contains("\"saved\":false", json);
        Assert.NotNull(deserialized);
        Assert.Equal(10, deserialized.RowId);
        Assert.Equal("INSERT", deserialized.Action);
        Assert.False(deserialized.Saved);
    }

    [Fact]
    public void ExpenseEntity_ShouldSerializeBaseProperties_WithCorrectJsonNames()
    {
        // Arrange
        var expense = new ExpenseEntity
        {
            RowId = 5,
            Action = "DELETE",
            Saved = true,
            Date = "2024-01-15",  // Changed to string to match entity definition
            Category = "Fuel"
        };

        // Act
        var json = JsonSerializer.Serialize(expense);
        var deserialized = JsonSerializer.Deserialize<ExpenseEntity>(json);

        // Assert
        Assert.Contains("\"rowId\":5", json);
        Assert.Contains("\"action\":\"DELETE\"", json);
        Assert.Contains("\"saved\":true", json);
        Assert.NotNull(deserialized);
        Assert.Equal(5, deserialized.RowId);
        Assert.Equal("DELETE", deserialized.Action);
        Assert.True(deserialized.Saved);
    }

    [Fact]
    public void BaseProperties_ShouldUse_CamelCaseJsonNames()
    {
        // Arrange
        var trip = new TripEntity
        {
            RowId = 1,
            Action = "TEST",
            Saved = false
        };

        // Act
        var json = JsonSerializer.Serialize(trip);

        // Assert - Verify camelCase (not PascalCase)
        Assert.DoesNotContain("\"RowId\"", json);
        Assert.DoesNotContain("\"Action\"", json);
        Assert.DoesNotContain("\"Saved\"", json);
        
        Assert.Contains("\"rowId\"", json);
        Assert.Contains("\"action\"", json);
        Assert.Contains("\"saved\"", json);
    }
}
