using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Helpers;
using Xunit;

namespace RaptorSheets.Core.Tests.Helpers;

public class TypeInferenceHelperTests
{
    [Theory]
    [InlineData(typeof(string), FieldTypeEnum.String)]
    [InlineData(typeof(int), FieldTypeEnum.Integer)]
    [InlineData(typeof(int?), FieldTypeEnum.Integer)]
    [InlineData(typeof(long), FieldTypeEnum.Integer)]
    [InlineData(typeof(long?), FieldTypeEnum.Integer)]
    [InlineData(typeof(short), FieldTypeEnum.Integer)]
    [InlineData(typeof(byte), FieldTypeEnum.Integer)]
    [InlineData(typeof(decimal), FieldTypeEnum.Currency)]
    [InlineData(typeof(decimal?), FieldTypeEnum.Currency)]
    [InlineData(typeof(double), FieldTypeEnum.Number)]
    [InlineData(typeof(double?), FieldTypeEnum.Number)]
    [InlineData(typeof(float), FieldTypeEnum.Number)]
    [InlineData(typeof(bool), FieldTypeEnum.Boolean)]
    [InlineData(typeof(bool?), FieldTypeEnum.Boolean)]
    [InlineData(typeof(DateTime), FieldTypeEnum.DateTime)]
    [InlineData(typeof(DateTime?), FieldTypeEnum.DateTime)]
    public void InferFieldType_WithVariousTypes_ReturnsCorrectFieldType(Type propertyType, FieldTypeEnum expected)
    {
        // Act
        var result = TypeInferenceHelper.InferFieldType(propertyType);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void InferFieldType_WithUnknownType_ReturnsString()
    {
        // Arrange
        var unknownType = typeof(System.Guid);

        // Act
        var result = TypeInferenceHelper.InferFieldType(unknownType);

        // Assert
        Assert.Equal(FieldTypeEnum.String, result);
    }

    [Theory]
    [InlineData(FieldTypeEnum.Currency, FormatEnum.CURRENCY)]
    [InlineData(FieldTypeEnum.Accounting, FormatEnum.ACCOUNTING)]
    [InlineData(FieldTypeEnum.DateTime, FormatEnum.DATE)]
    [InlineData(FieldTypeEnum.Time, FormatEnum.TIME)]
    [InlineData(FieldTypeEnum.Duration, FormatEnum.DURATION)]
    [InlineData(FieldTypeEnum.Number, FormatEnum.NUMBER)]
    [InlineData(FieldTypeEnum.Percentage, FormatEnum.PERCENT)]
    [InlineData(FieldTypeEnum.Integer, FormatEnum.NUMBER)]
    [InlineData(FieldTypeEnum.Boolean, FormatEnum.TEXT)]
    [InlineData(FieldTypeEnum.String, FormatEnum.TEXT)]
    public void GetDefaultFormatForFieldType_ReturnsCorrectFormat(FieldTypeEnum fieldType, FormatEnum expected)
    {
        // Act
        var result = TypeInferenceHelper.GetDefaultFormatForFieldType(fieldType);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(FormatEnum.TIME, true)]
    [InlineData(FormatEnum.DURATION, true)]
    [InlineData(FormatEnum.DATE, true)]
    [InlineData(FormatEnum.CURRENCY, false)]
    [InlineData(FormatEnum.NUMBER, false)]
    [InlineData(FormatEnum.TEXT, false)]
    [InlineData(FormatEnum.ACCOUNTING, false)]
    public void RequiresSpecialConversion_ReturnsCorrectValue(FormatEnum format, bool expected)
    {
        // Act
        var result = TypeInferenceHelper.RequiresSpecialConversion(format);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(typeof(string), FormatEnum.TIME, FieldTypeEnum.Time)]
    [InlineData(typeof(string), FormatEnum.DURATION, FieldTypeEnum.Duration)]
    [InlineData(typeof(string), FormatEnum.DATE, FieldTypeEnum.DateTime)]
    [InlineData(typeof(string), FormatEnum.TEXT, FieldTypeEnum.String)]
    [InlineData(typeof(string), FormatEnum.CURRENCY, FieldTypeEnum.String)]
    [InlineData(typeof(decimal), FormatEnum.DATE, FieldTypeEnum.Currency)]
    [InlineData(typeof(int), FormatEnum.TIME, FieldTypeEnum.Integer)]
    public void InferFieldTypeFromFormat_WithStringAndSpecialFormat_ReturnsCorrectType(
        Type propertyType, FormatEnum format, FieldTypeEnum expected)
    {
        // Act
        var result = TypeInferenceHelper.InferFieldTypeFromFormat(propertyType, format);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetInferenceDescription_ReturnsDescriptiveString()
    {
        // Arrange
        var propertyType = typeof(decimal?);
        var format = FormatEnum.ACCOUNTING;

        // Act
        var result = TypeInferenceHelper.GetInferenceDescription(propertyType, format);

        // Assert
        Assert.Contains("Decimal", result);
        Assert.Contains("Currency", result);
        Assert.Contains("ACCOUNTING", result);
    }

    [Fact]
    public void GetInferenceDescription_WithDefaultFormat_ShowsInferredFormat()
    {
        // Arrange
        var propertyType = typeof(decimal);
        var format = FormatEnum.DEFAULT;

        // Act
        var result = TypeInferenceHelper.GetInferenceDescription(propertyType, format);

        // Assert
        Assert.Contains("Decimal", result);
        Assert.Contains("Currency", result);
        Assert.Contains("CURRENCY", result); // Should show the inferred format
    }

    [Theory]
    [InlineData(typeof(string), FormatEnum.TIME)]
    [InlineData(typeof(string), FormatEnum.DURATION)]
    [InlineData(typeof(string), FormatEnum.DATE)]
    public void InferFieldTypeFromFormat_StringWithTimeFormats_OverridesBaseType(Type propertyType, FormatEnum format)
    {
        // Act
        var baseType = TypeInferenceHelper.InferFieldType(propertyType);
        var overriddenType = TypeInferenceHelper.InferFieldTypeFromFormat(propertyType, format);

        // Assert
        Assert.Equal(FieldTypeEnum.String, baseType);
        Assert.NotEqual(baseType, overriddenType);
        Assert.True(TypeInferenceHelper.RequiresSpecialConversion(format));
    }

    [Fact]
    public void InferFieldType_HandlesNullableTypesCorrectly()
    {
        // Arrange
        var nullableDecimal = typeof(decimal?);
        var nonNullableDecimal = typeof(decimal);

        // Act
        var nullableResult = TypeInferenceHelper.InferFieldType(nullableDecimal);
        var nonNullableResult = TypeInferenceHelper.InferFieldType(nonNullableDecimal);

        // Assert
        Assert.Equal(nonNullableResult, nullableResult);
        Assert.Equal(FieldTypeEnum.Currency, nullableResult);
    }
}
