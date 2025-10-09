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
        var dateProperty = properties.FirstOrDefault(p => p.Column.GetEffectiveHeaderName().Contains("TestDateTime"));
        Assert.NotNull(dateProperty);
        Assert.Equal(FieldTypeEnum.DateTime, dateProperty.Column.FieldType);
    }

    [Theory]
    [InlineData("123.45", typeof(decimal), FieldTypeEnum.Currency, 123.45)]
    [InlineData("$123.45", typeof(decimal), FieldTypeEnum.Currency, 123.45)]
    [InlineData("1,234.56", typeof(decimal), FieldTypeEnum.Currency, 1234.56)]
    [InlineData("true", typeof(bool), FieldTypeEnum.Boolean, true)]
    [InlineData("false", typeof(bool), FieldTypeEnum.Boolean, false)]
    [InlineData("42", typeof(int), FieldTypeEnum.Integer, 42)]
    [InlineData("3.14159", typeof(double), FieldTypeEnum.Number, 3.14159)]
    public void ConvertFromSheetValue_ShouldConvertCorrectly(string input, Type targetType, FieldTypeEnum fieldType, object expected)
    {
        // Arrange
        var attribute = new ColumnAttribute("test", fieldType);
        
        // Act
        var result = TypedFieldUtils.ConvertFromSheetValue(input, targetType, attribute);
        
        // Assert
        Assert.NotNull(result);
        
        // Handle decimal comparison specifically
        if (expected is double expectedDouble && result is decimal resultDecimal)
        {
            Assert.Equal(expectedDouble, (double)resultDecimal, 5); // 5 decimal places precision
        }
        else
        {
            Assert.Equal(expected, result);
        }
    }

    [Theory]
    [InlineData(null, typeof(decimal?), FieldTypeEnum.Currency, null)]
    [InlineData("", typeof(string), FieldTypeEnum.String, "")]
    [InlineData("   ", typeof(string), FieldTypeEnum.String, "   ")]
    public void ConvertFromSheetValue_ShouldHandleNullAndEmpty(object input, Type targetType, FieldTypeEnum fieldType, object expected)
    {
        // Arrange
        var attribute = new ColumnAttribute("test", fieldType);
        
        // Act
        var result = TypedFieldUtils.ConvertFromSheetValue(input, targetType, attribute);
        
        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ConvertFromSheetValue_DateTimeSerial_ShouldConvertCorrectly()
    {
        // Arrange - Google Sheets serial number for 2024-01-01
        var serialNumber = 45292.0; // Approximate serial number for 2024-01-01
        var attribute = new ColumnAttribute("test", FieldTypeEnum.DateTime);
        
        // Act
        var result = TypedFieldUtils.ConvertFromSheetValue(serialNumber, typeof(DateTime), attribute);
        
        // Assert
        Assert.NotNull(result);
        Assert.IsType<DateTime>(result);
        var dateResult = (DateTime)result;
        Assert.Equal(2024, dateResult.Year);
        Assert.Equal(1, dateResult.Month);
        Assert.Equal(1, dateResult.Day);
    }

    [Theory]
    [InlineData("5551234567", typeof(long), FieldTypeEnum.PhoneNumber, 5551234567L)]
    [InlineData("15551234567", typeof(long), FieldTypeEnum.PhoneNumber, 5551234567L)] // Remove US country code
    [InlineData("(555) 123-4567", typeof(long), FieldTypeEnum.PhoneNumber, 5551234567L)]
    public void ConvertFromSheetValue_PhoneNumber_ShouldConvertCorrectly(string input, Type targetType, FieldTypeEnum fieldType, long expected)
    {
        // Arrange
        var attribute = new ColumnAttribute("test", fieldType);
        
        // Act
        var result = TypedFieldUtils.ConvertFromSheetValue(input, targetType, attribute);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(123.45, FieldTypeEnum.Currency, 123.45)]
    [InlineData(5551234567L, FieldTypeEnum.PhoneNumber, 5551234567L)]
    [InlineData(true, FieldTypeEnum.Boolean, true)]
    [InlineData("Hello", FieldTypeEnum.String, "Hello")]
    public void ConvertToSheetValue_ShouldConvertCorrectly(object input, FieldTypeEnum fieldType, object expected)
    {
        // Arrange
        var attribute = new ColumnAttribute("test", fieldType);
        
        // Act
        var result = TypedFieldUtils.ConvertToSheetValue(input, attribute);
        
        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ConvertToSheetValue_DateTime_ShouldConvertToSerial()
    {
        // Arrange
        var dateTime = new DateTime(2024, 1, 1);
        var attribute = new ColumnAttribute("test", FieldTypeEnum.DateTime);
        
        // Act
        var result = TypedFieldUtils.ConvertToSheetValue(dateTime, attribute);
        
        // Assert
        Assert.NotNull(result);
        Assert.IsType<double>(result);
        var serialResult = (double)result;
        Assert.True(serialResult > 45000); // Should be a reasonable serial number for 2024
    }

    [Theory]
    [InlineData(FieldTypeEnum.Currency, FormatEnum.CURRENCY)]
    [InlineData(FieldTypeEnum.DateTime, FormatEnum.DATE)]
    [InlineData(FieldTypeEnum.Number, FormatEnum.NUMBER)]
    [InlineData(FieldTypeEnum.Integer, FormatEnum.NUMBER)]
    [InlineData(FieldTypeEnum.Percentage, FormatEnum.PERCENT)]
    [InlineData(FieldTypeEnum.String, FormatEnum.TEXT)]
    [InlineData(null, null)]
    public void GetFormatFromFieldType_ShouldReturnCorrectFormat(FieldTypeEnum? fieldType, FormatEnum? expected)
    {
        // Act
        var result = TypedFieldUtils.GetFormatFromFieldType(fieldType);
        
        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(FieldTypeEnum.Currency, "\"$\"#,##0.00")]
    [InlineData(FieldTypeEnum.DateTime, "M/d/yyyy H:mm:ss")]
    [InlineData(FieldTypeEnum.Number, "#,##0.00")]
    [InlineData(FieldTypeEnum.Integer, "0")]
    [InlineData(FieldTypeEnum.String, "@")]
    public void GetNumberFormatPattern_ShouldReturnDefaultPattern(FieldTypeEnum fieldType, string expected)
    {
        // Arrange
        var attribute = new ColumnAttribute("test", fieldType);
        
        // Act
        var result = TypedFieldUtils.GetNumberFormatPattern(attribute);
        
        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetNumberFormatPattern_ShouldReturnCustomPattern()
    {
        // Arrange
        var customPattern = "\"£\"#,##0.00";
        var attribute = new ColumnAttribute("TestCurrency", FieldTypeEnum.Currency, customPattern);
        
        // Act
        var result = TypedFieldUtils.GetNumberFormatPattern(attribute);
        
        // Assert
        Assert.Equal(customPattern, result);
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

    [Fact]
    public void GetColumnProperties_TestEntity_ShouldReturnOnlyColumnProperties()
    {
        // Act
        var properties = TypedFieldUtils.GetColumnProperties<TestEntity>();
        
        // Assert
        Assert.Equal(5, properties.Count);
        Assert.All(properties, p => Assert.NotNull(p.Column));
        
        // Should not include IgnoredProperty
        Assert.DoesNotContain(properties, p => p.Property.Name == "IgnoredProperty");
    }
}