using System.Reflection;
using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Utilities;

namespace RaptorSheets.Core.Mappers;

/// <summary>
/// Provides automatic mapping between entities and Google Sheets data with type-aware conversion
/// Works with ColumnAttribute for comprehensive field configuration
/// </summary>
public static class TypedEntityMapper<T> where T : class, new()
{
    /// <summary>
    /// Maps Google Sheets data to strongly typed entities using Column attributes for conversion
    /// </summary>
    /// <param name="values">Raw Google Sheets data rows</param>
    /// <param name="headers">Header to column index mapping</param>
    /// <returns>List of typed entities</returns>
    public static List<T> MapFromRangeData(IList<IList<object>> values, Dictionary<int, string> headers)
    {
        var entities = new List<T>();
        var columnProperties = TypedFieldUtils.GetColumnProperties<T>();
        
        // Create reverse lookup for header names to column indexes
        var headerToIndex = new Dictionary<string, int>();
        foreach (var kvp in headers)
        {
            headerToIndex[kvp.Value] = kvp.Key;
        }
        
        for (int rowIndex = 0; rowIndex < values.Count; rowIndex++)
        {
            var entity = new T();
            var row = values[rowIndex];
            
            foreach (var (property, columnAttr) in columnProperties)
            {
                var headerName = columnAttr.GetEffectiveHeaderName();
                if (headerToIndex.TryGetValue(headerName, out var columnIndex) && columnIndex < row.Count)
                {
                    var cellValue = row[columnIndex];
                    var convertedValue = TypedFieldUtils.ConvertFromSheetValue(cellValue, property.PropertyType, columnAttr);
                    property.SetValue(entity, convertedValue);
                }
            }
            
            // Set row ID if the entity has one
            SetRowId(entity, rowIndex + 2); // +2 because Google Sheets is 1-indexed and has header row
            
            entities.Add(entity);
        }
        
        return entities;
    }
    
    /// <summary>
    /// Maps strongly typed entities to Google Sheets data format using Column attributes for conversion
    /// </summary>
    /// <param name="entities">List of typed entities</param>
    /// <param name="headerNames">List of header names in column order</param>
    /// <returns>Google Sheets compatible data rows</returns>
    public static IList<IList<object?>> MapToRangeData(List<T> entities, IList<string> headerNames)
    {
        var result = new List<IList<object?>>();
        var columnProperties = TypedFieldUtils.GetColumnProperties<T>();
        
        // Create property lookup by header name
        var propertyLookup = new Dictionary<string, (PropertyInfo Property, ColumnAttribute Column)>();
        foreach (var (property, columnAttr) in columnProperties)
        {
            var headerName = columnAttr.GetEffectiveHeaderName();
            propertyLookup[headerName] = (property, columnAttr);
        }
        
        foreach (var entity in entities)
        {
            var row = new List<object?>();
            
            foreach (var headerName in headerNames)
            {
                object? cellValue = null;
                
                if (propertyLookup.TryGetValue(headerName, out var propertyInfo))
                {
                    var (property, columnAttr) = propertyInfo;
                    var propertyValue = property.GetValue(entity);
                    cellValue = TypedFieldUtils.ConvertToSheetValue(propertyValue, columnAttr);
                }
                
                row.Add(cellValue);
            }
            
            result.Add(row);
        }
        
        return result;
    }
    
    /// <summary>
    /// Maps Google Sheets data to strongly typed entities without header mapping (uses column order)
    /// </summary>
    /// <param name="values">Raw Google Sheets data rows</param>
    /// <param name="startColumnIndex">Starting column index (0-based)</param>
    /// <returns>List of typed entities</returns>
    public static List<T> MapFromRangeDataByIndex(IList<IList<object>> values, int startColumnIndex = 0)
    {
        var entities = new List<T>();
        var columnProperties = TypedFieldUtils.GetColumnProperties<T>();
        
        for (int rowIndex = 0; rowIndex < values.Count; rowIndex++)
        {
            var entity = new T();
            var row = values[rowIndex];
            
            for (int propIndex = 0; propIndex < columnProperties.Count; propIndex++)
            {
                var columnIndex = startColumnIndex + propIndex;
                if (columnIndex < row.Count)
                {
                    var (property, columnAttr) = columnProperties[propIndex];
                    var cellValue = row[columnIndex];
                    var convertedValue = TypedFieldUtils.ConvertFromSheetValue(cellValue, property.PropertyType, columnAttr);
                    property.SetValue(entity, convertedValue);
                }
            }
            
            SetRowId(entity, rowIndex + 2);
            entities.Add(entity);
        }
        
        return entities;
    }
    
    /// <summary>
    /// Gets the header names in the order they should appear in the sheet
    /// </summary>
    /// <returns>List of header names in entity-defined order</returns>
    public static List<string> GetHeaderNames()
    {
        var columnProperties = TypedFieldUtils.GetColumnProperties<T>();
        return columnProperties.Select(p => p.Column.GetEffectiveHeaderName()).ToList();
    }
    
    /// <summary>
    /// Validates that an entity can be properly mapped using its Column attributes
    /// </summary>
    /// <returns>List of validation errors (empty if valid)</returns>
    public static List<string> ValidateEntityMapping()
    {
        return EntitySheetConfigHelper.ValidateEntityForSheetGeneration<T>();
    }
    
    /// <summary>
    /// Gets the format information for all typed fields in the entity
    /// </summary>
    /// <returns>Dictionary mapping header names to format information</returns>
    public static Dictionary<string, (Enums.FormatEnum? Format, string? NumberPattern)> GetFormatInfo()
    {
        var formatInfo = new Dictionary<string, (Enums.FormatEnum? Format, string? NumberPattern)>();
        var columnProperties = TypedFieldUtils.GetColumnProperties<T>();
        
        foreach (var (property, columnAttr) in columnProperties)
        {
            var headerName = columnAttr.GetEffectiveHeaderName();
            var format = TypedFieldUtils.GetFormatFromFieldType(columnAttr.FieldType);
            var pattern = TypedFieldUtils.GetNumberFormatPattern(columnAttr);
            formatInfo[headerName] = (format, pattern);
        }
        
        return formatInfo;
    }
    
    /// <summary>
    /// Sets the RowId property on an entity if it exists
    /// </summary>
    private static void SetRowId(T entity, int rowId)
    {
        var rowIdProperty = typeof(T).GetProperty("RowId");
        if (rowIdProperty != null && rowIdProperty.CanWrite && rowIdProperty.PropertyType == typeof(int))
        {
            rowIdProperty.SetValue(entity, rowId);
        }
    }
}