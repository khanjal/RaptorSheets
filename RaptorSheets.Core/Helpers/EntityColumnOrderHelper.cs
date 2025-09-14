using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Models.Google;

namespace RaptorSheets.Core.Helpers;

/// <summary>
/// Helper class for extracting sheet column order from entity attributes.
/// Supports inheritance hierarchies and maintains consistent ordering based on SheetOrderAttribute.
/// </summary>
public static class EntityColumnOrderHelper
{
    /// <summary>
    /// Extracts the column order from an entity type based on SheetOrderAttribute decorations.
    /// Processes inheritance hierarchy from base classes up to derived classes.
    /// </summary>
    /// <typeparam name="T">The entity type to analyze</typeparam>
    /// <param name="sheetHeaders">The sheet headers collection to update with proper ordering</param>
    /// <param name="headerOrder">Optional predefined header order from SheetsConfig (used as reference)</param>
    /// <returns>List of header names in the order defined by the entity properties</returns>
    public static List<string> GetColumnOrderFromEntity<T>(
        List<SheetCellModel>? sheetHeaders = null, 
        List<string>? headerOrder = null)
    {
        var entityType = typeof(T);
        var orderedHeaders = new List<string>();
        var processedHeaders = new HashSet<string>();

        // Get all properties from the inheritance hierarchy (base class first)
        var allProperties = GetPropertiesInInheritanceOrder(entityType);

        // Process properties with SheetOrder attributes
        foreach (var property in allProperties)
        {
            var sheetOrderAttr = property.GetCustomAttribute<SheetOrderAttribute>();
            if (sheetOrderAttr != null && !processedHeaders.Contains(sheetOrderAttr.HeaderName))
            {
                orderedHeaders.Add(sheetOrderAttr.HeaderName);
                processedHeaders.Add(sheetOrderAttr.HeaderName);
            }
        }

        // If sheet headers are provided, reorder them to match entity order
        if (sheetHeaders != null)
        {
            ReorderSheetHeaders(sheetHeaders, orderedHeaders, headerOrder);
        }

        return orderedHeaders;
    }

    /// <summary>
    /// Validates that entity properties with SheetOrderAttribute match available sheet headers.
    /// </summary>
    /// <typeparam name="T">The entity type to validate</typeparam>
    /// <param name="availableHeaders">Available header names from SheetsConfig</param>
    /// <returns>List of validation errors (empty if all valid)</returns>
    public static List<string> ValidateEntityHeaderMapping<T>(IEnumerable<string> availableHeaders)
    {
        var entityType = typeof(T);
        var errors = new List<string>();
        var availableHeaderSet = availableHeaders.ToHashSet();
        
        var allProperties = GetPropertiesInInheritanceOrder(entityType);
        
        foreach (var property in allProperties)
        {
            var sheetOrderAttr = property.GetCustomAttribute<SheetOrderAttribute>();
            if (sheetOrderAttr != null)
            {
                if (!availableHeaderSet.Contains(sheetOrderAttr.HeaderName))
                {
                    errors.Add($"Property '{property.Name}' references unknown header '{sheetOrderAttr.HeaderName}'. " +
                             $"Available headers should be from SheetsConfig.HeaderNames constants.");
                }
            }
        }

        return errors;
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
            var properties = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
            
            foreach (var property in properties)
            {
                // Skip if we've already processed this property name (no virtual/override support needed for attributes)
                if (!processedProperties.Contains(property.Name))
                {
                    orderedProperties.Add(property);
                    processedProperties.Add(property.Name);
                }
            }
        }

        return orderedProperties;
    }

    /// <summary>
    /// Reorders sheet headers to match the entity column order.
    /// </summary>
    private static void ReorderSheetHeaders(
        List<SheetCellModel> sheetHeaders, 
        List<string> entityOrder, 
        List<string>? fallbackOrder)
    {
        if (!entityOrder.Any()) return;

        var reorderedHeaders = new List<SheetCellModel>();
        var remainingHeaders = sheetHeaders.ToList();

        // First, add headers in entity order
        foreach (var headerName in entityOrder)
        {
            var header = remainingHeaders.FirstOrDefault(h => h.Name == headerName);
            if (header != null)
            {
                reorderedHeaders.Add(header);
                remainingHeaders.Remove(header);
            }
        }

        // Add any remaining headers (not specified in entity) at the end
        // Use fallback order if provided, otherwise maintain original order
        if (fallbackOrder != null)
        {
            foreach (var headerName in fallbackOrder)
            {
                var header = remainingHeaders.FirstOrDefault(h => h.Name == headerName);
                if (header != null)
                {
                    reorderedHeaders.Add(header);
                    remainingHeaders.Remove(header);
                }
            }
        }

        // Add any truly unordered headers at the end
        reorderedHeaders.AddRange(remainingHeaders);

        // Replace the original list contents
        sheetHeaders.Clear();
        sheetHeaders.AddRange(reorderedHeaders);
    }
}