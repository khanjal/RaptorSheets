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
    [InlineData(typeof(decimal), FieldType.Number)]
    [InlineData(typeof(decimal?), FieldType.Number)]
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
    [InlineData(FieldType.Currency, Format.CURRENCY)]
    [InlineData(FieldType.Accounting, Format.ACCOUNTING)]
    [InlineData(FieldType.DateTime, Format.DATE)]
    [InlineData(FieldType.Time, Format.TIME)]
    [InlineData(FieldType.Duration, Format.DURATION)]
    [InlineData(FieldType.Number, Format.NUMBER)]
    [InlineData(FieldType.Percentage, Format.PERCENT)]
    [InlineData(FieldType.Integer, Format.NUMBER)]
    [InlineData(FieldType.Boolean, Format.TEXT)]
    [InlineData(FieldType.String, Format.TEXT)]
    [InlineData(FieldType.Distance, Format.NUMBER)]
    public void GetDefaultFormatForFieldType_ReturnsCorrectFormat(FieldType fieldType, Format expected)
    {
        // Act
        var result = TypeInferenceHelper.GetDefaultFormatForFieldType(fieldType);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(Format.TIME, true)]
    [InlineData(Format.DURATION, true)]
    [InlineData(Format.DATE, true)]
    [InlineData(Format.CURRENCY, false)]
    [InlineData(Format.NUMBER, false)]
    [InlineData(Format.TEXT, false)]
    [InlineData(Format.ACCOUNTING, false)]
    public void RequiresSpecialConversion_ReturnsCorrectValue(Format format, bool expected)
    {
        // Act
        var result = TypeInferenceHelper.RequiresSpecialConversion(format);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(typeof(string), Format.TIME, FieldType.Time)]
    [InlineData(typeof(string), Format.DURATION, FieldType.Duration)]
    [InlineData(typeof(string), Format.DATE, FieldType.DateTime)]
    [InlineData(typeof(string), Format.TEXT, FieldType.String)]
    [InlineData(typeof(string), Format.CURRENCY, FieldType.String)]
    [InlineData(typeof(decimal), Format.DISTANCE, FieldType.Number)]
    [InlineData(typeof(int), Format.TIME, FieldType.Integer)]
    public void InferFieldTypeFromFormat_WithStringAndSpecialFormat_ReturnsCorrectType(
        Type propertyType, Format format, FieldType expected)
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
        var format = Format.ACCOUNTING;

        // Act
        var result = TypeInferenceHelper.GetInferenceDescription(propertyType, format);

        // Assert
        Assert.Contains("Decimal", result);
        Assert.Contains("Number", result);
        Assert.Contains("ACCOUNTING", result);
    }

    [Fact]
    public void GetInferenceDescription_WithDefaultFormat_ShowsInferredFormat()
    {
        // Arrange
        var propertyType = typeof(decimal);
        var format = Format.DEFAULT;

        // Act
        var result = TypeInferenceHelper.GetInferenceDescription(propertyType, format);

        // Assert
        Assert.Contains("Decimal", result);
        Assert.Contains("Number", result);
        Assert.Contains("NUMBER", result); // Should show the inferred format
    }

    [Theory]
    [InlineData(typeof(string), Format.TIME)]
    [InlineData(typeof(string), Format.DURATION)]
    [InlineData(typeof(string), Format.DATE)]
    public void InferFieldTypeFromFormat_StringWithTimeFormats_OverridesBaseType(Type propertyType, Format format)
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
        Assert.Equal(FieldType.Number, nullableResult);
    }
}
