using System.Reflection;
using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Enums;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Attributes;

public class SmartFieldAttributeTests
{
    private class TestEntity
    {
        [SmartField]
        public string Name { get; set; } = "";

        [SmartField(FieldTypeEnum.Currency)]
        public decimal Amount { get; set; }

        [SmartField("Custom Header")]
        public string CustomField { get; set; } = "";

        [SmartField(FieldTypeEnum.DateTime, "Date Header")]
        public DateTime DateField { get; set; }
    }

    [Fact]
    public void InferFieldType_ShouldReturnCorrectFieldType()
    {
        // Arrange
        var properties = typeof(TestEntity).GetProperties();

        // Act & Assert
        Assert.Equal(FieldTypeEnum.String, FieldConventions.InferFieldType(properties.First(p => p.Name == "Name")));
        Assert.Equal(FieldTypeEnum.Currency, FieldConventions.InferFieldType(properties.First(p => p.Name == "Amount")));
        Assert.Equal(FieldTypeEnum.String, FieldConventions.InferFieldType(properties.First(p => p.Name == "CustomField")));
        Assert.Equal(FieldTypeEnum.DateTime, FieldConventions.InferFieldType(properties.First(p => p.Name == "DateField")));
    }

    [Fact]
    public void InferHeaderName_ShouldReturnCorrectHeaderName()
    {
        // Arrange
        var properties = typeof(TestEntity).GetProperties();

        // Act & Assert
        Assert.Equal("Name", FieldConventions.InferHeaderName(properties.First(p => p.Name == "Name")));
        Assert.Equal("Amount", FieldConventions.InferHeaderName(properties.First(p => p.Name == "Amount")));
        Assert.Equal("Custom Header", FieldConventions.InferHeaderName(properties.First(p => p.Name == "CustomField")));
        Assert.Equal("Date Header", FieldConventions.InferHeaderName(properties.First(p => p.Name == "DateField")));
    }

    [Fact]
    public void InferJsonPropertyName_ShouldReturnCorrectJsonPropertyName()
    {
        // Arrange
        var properties = typeof(TestEntity).GetProperties();

        // Act & Assert
        Assert.Equal("name", FieldConventions.InferJsonPropertyName(properties.First(p => p.Name == "Name")));
        Assert.Equal("amount", FieldConventions.InferJsonPropertyName(properties.First(p => p.Name == "Amount")));
        Assert.Equal("customField", FieldConventions.InferJsonPropertyName(properties.First(p => p.Name == "CustomField")));
        Assert.Equal("dateField", FieldConventions.InferJsonPropertyName(properties.First(p => p.Name == "DateField")));
    }

    [Fact]
    public void SmartFieldAttribute_ShouldApplyCustomConfigurations()
    {
        // Arrange
        var properties = typeof(TestEntity).GetProperties();

        // Act
        var nameAttribute = properties.First(p => p.Name == "Name").GetCustomAttribute<SmartFieldAttribute>();
        var amountAttribute = properties.First(p => p.Name == "Amount").GetCustomAttribute<SmartFieldAttribute>();
        var customFieldAttribute = properties.First(p => p.Name == "CustomField").GetCustomAttribute<SmartFieldAttribute>();
        var dateFieldAttribute = properties.First(p => p.Name == "DateField").GetCustomAttribute<SmartFieldAttribute>();

        // Assert
        Assert.NotNull(nameAttribute);
        Assert.Null(nameAttribute.FieldType);
        Assert.Null(nameAttribute.HeaderName);

        Assert.NotNull(amountAttribute);
        Assert.Equal(FieldTypeEnum.Currency, amountAttribute.FieldType);
        Assert.Null(amountAttribute.HeaderName);

        Assert.NotNull(customFieldAttribute);
        Assert.Null(customFieldAttribute.FieldType);
        Assert.Equal("Custom Header", customFieldAttribute.HeaderName);

        Assert.NotNull(dateFieldAttribute);
        Assert.Equal(FieldTypeEnum.DateTime, dateFieldAttribute.FieldType);
        Assert.Equal("Date Header", dateFieldAttribute.HeaderName);
    }
}