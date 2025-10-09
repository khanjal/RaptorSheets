using RaptorSheets.Core.Enums;

namespace RaptorSheets.Core.Attributes;

/// <summary>
/// Specifies the field type and formatting for automatic type conversion and Google Sheets formatting.
/// This attribute enables type-aware data conversion similar to GoogleSheetsWrapper's approach.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class TypedFieldAttribute : Attribute
{
    /// <summary>
    /// Gets the field type for automatic conversion and formatting
    /// </summary>
    public FieldTypeEnum FieldType { get; }

    /// <summary>
    /// Gets the custom number format pattern for Google Sheets (optional)
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
    /// Initializes a new instance of the TypedFieldAttribute with basic field type
    /// </summary>
    /// <param name="fieldType">The field type for automatic conversion and formatting</param>
    public TypedFieldAttribute(FieldTypeEnum fieldType)
    {
        FieldType = fieldType;
    }

    /// <summary>
    /// Initializes a new instance of the TypedFieldAttribute with custom format pattern
    /// </summary>
    /// <param name="fieldType">The field type for automatic conversion and formatting</param>
    /// <param name="formatPattern">Custom number format pattern for Google Sheets</param>
    public TypedFieldAttribute(FieldTypeEnum fieldType, string formatPattern)
    {
        FieldType = fieldType;
        NumberFormatPattern = formatPattern;
    }

    /// <summary>
    /// Initializes a new instance of the TypedFieldAttribute with validation
    /// </summary>
    /// <param name="fieldType">The field type for automatic conversion and formatting</param>
    /// <param name="formatPattern">Custom number format pattern for Google Sheets</param>
    /// <param name="enableValidation">Whether to enable validation for this field</param>
    /// <param name="validationPattern">Custom validation pattern</param>
    public TypedFieldAttribute(FieldTypeEnum fieldType, string? formatPattern = null, bool enableValidation = false, string? validationPattern = null)
    {
        FieldType = fieldType;
        NumberFormatPattern = formatPattern;
        EnableValidation = enableValidation;
        ValidationPattern = validationPattern;
    }
}