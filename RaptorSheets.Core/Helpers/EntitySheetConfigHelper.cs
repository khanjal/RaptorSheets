using System.Reflection;
using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Core.Utilities;

namespace RaptorSheets.Core.Helpers;

/// <summary>
/// Helper class for generating sheet headers directly from entity ColumnOrder and Column attributes.
/// This eliminates the need for manual header definition in SheetsConfig and provides automatic formatting.
/// </summary>
public static class EntitySheetConfigHelper
{
    /// <summary>
    /// Generates sheet headers from an entity type based on Column attributes.
    /// Headers are created in inheritance order (base class properties first) with automatic formatting applied.
    /// Automatically infers FieldType from property type if not explicitly specified.
    /// </summary>
    /// <typeparam name="T">The entity type to generate headers from</typeparam>
    /// <returns>List of SheetCellModel headers in entity-defined order with automatic formatting</returns>
    public static List<SheetCellModel> GenerateHeadersFromEntity<T>()
    {
        var entityType = typeof(T);
        var headers = new List<SheetCellModel>();
        var processedHeaders = new HashSet<string>();

        // Get column properties (these have comprehensive configuration)
        var columnProperties = TypedFieldUtils.GetColumnProperties<T>();

        foreach (var (property, columnAttr) in columnProperties)
        {
            // Auto-infer field type from property if not explicitly set
            columnAttr.SetFieldTypeFromProperty(property.PropertyType);

            var headerName = columnAttr.GetEffectiveHeaderName();
            if (!processedHeaders.Contains(headerName))
            {
                var header = CreateHeader(columnAttr, headerName);
                ApplyNotesAndValidation(header, columnAttr);

                headers.Add(header);
                processedHeaders.Add(headerName);
            }
        }

        return headers;
    }

    private static SheetCellModel CreateHeader(ColumnAttribute columnAttr, string headerName)
    {
        var header = new SheetCellModel
        {
            Name = headerName,
            Format = columnAttr.GetEffectiveFormat()
        };

        // Populate FormatPattern - this becomes the single source of truth
        if (!string.IsNullOrEmpty(columnAttr.NumberFormatPattern))
        {
            header.FormatPattern = columnAttr.NumberFormatPattern;

            if (!header.Format.HasValue || header.Format.Value == Enums.FormatEnum.DEFAULT)
            {
                header.Format = InferFormatFromPattern(columnAttr.NumberFormatPattern);
            }
        }
        else if (header.Format.HasValue && header.Format.Value != Enums.FormatEnum.DEFAULT)
        {
            header.FormatPattern = GetPatternForFormat(header.Format.Value);
        }

        return header;
    }

    private static void ApplyNotesAndValidation(SheetCellModel header, ColumnAttribute columnAttr)
    {
        if (!string.IsNullOrEmpty(columnAttr.Note))
        {
            header.Note = columnAttr.Note;
        }

        var numberPattern = TypedFieldUtils.GetNumberFormatPattern(columnAttr);
        if (!string.IsNullOrEmpty(numberPattern) && numberPattern != "@")
        {
            if (string.IsNullOrEmpty(header.Note))
            {
                header.Note = $"NumberFormat:{numberPattern}";
            }
            else
            {
                header.Note += $"\nNumberFormat:{numberPattern}";
            }
        }

        if (columnAttr.EnableValidation)
        {
            var validationPattern = columnAttr.ValidationPattern ??
                Constants.TypedFieldPatterns.GetDefaultPattern(columnAttr.FieldType);
            if (!string.IsNullOrEmpty(validationPattern))
            {
                header.Validation = validationPattern;
            }
        }
    }

    /// <summary>
    /// Generates sheet headers from an entity type and merges with additional headers.
    /// Entity headers come first, followed by additional headers not in the entity.
    /// </summary>
    /// <typeparam name="T">The entity type to generate headers from</typeparam>
    /// <param name="additionalHeaders">Additional headers to include (e.g., calculated columns)</param>
    /// <returns>List of SheetCellModel headers with entity headers first and automatic formatting applied</returns>
    public static List<SheetCellModel> GenerateHeadersFromEntity<T>(params SheetCellModel[] additionalHeaders)
    {
        var entityHeaders = GenerateHeadersFromEntity<T>();
        var entityHeaderNames = entityHeaders.Select(h => h.Name).ToHashSet();

        // Add additional headers that aren't already in the entity
        foreach (var additionalHeader in additionalHeaders.Where(h => !entityHeaderNames.Contains(h.Name)))
        {
            entityHeaders.Add(additionalHeader);
        }

        return entityHeaders;
    }

    /// <summary>
    /// Generates sheet headers from an entity type and merges with common header patterns.
    /// </summary>
    /// <typeparam name="T">The entity type to generate headers from</typeparam>
    /// <param name="commonHeaderPatterns">Common header patterns to merge (e.g., CommonIncomeHeaders)</param>
    /// <returns>List of SheetCellModel headers with entity ordering prioritized and automatic formatting applied</returns>
    public static List<SheetCellModel> GenerateHeadersFromEntity<T>(params IEnumerable<SheetCellModel>[] commonHeaderPatterns)
    {
        var entityHeaders = GenerateHeadersFromEntity<T>();
        var entityHeaderNames = entityHeaders.Select(h => h.Name).ToHashSet();

        // Add headers from common patterns that aren't already in the entity
        foreach (var pattern in commonHeaderPatterns)
        {
            foreach (var header in pattern.Where(h => !entityHeaderNames.Contains(h.Name)))
            {
                entityHeaders.Add(header);
                entityHeaderNames.Add(header.Name);
            }
        }

        return entityHeaders;
    }

    /// <summary>
    /// Validates that an entity has the required Column attributes for sheet generation.
    /// </summary>
    /// <typeparam name="T">The entity type to validate</typeparam>
    /// <param name="requiredHeaders">Optional list of required header names</param>
    /// <returns>List of validation errors (empty if valid)</returns>
    public static List<string> ValidateEntityForSheetGeneration<T>(IEnumerable<string>? requiredHeaders = null)
    {
        var entityType = typeof(T);
        var errors = new List<string>();
        
        var columnProperties = TypedFieldUtils.GetColumnProperties<T>();
        
        var entityHeaders = columnProperties.Select(p => p.Column.GetEffectiveHeaderName()).ToList();

        if (entityHeaders.Count == 0)
        {
            errors.Add($"Entity '{entityType.Name}' has no properties with Column attributes. Cannot generate sheet headers.");
        }

        // Validate Column attributes for consistency
        foreach (var (property, columnAttr) in columnProperties)
        {
            // Validate that the Column attribute matches the property type
            var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
            var isValidType = ValidateFieldTypeMatchesProperty(columnAttr.FieldType, propertyType);
            
            if (!isValidType)
            {
                errors.Add($"Property '{property.Name}' in entity '{entityType.Name}' has Column attribute '{columnAttr.FieldType}' that doesn't match property type '{propertyType.Name}'.");
            }
        }

        if (requiredHeaders != null)
        {
            var entityHeaderSet = entityHeaders.ToHashSet();
            foreach (var requiredHeader in requiredHeaders.Where(rh => !entityHeaderSet.Contains(rh)))
            {
                errors.Add($"Entity '{entityType.Name}' is missing required header '{requiredHeader}' with Column attribute.");
            }
        }

        return errors;
    }

    /// <summary>
    /// Validates that a Column attribute matches the property type
    /// </summary>
    private static bool ValidateFieldTypeMatchesProperty(Enums.FieldType fieldType, Type propertyType)
    {
        return fieldType switch
        {
            Enums.FieldType.String => propertyType == typeof(string),
            Enums.FieldType.Currency or Enums.FieldType.Accounting => propertyType == typeof(decimal) || propertyType == typeof(double) || propertyType == typeof(float),
            Enums.FieldType.DateTime or Enums.FieldType.Time or Enums.FieldType.Duration => propertyType == typeof(DateTime) || propertyType == typeof(DateTimeOffset) || propertyType == typeof(string), // Allow string for date/time fields
            Enums.FieldType.Boolean => propertyType == typeof(bool),
            Enums.FieldType.Number => propertyType == typeof(decimal) || propertyType == typeof(double) || propertyType == typeof(float),
            Enums.FieldType.Integer => propertyType == typeof(int) || propertyType == typeof(long) || propertyType == typeof(short),
            Enums.FieldType.PhoneNumber => propertyType == typeof(long) || propertyType == typeof(string),
            Enums.FieldType.Email => propertyType == typeof(string),
            Enums.FieldType.Url => propertyType == typeof(string),
            Enums.FieldType.Percentage => propertyType == typeof(decimal) || propertyType == typeof(double) || propertyType == typeof(float),
            _ => true // Allow unknown types
        };
    }

    /// <summary>
    /// Gets properties from an entity type, ordered by inheritance hierarchy (base class properties first).
    /// </summary>
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

    /// <summary>
    /// Gets the default number format pattern for a given FormatEnum.
    /// Returns null for formats that don't have a pattern (like TEXT).
    /// </summary>
    private static string? GetPatternForFormat(Enums.FormatEnum format)
    {
        return format switch
        {
            Enums.FormatEnum.ACCOUNTING => Constants.CellFormatPatterns.Accounting,
            Enums.FormatEnum.CURRENCY => Constants.CellFormatPatterns.Currency,
            Enums.FormatEnum.DATE => Constants.CellFormatPatterns.Date,
            Enums.FormatEnum.DISTANCE => Constants.CellFormatPatterns.Distance,
            Enums.FormatEnum.DURATION => Constants.CellFormatPatterns.Duration,
            Enums.FormatEnum.NUMBER => Constants.CellFormatPatterns.Number,
            Enums.FormatEnum.TIME => Constants.CellFormatPatterns.Time,
            Enums.FormatEnum.WEEKDAY => Constants.CellFormatPatterns.Weekday,
            Enums.FormatEnum.TEXT => null, // TEXT doesn't have a pattern
            Enums.FormatEnum.DEFAULT => null,
            _ => null
        };
    }

    /// <summary>
    /// Infers the FormatEnum from a number format pattern.
    /// This allows using formatPattern alone without needing to specify formatType.
    /// </summary>
    private static Enums.FormatEnum InferFormatFromPattern(string pattern)
    {
        // Match against known patterns first
        var exactMatch = TryGetExactPatternMatch(pattern);
        if (exactMatch.HasValue)
            return exactMatch.Value;

        // Pattern-based heuristics for custom patterns
        return InferFormatFromPatternHeuristics(pattern);
    }

    /// <summary>
    /// Attempts to match the pattern against known exact patterns.
    /// </summary>
    private static Enums.FormatEnum? TryGetExactPatternMatch(string pattern)
    {
        if (pattern == Constants.CellFormatPatterns.Accounting) return Enums.FormatEnum.ACCOUNTING;
        if (pattern == Constants.CellFormatPatterns.Currency) return Enums.FormatEnum.CURRENCY;
        if (pattern == Constants.CellFormatPatterns.Date) return Enums.FormatEnum.DATE;
        if (pattern == Constants.CellFormatPatterns.Distance) return Enums.FormatEnum.DISTANCE;
        if (pattern == Constants.CellFormatPatterns.Duration) return Enums.FormatEnum.DURATION;
        if (pattern == Constants.CellFormatPatterns.Number) return Enums.FormatEnum.NUMBER;
        if (pattern == Constants.CellFormatPatterns.Time) return Enums.FormatEnum.TIME;
        if (pattern == Constants.CellFormatPatterns.Weekday) return Enums.FormatEnum.WEEKDAY;
        
        return null;
    }

    /// <summary>
    /// Infers format type using pattern-based heuristics when exact match is not found.
    /// </summary>
    private static Enums.FormatEnum InferFormatFromPatternHeuristics(string pattern)
    {
        // Duration patterns: [h]:mm or similar
        if (IsDurationPattern(pattern))
            return Enums.FormatEnum.DURATION;

        // Time patterns: contain h, m, s with colons
        if (IsTimePattern(pattern))
            return Enums.FormatEnum.TIME;

        // Date patterns: contain y, m, d
        if (IsDatePattern(pattern))
            return Enums.FormatEnum.DATE;

        // Weekday patterns: ddd or dddd
        if (IsWeekdayPattern(pattern))
            return Enums.FormatEnum.WEEKDAY;

        // Currency patterns: start with $ or contain currency symbols
        if (IsCurrencyPattern(pattern))
            return Enums.FormatEnum.CURRENCY;

        // Accounting patterns: complex with underscores and parentheses for negatives
        if (IsAccountingPattern(pattern))
            return Enums.FormatEnum.ACCOUNTING;

        // Percentage patterns: contain %
        if (IsPercentagePattern(pattern))
            return Enums.FormatEnum.PERCENT;

        // Number patterns: contain # or 0 but no special formatting
        if (IsNumberPattern(pattern))
            return Enums.FormatEnum.NUMBER;

        // Default fallback
        return Enums.FormatEnum.NUMBER;
    }

    private static bool IsDurationPattern(string pattern)
    {
        return pattern.Contains("[h]") || pattern.Contains("[m]");
    }

    private static bool IsTimePattern(string pattern)
    {
        return (pattern.Contains("h") || pattern.Contains("m") || pattern.Contains("s")) && 
               pattern.Contains(":") && 
               !pattern.Contains("[");
    }

    private static bool IsDatePattern(string pattern)
    {
        return (pattern.Contains("y") || pattern.Contains("d")) && 
               (pattern.Contains("m") || pattern.Contains("M")) &&
               !pattern.Contains(":");
    }

    private static bool IsWeekdayPattern(string pattern)
    {
        return pattern.Contains("ddd") && 
               !pattern.Contains("y") && 
               !pattern.Contains("/");
    }

    private static bool IsCurrencyPattern(string pattern)
    {
        return pattern.StartsWith("$") || pattern.StartsWith("\"$\"");
    }

    private static bool IsAccountingPattern(string pattern)
    {
        return pattern.Contains("_(") && pattern.Contains("\"$\"");
    }

    private static bool IsPercentagePattern(string pattern)
    {
        return pattern.Contains("%");
    }

    private static bool IsNumberPattern(string pattern)
    {
        return pattern.Contains("#") || pattern.Contains("0");
    }
}