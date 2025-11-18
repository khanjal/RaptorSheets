using System.Globalization;
using System.Reflection;
using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Enums;

namespace RaptorSheets.Core.Utilities;

/// <summary>
/// Utility class for handling typed field conversions and formatting
/// Works with the ColumnAttribute for comprehensive field configuration
/// </summary>
public static class TypedFieldUtils
{
    /// <summary>
    /// Gets all properties with Column attributes from a type, ordered by declaration or explicit order
    /// </summary>
    /// <typeparam name="T">The type to analyze</typeparam>
    /// <returns>List of property info with column attributes</returns>
    public static List<(PropertyInfo Property, ColumnAttribute Column)> GetColumnProperties<T>()
    {
        return GetColumnProperties(typeof(T));
    }

    /// <summary>
    /// Gets all properties with Column attributes from a type
    /// </summary>
    /// <param name="type">The type to analyze</param>
    /// <returns>List of property info with column attributes</returns>
    public static List<(PropertyInfo Property, ColumnAttribute Column)> GetColumnProperties(Type type)
    {
        var properties = new List<(PropertyInfo Property, ColumnAttribute Column, int DeclarationOrder)>();
        
        // Get properties in inheritance order (base class first)
        var allProperties = GetPropertiesInInheritanceOrder(type);
        
        for (int i = 0; i < allProperties.Count; i++)
        {
            var property = allProperties[i];
            var columnAttr = property.GetCustomAttribute<ColumnAttribute>();
            if (columnAttr != null)
            {
                // Set the field type from property if not explicitly set
                columnAttr.SetFieldTypeFromProperty(property.PropertyType);
                properties.Add((property, columnAttr, i));
            }
        }
        
        // Sort by explicit order if provided, otherwise by declaration order
        return properties
            .OrderBy(p => p.Column.HasExplicitOrder ? p.Column.Order : p.DeclarationOrder)
            .Select(p => (p.Property, p.Column))
            .ToList();
    }

    /// <summary>
    /// Converts a value from Google Sheets to the target property type using Column information
    /// </summary>
    /// <param name="cellValue">The raw value from Google Sheets</param>
    /// <param name="targetType">The target property type</param>
    /// <param name="columnAttr">The Column attribute with conversion information</param>
    /// <returns>Converted value or null if conversion fails</returns>
    public static object? ConvertFromSheetValue(object? cellValue, Type targetType, ColumnAttribute columnAttr)
    {
        if (cellValue == null)
        {
            return GetDefaultValue(targetType);
        }

        // Special handling for strings - preserve empty strings and whitespace
        if (columnAttr.FieldType == FieldType.String)
        {
            return cellValue.ToString() ?? "";
        }

        // Handle cases where FieldType is DateTime but targetType is string
        if (columnAttr.FieldType == FieldType.DateTime && targetType == typeof(string))
        {
            var dateValue = ParseDateTime(cellValue);
            return dateValue != null ? ((DateTime)dateValue).ToString(CellFormatPatterns.Date, CultureInfo.InvariantCulture) : "";
        }

        // For other types, treat empty/whitespace as null
        if (cellValue is string str && string.IsNullOrWhiteSpace(str))
        {
            return GetDefaultValue(targetType);
        }

        var stringValue = cellValue.ToString() ?? "";

        try
        {
            return columnAttr.FieldType switch
            {
                FieldType.String => stringValue, // Already handled above, but kept for completeness
                FieldType.Currency or FieldType.Accounting => ParseCurrency(stringValue),
                FieldType.PhoneNumber => ParsePhoneNumber(stringValue),
                FieldType.DateTime => ParseDateTime(cellValue),
                FieldType.Boolean => Convert.ToBoolean(cellValue, CultureInfo.InvariantCulture),
                FieldType.Number => ParseNumber(stringValue, targetType),
                FieldType.Integer => ParseInteger(stringValue, targetType),
                FieldType.Percentage => ParsePercentage(stringValue, targetType),
                FieldType.Email => ParseEmail(stringValue),
                FieldType.Url => ParseUrl(stringValue),
                _ => Convert.ChangeType(cellValue, GetNullableUnderlyingType(targetType), CultureInfo.InvariantCulture)
            };
        }
        catch (Exception)
        {
            // Return default value if conversion fails
            return GetDefaultValue(targetType);
        }
    }

    /// <summary>
    /// Converts a property value to Google Sheets format using Column information
    /// </summary>
    /// <param name="value">The property value</param>
    /// <param name="columnAttr">The Column attribute with conversion information</param>
    /// <returns>Value formatted for Google Sheets</returns>
    public static object? ConvertToSheetValue(object? value, ColumnAttribute columnAttr)
    {
        if (value == null)
        {
            return null;
        }

        return columnAttr.FieldType switch
        {
            FieldType.String => value.ToString(),
            FieldType.Currency or FieldType.Accounting => ConvertCurrencyToSheet(value),
            FieldType.PhoneNumber => ConvertPhoneNumberToSheet(value),
            FieldType.DateTime => ConvertDateTimeToSheet(value),
            FieldType.Boolean => Convert.ToBoolean(value, CultureInfo.InvariantCulture),
            FieldType.Number => Convert.ToDouble(value, CultureInfo.InvariantCulture),
            FieldType.Integer => Convert.ToInt64(value, CultureInfo.InvariantCulture),
            FieldType.Percentage => ConvertPercentageToSheet(value),
            FieldType.Email => value.ToString(),
            FieldType.Url => value.ToString(),
            _ => value
        };
    }

    /// <summary>
    /// Gets the format enum that corresponds to a field type
    /// </summary>
    /// <param name="fieldType">The field type</param>
    /// <returns>Corresponding FormatEnum value</returns>
    public static FormatEnum? GetFormatFromFieldType(FieldType? fieldType)
    {
        return fieldType switch
        {
            FieldType.Currency => FormatEnum.CURRENCY,
            FieldType.Accounting => FormatEnum.ACCOUNTING,
            FieldType.DateTime => FormatEnum.DATE,
            FieldType.Number => FormatEnum.NUMBER,
            FieldType.Integer => FormatEnum.NUMBER,
            FieldType.Percentage => FormatEnum.PERCENT,
            FieldType.String or FieldType.Email or FieldType.Url or FieldType.PhoneNumber => FormatEnum.TEXT,
            _ => null
        };
    }

    /// <summary>
    /// Gets the number format pattern for a column attribute
    /// </summary>
    /// <param name="columnAttr">The column attribute</param>
    /// <returns>Number format pattern string (custom or default)</returns>
    public static string GetNumberFormatPattern(ColumnAttribute? columnAttr)
    {
        if (columnAttr == null)
        {
            return "@"; // Default to text format
        }

        return columnAttr.GetEffectiveNumberFormatPattern();
    }

    /// <summary>
    /// Gets the effective header name from a Column attribute
    /// </summary>
    /// <param name="columnAttr">The column attribute</param>
    /// <returns>Effective header name</returns>
    public static string GetEffectiveHeaderName(ColumnAttribute columnAttr)
    {
        return columnAttr.GetEffectiveHeaderName();
    }

    /// <summary>
    /// Creates a JSON property name attribute equivalent for the Column
    /// </summary>
    /// <param name="columnAttr">The column attribute</param>
    /// <returns>JSON property name</returns>
    public static string GetJsonPropertyName(ColumnAttribute columnAttr)
    {
        return columnAttr.JsonPropertyName;
    }

    #region Private Conversion Methods

    private static object? ParseCurrency(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        // Remove currency symbols and parse
        var cleanValue = value.Replace("$", "").Replace(",", "").Trim();
        return decimal.TryParse(cleanValue, NumberStyles.Currency, CultureInfo.InvariantCulture, out var result) ? result : null;
    }

    private static object? ParsePhoneNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        // Remove formatting and extract digits
        var digitsOnly = new string(value.Where(char.IsDigit).ToArray());
        
        // Remove US international code if present
        if (digitsOnly.StartsWith("1") && digitsOnly.Length == 11)
        {
            digitsOnly = digitsOnly[1..];
        }

        return long.TryParse(digitsOnly, out var result) ? result : null;
    }

    private static object? ParseDateTime(object value)
    {
        // Handle Google Sheets serial number format
        if (value is double serialNumber)
        {
            // Google Sheets uses 1900-01-01 as day 1, but Excel/Google treats 1900 as a leap year
            var baseDate = new DateTime(1899, 12, 30);
            return baseDate.AddDays(serialNumber);
        }

        if (value is string dateStr && DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return date;
        }

        return null;
    }

    private static object? ParseNumber(string value, Type targetType)
    {
        if (string.IsNullOrWhiteSpace(value))
            return GetDefaultValue(targetType);

        var underlyingType = GetNullableUnderlyingType(targetType);
        
        if (double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var doubleResult))
        {
            return Convert.ChangeType(doubleResult, underlyingType, CultureInfo.InvariantCulture);
        }

        return GetDefaultValue(targetType);
    }

    private static object? ParseInteger(string value, Type targetType)
    {
        if (string.IsNullOrWhiteSpace(value))
            return GetDefaultValue(targetType);

        var underlyingType = GetNullableUnderlyingType(targetType);
        
        if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
        {
            return Convert.ChangeType(result, underlyingType, CultureInfo.InvariantCulture);
        }

        return GetDefaultValue(targetType);
    }

    private static object? ParsePercentage(string value, Type targetType)
    {
        if (string.IsNullOrWhiteSpace(value))
            return GetDefaultValue(targetType);

        var cleanValue = value.Replace("%", "").Trim();
        if (double.TryParse(cleanValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var result))
        {
            // Convert percentage to decimal (e.g., 50% -> 0.5)
            return Convert.ChangeType(result / 100.0, GetNullableUnderlyingType(targetType), CultureInfo.InvariantCulture);
        }

        return GetDefaultValue(targetType);
    }

    private static string? ParseEmail(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string? ParseUrl(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static object? ConvertCurrencyToSheet(object value)
    {
        return Convert.ToDouble(value, CultureInfo.InvariantCulture);
    }

    private static object? ConvertPhoneNumberToSheet(object value)
    {
        return Convert.ToInt64(value, CultureInfo.InvariantCulture);
    }

    private static object? ConvertDateTimeToSheet(object value)
    {
        if (value is DateTime dateTime)
        {
            // Convert to Google Sheets serial number
            var baseDate = new DateTime(1899, 12, 30);
            return (dateTime - baseDate).TotalDays;
        }
        return value;
    }

    private static object? ConvertPercentageToSheet(object value)
    {
        // Google Sheets expects percentage values as decimals (e.g., 0.5 for 50%)
        return Convert.ToDouble(value, CultureInfo.InvariantCulture);
    }

    private static Type GetNullableUnderlyingType(Type type)
    {
        return Nullable.GetUnderlyingType(type) ?? type;
    }

    private static object? GetDefaultValue(Type type)
    {
        if (type.IsValueType && Nullable.GetUnderlyingType(type) == null)
        {
            return Activator.CreateInstance(type);
        }
        return null;
    }

    private static List<PropertyInfo> GetPropertiesInInheritanceOrder(Type entityType)
    {
        var typeHierarchy = new List<Type>();
        var currentType = entityType;

        // Build inheritance chain from derived to base
        while (currentType != null && currentType != typeof(object))
        {
            typeHierarchy.Add(currentType);
            currentType = currentType.BaseType;
        }

        // Reverse to get base class first
        typeHierarchy.Reverse();

        var orderedProperties = new List<PropertyInfo>();
        var processedProperties = new HashSet<string>();

        // Process properties from base classes first
        foreach (var type in typeHierarchy)
        {
            var properties = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
                .Where(property => !processedProperties.Contains(property.Name));
            foreach (var property in properties)
            {
                orderedProperties.Add(property);
                processedProperties.Add(property.Name);
            }
        }

        return orderedProperties;
    }

    #endregion
}