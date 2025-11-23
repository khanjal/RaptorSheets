using System.Text.Json;
using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Serialization;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Serialization;

/// <summary>
/// Tests for Column attribute JSON serialization behavior.
/// Verifies that jsonPropertyName in Column attribute is correctly applied during JSON serialization.
/// </summary>
public class ColumnAttributeJsonSerializationTests
{
    private readonly JsonSerializerOptions _options;

    public ColumnAttributeJsonSerializationTests()
    {
        _options = JsonSerializerOptionsExtensions.CreateWithColumnAttributeNaming();
    }

    /// <summary>
    /// Test entity that simulates real-world scenarios where header constants differ from property names
    /// </summary>
    private class TestEntity
    {
        // Scenario 1: Symbol as header constant (like "#" for Number)
        [Column("#", isInput: true, jsonPropertyName: "number")]
        public int? Number { get; set; }

        // Scenario 2: Single letter as header constant (like "X" for Exclude)
        [Column("X", isInput: true, jsonPropertyName: "exclude")]
        public bool Exclude { get; set; }

        // Scenario 3: Abbreviation as header constant (like "Dist" for Distance)
        [Column("Dist", isInput: true, jsonPropertyName: "distance")]
        public decimal? Distance { get; set; }

        // Scenario 4: Property name doesn't match desired JSON name
        [Column("Pickup", isInput: true, jsonPropertyName: "pickupTime")]
        public string Pickup { get; set; } = "";

        // Scenario 5: Property name order doesn't match desired JSON name
        [Column("Odo Start", isInput: true, jsonPropertyName: "startOdometer")]
        public decimal? OdometerStart { get; set; }

        // Scenario 6: No explicit jsonPropertyName - should use header name converted to camelCase
        [Column("Service", isInput: true)]
        public string Service { get; set; } = "";

        // Scenario 7: Multi-word header without explicit jsonPropertyName
        [Column("Start Address", isInput: true)]
        public string StartAddress { get; set; } = "";

        // Scenario 8: Property name matches camelCase of header
        [Column("Date", isInput: true)]
        public string Date { get; set; } = "";

        // Scenario 9: Complex header with explicit jsonPropertyName override
        [Column("Order #", isInput: true, jsonPropertyName: "orderNumber")]
        public string OrderNumber { get; set; } = "";

        // Scenario 10: Format type with jsonPropertyName
        [Column("Amt/Hour", FormatEnum.ACCOUNTING, "amountPerTime")]
        public decimal? AmountPerTime { get; set; }
    }

    [Fact]
    public void ColumnAttribute_SymbolHeader_ShouldUseJsonPropertyName()
    {
        // Arrange - Header is "#" but jsonPropertyName is "number"
        var entity = new TestEntity { Number = 42 };

        // Act
        var json = JsonSerializer.Serialize(entity, _options);

        // Assert - Should use "number" not "#"
        Assert.Contains("\"number\":42", json);
        Assert.DoesNotContain("\"#\":", json);
    }

    [Fact]
    public void ColumnAttribute_SingleLetterHeader_ShouldUseJsonPropertyName()
    {
        // Arrange - Header is "X" but jsonPropertyName is "exclude"
        var entity = new TestEntity { Exclude = true };

        // Act
        var json = JsonSerializer.Serialize(entity, _options);

        // Assert - Should use "exclude" not "x" or "X"
        Assert.Contains("\"exclude\":true", json);
        Assert.DoesNotContain("\"x\":", json);
        Assert.DoesNotContain("\"X\":", json);
    }

    [Fact]
    public void ColumnAttribute_AbbreviationHeader_ShouldUseJsonPropertyName()
    {
        // Arrange - Header is "Dist" but jsonPropertyName is "distance"
        var entity = new TestEntity { Distance = 15.5m };

        // Act
        var json = JsonSerializer.Serialize(entity, _options);

        // Assert - Should use "distance" not "dist"
        Assert.Contains("\"distance\":15.5", json);
        Assert.DoesNotContain("\"dist\":", json);
    }

    [Fact]
    public void ColumnAttribute_PropertyNameOverride_ShouldUseJsonPropertyName()
    {
        // Arrange - Property is "Pickup" but jsonPropertyName is "pickupTime"
        var entity = new TestEntity { Pickup = "09:00 AM" };

        // Act
        var json = JsonSerializer.Serialize(entity, _options);

        // Assert - Should use "pickupTime" not "pickup"
        Assert.Contains("\"pickupTime\":\"09:00 AM\"", json);
        Assert.DoesNotContain("\"pickup\":", json);
    }

    [Fact]
    public void ColumnAttribute_ReorderedPropertyName_ShouldUseJsonPropertyName()
    {
        // Arrange - Property is "OdometerStart" but jsonPropertyName is "startOdometer"
        var entity = new TestEntity { OdometerStart = 12345.6m };

        // Act
        var json = JsonSerializer.Serialize(entity, _options);

        // Assert - Should use "startOdometer" not "odometerStart"
        Assert.Contains("\"startOdometer\":12345.6", json);
        Assert.DoesNotContain("\"odometerStart\":", json);
    }

    [Fact]
    public void ColumnAttribute_NoJsonPropertyName_ShouldUseCamelCaseHeaderName()
    {
        // Arrange - Header is "Service", no explicit jsonPropertyName
        var entity = new TestEntity { Service = "Test" };

        // Act
        var json = JsonSerializer.Serialize(entity, _options);

        // Assert - Should use camelCase of header "service"
        Assert.Contains("\"service\":\"Test\"", json);
    }

    [Fact]
    public void ColumnAttribute_MultiWordHeader_ShouldUseCamelCaseHeaderName()
    {
        // Arrange - Header is "Start Address", no explicit jsonPropertyName
        var entity = new TestEntity { StartAddress = "123 Main St" };

        // Act
        var json = JsonSerializer.Serialize(entity, _options);

        // Assert - Should use camelCase of header "startAddress"
        Assert.Contains("\"startAddress\":\"123 Main St\"", json);
    }

    [Fact]
    public void ColumnAttribute_PropertyMatchesHeader_ShouldUseCamelCaseHeaderName()
    {
        // Arrange - Property "Date" matches header "Date"
        var entity = new TestEntity { Date = "2024-01-15" };

        // Act
        var json = JsonSerializer.Serialize(entity, _options);

        // Assert - Should use camelCase of header "date"
        Assert.Contains("\"date\":\"2024-01-15\"", json);
    }

    [Fact]
    public void ColumnAttribute_SpecialCharacterHeader_ShouldUseJsonPropertyName()
    {
        // Arrange - Header is "Order #" but jsonPropertyName is "orderNumber"
        var entity = new TestEntity { OrderNumber = "ORD-123" };

        // Act
        var json = JsonSerializer.Serialize(entity, _options);

        // Assert - Should use "orderNumber" not "order#" or "order "
        Assert.Contains("\"orderNumber\":\"ORD-123\"", json);
        Assert.DoesNotContain("\"order\":", json);
    }

    [Fact]
    public void ColumnAttribute_FormatTypeWithJsonPropertyName_ShouldUseJsonPropertyName()
    {
        // Arrange - Complex header with format and jsonPropertyName
        var entity = new TestEntity { AmountPerTime = 25.50m };

        // Act
        var json = JsonSerializer.Serialize(entity, _options);

        // Assert - Should use "amountPerTime"
        Assert.Contains("\"amountPerTime\":25.5", json);
    }

    [Fact]
    public void ColumnAttribute_AllProperties_ShouldSerializeCorrectly()
    {
        // Arrange - All properties set
        var entity = new TestEntity
        {
            Number = 5,
            Exclude = true,
            Distance = 10.5m,
            Pickup = "09:00 AM",
            OdometerStart = 100.0m,
            Service = "TestService",
            StartAddress = "123 Main St",
            Date = "2024-01-15",
            OrderNumber = "ORD-001",
            AmountPerTime = 15.75m
        };

        // Act
        var json = JsonSerializer.Serialize(entity, _options);

        // Assert - All properties should use correct JSON names
        Assert.Contains("\"number\":5", json);
        Assert.Contains("\"exclude\":true", json);
        Assert.Contains("\"distance\":10.5", json);
        Assert.Contains("\"pickupTime\":\"09:00 AM\"", json);
        Assert.Contains("\"startOdometer\":100", json);
        Assert.Contains("\"service\":\"TestService\"", json);
        Assert.Contains("\"startAddress\":\"123 Main St\"", json);
        Assert.Contains("\"date\":\"2024-01-15\"", json);
        Assert.Contains("\"orderNumber\":\"ORD-001\"", json);
        Assert.Contains("\"amountPerTime\":15.75", json);

        // Should NOT contain incorrect names
        Assert.DoesNotContain("\"#\":", json);
        Assert.DoesNotContain("\"X\":", json);
        Assert.DoesNotContain("\"x\":", json);
        Assert.DoesNotContain("\"Dist\":", json);
        Assert.DoesNotContain("\"dist\":", json);
        Assert.DoesNotContain("\"pickup\":", json);
        Assert.DoesNotContain("\"odometerStart\":", json);
    }

    [Fact]
    public void ColumnAttribute_Deserialization_ShouldWorkWithJsonPropertyNames()
    {
        // Arrange
        var json = """
        {
            "number": 42,
            "exclude": true,
            "distance": 15.5,
            "pickupTime": "10:00 AM",
            "startOdometer": 5000.0,
            "service": "ServiceName",
            "startAddress": "456 Oak Ave",
            "date": "2024-02-20",
            "orderNumber": "ORD-999",
            "amountPerTime": 30.25
        }
        """;

        // Act
        var entity = JsonSerializer.Deserialize<TestEntity>(json, _options);

        // Assert
        Assert.NotNull(entity);
        Assert.Equal(42, entity.Number);
        Assert.True(entity.Exclude);
        Assert.Equal(15.5m, entity.Distance);
        Assert.Equal("10:00 AM", entity.Pickup);
        Assert.Equal(5000.0m, entity.OdometerStart);
        Assert.Equal("ServiceName", entity.Service);
        Assert.Equal("456 Oak Ave", entity.StartAddress);
        Assert.Equal("2024-02-20", entity.Date);
        Assert.Equal("ORD-999", entity.OrderNumber);
        Assert.Equal(30.25m, entity.AmountPerTime);
    }

    [Fact]
    public void ColumnAttribute_NullValues_ShouldSerializeCorrectly()
    {
        // Arrange - Entity with null nullable properties
        var entity = new TestEntity
        {
            Number = null,
            Distance = null,
            OdometerStart = null,
            AmountPerTime = null
        };

        // Act
        var json = JsonSerializer.Serialize(entity, _options);

        // Assert - Null values should serialize with correct property names
        Assert.Contains("\"number\":null", json);
        Assert.Contains("\"distance\":null", json);
        Assert.Contains("\"startOdometer\":null", json);
        Assert.Contains("\"amountPerTime\":null", json);
    }

    [Fact]
    public void ColumnAttribute_EdgeCase_EmptyStrings_ShouldSerializeCorrectly()
    {
        // Arrange - Entity with empty strings
        var entity = new TestEntity
        {
            Pickup = "",
            Service = "",
            StartAddress = "",
            Date = "",
            OrderNumber = ""
        };

        // Act
        var json = JsonSerializer.Serialize(entity, _options);

        // Assert - Empty strings should serialize with correct property names
        Assert.Contains("\"pickupTime\":\"\"", json);
        Assert.Contains("\"service\":\"\"", json);
        Assert.Contains("\"startAddress\":\"\"", json);
        Assert.Contains("\"date\":\"\"", json);
        Assert.Contains("\"orderNumber\":\"\"", json);
    }

    [Fact]
    public void ColumnAttribute_EdgeCase_ZeroValues_ShouldSerializeCorrectly()
    {
        // Arrange - Entity with zero values
        var entity = new TestEntity
        {
            Number = 0,
            Distance = 0m,
            OdometerStart = 0m,
            AmountPerTime = 0m
        };

        // Act
        var json = JsonSerializer.Serialize(entity, _options);

        // Assert - Zero values should serialize with correct property names
        Assert.Contains("\"number\":0", json);
        Assert.Contains("\"distance\":0", json);
        Assert.Contains("\"startOdometer\":0", json);
        Assert.Contains("\"amountPerTime\":0", json);
    }

    [Fact]
    public void ColumnAttribute_EdgeCase_NegativeValues_ShouldSerializeCorrectly()
    {
        // Arrange - Entity with negative values
        var entity = new TestEntity
        {
            Number = -5,
            Distance = -10.5m,
            OdometerStart = -100.0m,
            AmountPerTime = -25.75m
        };

        // Act
        var json = JsonSerializer.Serialize(entity, _options);

        // Assert - Negative values should serialize with correct property names
        Assert.Contains("\"number\":-5", json);
        Assert.Contains("\"distance\":-10.5", json);
        Assert.Contains("\"startOdometer\":-100", json);
        Assert.Contains("\"amountPerTime\":-25.75", json);
    }
}
