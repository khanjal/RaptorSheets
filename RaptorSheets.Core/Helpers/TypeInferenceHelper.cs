using RaptorSheets.Core.Enums;

namespace RaptorSheets.Core.Helpers;

/// <summary>
/// Helper class for inferring field types and formats from C# property types.
/// Eliminates the need to manually specify FieldType when it matches the property type.
/// </summary>
public static class TypeInferenceHelper
{
    /// <summary>
    /// Infers the FieldType from a property's C# type.
    /// Unwraps nullable types to get the underlying type.
    /// </summary>
    /// <param name="propertyType">The C# property type to analyze</param>
    /// <returns>The inferred FieldType</returns>
    public static FieldType InferFieldType(Type propertyType)
    {
        // Unwrap nullable types (int? -> int, decimal? -> decimal)
        var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        
        return underlyingType.Name switch
        {
            nameof(String) => FieldType.String,
            nameof(Int32) => FieldType.Integer,
            nameof(Int64) => FieldType.Integer,
            nameof(Int16) => FieldType.Integer,
            nameof(Byte) => FieldType.Integer,
            nameof(Decimal) => FieldType.Number,
            nameof(Double) => FieldType.Number,
            nameof(Single) => FieldType.Number,
            nameof(Boolean) => FieldType.Boolean,
            nameof(DateTime) => FieldType.DateTime,
            _ => FieldType.String  // Safe default for unknown types
        };
    }

    /// <summary>
    /// Infers the default Format from a FieldType.
    /// Used when Format.DEFAULT is specified.
    /// </summary>
    /// <param name="fieldType">The field type to get default format for</param>
    /// <returns>The default Format for the field type</returns>
    public static Format GetDefaultFormatForFieldType(FieldType fieldType)
    {
        return fieldType switch
        {
            FieldType.Currency => Format.CURRENCY,
            FieldType.Accounting => Format.ACCOUNTING,
            FieldType.DateTime => Format.DATE,
            FieldType.Time => Format.TIME,
            FieldType.Duration => Format.DURATION,
            FieldType.Number => Format.NUMBER,
            FieldType.Percentage => Format.PERCENT,
            FieldType.Integer => Format.NUMBER,
            FieldType.Boolean => Format.TEXT,
            FieldType.String => Format.TEXT,
            FieldType.Distance => Format.NUMBER,
            _ => Format.TEXT
        };
    }

    /// <summary>
    /// Determines if a format enum requires special string conversion.
    /// These formats (TIME, DURATION, DATE) need ToSerialTime/ToSerialDuration/ToSerialDate conversions.
    /// </summary>
    public static bool RequiresSpecialConversion(Format format)
    {
        return format switch
        {
            Format.TIME => true,
            Format.DURATION => true,
            Format.DATE => true,
            _ => false
        };
    }

    /// <summary>
    /// Infers the FieldType that should be used when a specific Format is applied to a string property.
    /// For example, Format.TIME on a string should use FieldType.Time for proper serialization.
    /// </summary>
    public static FieldType InferFieldTypeFromFormat(Type propertyType, Format format)
    {
        // First get the base type inference
        var baseFieldType = InferFieldType(propertyType);
        
        // If it's a string and has a special format, override the field type
        if (baseFieldType == FieldType.String)
        {
            return format switch
            {
                Format.TIME => FieldType.Time,
                Format.DURATION => FieldType.Duration,
                Format.DATE => FieldType.DateTime,
                _ => baseFieldType
            };
        }
        
        return baseFieldType;
    }

    /// <summary>
    /// Gets a user-friendly description of what type inference determined.
    /// Useful for debugging and error messages.
    /// </summary>
    public static string GetInferenceDescription(Type propertyType, Format format)
    {
        var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        var fieldType = InferFieldTypeFromFormat(propertyType, format);
        var defaultFormat = format == Format.DEFAULT 
            ? GetDefaultFormatForFieldType(fieldType) 
            : format;
        
        return $"Property type '{underlyingType.Name}' ? FieldType '{fieldType}' ? Format '{defaultFormat}'";
    }
}
