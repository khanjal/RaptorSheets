using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Enums;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Attributes;

public class TypedFieldAttributeTests
{
    [Fact]
    public void Constructor_WithFieldType_ShouldSetFieldType()
    {
        // Act
        var attribute = new TypedFieldAttribute(FieldType.Currency);

        // Assert
        Assert.Equal(FieldType.Currency, attribute.FieldType);
        Assert.Null(attribute.NumberFormatPattern);
        Assert.False(attribute.EnableValidation);
        Assert.Null(attribute.ValidationPattern);
    }

    [Fact]
    public void Constructor_WithFieldTypeAndFormatPattern_ShouldSetProperties()
    {
        // Act
        var attribute = new TypedFieldAttribute(FieldType.DateTime, "MM/DD/YYYY");

        // Assert
        Assert.Equal(FieldType.DateTime, attribute.FieldType);
        Assert.Equal("MM/DD/YYYY", attribute.NumberFormatPattern);
        Assert.False(attribute.EnableValidation);
        Assert.Null(attribute.ValidationPattern);
    }

    [Fact]
    public void Constructor_WithValidation_ShouldSetAllProperties()
    {
        // Act
        var attribute = new TypedFieldAttribute(FieldType.Number, "#,##0.00", true, "^\\d+$");

        // Assert
        Assert.Equal(FieldType.Number, attribute.FieldType);
        Assert.Equal("#,##0.00", attribute.NumberFormatPattern);
        Assert.True(attribute.EnableValidation);
        Assert.Equal("^\\d+$", attribute.ValidationPattern);
    }

    [Fact]
    public void AttributeUsage_ShouldBeConfiguredCorrectly()
    {
        // Arrange
        var attributeType = typeof(TypedFieldAttribute);

        // Act
        var attributeUsage = (AttributeUsageAttribute)Attribute.GetCustomAttribute(attributeType, typeof(AttributeUsageAttribute))!;

        // Assert
        Assert.NotNull(attributeUsage);
        Assert.Equal(AttributeTargets.Property, attributeUsage.ValidOn);
        Assert.False(attributeUsage.AllowMultiple);
    }

    [Fact]
    public void DefaultProperties_ShouldBeNullOrFalse()
    {
        // Act
        var attribute = new TypedFieldAttribute(FieldType.String);

        // Assert
        Assert.Null(attribute.NumberFormatPattern);
        Assert.False(attribute.EnableValidation);
        Assert.Null(attribute.ValidationPattern);
    }
}