using System.Reflection;
using RaptorSheets.Core.Enums;

namespace RaptorSheets.Core.Attributes;

/// <summary>
/// Minimal attribute that uses smart conventions to reduce boilerplate
/// Uses property name and type information to infer most settings
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class SmartFieldAttribute : Attribute
{
    /// <summary>
    /// Override the inferred field type (optional)
    /// </summary>
    public FieldType? FieldType { get; set; }

    /// <summary>
    /// Override the inferred header name (optional)
    /// </summary>
    public string? HeaderName { get; set; }

    /// <summary>
    /// Override the inferred JSON property name (optional)
    /// </summary>
    public string? JsonPropertyName { get; set; }

    /// <summary>
    /// Custom format pattern (optional)
    /// </summary>
    public string? FormatPattern { get; set; }

    /// <summary>
    /// Explicit column order (optional - uses declaration order by default)
    /// </summary>
    public int Order { get; set; } = 0;

    /// <summary>
    /// Enable validation (default: false)
    /// </summary>
    public bool EnableValidation { get; set; }

    public SmartFieldAttribute() { }

    public SmartFieldAttribute(FieldType fieldType)
    {
        FieldType = fieldType;
    }

    public SmartFieldAttribute(string headerName)
    {
        HeaderName = headerName;
    }

    public SmartFieldAttribute(FieldType fieldType, string headerName)
    {
        FieldType = fieldType;
        HeaderName = headerName;
    }
}

/// <summary>
/// Helper to infer field configuration from property information and conventions
/// </summary>
public static class FieldConventions
{
    public static FieldType InferFieldType(PropertyInfo property)
    {
        ArgumentNullException.ThrowIfNull(property);
        
        var type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        
        return Type.GetTypeCode(type) switch
        {
            TypeCode.String when IsEmail(property.Name) => FieldType.Email,
            TypeCode.String when IsPhone(property.Name) => FieldType.PhoneNumber,
            TypeCode.String when IsUrl(property.Name) => FieldType.Url,
            TypeCode.String => FieldType.String,
            TypeCode.DateTime => FieldType.DateTime,
            TypeCode.Boolean => FieldType.Boolean,
            TypeCode.Decimal when IsCurrency(property.Name) => FieldType.Currency,
            TypeCode.Decimal when IsPercentage(property.Name) => FieldType.Percentage,
            TypeCode.Decimal => FieldType.Number,
            TypeCode.Double => FieldType.Number,
            TypeCode.Single => FieldType.Number,
            TypeCode.Int32 => FieldType.Integer,
            TypeCode.Int64 => FieldType.Integer,
            TypeCode.Int16 => FieldType.Integer,
            _ => FieldType.String
        };
    }

    public static string InferJsonPropertyName(PropertyInfo property)
    {
        ArgumentNullException.ThrowIfNull(property);
        
        // Convert PascalCase to camelCase
        var name = property.Name;
        return char.ToLowerInvariant(name[0]) + name[1..];
    }

    public static string InferHeaderName(PropertyInfo property)
    {
        ArgumentNullException.ThrowIfNull(property);
        
        // Check if the property has a SmartFieldAttribute with a custom HeaderName
        var smartFieldAttribute = property.GetCustomAttribute<SmartFieldAttribute>();
        if (smartFieldAttribute?.HeaderName != null)
        {
            return smartFieldAttribute.HeaderName;
        }

        // Convert PascalCase to "Title Case" with spaces
        var name = property.Name;
        return string.Concat(name.Select((c, i) => 
            i > 0 && char.IsUpper(c) ? " " + c : c.ToString()));
    }

    private static bool IsCurrency(string propertyName) =>
        propertyName.Contains("Pay", StringComparison.OrdinalIgnoreCase) ||
        propertyName.Contains("Amount", StringComparison.OrdinalIgnoreCase) ||
        propertyName.Contains("Price", StringComparison.OrdinalIgnoreCase) ||
        propertyName.Contains("Cost", StringComparison.OrdinalIgnoreCase) ||
        propertyName.Contains("Fee", StringComparison.OrdinalIgnoreCase) ||
        propertyName.Contains("Salary", StringComparison.OrdinalIgnoreCase) ||
        propertyName.Contains("Total", StringComparison.OrdinalIgnoreCase) ||
        propertyName.Contains("Tip", StringComparison.OrdinalIgnoreCase) ||
        propertyName.Contains("Bonus", StringComparison.OrdinalIgnoreCase) ||
        propertyName.Contains("Cash", StringComparison.OrdinalIgnoreCase);

    private static bool IsPercentage(string propertyName) =>
        propertyName.Contains("Percent", StringComparison.OrdinalIgnoreCase) ||
        propertyName.Contains("Rate", StringComparison.OrdinalIgnoreCase) ||
        propertyName.Contains("Score", StringComparison.OrdinalIgnoreCase);

    private static bool IsEmail(string propertyName) =>
        propertyName.Contains("Email", StringComparison.OrdinalIgnoreCase);

    private static bool IsPhone(string propertyName) =>
        propertyName.Contains("Phone", StringComparison.OrdinalIgnoreCase) ||
        propertyName.Contains("Number", StringComparison.OrdinalIgnoreCase);

    private static bool IsUrl(string propertyName) =>
        propertyName.Contains("Url", StringComparison.OrdinalIgnoreCase) ||
        propertyName.Contains("Link", StringComparison.OrdinalIgnoreCase) ||
        propertyName.Contains("Website", StringComparison.OrdinalIgnoreCase);
}