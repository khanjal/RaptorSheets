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
    [InlineData("InvalidData", typeof(int))]
    [InlineData("NotABoolean", typeof(bool))]
    [InlineData("12/34/5678", typeof(DateTime))] // Invalid date
    [InlineData("$12.34", typeof(decimal))]   // Invalid currency
    public void ConvertFromSheetValue_ShouldReturnDefaultValue_WhenDataIsInvalid(string input, Type targetType)
    {
        // Arrange
        var attribute = new ColumnAttribute("test");

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
        else if (targetType == typeof(DateTime))
        {
            Assert.Equal(DateTime.MinValue, result);
        }
        else if (targetType == typeof(decimal))
        {
            Assert.Equal(0.0m, result);
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