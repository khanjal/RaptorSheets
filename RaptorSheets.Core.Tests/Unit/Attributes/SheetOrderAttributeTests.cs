using RaptorSheets.Core.Attributes;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Attributes;

public class SheetOrderAttributeTests
{
    [Fact]
    public void Constructor_WithValidOrderAndSheetName_ShouldSetProperties()
    {
        // Arrange
        const int order = 5;
        const string sheetName = "Test Sheet";

        // Act
        var attribute = new SheetOrderAttribute(order, sheetName);

        // Assert
        Assert.Equal(order, attribute.Order);
        Assert.Equal(sheetName, attribute.SheetName);
    }

    [Theory]
    [InlineData(0, "Trips")]
    [InlineData(1, "Shifts")]
    [InlineData(10, "Setup")]
    public void Constructor_WithVariousValidValues_ShouldSetProperties(int order, string sheetName)
    {
        // Act
        var attribute = new SheetOrderAttribute(order, sheetName);

        // Assert
        Assert.Equal(order, attribute.Order);
        Assert.Equal(sheetName, attribute.SheetName);
    }

    [Fact]
    public void Constructor_WithNullSheetName_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new SheetOrderAttribute(0, null!));
        Assert.Equal("sheetName", exception.ParamName);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Constructor_WithNegativeOrder_ShouldStillWork(int order)
    {
        // Arrange
        const string sheetName = "Test Sheet";

        // Act
        var attribute = new SheetOrderAttribute(order, sheetName);

        // Assert
        Assert.Equal(order, attribute.Order);
        Assert.Equal(sheetName, attribute.SheetName);
    }

    [Fact]
    public void AttributeUsage_ShouldBeConfiguredCorrectly()
    {
        // Arrange
        var attributeType = typeof(SheetOrderAttribute);

        // Act
        var attributeUsageAttribute = (AttributeUsageAttribute)Attribute.GetCustomAttribute(attributeType, typeof(AttributeUsageAttribute))!;

        // Assert
        Assert.NotNull(attributeUsageAttribute);
        Assert.Equal(AttributeTargets.Property, attributeUsageAttribute.ValidOn);
        Assert.False(attributeUsageAttribute.AllowMultiple);
        Assert.True(attributeUsageAttribute.Inherited); // Default value
    }
}