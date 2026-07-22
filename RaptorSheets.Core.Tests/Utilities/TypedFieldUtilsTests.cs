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
        
        Assert.Equal(Format.DATE, dateProperty.Column.FormatType);
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
    [InlineData("InvalidData", typeof(int), FieldType.Integer)]
    [InlineData("NotABoolean", typeof(bool), FieldType.Boolean)]
    [InlineData("12/34/5678", typeof(DateTime?), FieldType.DateTime)] // Invalid date - nullable
    [InlineData("", typeof(int), FieldType.Integer)] // Empty string for int
    [InlineData(null, typeof(int), FieldType.Integer)] // Null value for int
    [InlineData("NotANumber", typeof(double), FieldType.Number)] // Invalid double
    [InlineData("", typeof(bool), FieldType.Boolean)] // Empty string for bool
    [InlineData(null, typeof(bool), FieldType.Boolean)] // Null value for bool
    [InlineData("InvalidDate", typeof(DateTime?), FieldType.DateTime)] // Invalid date string - nullable
    [InlineData("", typeof(DateTime?), FieldType.DateTime)] // Empty string for DateTime - nullable
    [InlineData(null, typeof(DateTime?), FieldType.DateTime)] // Null value for DateTime - nullable
    [InlineData("NotACurrency", typeof(decimal?), FieldType.Number)] // Invalid decimal - nullable
    [InlineData("", typeof(decimal?), FieldType.Number)] // Empty string for decimal - nullable
    [InlineData(null, typeof(decimal?), FieldType.Number)] // Null value for decimal - nullable
    public void ConvertFromSheetValue_ShouldReturnDefaultValue_WhenDataIsInvalid(string? input, Type targetType, FieldType fieldType)
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

        if (isNullable)
        {
            Assert.Null(result);
        }
        else
        {
            Assert.Equal(GetExpectedNonNullableDefault(actualType), result);
        }

        // Use the fieldType parameter to validate the expected field type
        Assert.Equal(fieldType, attribute.FieldType);
    }

    private static object? GetExpectedNonNullableDefault(Type actualType)
    {
        if (actualType == typeof(int)) return 0;
        if (actualType == typeof(bool)) return false;
        if (actualType == typeof(DateTime)) return DateTime.MinValue;
        if (actualType == typeof(decimal)) return 0.0m;
        if (actualType == typeof(double)) return 0.0;
        return null;
    }

    [Fact]
    public void ConvertFromSheetValue_ShouldParseCurrencyWithDollarSign()
    {
        // Arrange
        var attribute = new ColumnAttribute("test");
        attribute.SetFieldTypeFromProperty(typeof(decimal));

        // Act
        var result = TypedFieldUtils.ConvertFromSheetValue("$12.34", typeof(decimal), attribute);

        // Assert - Currency parsing should successfully remove the $ sign and convert to decimal
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

        [Column("TestCurrency", formatType: Format.CURRENCY)]
        public decimal? TestCurrency { get; set; }

        [Column("TestDateTime", formatType: Format.DATE)]
        public DateTime? TestDateTime { get; set; }

        [Column("TestInteger", formatType: Format.NUMBER)]
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