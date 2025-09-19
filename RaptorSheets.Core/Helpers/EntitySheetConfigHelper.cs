using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;

namespace RaptorSheets.Core.Helpers;

/// <summary>
/// Helper class for generating sheet headers directly from entity ColumnOrder attributes.
/// This eliminates the need for manual header definition in SheetsConfig.
/// </summary>
public static class EntitySheetConfigHelper
{
    /// <summary>
    /// Generates sheet headers from an entity type based on ColumnOrder attributes.
    /// Headers are created in inheritance order (base class properties first).
    /// </summary>
    /// <typeparam name="T">The entity type to generate headers from</typeparam>
    /// <returns>List of SheetCellModel headers in entity-defined order</returns>
    public static List<SheetCellModel> GenerateHeadersFromEntity<T>()
    {
        var entityType = typeof(T);
        var headers = new List<SheetCellModel>();
        var processedHeaders = new HashSet<string>();

        // Get all properties from the inheritance hierarchy (base class first)
        var allProperties = GetPropertiesInInheritanceOrder(entityType);

        // Process properties with ColumnOrder attributes
        foreach (var property in allProperties)
        {
            var columnOrderAttr = property.GetCustomAttribute<ColumnOrderAttribute>();
            if (columnOrderAttr != null && !processedHeaders.Contains(columnOrderAttr.HeaderName))
            {
                headers.Add(new SheetCellModel { Name = columnOrderAttr.HeaderName });
                processedHeaders.Add(columnOrderAttr.HeaderName);
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
    /// <returns>List of SheetCellModel headers with entity headers first</returns>
    public static List<SheetCellModel> GenerateHeadersFromEntity<T>(params SheetCellModel[] additionalHeaders)
    {
        var entityHeaders = GenerateHeadersFromEntity<T>();
        var entityHeaderNames = entityHeaders.Select(h => h.Name).ToHashSet();

        // Add additional headers that aren't already in the entity
        foreach (var additionalHeader in additionalHeaders)
        {
            if (!entityHeaderNames.Contains(additionalHeader.Name))
            {
                entityHeaders.Add(additionalHeader);
            }
        }

        return entityHeaders;
    }

    /// <summary>
    /// Generates sheet headers from an entity type and merges with common header patterns.
    /// </summary>
    /// <typeparam name="T">The entity type to generate headers from</typeparam>
    /// <param name="commonHeaderPatterns">Common header patterns to merge (e.g., CommonIncomeHeaders)</param>
    /// <returns>List of SheetCellModel headers with entity ordering prioritized</returns>
    public static List<SheetCellModel> GenerateHeadersFromEntity<T>(params IEnumerable<SheetCellModel>[] commonHeaderPatterns)
    {
        var entityHeaders = GenerateHeadersFromEntity<T>();
        var entityHeaderNames = entityHeaders.Select(h => h.Name).ToHashSet();

        // Add headers from common patterns that aren't already in the entity
        foreach (var pattern in commonHeaderPatterns)
        {
            foreach (var header in pattern)
            {
                if (!entityHeaderNames.Contains(header.Name))
                {
                    entityHeaders.Add(header);
                    entityHeaderNames.Add(header.Name);
                }
            }
        }

        return entityHeaders;
    }

    /// <summary>
    /// Validates that an entity has the required ColumnOrder attributes for sheet generation.
    /// </summary>
    /// <typeparam name="T">The entity type to validate</typeparam>
    /// <param name="requiredHeaders">Optional list of required header names</param>
    /// <returns>List of validation errors (empty if valid)</returns>
    public static List<string> ValidateEntityForSheetGeneration<T>(IEnumerable<string>? requiredHeaders = null)
    {
        var entityType = typeof(T);
        var errors = new List<string>();
        
        var allProperties = GetPropertiesInInheritanceOrder(entityType);
        var entityHeaders = allProperties
            .Select(p => p.GetCustomAttribute<ColumnOrderAttribute>()?.HeaderName)
            .Where(h => h != null)
            .ToList();

        if (!entityHeaders.Any())
        {
            errors.Add($"Entity '{entityType.Name}' has no properties with ColumnOrder attributes. Cannot generate sheet headers.");
        }

        if (requiredHeaders != null)
        {
            var entityHeaderSet = entityHeaders.ToHashSet();
            foreach (var requiredHeader in requiredHeaders)
            {
                if (!entityHeaderSet.Contains(requiredHeader))
                {
                    errors.Add($"Entity '{entityType.Name}' is missing required header '{requiredHeader}' with ColumnOrder attribute.");
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
                // Skip if we've already processed this property name
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