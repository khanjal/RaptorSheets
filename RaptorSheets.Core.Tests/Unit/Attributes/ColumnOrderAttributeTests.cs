using RaptorSheets.Core.Attributes;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Attributes;

public class ColumnOrderAttributeTests
{
    [Fact]
    public void Constructor_WithValidHeaderName_ShouldSetHeaderName()
    {
        // Arrange
        const string headerName = "Test Header";

        // Act
        var attribute = new ColumnOrderAttribute(headerName);

        // Assert
        Assert.Equal(headerName, attribute.HeaderName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Constructor_WithEmptyOrWhitespaceHeaderName_ShouldStillWork(string headerName)
    {
        // Note: ColumnOrderAttribute constructor does not validate empty strings,
        // validation happens in the helper methods
        
        // Act
        var attribute = new ColumnOrderAttribute(headerName);

        // Assert
        Assert.Equal(headerName, attribute.HeaderName);
    }

    [Fact]
    public void Constructor_WithNullHeaderName_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new ColumnOrderAttribute(null!));
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
        var attribute = new ColumnOrderAttribute(headerName);

        // Assert
        Assert.Equal(headerName, attribute.HeaderName);
    }

    [Fact]
    public void AttributeUsage_ShouldBeConfiguredCorrectly()
    {
        // Arrange
        var attributeType = typeof(ColumnOrderAttribute);

        // Act
        var attributeUsageAttribute = (AttributeUsageAttribute)Attribute.GetCustomAttribute(attributeType, typeof(AttributeUsageAttribute))!;

        // Assert
        Assert.NotNull(attributeUsageAttribute);
        Assert.Equal(AttributeTargets.Property, attributeUsageAttribute.ValidOn);
        Assert.False(attributeUsageAttribute.AllowMultiple);
        Assert.True(attributeUsageAttribute.Inherited); // Default value
    }
}