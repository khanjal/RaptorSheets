using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Tests.Data;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Helpers;

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
public class TestInvalidSheetOrderEntity
{
    [SheetOrder(0, "InvalidSheet")]
    public bool InvalidSheet { get; set; } = true;

    [SheetOrder(1, "Trips")]
    public bool Trips { get; set; } = true;
}

// Test entity with duplicate orders
public class TestDuplicateOrderEntity
{
    [SheetOrder(0, "Trips")]
    public bool Trips { get; set; } = true;

    [SheetOrder(0, "Shifts")] // Duplicate order
    public bool Shifts { get; set; } = true;
}

// Test entity with duplicate sheet names
public class TestDuplicateSheetEntity
{
    [SheetOrder(0, "Trips")]
    public bool Trips1 { get; set; } = true;

    [SheetOrder(1, "Trips")] // Duplicate sheet name
    public bool Trips2 { get; set; } = true;
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
}