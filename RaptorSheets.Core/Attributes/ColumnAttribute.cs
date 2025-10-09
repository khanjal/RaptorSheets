using System.Text.Json.Serialization;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Constants;

namespace RaptorSheets.Core.Attributes;

/// <summary>
/// Comprehensive attribute that defines column configuration for Google Sheets
/// Uses header name as default JSON property name with optional override
/// Automatically applies default format patterns based on field type
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class ColumnAttribute : Attribute
{
    /// <summary>
    /// Gets the header name for the sheet column
    /// </summary>
    public string HeaderName { get; }

    /// <summary>
    /// Gets the JSON property name for serialization (defaults to header name converted to camelCase)
    /// </summary>
    public string JsonPropertyName { get; }

    /// <summary>
    /// Gets the field type for automatic conversion and formatting
    /// </summary>
    public FieldTypeEnum FieldType { get; }

    /// <summary>
    /// Gets the custom number format pattern for Google Sheets (uses default if not specified)
    /// </summary>
    public string? NumberFormatPattern { get; }

    /// <summary>
    /// Gets whether this field should be validated (optional)
    /// </summary>
    public bool EnableValidation { get; }

    /// <summary>
    /// Gets custom validation pattern for the field (optional)
    /// </summary>
    public string? ValidationPattern { get; }

    /// <summary>
    /// Gets the column order priority (-1 means use property declaration order)
    /// </summary>
    public int Order { get; }

    /// <summary>
    /// Initializes a column configuration using header name as JSON property name with default formatting
    /// </summary>
    /// <param name="headerName">Header name for sheet column (also used for JSON property name)</param>
    /// <param name="fieldType">Field type for automatic conversion and formatting</param>
    public ColumnAttribute(string headerName, FieldTypeEnum fieldType)
    {
        HeaderName = headerName ?? throw new ArgumentNullException(nameof(headerName));
        JsonPropertyName = ConvertHeaderNameToJsonPropertyName(headerName);
        FieldType = fieldType;
        NumberFormatPattern = null; // Will use default pattern
        Order = -1;
    }

    /// <summary>
    /// Initializes a column configuration with custom format pattern (only when different from default)
    /// </summary>
    /// <param name="headerName">Header name for sheet column</param>
    /// <param name="fieldType">Field type for automatic conversion and formatting</param>
    /// <param name="formatPattern">Custom number format pattern (specify only when different from default)</param>
    public ColumnAttribute(string headerName, FieldTypeEnum fieldType, string formatPattern)
    {
        HeaderName = headerName ?? throw new ArgumentNullException(nameof(headerName));
        JsonPropertyName = ConvertHeaderNameToJsonPropertyName(headerName);
        FieldType = fieldType;
        NumberFormatPattern = formatPattern;
        Order = -1;
    }

    /// <summary>
    /// Initializes a column configuration with full customization options
    /// </summary>
    /// <param name="headerName">Header name for sheet column</param>
    /// <param name="fieldType">Field type for automatic conversion and formatting</param>
    /// <param name="formatPattern">Custom number format pattern (null = use default)</param>
    /// <param name="jsonPropertyName">Custom JSON property name (null = auto-generate from header)</param>
    /// <param name="order">Column order priority (-1 = use declaration order)</param>
    /// <param name="enableValidation">Enable field validation (default: false)</param>
    /// <param name="validationPattern">Custom validation pattern (null = use default for field type)</param>
    public ColumnAttribute(
        string headerName, 
        FieldTypeEnum fieldType,
        string? formatPattern = null,
        string? jsonPropertyName = null,
        int order = -1,
        bool enableValidation = false,
        string? validationPattern = null)
    {
        HeaderName = headerName ?? throw new ArgumentNullException(nameof(headerName));
        JsonPropertyName = jsonPropertyName ?? ConvertHeaderNameToJsonPropertyName(headerName);
        FieldType = fieldType;
        NumberFormatPattern = formatPattern;
        Order = order;
        EnableValidation = enableValidation;
        ValidationPattern = validationPattern;
    }

    /// <summary>
    /// Gets the effective header name (same as HeaderName since it's the primary identifier)
    /// </summary>
    public string GetEffectiveHeaderName() => HeaderName;

    /// <summary>
    /// Gets whether this column has an explicit order (Order >= 0)
    /// </summary>
    public bool HasExplicitOrder => Order >= 0;

    /// <summary>
    /// Gets the effective number format pattern (custom pattern or default for field type)
    /// </summary>
    public string GetEffectiveNumberFormatPattern()
    {
        return NumberFormatPattern ?? TypedFieldPatterns.GetDefaultPattern(FieldType);
    }

    /// <summary>
    /// Gets whether this column uses a custom format pattern (not the default)
    /// </summary>
    public bool HasCustomFormatPattern => NumberFormatPattern != null;

    /// <summary>
    /// Converts a header name to a camelCase JSON property name
    /// Examples: "Start Address" -> "startAddress", "Pay" -> "pay", "Amount Per Time" -> "amountPerTime"
    /// </summary>
    private static string ConvertHeaderNameToJsonPropertyName(string headerName)
    {
        if (string.IsNullOrWhiteSpace(headerName))
        {
            return headerName;
        }

        // Remove common punctuation and split on spaces/special characters
        var words = headerName
            .Replace("-", " ")
            .Replace("_", " ")
            .Replace(".", " ")
            .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        if (words.Length == 0)
        {
            return headerName.ToLowerInvariant();
        }

        // First word lowercase, subsequent words title case
        var result = words[0].ToLowerInvariant();
        for (int i = 1; i < words.Length; i++)
        {
            var word = words[i];
            if (word.Length > 0)
            {
                result += char.ToUpperInvariant(word[0]) + word[1..].ToLowerInvariant();
            }
        }

        return result;
    }
}