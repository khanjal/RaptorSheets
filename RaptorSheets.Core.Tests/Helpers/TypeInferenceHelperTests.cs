using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Helpers;
using Xunit;

namespace RaptorSheets.Core.Tests.Helpers;

public class TypeInferenceHelperTests
{
    [Theory]
    [InlineData(typeof(string), FieldType.String)]
    [InlineData(typeof(int), FieldType.Integer)]
    [InlineData(typeof(int?), FieldType.Integer)]
    [InlineData(typeof(long), FieldType.Integer)]
    [InlineData(typeof(long?), FieldType.Integer)]
    [InlineData(typeof(short), FieldType.Integer)]
    [InlineData(typeof(byte), FieldType.Integer)]
    [InlineData(typeof(decimal), FieldType.Currency)]
    [InlineData(typeof(decimal?), FieldType.Currency)]
    [InlineData(typeof(double), FieldType.Number)]
    [InlineData(typeof(double?), FieldType.Number)]
    [InlineData(typeof(float), FieldType.Number)]
    [InlineData(typeof(bool), FieldType.Boolean)]
    [InlineData(typeof(bool?), FieldType.Boolean)]
    [InlineData(typeof(DateTime), FieldType.DateTime)]
    [InlineData(typeof(DateTime?), FieldType.DateTime)]
    public void InferFieldType_WithVariousTypes_ReturnsCorrectFieldType(Type propertyType, FieldType expected)
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
        Assert.Equal(FieldType.String, result);
    }

    [Theory]
    [InlineData(FieldType.Currency, FormatEnum.CURRENCY)]
    [InlineData(FieldType.Accounting, FormatEnum.ACCOUNTING)]
    [InlineData(FieldType.DateTime, FormatEnum.DATE)]
    [InlineData(FieldType.Time, FormatEnum.TIME)]
    [InlineData(FieldType.Duration, FormatEnum.DURATION)]
    [InlineData(FieldType.Number, FormatEnum.NUMBER)]
    [InlineData(FieldType.Percentage, FormatEnum.PERCENT)]
    [InlineData(FieldType.Integer, FormatEnum.NUMBER)]
    [InlineData(FieldType.Boolean, FormatEnum.TEXT)]
    [InlineData(FieldType.String, FormatEnum.TEXT)]
    public void GetDefaultFormatForFieldType_ReturnsCorrectFormat(FieldType fieldType, FormatEnum expected)
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
    [InlineData(typeof(string), FormatEnum.TIME, FieldType.Time)]
    [InlineData(typeof(string), FormatEnum.DURATION, FieldType.Duration)]
    [InlineData(typeof(string), FormatEnum.DATE, FieldType.DateTime)]
    [InlineData(typeof(string), FormatEnum.TEXT, FieldType.String)]
    [InlineData(typeof(string), FormatEnum.CURRENCY, FieldType.String)]
    [InlineData(typeof(decimal), FormatEnum.DATE, FieldType.Currency)]
    [InlineData(typeof(int), FormatEnum.TIME, FieldType.Integer)]
    public void InferFieldTypeFromFormat_WithStringAndSpecialFormat_ReturnsCorrectType(
        Type propertyType, FormatEnum format, FieldType expected)
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
        Assert.Equal(FieldType.String, baseType);
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
        Assert.Equal(FieldType.Currency, nullableResult);
    }
}
