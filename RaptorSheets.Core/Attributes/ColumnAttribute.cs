using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Helpers;
using System.Text;

namespace RaptorSheets.Core.Attributes;

/// <summary>
/// Comprehensive attribute that defines column configuration for Google Sheets.
/// Automatically infers FieldType from property type - you only need to specify FormatType for special formatting.
/// Uses header name as default JSON property name with optional override.
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
    /// Gets the field type for data conversion between C# and Google Sheets.
    /// This is automatically inferred from the property type unless explicitly specified.
    /// Controls how data is parsed when reading from sheets and formatted when writing to sheets.
    /// </summary>
    public FieldTypeEnum FieldType { get; private set; }

    /// <summary>
    /// Gets whether the FieldType was explicitly set (true) or auto-inferred (false).
    /// </summary>
    internal bool IsFieldTypeExplicit { get; private set; }

    /// <summary>
    /// Gets the format type for Google Sheets display formatting.
    /// This controls how the cell is visually formatted in Google Sheets (e.g., DATE, CURRENCY, TEXT).
    /// Can differ from FieldType (e.g., FieldType=String but FormatType=DATE for date strings).
    /// If DEFAULT, uses format matching FieldType.
    /// </summary>
    public FormatEnum FormatType { get; }

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
    /// Gets whether this is an input column (user-entered data that should be written to sheets).
    /// Default is false (output/formula column that is read-only from write perspective).
    /// Set to true for columns where user data should be written back to Google Sheets.
    /// </summary>
    public bool IsInput { get; }

    /// <summary>
    /// Gets the note/comment to display for this column in Google Sheets.
    /// Useful for providing instructions or context to users.
    /// </summary>
    public string? Note { get; }

    /// <summary>
    /// Initializes a column configuration for an OUTPUT column (formula/calculated).
    /// FieldType is automatically inferred from the property type.
    /// This is the most common case - use this constructor for columns with formulas.
    /// For input columns (user-entered data), use the constructor with isInput: true.
    /// </summary>
    /// <param name="headerName">Header name for sheet column (also used for JSON property name)</param>
    public ColumnAttribute(string headerName)
    {
        HeaderName = headerName ?? throw new ArgumentNullException(nameof(headerName));
        JsonPropertyName = ConvertHeaderNameToJsonPropertyName(headerName);
        FieldType = FieldTypeEnum.String; // Default, will be set by SetFieldTypeFromProperty
        IsFieldTypeExplicit = false;
        FormatType = FormatEnum.DEFAULT;
        NumberFormatPattern = null;
        Order = -1;
        IsInput = false;
        Note = null;
    }

    /// <summary>
    /// Initializes a column configuration with format type override.
    /// FieldType is automatically inferred from the property type.
    /// Use this when you need to override the default format (e.g., string date with DATE format).
    /// </summary>
    /// <param name="headerName">Header name for sheet column</param>
    /// <param name="formatType">Format type for Google Sheets display</param>
    /// <param name="isInput">True if this is a user-input column (default: false)</param>
    public ColumnAttribute(string headerName, FormatEnum formatType, bool isInput = false)
    {
        HeaderName = headerName ?? throw new ArgumentNullException(nameof(headerName));
        JsonPropertyName = ConvertHeaderNameToJsonPropertyName(headerName);
        FieldType = FieldTypeEnum.String; // Default, will be set by SetFieldTypeFromProperty
        IsFieldTypeExplicit = false;
        FormatType = formatType;
        NumberFormatPattern = null;
        Order = -1;
        IsInput = isInput;
        Note = null;
    }

    /// <summary>
    /// Initializes a column configuration using header name as JSON property name with default formatting.
    /// Explicitly requires specifying whether this is an input or output column.
    /// FieldType is automatically inferred from the property type.
    /// </summary>
    /// <param name="headerName">Header name for sheet column (also used for JSON property name)</param>
    /// <param name="isInput">True if this is a user-input column that should be written to sheets, false for output/formula columns</param>
    public ColumnAttribute(string headerName, bool isInput)
    {
        HeaderName = headerName ?? throw new ArgumentNullException(nameof(headerName));
        JsonPropertyName = ConvertHeaderNameToJsonPropertyName(headerName);
        FieldType = FieldTypeEnum.String; // Default, will be set by SetFieldTypeFromProperty
        IsFieldTypeExplicit = false;
        FormatType = FormatEnum.DEFAULT;
        NumberFormatPattern = null;
        Order = -1;
        IsInput = isInput;
        EnableValidation = false;
        ValidationPattern = null;
        Note = null;
    }

    /// <summary>
    /// Initializes a column configuration with advanced options using ColumnOptions.
    /// FieldType is automatically inferred from the property type.
    /// This constructor is recommended when you need to customize multiple optional parameters.
    /// Use ColumnOptions.Builder() for a fluent API, or pass named parameters directly.
    /// </summary>
    /// <param name="headerName">Header name for sheet column</param>
    /// <param name="options">Configuration options for advanced customization</param>
    public ColumnAttribute(string headerName, ColumnOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        
        HeaderName = headerName ?? throw new ArgumentNullException(nameof(headerName));
        JsonPropertyName = options.JsonPropertyName ?? ConvertHeaderNameToJsonPropertyName(headerName);
        FieldType = FieldTypeEnum.String; // Default, will be set by SetFieldTypeFromProperty
        IsFieldTypeExplicit = false;
        FormatType = options.FormatType;
        NumberFormatPattern = options.FormatPattern;
        Order = options.Order;
        IsInput = options.IsInput;
        EnableValidation = options.EnableValidation;
        ValidationPattern = options.ValidationPattern;
        Note = options.Note;
    }

    /// <summary>
    /// Initializes a column configuration with advanced named parameters.
    /// FieldType is automatically inferred from the property type.
    /// Use this when you need jsonPropertyName, formatPattern, or other advanced options.
    /// </summary>
    /// <param name="headerName">Header name for sheet column</param>
    /// <param name="isInput">True if this is a user-input column (default: false)</param>
    /// <param name="jsonPropertyName">Custom JSON property name (null = auto-generate from header)</param>
    /// <param name="formatPattern">Custom number format pattern (null = use default)</param>
    /// <param name="note">Note/comment to display in Google Sheets</param>
    /// <param name="enableValidation">Enable field validation (default: false)</param>
    /// <param name="validationPattern">Custom validation pattern (null = use default for field type)</param>
    /// <param name="order">Column order priority (-1 = use declaration order)</param>
    /// <param name="formatType">Format type for Google Sheets display (DEFAULT = use default from fieldType)</param>
    public ColumnAttribute(
        string headerName,
        bool isInput = false,
        string? jsonPropertyName = null,
        string? formatPattern = null,
        string? note = null,
        bool enableValidation = false,
        string? validationPattern = null,
        int order = -1,
        FormatEnum formatType = FormatEnum.DEFAULT)
    {
        HeaderName = headerName ?? throw new ArgumentNullException(nameof(headerName));
        JsonPropertyName = jsonPropertyName ?? ConvertHeaderNameToJsonPropertyName(headerName);
        FieldType = FieldTypeEnum.String; // Default, will be set by SetFieldTypeFromProperty
        IsFieldTypeExplicit = false;
        FormatType = formatType;
        NumberFormatPattern = formatPattern;
        Order = order;
        IsInput = isInput;
        EnableValidation = enableValidation;
        ValidationPattern = validationPattern;
        Note = note;
    }

    // LEGACY CONSTRUCTORS - Kept for backward compatibility

    /// <summary>
    /// LEGACY: Initializes a column configuration for an OUTPUT column with explicit FieldType.
    /// NOTE: FieldType is now automatically inferred from property type. Use new constructors without FieldType parameter.
    /// </summary>
    /// <param name="headerName">Header name for sheet column</param>
    /// <param name="fieldType">Field type (LEGACY - auto-inferred in new constructors)</param>
    [Obsolete("FieldType is now automatically inferred from property type. Use ColumnAttribute(headerName) instead.")]
    public ColumnAttribute(string headerName, FieldTypeEnum fieldType)
    {
        HeaderName = headerName ?? throw new ArgumentNullException(nameof(headerName));
        JsonPropertyName = ConvertHeaderNameToJsonPropertyName(headerName);
        FieldType = fieldType;
        IsFieldTypeExplicit = true;
        FormatType = FormatEnum.DEFAULT;
        NumberFormatPattern = null;
        Order = -1;
        IsInput = false;
        Note = null;
    }

    /// <summary>
    /// LEGACY: Initializes a column configuration with explicit FieldType and input flag.
    /// NOTE: FieldType is now automatically inferred from property type. Use new constructors without FieldType parameter.
    /// </summary>
    [Obsolete("FieldType is now automatically inferred from property type. Use ColumnAttribute(headerName, isInput) instead.")]
    public ColumnAttribute(string headerName, FieldTypeEnum fieldType, bool isInput)
    {
        HeaderName = headerName ?? throw new ArgumentNullException(nameof(headerName));
        JsonPropertyName = ConvertHeaderNameToJsonPropertyName(headerName);
        FieldType = fieldType;
        IsFieldTypeExplicit = true;
        FormatType = FormatEnum.DEFAULT;
        NumberFormatPattern = null;
        Order = -1;
        IsInput = isInput;
        Note = null;
    }

    /// <summary>
    /// LEGACY: Initializes a column configuration with full customization options.
    /// NOTE: This constructor has many parameters AND FieldType is now auto-inferred. 
    /// Use ColumnAttribute(headerName, ColumnOptions) instead for better readability.
    /// </summary>
    [Obsolete("FieldType is now automatically inferred. Use ColumnAttribute(headerName, ColumnOptions) for complex scenarios.")]
    public ColumnAttribute(
        string headerName,
        FieldTypeEnum fieldType,
        string? formatPattern = null,
        string? jsonPropertyName = null,
        int order = -1,
        bool isInput = false,
        bool enableValidation = false,
        string? validationPattern = null,
        string? note = null,
        FormatEnum formatType = FormatEnum.DEFAULT)
    {
        HeaderName = headerName ?? throw new ArgumentNullException(nameof(headerName));
        JsonPropertyName = jsonPropertyName ?? ConvertHeaderNameToJsonPropertyName(headerName);
        FieldType = fieldType;
        IsFieldTypeExplicit = true;
        FormatType = formatType;
        NumberFormatPattern = formatPattern;
        Order = order;
        IsInput = isInput;
        EnableValidation = enableValidation;
        ValidationPattern = validationPattern;
        Note = note;
    }

    /// <summary>
    /// Sets the FieldType based on the property's C# type.
    /// Called by reflection helpers to auto-infer the field type.
    /// If FieldType was explicitly set, this method respects that and doesn't override.
    /// </summary>
    /// <param name="propertyType">The C# property type to infer from</param>
    internal void SetFieldTypeFromProperty(Type propertyType)
    {
        if (!IsFieldTypeExplicit)
        {
            // Infer field type from property type and format
            FieldType = TypeInferenceHelper.InferFieldTypeFromFormat(propertyType, FormatType);
        }
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
    /// Gets whether this is an output column (formula/calculated field that should NOT be written to sheets)
    /// </summary>
    public bool IsOutput => !IsInput;

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
    /// Gets the effective format enum for Google Sheets display.
    /// Returns the explicit FormatType if specified (not DEFAULT), otherwise defaults to format matching FieldType.
    /// </summary>
    public FormatEnum? GetEffectiveFormat()
    {
        // Use explicit FormatType if provided (not DEFAULT), otherwise derive from FieldType
        return FormatType != FormatEnum.DEFAULT ? FormatType : GetDefaultFormatFromFieldType(FieldType);
    }

    /// <summary>
    /// Gets the default FormatEnum for a given field type.
    /// Used when FormatType is DEFAULT.
    /// </summary>
    private static FormatEnum? GetDefaultFormatFromFieldType(FieldTypeEnum fieldType)
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
            FieldTypeEnum.String => FormatEnum.TEXT,
            _ => null
        };
    }

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

        // Use StringBuilder for efficient concatenation
        var result = new StringBuilder(words[0].ToLowerInvariant());
        for (int i = 1; i < words.Length; i++)
        {
            var word = words[i];
            if (word.Length > 0)
            {
                result.Append(char.ToUpperInvariant(word[0])).Append(word[1..].ToLowerInvariant());
            }
        }

        return result.ToString();
    }
}