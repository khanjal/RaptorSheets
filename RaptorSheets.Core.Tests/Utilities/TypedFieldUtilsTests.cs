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
        
        // Log retrieved headers for debugging
        var retrievedHeaders = properties.Select(p => p.Column.GetEffectiveHeaderName()).ToList();
        Assert.NotEmpty(retrievedHeaders);
        
        // Assert
        Assert.All(properties, p => Assert.NotNull(p.Column));
        
        // Should include properties like TestDateTime, TestCurrency, etc.
        var dateProperty = properties.FirstOrDefault(p => 
            p.Column.GetEffectiveHeaderName().Equals(TypedFieldUtilsTestHelper.TestDateTimeHeader, StringComparison.OrdinalIgnoreCase));
        
        Assert.Equal(FormatEnum.DATE, dateProperty.Column.FormatType);
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
    [InlineData("12/34/5678", typeof(DateTime?), FieldTypeEnum.DateTime)] // Invalid date - nullable
    [InlineData("", typeof(int), FieldTypeEnum.Integer)] // Empty string for int
    [InlineData(null, typeof(int), FieldTypeEnum.Integer)] // Null value for int
    [InlineData("NotANumber", typeof(double), FieldTypeEnum.Number)] // Invalid double
    [InlineData("", typeof(bool), FieldTypeEnum.Boolean)] // Empty string for bool
    [InlineData(null, typeof(bool), FieldTypeEnum.Boolean)] // Null value for bool
    [InlineData("InvalidDate", typeof(DateTime?), FieldTypeEnum.DateTime)] // Invalid date string - nullable
    [InlineData("", typeof(DateTime?), FieldTypeEnum.DateTime)] // Empty string for DateTime - nullable
    [InlineData(null, typeof(DateTime?), FieldTypeEnum.DateTime)] // Null value for DateTime - nullable
    [InlineData("NotACurrency", typeof(decimal?), FieldTypeEnum.Currency)] // Invalid decimal - nullable
    [InlineData("", typeof(decimal?), FieldTypeEnum.Currency)] // Empty string for decimal - nullable
    [InlineData(null, typeof(decimal?), FieldTypeEnum.Currency)] // Null value for decimal - nullable
    public void ConvertFromSheetValue_ShouldReturnDefaultValue_WhenDataIsInvalid(string? input, Type targetType, FieldTypeEnum fieldType)
    {
        // Arrange
        var attribute = new ColumnAttribute("test");
        attribute.SetFieldTypeFromProperty(targetType);

        // Act
        var result = TypedFieldUtils.ConvertFromSheetValue(input, targetType, attribute);

        // Assert
        // Check if it's a nullable type
        var underlyingType = Nullable.GetUnderlyingType(targetType);
        bool isNullable = underlyingType != null;
        var actualType = underlyingType ?? targetType;
        
        if (actualType == typeof(int))
        {
            if (isNullable)
                Assert.Null(result);
            else
                Assert.Equal(0, result);
        }
        else if (actualType == typeof(bool))
        {
            if (isNullable)
                Assert.Null(result);
            else
                Assert.False((bool?)result);
        }
        else if (actualType == typeof(DateTime))
        {
            if (isNullable)
                Assert.Null(result);
            else
                Assert.Equal(DateTime.MinValue, result);
        }
        else if (actualType == typeof(decimal))
        {
            if (isNullable)
                Assert.Null(result);
            else
                Assert.Equal(0.0m, result);
        }
        else if (actualType == typeof(double))
        {
            if (isNullable)
                Assert.Null(result);
            else
                Assert.Equal(0.0, result);
        }
        else
        {
            Assert.Null(result);
        }

        // Use the fieldType parameter to validate the expected field type
        Assert.Equal(fieldType, attribute.FieldType);
    }

    [Fact]
    public void ConvertFromSheetValue_ShouldParseCurrencyWithDollarSign()
    {
        // Arrange
        var attribute = new ColumnAttribute("test");
        attribute.SetFieldTypeFromProperty(typeof(decimal?));

        // Act
        var result = TypedFieldUtils.ConvertFromSheetValue("$12.34", typeof(decimal?), attribute);

        // Assert - Currency parsing should successfully remove the $ sign
        Assert.Equal(12.34m, result);
    }

    [Fact]
    public void ConvertToSheetValue_ShouldReturnNull_WhenInputIsNull()
    {
        // Arrange
        var attribute = new ColumnAttribute("test");

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
        [Column("TestString")]
        public string TestString { get; set; } = "";

        [Column("TestCurrency", formatType: FormatEnum.CURRENCY)]
        public decimal? TestCurrency { get; set; }

        [Column("TestDateTime", formatType: FormatEnum.DATE)]
        public DateTime? TestDateTime { get; set; }

        [Column("TestInteger", formatType: FormatEnum.NUMBER)]
        public int? TestInteger { get; set; }

        [Column("TestBoolean")]
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