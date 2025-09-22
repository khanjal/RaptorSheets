using System.Reflection;
using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Models.Google;

namespace RaptorSheets.Core.Helpers;

/// <summary>
/// Helper class for extracting and applying column ordering from entity ColumnOrder attributes.
/// This provides a centralized way to maintain column order consistency across sheets.
/// </summary>
public static class EntityColumnOrderHelper
{
    /// <summary>
    /// Extracts column order from an entity type based on ColumnOrder attributes.
    /// Returns headers in inheritance order (base class properties first).
    /// </summary>
    /// <typeparam name="T">The entity type to extract column order from</typeparam>
    /// <param name="sheetHeaders">Optional existing sheet headers to reorder</param>
    /// <param name="additionalHeaders">Optional additional headers to include at the end</param>
    /// <returns>List of header names in entity-defined order</returns>
    public static List<string> GetColumnOrderFromEntity<T>(
        List<SheetCellModel>? sheetHeaders = null, 
        List<SheetCellModel>? additionalHeaders = null)
    {
        var entityType = typeof(T);
        var columnOrder = new List<string>();
        var processedHeaders = new HashSet<string>();

        // Get all properties from the inheritance hierarchy (base class first)
        var allProperties = GetPropertiesInInheritanceOrder(entityType);

        // Process properties with ColumnOrder attributes
        foreach (var property in allProperties)
        {
            var columnOrderAttr = property.GetCustomAttribute<ColumnOrderAttribute>();
            if (columnOrderAttr != null && processedHeaders.Add(columnOrderAttr.HeaderName))
            {
                columnOrder.Add(columnOrderAttr.HeaderName);
            }
        }

        // Local function to add headers if not already present
        void AddHeaders(IEnumerable<SheetCellModel>? headers)
        {
            if (headers == null) return;
            foreach (var header in headers)
            {
                if (processedHeaders.Add(header.Name))
                {
                    columnOrder.Add(header.Name);
                }
            }
        }

        AddHeaders(additionalHeaders);
        AddHeaders(sheetHeaders);

        return columnOrder;
    }

    /// <summary>
    /// Applies entity-defined column ordering to a list of sheet headers.
    /// Reorders the headers based on ColumnOrder attributes in the entity.
    /// </summary>
    /// <typeparam name="T">The entity type that defines the column order</typeparam>
    /// <param name="headers">The sheet headers to reorder</param>
    /// <param name="additionalHeaders">Optional additional headers to include at the end</param>
    public static void ApplyEntityColumnOrder<T>(
        List<SheetCellModel> headers, 
        List<SheetCellModel>? additionalHeaders = null)
    {
        var entityOrder = GetColumnOrderFromEntity<T>(headers, additionalHeaders);
        var headerDict = headers.ToDictionary(h => h.Name, h => h);
        
        headers.Clear();
        
        foreach (var headerName in entityOrder)
        {
            if (headerDict.TryGetValue(headerName, out var header))
            {
                headers.Add(header);
            }
        }
    }

    /// <summary>
    /// Validates that all ColumnOrder attributes in an entity reference valid header names.
    /// </summary>
    /// <typeparam name="T">The entity type to validate</typeparam>
    /// <param name="availableHeaders">List of valid header names (e.g., from SheetsConfig.HeaderNames)</param>
    /// <returns>List of validation errors (empty if valid)</returns>
    public static List<string> ValidateEntityHeaderMapping<T>(IEnumerable<string> availableHeaders)
    {
        var entityType = typeof(T);
        var availableHeadersSet = availableHeaders.ToHashSet();
        var errors = new List<string>();

        var allProperties = GetPropertiesInInheritanceOrder(entityType);

        foreach (var property in allProperties)
        {
            var columnOrderAttr = property.GetCustomAttribute<ColumnOrderAttribute>();
            if (columnOrderAttr != null && !availableHeadersSet.Contains(columnOrderAttr.HeaderName))
            {
                errors.Add($"Property '{property.Name}' in entity '{entityType.Name}' " +
                            $"references header '{columnOrderAttr.HeaderName}' which is not available in " +
                            $"SheetsConfig.HeaderNames. Please add this header to the constants or " +
                            $"update the ColumnOrder attribute.");
            }
        }

        return errors;
    }

    /// <summary>
    /// Gets properties from an entity type, ordered by inheritance hierarchy (base class properties first).
    /// This ensures that when applying column ordering, base class properties appear before derived class properties.
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
            var properties = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
            
            foreach (var property in properties)
            {
                // Skip if we've already processed this property name (handles overrides)
                if (!processedProperties.Contains(property.Name))
                {
                    orderedProperties.Add(property);
                    processedProperties.Add(property.Name);
                }
            }
        }

        return orderedProperties;
    }
}