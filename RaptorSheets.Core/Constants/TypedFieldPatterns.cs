using RaptorSheets.Core.Enums;

namespace RaptorSheets.Core.Constants;

/// <summary>
/// Default format patterns for different field types in Google Sheets
/// </summary>
public static class TypedFieldPatterns
{
    /// <summary>
    /// Default format patterns for each field type
    /// </summary>
    public static readonly Dictionary<FieldTypeEnum, string> DefaultPatterns = new()
    {
        { FieldTypeEnum.String, "@" },
        { FieldTypeEnum.Number, "#,##0.00" },
        { FieldTypeEnum.Currency, "\"$\"#,##0.00" },
        { FieldTypeEnum.DateTime, "M/d/yyyy H:mm:ss" },
        { FieldTypeEnum.PhoneNumber, "(###) ###-####" },
        { FieldTypeEnum.Boolean, "@" },
        { FieldTypeEnum.Integer, "0" },
        { FieldTypeEnum.Email, "@" },
        { FieldTypeEnum.Url, "@" },
        { FieldTypeEnum.Percentage, "0.00%" }
    };

    /// <summary>
    /// Google Sheets number format types for each field type
    /// </summary>
    public static readonly Dictionary<FieldTypeEnum, string> NumberFormatTypes = new()
    {
        { FieldTypeEnum.String, "TEXT" },
        { FieldTypeEnum.Number, "NUMBER" },
        { FieldTypeEnum.Currency, "CURRENCY" },
        { FieldTypeEnum.DateTime, "DATE_TIME" },
        { FieldTypeEnum.PhoneNumber, "NUMBER" },
        { FieldTypeEnum.Boolean, "TEXT" },
        { FieldTypeEnum.Integer, "NUMBER" },
        { FieldTypeEnum.Email, "TEXT" },
        { FieldTypeEnum.Url, "TEXT" },
        { FieldTypeEnum.Percentage, "PERCENT" }
    };

    /// <summary>
    /// Gets the default format pattern for a field type
    /// </summary>
    /// <param name="fieldType">The field type</param>
    /// <returns>Default format pattern</returns>
    public static string GetDefaultPattern(FieldTypeEnum fieldType)
    {
        return DefaultPatterns.TryGetValue(fieldType, out var pattern) ? pattern : "@";
    }

    /// <summary>
    /// Gets the Google Sheets number format type for a field type
    /// </summary>
    /// <param name="fieldType">The field type</param>
    /// <returns>Google Sheets format type</returns>
    public static string GetNumberFormatType(FieldTypeEnum fieldType)
    {
        return NumberFormatTypes.TryGetValue(fieldType, out var type) ? type : "TEXT";
    }

    /// <summary>
    /// Validation patterns for different field types
    /// </summary>
    public static readonly Dictionary<FieldTypeEnum, string> ValidationPatterns = new()
    {
        { FieldTypeEnum.Email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$" },
        { FieldTypeEnum.PhoneNumber, @"^\+?1?\d{9,15}$" },
        { FieldTypeEnum.Url, @"^https?://.+$" }
    };

    /// <summary>
    /// Gets the default validation pattern for a field type
    /// </summary>
    /// <param name="fieldType">The field type</param>
    /// <returns>Validation pattern or null if none exists</returns>
    public static string? GetDefaultValidationPattern(FieldTypeEnum fieldType)
    {
        return ValidationPatterns.TryGetValue(fieldType, out var pattern) ? pattern : null;
    }
}