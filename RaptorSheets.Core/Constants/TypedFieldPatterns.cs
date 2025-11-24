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
        { FieldType.String, CellFormatPatterns.Text },
        { FieldType.Number, CellFormatPatterns.NumberWithDecimals },
        { FieldType.Currency, CellFormatPatterns.Currency },
        { FieldType.Accounting, CellFormatPatterns.Accounting },
        { FieldType.DateTime, CellFormatPatterns.Date },
        { FieldType.Time, CellFormatPatterns.Time },
        { FieldType.Duration, CellFormatPatterns.Duration },
        { FieldType.PhoneNumber, CellFormatPatterns.Phone },
        { FieldType.Boolean, CellFormatPatterns.Text },
        { FieldType.Integer, CellFormatPatterns.Integer },
        { FieldType.Email, CellFormatPatterns.Text },
        { FieldType.Url, CellFormatPatterns.Text },
        { FieldType.Percentage, CellFormatPatterns.Percentage },
        { FieldType.Distance, CellFormatPatterns.Distance }
    }.ToImmutableDictionary();

    /// <summary>
    /// Google Sheets number format types for each field type
    /// </summary>
    private static readonly ImmutableDictionary<FieldType, string> _numberFormatTypes = new Dictionary<FieldType, string>
    {
        { FieldType.String, CellFormatPatterns.CellFormatText },
        { FieldType.Number, CellFormatPatterns.CellFormatNumber },
        { FieldType.Currency, CellFormatPatterns.CellFormatCurrency },
        { FieldType.Accounting, CellFormatPatterns.CellFormatNumber },
        { FieldType.DateTime, CellFormatPatterns.CellFormatDateTime },
        { FieldType.Time, CellFormatPatterns.CellFormatDateTime },
        { FieldType.Duration, CellFormatPatterns.CellFormatDateTime },
        { FieldType.PhoneNumber, CellFormatPatterns.CellFormatNumber },
        { FieldType.Boolean, CellFormatPatterns.CellFormatText },
        { FieldType.Integer, CellFormatPatterns.CellFormatNumber },
        { FieldType.Email, CellFormatPatterns.CellFormatText },
        { FieldType.Url, CellFormatPatterns.CellFormatText },
        { FieldType.Percentage, CellFormatPatterns.CellFormatPercent },
        { FieldType.Distance, CellFormatPatterns.CellFormatNumber }
    }.ToImmutableDictionary();

    /// <summary>
    /// Validation patterns for different field types
    /// </summary>
    private static readonly ImmutableDictionary<FieldType, string> _validationPatterns = new Dictionary<FieldType, string>
    {
        { FieldType.Email, CellFormatPatterns.ValidationEmail },
        { FieldType.PhoneNumber, CellFormatPatterns.ValidationPhoneNumber },
        { FieldType.Url, CellFormatPatterns.ValidationUrl }
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