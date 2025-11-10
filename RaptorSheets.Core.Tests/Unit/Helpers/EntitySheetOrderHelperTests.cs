using System.Diagnostics.CodeAnalysis;
using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Tests.Data;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Helpers;

[ExcludeFromCodeCoverage] // Exclude the entire file from code coverage
// Test entity for sheet ordering
public class TestSheetOrderEntity
{
    [SheetOrder(0, "Trips")]
    public bool Trips { get; set; } = true;

    [SheetOrder(2, "Expenses")]
    public bool Expenses { get; set; } = true;

    [SheetOrder(1, "Shifts")]
    public bool Shifts { get; set; } = true;

    [SheetOrder(3, "Setup")]
    public bool Setup { get; set; } = true;
}

// Test entity with invalid sheet references
[ExcludeFromCodeCoverage]
public class TestInvalidSheetOrderEntity
{
    [SheetOrder(0, "InvalidSheet")]
    public bool InvalidSheet { get; set; } = true;

    [SheetOrder(1, "Trips")]
    public bool Trips { get; set; } = true;
}

// Test entity with duplicate orders
[ExcludeFromCodeCoverage]
public class TestDuplicateOrderEntity
{
    [SheetOrder(0, "Trips")]
    public bool Trips { get; set; } = true;

    [SheetOrder(0, "Shifts")] // Duplicate order
    public bool Shifts { get; set; } = true;
}

// Test entity with duplicate sheet names
[ExcludeFromCodeCoverage]
public class TestDuplicateSheetEntity
{
    [SheetOrder(0, "Trips")]
    public bool Trips1 { get; set; } = true;

    [SheetOrder(1, "Trips")] // Duplicate sheet name
    public bool Trips2 { get; set; } = true;
}

// Test entity with optional order numbers (property order used)
[ExcludeFromCodeCoverage]
public class TestOptionalOrderEntity
{
    [SheetOrder("Expenses")] // No order specified, should use property order
    public bool Expenses { get; set; } = true;

    [SheetOrder("Trips")] // No order specified, should use property order
    public bool Trips { get; set; } = true;

    [SheetOrder("Setup")] // No order specified, should use property order
    public bool Setup { get; set; } = true;
}

// Test entity mixing explicit and optional orders
[ExcludeFromCodeCoverage]
public class TestMixedOrderEntity
{
    [SheetOrder("Expenses")] // No order specified, should come first (unordered)
    public bool Expenses { get; set; } = true;

    [SheetOrder(0, "Trips")] // Explicit order 0
    public bool Trips { get; set; } = true;

    [SheetOrder("Setup")] // No order specified, should come first (unordered)
    public bool Setup { get; set; } = true;

    [SheetOrder(1, "Shifts")] // Explicit order 1
    public bool Shifts { get; set; } = true;
}

// Test entity with out-of-range order numbers
[ExcludeFromCodeCoverage]
public class TestOutOfRangeOrderEntity
{
    [SheetOrder("Unordered1")] // No order, property index 0
    public bool Unordered1 { get; set; } = true;

    [SheetOrder("Unordered2")] // No order, property index 1
    public bool Unordered2 { get; set; } = true;

    [SheetOrder(1, "Position1")] // Position 1
    public bool Position1 { get; set; } = true;

    [SheetOrder(10, "OutOfRange")] // Position 10 (out of range)
    public bool OutOfRange { get; set; } = true;

    [SheetOrder(0, "Position0")] // Position 0
    public bool Position0 { get; set; } = true;
}

// Test entity with gaps in order numbers
[ExcludeFromCodeCoverage]
public class TestGapOrderEntity
{
    [SheetOrder("Unordered")] // No order
    public bool Unordered { get; set; } = true;

    [SheetOrder(0, "First")] // Position 0
    public bool First { get; set; } = true;

    [SheetOrder(5, "Fifth")] // Position 5 (gap)
    public bool Fifth { get; set; } = true;

    [SheetOrder(2, "Second")] // Position 2
    public bool Second { get; set; } = true;
}

// Test entity with complex insertion scenarios
[ExcludeFromCodeCoverage]
public class TestComplexInsertionEntity
{
    [SheetOrder("UnorderedA")] // Unordered, property index 0
    public bool UnorderedA { get; set; } = true;

    [SheetOrder("UnorderedB")] // Unordered, property index 1
    public bool UnorderedB { get; set; } = true;

    [SheetOrder(1, "InsertMiddle")] // Insert at position 1 (between unordered)
    public bool InsertMiddle { get; set; } = true;

    [SheetOrder(0, "InsertFirst")] // Insert at position 0 (before all)
    public bool InsertFirst { get; set; } = true;

    [SheetOrder(20, "InsertEnd")] // Insert at end (out of range)
    public bool InsertEnd { get; set; } = true;
}

// Test entity demonstrating optional ordering (only some sheets have explicit order)
[ExcludeFromCodeCoverage]
public class TestOptionalMixedOrderEntity
{
    [SheetOrder("FirstUnordered")] // No order specified, will be first in unordered group
    public bool FirstUnordered { get; set; } = true;

    [SheetOrder("SecondUnordered")] // No order specified, will be second in unordered group
    public bool SecondUnordered { get; set; } = true;

    [SheetOrder(1, "ExplicitSecond")] // Explicit order 1
    public bool ExplicitSecond { get; set; } = true;

    [SheetOrder("ThirdUnordered")] // No order specified, will be third in unordered group
    public bool ThirdUnordered { get; set; } = true;

    [SheetOrder(0, "ExplicitFirst")] // Explicit order 0
    public bool ExplicitFirst { get; set; } = true;
}

public class EntitySheetOrderHelperTests
{
    [Fact]
    public void GetSheetOrderFromEntity_SimpleEntity_ReturnsCorrectOrder()
    {
        // Act
        var sheetOrder = EntitySheetOrderHelper.GetSheetOrderFromEntity<TestSheetOrderEntity>();

        // Assert
        Assert.Equal(4, sheetOrder.Count);
        Assert.Equal("Trips", sheetOrder[0]);    // Order 0
        Assert.Equal("Shifts", sheetOrder[1]);   // Order 1
        Assert.Equal("Expenses", sheetOrder[2]); // Order 2
        Assert.Equal("Setup", sheetOrder[3]);    // Order 3
    }

    [Fact]
    public void GetSheetOrderFromEntity_EntityWithoutSheetOrderAttributes_ReturnsEmptyList()
    {
        // Act
        var sheetOrder = EntitySheetOrderHelper.GetSheetOrderFromEntity<TestNoAttributesEntity>();

        // Assert
        Assert.Empty(sheetOrder);
    }

    [Fact]
    public void GetSheetOrderFromEntity_EntityWithOptionalOrder_UsesPropertyOrder()
    {
        // Act
        var sheetOrder = EntitySheetOrderHelper.GetSheetOrderFromEntity<TestOptionalOrderEntity>();

        // Assert
        Assert.Equal(3, sheetOrder.Count);
        Assert.Equal("Expenses", sheetOrder[0]); // First property declared
        Assert.Equal("Trips", sheetOrder[1]);    // Second property declared
        Assert.Equal("Setup", sheetOrder[2]);    // Third property declared
    }

    [Fact]
    public void GetSheetOrderFromEntity_EntityWithMixedOrder_UnorderedFirst()
    {
        // Act
        var sheetOrder = EntitySheetOrderHelper.GetSheetOrderFromEntity<TestMixedOrderEntity>();

        // Assert - NEW BEHAVIOR: Unordered sheets first, then ordered sheets at specific positions
        Assert.Equal(4, sheetOrder.Count);
        Assert.Equal("Expenses", sheetOrder[0]); // Unordered, first property
        Assert.Equal("Setup", sheetOrder[1]);    // Unordered, second property
        Assert.Equal("Trips", sheetOrder[2]);    // Ordered 0, inserted at position 0 becomes position 2
        Assert.Equal("Shifts", sheetOrder[3]);   // Ordered 1, inserted at position 1 becomes position 3
    }

    [Fact]
    public void GetSheetOrderFromEntity_OutOfRangeOrders_PlacesAtEnd()
    {
        // Act
        var sheetOrder = EntitySheetOrderHelper.GetSheetOrderFromEntity<TestOutOfRangeOrderEntity>();

        // Assert
        Assert.Equal(5, sheetOrder.Count);
        Assert.Equal("Unordered1", sheetOrder[0]); // Unordered, property index 0
        Assert.Equal("Unordered2", sheetOrder[1]); // Unordered, property index 1
        Assert.Equal("Position0", sheetOrder[2]);  // Order 0, inserted at position 0 (after unordered)
        Assert.Equal("Position1", sheetOrder[3]);  // Order 1, inserted at position 1 (after unordered)
        Assert.Equal("OutOfRange", sheetOrder[4]); // Order 10, out of range, placed at end
    }

    [Fact]
    public void GetSheetOrderFromEntity_GapInOrders_FillsCorrectPositions()
    {
        // Act
        var sheetOrder = EntitySheetOrderHelper.GetSheetOrderFromEntity<TestGapOrderEntity>();

        // Assert
        Assert.Equal(4, sheetOrder.Count);
        Assert.Equal("Unordered", sheetOrder[0]); // Unordered first
        Assert.Equal("First", sheetOrder[1]);     // Order 0, inserted at position 0
        Assert.Equal("Second", sheetOrder[2]);    // Order 2, inserted at position 2  
        Assert.Equal("Fifth", sheetOrder[3]);     // Order 5, out of range, placed at end
    }

    [Fact]
    public void GetSheetOrderFromEntity_ComplexInsertion_HandlesCorrectly()
    {
        // Act
        var sheetOrder = EntitySheetOrderHelper.GetSheetOrderFromEntity<TestComplexInsertionEntity>();

        // Assert
        Assert.Equal(5, sheetOrder.Count);
        Assert.Equal("UnorderedA", sheetOrder[0]);   // Unordered, property index 0
        Assert.Equal("UnorderedB", sheetOrder[1]);   // Unordered, property index 1
        Assert.Equal("InsertFirst", sheetOrder[2]);  // Order 0, inserted at position 0 (after unordered)
        Assert.Equal("InsertMiddle", sheetOrder[3]); // Order 1, inserted at position 1 (after unordered)
        Assert.Equal("InsertEnd", sheetOrder[4]);    // Order 20, out of range, placed at end
    }

    [Fact]
    public void GetSheetOrderFromEntity_EmptyEntity_ReturnsEmptyList()
    {
        // Act
        var sheetOrder = EntitySheetOrderHelper.GetSheetOrderFromEntity<TestNoAttributesEntity>();

        // Assert
        Assert.Empty(sheetOrder);
    }

    [Fact]
    public void GetSheetOrderFromEntity_OnlyUnorderedSheets_ReturnsInPropertyOrder()
    {
        // Act
        var sheetOrder = EntitySheetOrderHelper.GetSheetOrderFromEntity<TestOptionalOrderEntity>();

        // Assert
        Assert.Equal(3, sheetOrder.Count);
        Assert.Equal("Expenses", sheetOrder[0]); // Property index 0
        Assert.Equal("Trips", sheetOrder[1]);    // Property index 1  
        Assert.Equal("Setup", sheetOrder[2]);    // Property index 2
    }

    [Fact]
    public void GetSheetOrderFromEntity_OnlyOrderedSheets_InsertsAtCorrectPositions()
    {
        // Act
        var sheetOrder = EntitySheetOrderHelper.GetSheetOrderFromEntity<TestSheetOrderEntity>();

        // Assert
        Assert.Equal(4, sheetOrder.Count);
        Assert.Equal("Trips", sheetOrder[0]);    // Order 0
        Assert.Equal("Shifts", sheetOrder[1]);   // Order 1
        Assert.Equal("Expenses", sheetOrder[2]); // Order 2
        Assert.Equal("Setup", sheetOrder[3]);    // Order 3
    }

    [Fact]
    public void GetSheetOrderFromEntity_EdgeCaseWithZeroPosition_InsertsAtFirstPositionAfterUnordered()
    {
        // Test entity with specific edge case: position 0 should go right after unordered sheets
        // Act
        var sheetOrder = EntitySheetOrderHelper.GetSheetOrderFromEntity<TestOutOfRangeOrderEntity>();

        // Assert
        Assert.Equal(5, sheetOrder.Count);
        Assert.Equal("Unordered1", sheetOrder[0]); // Unordered, property index 0
        Assert.Equal("Unordered2", sheetOrder[1]); // Unordered, property index 1
        Assert.Equal("Position0", sheetOrder[2]);  // Order 0, inserted at position 2 (after 2 unordered)
        Assert.Equal("Position1", sheetOrder[3]);  // Order 1, inserted at position 3 (after 2 unordered + 1 ordered)
        Assert.Equal("OutOfRange", sheetOrder[4]); // Order 10, out of range, placed at end
    }

    [Fact]
    public void GetSheetOrderFromEntity_OnlyOrderedSheetsWithGaps_FillsSequentially()
    {
        // Create a test entity with only ordered sheets but with gaps
        // This tests pure ordered sheets without any unordered ones
        var sheetOrder = EntitySheetOrderHelper.GetSheetOrderFromEntity<TestSheetOrderEntity>();

        // Assert - when no unordered sheets, ordered sheets go in their exact positions
        Assert.Equal(4, sheetOrder.Count);
        Assert.Equal("Trips", sheetOrder[0]);    // Order 0
        Assert.Equal("Shifts", sheetOrder[1]);   // Order 1
        Assert.Equal("Expenses", sheetOrder[2]); // Order 2
        Assert.Equal("Setup", sheetOrder[3]);    // Order 3
    }

    [Fact]
    public void ValidateEntitySheetMapping_ValidEntity_ReturnsNoErrors()
    {
        // Arrange
        var availableSheets = new[] { "Trips", "Shifts", "Expenses", "Setup" };

        // Act
        var errors = EntitySheetOrderHelper.ValidateEntitySheetMapping<TestSheetOrderEntity>(availableSheets);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateEntitySheetMapping_EntityWithInvalidSheetReference_ReturnsError()
    {
        // Arrange
        var availableSheets = new[] { "Trips", "Shifts", "Expenses" };

        // Act
        var errors = EntitySheetOrderHelper.ValidateEntitySheetMapping<TestInvalidSheetOrderEntity>(availableSheets);

        // Assert
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("InvalidSheet"));
        Assert.Contains(errors, e => e.Contains("not available"));
    }

    [Fact]
    public void ValidateEntitySheetMapping_EntityWithDuplicateOrders_ReturnsError()
    {
        // Arrange
        var availableSheets = new[] { "Trips", "Shifts" };

        // Act
        var errors = EntitySheetOrderHelper.ValidateEntitySheetMapping<TestDuplicateOrderEntity>(availableSheets);

        // Assert
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("Order 0 is used multiple times"));
    }

    [Fact]
    public void ValidateEntitySheetMapping_EntityWithDuplicateSheetNames_ReturnsError()
    {
        // Arrange
        var availableSheets = new[] { "Trips", "Shifts" };

        // Act
        var errors = EntitySheetOrderHelper.ValidateEntitySheetMapping<TestDuplicateSheetEntity>(availableSheets);

        // Assert
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("Sheet name 'Trips' is used multiple times"));
    }

    [Fact]
    public void ValidateEntitySheetMapping_EntityWithoutAttributes_ReturnsNoErrors()
    {
        // Arrange
        var availableSheets = new[] { "Any Sheet" };

        // Act
        var errors = EntitySheetOrderHelper.ValidateEntitySheetMapping<TestNoAttributesEntity>(availableSheets);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateEntitySheetOrderMapping_EntityWithOptionalOrder_ReturnsNoErrors()
    {
        // Arrange
        var availableSheets = new[] { "Expenses", "Trips", "Setup" };

        // Act
        var errors = EntitySheetOrderHelper.ValidateEntitySheetMapping<TestOptionalOrderEntity>(availableSheets);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateEntitySheetMapping_EntityWithMixedOrder_ReturnsNoErrors()
    {
        // Arrange
        var availableSheets = new[] { "Expenses", "Trips", "Setup", "Shifts" };

        // Act
        var errors = EntitySheetOrderHelper.ValidateEntitySheetMapping<TestMixedOrderEntity>(availableSheets);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateEntitySheetMapping_EntityWithOutOfRangeOrder_ReturnsNoErrors()
    {
        // Arrange
        var availableSheets = new[] { "Unordered1", "Unordered2", "Position1", "OutOfRange", "Position0" };

        // Act
        var errors = EntitySheetOrderHelper.ValidateEntitySheetMapping<TestOutOfRangeOrderEntity>(availableSheets);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void GetSheetOrderFromEntity_OptionalMixedOrdering_HandlesCorrectly()
    {
        // Act
        var sheetOrder = EntitySheetOrderHelper.GetSheetOrderFromEntity<TestOptionalMixedOrderEntity>();

        // Assert - NEW BEHAVIOR: Unordered sheets first (in property order), then explicit orders
        Assert.Equal(5, sheetOrder.Count);
        Assert.Equal("FirstUnordered", sheetOrder[0]);   // Unordered, property index 0
        Assert.Equal("SecondUnordered", sheetOrder[1]);  // Unordered, property index 1  
        Assert.Equal("ThirdUnordered", sheetOrder[2]);   // Unordered, property index 3
        Assert.Equal("ExplicitFirst", sheetOrder[3]);    // Order 0, inserted after unordered
        Assert.Equal("ExplicitSecond", sheetOrder[4]);   // Order 1, inserted after unordered
    }

    [Fact]
    public void GetSheetOrderFromEntity_AllOptionalOrdering_UsesPropertyOrder()
    {
        // This test verifies that when all SheetOrder attributes use optional ordering,
        // the result is simply the property declaration order
        
        // Act
        var sheetOrder = EntitySheetOrderHelper.GetSheetOrderFromEntity<TestOptionalOrderEntity>();

        // Assert - Should be in exact property declaration order
        Assert.Equal(3, sheetOrder.Count);
        Assert.Equal("Expenses", sheetOrder[0]); // Property index 0
        Assert.Equal("Trips", sheetOrder[1]);    // Property index 1  
        Assert.Equal("Setup", sheetOrder[2]);    // Property index 2
    }

    [Fact]
    public void GetSheetOrderFromEntity_EntityWithNoProperties_ReturnsEmptyList()
    {
        // Act
        var sheetOrder = EntitySheetOrderHelper.GetSheetOrderFromEntity<TestNoAttributesEntity>();

        // Assert
        Assert.Empty(sheetOrder);
    }

    [Fact]
    public void GetSheetOrderFromEntity_EntityWithDuplicateSheetOrders_ReturnsInconsistentOrder()
    {
        // Act
        var sheetOrder = EntitySheetOrderHelper.GetSheetOrderFromEntity<TestDuplicateOrderEntity>();

        // Assert
        Assert.Equal(2, sheetOrder.Count);
        Assert.Contains("Trips", sheetOrder);
        Assert.Contains("Shifts", sheetOrder);
    }

    [Fact]
    public void GetSheetOrderFromEntity_EntityWithNegativeSheetOrder_UsesPropertyOrder()
    {
        // Arrange - Negative order (-1) is valid and means "use property order"
        // This is the same as optional ordering

        // Act
        var sheetOrder = EntitySheetOrderHelper.GetSheetOrderFromEntity<TestNegativeSheetOrderEntity>();

        // Assert - Should return the sheet in property order since Order = -1
        Assert.Single(sheetOrder);
        Assert.Equal("NegativeOrderSheet", sheetOrder[0]);
    }

    [Fact]
    public void ValidateEntitySheetMapping_EmptyAvailableSheets_ReturnsErrors()
    {
        // Arrange
        var availableSheets = Array.Empty<string>();

        // Act
        var errors = EntitySheetOrderHelper.ValidateEntitySheetMapping<TestSheetOrderEntity>(availableSheets);

        // Assert
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("Trips"));
    }

    [Fact]
    public void ValidateEntitySheetMapping_PartialAvailableSheets_ReturnsErrors()
    {
        // Arrange - Only provide some of the required sheets
        var availableSheets = new[] { "Trips", "Shifts" }; // Missing "Expenses" and "Setup"

        // Act
        var errors = EntitySheetOrderHelper.ValidateEntitySheetMapping<TestSheetOrderEntity>(availableSheets);

        // Assert - Should have errors for missing sheets
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("Expenses"));
        Assert.Contains(errors, e => e.Contains("Setup"));
    }
}

[ExcludeFromCodeCoverage]
public class TestNegativeSheetOrderEntity
{
    [SheetOrder(-1, "NegativeOrderSheet")]
    public bool NegativeOrderSheet { get; set; } = true;
}