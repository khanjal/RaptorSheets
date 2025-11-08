using System.Reflection;
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
            var headerName = columnAttr.GetEffectiveHeaderName();
            if (!processedHeaders.Contains(headerName))
            {
                var header = new SheetCellModel 
                { 
                    Name = headerName,
                    // Apply format from attribute (override or default from field type)
                    Format = columnAttr.GetEffectiveFormat()
                };
                
                // Apply note if specified
                if (!string.IsNullOrEmpty(columnAttr.Note))
                {
                    header.Note = columnAttr.Note;
                }
                
                // Apply custom number format pattern if specified or use default
                var numberPattern = TypedFieldUtils.GetNumberFormatPattern(columnAttr);
                if (!string.IsNullOrEmpty(numberPattern) && numberPattern != "@")
                {
                    // If note is already set, append number format info; otherwise just store it
                    if (string.IsNullOrEmpty(header.Note))
                    {
                        header.Note = $"NumberFormat:{numberPattern}";
                    }
                    else
                    {
                        header.Note += $"\nNumberFormat:{numberPattern}";
                    }
                }
                
                // Apply validation if specified
                if (columnAttr.EnableValidation)
                {
                    var validationPattern = columnAttr.ValidationPattern ?? 
                        Constants.TypedFieldPatterns.GetDefaultValidationPattern(columnAttr.FieldType);
                    if (!string.IsNullOrEmpty(validationPattern))
                    {
                        header.Validation = validationPattern;
                    }
                }
                
                headers.Add(header);
                processedHeaders.Add(headerName);
            }
        }

        return headers;
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
    private static bool ValidateFieldTypeMatchesProperty(Enums.FieldTypeEnum fieldType, Type propertyType)
    {
        return fieldType switch
        {
            Enums.FieldTypeEnum.String => propertyType == typeof(string),
            Enums.FieldTypeEnum.Currency or Enums.FieldTypeEnum.Accounting => propertyType == typeof(decimal) || propertyType == typeof(double) || propertyType == typeof(float),
            Enums.FieldTypeEnum.DateTime or Enums.FieldTypeEnum.Time or Enums.FieldTypeEnum.Duration => propertyType == typeof(DateTime) || propertyType == typeof(DateTimeOffset) || propertyType == typeof(string), // Allow string for date/time fields
            Enums.FieldTypeEnum.Boolean => propertyType == typeof(bool),
            Enums.FieldTypeEnum.Number => propertyType == typeof(decimal) || propertyType == typeof(double) || propertyType == typeof(float),
            Enums.FieldTypeEnum.Integer => propertyType == typeof(int) || propertyType == typeof(long) || propertyType == typeof(short),
            Enums.FieldTypeEnum.PhoneNumber => propertyType == typeof(long) || propertyType == typeof(string),
            Enums.FieldTypeEnum.Email => propertyType == typeof(string),
            Enums.FieldTypeEnum.Url => propertyType == typeof(string),
            Enums.FieldTypeEnum.Percentage => propertyType == typeof(decimal) || propertyType == typeof(double) || propertyType == typeof(float),
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
}