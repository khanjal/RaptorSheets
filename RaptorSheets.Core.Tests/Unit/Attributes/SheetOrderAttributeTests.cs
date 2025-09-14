using RaptorSheets.Core.Attributes;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Attributes;

public class SheetOrderAttributeTests
{
    [Fact]
    public void Constructor_WithValidHeaderName_ShouldSetHeaderName()
    {
        // Arrange
        const string headerName = "Test Header";

        // Act
        var attribute = new SheetOrderAttribute(headerName);

        // Assert
        Assert.Equal(headerName, attribute.HeaderName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Constructor_WithNullOrWhitespaceHeaderName_ShouldThrowArgumentException(string headerName)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new SheetOrderAttribute(headerName));
        Assert.Equal("Header name cannot be null or empty (Parameter 'headerName')", exception.Message);
        Assert.Equal("headerName", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullHeaderName_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new SheetOrderAttribute(null!));
        Assert.Equal("Header name cannot be null or empty (Parameter 'headerName')", exception.Message);
        Assert.Equal("headerName", exception.ParamName);
    }

    [Theory]
    [InlineData("Address")]
    [InlineData("Pay")]
    [InlineData("Total Grand")]
    [InlineData("Amount Per Time")]
    [InlineData("Visit First")]
    public void Constructor_WithVariousValidHeaderNames_ShouldSetHeaderName(string headerName)
    {
        // Act
        var attribute = new SheetOrderAttribute(headerName);

        // Assert
        Assert.Equal(headerName, attribute.HeaderName);
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