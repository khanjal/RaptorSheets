using RaptorSheets.Core.Enums;

namespace RaptorSheets.Core.Helpers;

/// <summary>
/// Helper class for inferring field types and formats from C# property types.
/// Eliminates the need to manually specify FieldTypeEnum when it matches the property type.
/// </summary>
public static class TypeInferenceHelper
{
    /// <summary>
    /// Infers the FieldTypeEnum from a property's C# type.
    /// Unwraps nullable types to get the underlying type.
    /// </summary>
    /// <param name="propertyType">The C# property type to analyze</param>
    /// <returns>The inferred FieldTypeEnum</returns>
    public static FieldTypeEnum InferFieldType(Type propertyType)
    {
        // Unwrap nullable types (int? -> int, decimal? -> decimal)
        var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        
        return underlyingType.Name switch
        {
            nameof(String) => FieldTypeEnum.String,
            nameof(Int32) => FieldTypeEnum.Integer,
            nameof(Int64) => FieldTypeEnum.Integer,
            nameof(Int16) => FieldTypeEnum.Integer,
            nameof(Byte) => FieldTypeEnum.Integer,
            nameof(Decimal) => FieldTypeEnum.Currency,  // Default for decimal is currency
            nameof(Double) => FieldTypeEnum.Number,
            nameof(Single) => FieldTypeEnum.Number,
            nameof(Boolean) => FieldTypeEnum.Boolean,
            nameof(DateTime) => FieldTypeEnum.DateTime,
            _ => FieldTypeEnum.String  // Safe default for unknown types
        };
    }

    /// <summary>
    /// Infers the default FormatEnum from a FieldTypeEnum.
    /// Used when FormatEnum.DEFAULT is specified.
    /// </summary>
    /// <param name="fieldType">The field type to get default format for</param>
    /// <returns>The default FormatEnum for the field type</returns>
    public static FormatEnum GetDefaultFormatForFieldType(FieldTypeEnum fieldType)
    {
        return fieldType switch
        {
            FieldTypeEnum.Currency => FormatEnum.CURRENCY,
            FieldTypeEnum.Accounting => FormatEnum.ACCOUNTING,
            FieldTypeEnum.DateTime => FormatEnum.DATE,
            FieldTypeEnum.Time => FormatEnum.TIME,
            FieldTypeEnum.Duration => FormatEnum.DURATION,
            FieldTypeEnum.Number => FormatEnum.NUMBER,
            FieldTypeEnum.Percentage => FormatEnum.PERCENT,
            FieldTypeEnum.Integer => FormatEnum.NUMBER,
            FieldTypeEnum.Boolean => FormatEnum.TEXT,
            FieldTypeEnum.String => FormatEnum.TEXT,
            _ => FormatEnum.TEXT
        };
    }

    /// <summary>
    /// Determines if a format enum requires special string conversion.
    /// These formats (TIME, DURATION, DATE) need ToSerialTime/ToSerialDuration/ToSerialDate conversions.
    /// </summary>
    public static bool RequiresSpecialConversion(FormatEnum format)
    {
        return format switch
        {
            FormatEnum.TIME => true,
            FormatEnum.DURATION => true,
            FormatEnum.DATE => true,
            _ => false
        };
    }

    /// <summary>
    /// Infers the FieldTypeEnum that should be used when a specific FormatEnum is applied to a string property.
    /// For example, FormatEnum.TIME on a string should use FieldTypeEnum.Time for proper serialization.
    /// </summary>
    public static FieldTypeEnum InferFieldTypeFromFormat(Type propertyType, FormatEnum format)
    {
        // First get the base type inference
        var baseFieldType = InferFieldType(propertyType);
        
        // If it's a string and has a special format, override the field type
        if (baseFieldType == FieldTypeEnum.String)
        {
            return format switch
            {
                FormatEnum.TIME => FieldTypeEnum.Time,
                FormatEnum.DURATION => FieldTypeEnum.Duration,
                FormatEnum.DATE => FieldTypeEnum.DateTime,
                _ => baseFieldType
            };
        }
        
        return baseFieldType;
    }

    /// <summary>
    /// Gets a user-friendly description of what type inference determined.
    /// Useful for debugging and error messages.
    /// </summary>
    public static string GetInferenceDescription(Type propertyType, FormatEnum format)
    {
        var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        var fieldType = InferFieldTypeFromFormat(propertyType, format);
        var defaultFormat = format == FormatEnum.DEFAULT 
            ? GetDefaultFormatForFieldType(fieldType) 
            : format;
        
        return $"Property type '{underlyingType.Name}' ? FieldType '{fieldType}' ? Format '{defaultFormat}'";
    }
}
