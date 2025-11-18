using RaptorSheets.Core.Enums;
using System.Collections.Immutable;

namespace RaptorSheets.Core.Constants;

/// <summary>
/// Default format patterns for different field types in Google Sheets
/// </summary>
public static class TypedFieldPatterns
{
    /// <summary>
    /// Default format patterns for each field type
    /// </summary>
    private static readonly ImmutableDictionary<FieldType, string> _defaultPatterns = new Dictionary<FieldType, string>
    {
        { FieldType.String, "@" },
        { FieldType.Number, "#,##0.00" },
        { FieldType.Currency, "\"$\"#,##0.00" },
        { FieldType.DateTime, "M/d/yyyy H:mm:ss" },
        { FieldType.PhoneNumber, "(###) ###-####" },
        { FieldType.Boolean, "@" },
        { FieldType.Integer, "0" },
        { FieldType.Email, "@" },
        { FieldType.Url, "@" },
        { FieldType.Percentage, "0.00%" }
    }.ToImmutableDictionary();

    /// <summary>
    /// Google Sheets number format types for each field type
    /// </summary>
    private static readonly ImmutableDictionary<FieldType, string> _numberFormatTypes = new Dictionary<FieldType, string>
    {
        { FieldType.String, "TEXT" },
        { FieldType.Number, "NUMBER" },
        { FieldType.Currency, "CURRENCY" },
        { FieldType.DateTime, "DATE_TIME" },
        { FieldType.PhoneNumber, "NUMBER" },
        { FieldType.Boolean, "TEXT" },
        { FieldType.Integer, "NUMBER" },
        { FieldType.Email, "TEXT" },
        { FieldType.Url, "TEXT" },
        { FieldType.Percentage, "PERCENT" }
    }.ToImmutableDictionary();

    /// <summary>
    /// Validation patterns for different field types
    /// </summary>
    private static readonly ImmutableDictionary<FieldType, string> _validationPatterns = new Dictionary<FieldType, string>
    {
        { FieldType.Email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$" },
        { FieldType.PhoneNumber, @"^\+?1?\d{9,15}$" },
        { FieldType.Url, @"^https?://.+$" }
    }.ToImmutableDictionary();

    /// <summary>
    /// Gets the default format pattern for a field type
    /// </summary>
    /// <param name="fieldType">The field type</param>
    /// <returns>Default format pattern</returns>
    public static string GetDefaultPattern(FieldType fieldType)
    {
        return _defaultPatterns.TryGetValue(fieldType, out var pattern) ? pattern : "@";
    }

    /// <summary>
    /// Gets the Google Sheets number format type for a field type
    /// </summary>
    /// <param name="fieldType">The field type</param>
    /// <returns>Google Sheets format type</returns>
    public static string GetNumberFormatType(FieldType fieldType)
    {
        return _numberFormatTypes.TryGetValue(fieldType, out var type) ? type : "TEXT";
    }

    /// <summary>
    /// Gets the default validation pattern for a field type
    /// </summary>
    /// <param name="fieldType">The field type</param>
    /// <returns>Validation pattern or null if none exists</returns>
    public static string? GetDefaultValidationPattern(FieldType fieldType)
    {
        return _validationPatterns.TryGetValue(fieldType, out var pattern) ? pattern : null;
    }
}