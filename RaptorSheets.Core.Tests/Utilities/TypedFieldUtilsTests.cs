using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Utilities;
using Xunit;

namespace RaptorSheets.Core.Tests.Utilities;

public class TypedFieldUtilsTests
{
    [Fact]
    public void GetColumnProperties_ShouldReturnPropertiesWithColumnAttributes()
    {
        // Act
        var properties = TypedFieldUtils.GetColumnProperties<TestEntity>();
        
        // Assert
        Assert.NotEmpty(properties);
        Assert.All(properties, p => Assert.NotNull(p.Column));
        
        // Should include properties like TestDateTime, TestCurrency, etc.
        var dateProperty = properties.FirstOrDefault(p => p.Column.GetEffectiveHeaderName().Contains(TypedFieldUtilsTestHelper.TestDateTimeHeader));
        Assert.NotNull(dateProperty);
        Assert.Equal(FieldTypeEnum.DateTime, dateProperty.Column.FieldType);
    }

    [Fact]
    public void GetColumnProperties_ShouldReturnEmptyList_WhenNoColumnAttributes()
    {
        // Act
        var properties = TypedFieldUtils.GetColumnProperties<NoColumnEntity>();

        // Assert
        Assert.Empty(properties);
    }

    [Theory]
    [InlineData("InvalidData", typeof(int), FieldTypeEnum.Integer)]
    [InlineData("NotABoolean", typeof(bool), FieldTypeEnum.Boolean)]
    public void ConvertFromSheetValue_ShouldReturnDefaultValue_WhenDataIsInvalid(string input, Type targetType, FieldTypeEnum fieldType)
    {
        // Arrange
        var attribute = new ColumnAttribute("test", fieldType);

        // Act
        var result = TypedFieldUtils.ConvertFromSheetValue(input, targetType, attribute);

        // Assert
        // For value types, GetDefaultValue returns the default value (0 for int, false for bool)
        // not null, so we check for the default value instead
        if (targetType == typeof(int))
        {
            Assert.Equal(0, result);
        }
        else if (targetType == typeof(bool))
        {
            Assert.Equal(false, result);
        }
        else
        {
            Assert.Null(result);
        }
    }

    [Fact]
    public void ConvertToSheetValue_ShouldReturnNull_WhenInputIsNull()
    {
        // Arrange
        var attribute = new ColumnAttribute("test", FieldTypeEnum.String);

        // Act
        var result = TypedFieldUtils.ConvertToSheetValue(null, attribute);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetNumberFormatPattern_ShouldReturnDefaultPattern_WhenAttributeIsNull()
    {
        // Act
        var result = TypedFieldUtils.GetNumberFormatPattern(null);

        // Assert
        Assert.Equal("@", result); // Default to text format
    }

    // Test entity for validation
    private class TestEntity
    {
        [Column("TestString", FieldTypeEnum.String)]
        public string TestString { get; set; } = "";

        [Column("TestCurrency", FieldTypeEnum.Currency)]
        public decimal? TestCurrency { get; set; }

        [Column("TestDateTime", FieldTypeEnum.DateTime)]
        public DateTime? TestDateTime { get; set; }

        [Column("TestInteger", FieldTypeEnum.Integer)]
        public int? TestInteger { get; set; }

        [Column("TestBoolean", FieldTypeEnum.Boolean)]
        public bool TestBoolean { get; set; }

        // Property without Column attribute should be ignored
        public string IgnoredProperty { get; set; } = "";
    }

    private class NoColumnEntity
    {
        public string NoAttributeProperty { get; set; } = "";
    }
}

public static class TypedFieldUtilsTestHelper
{
    public const string TestDateTimeHeader = "TestDateTime";
    public const string TestCurrencyHeader = "TestCurrency";
    public const string TestIntegerHeader = "TestInteger";
    public const string TestBooleanHeader = "TestBoolean";
}