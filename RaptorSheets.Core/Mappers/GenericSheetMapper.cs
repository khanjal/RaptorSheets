using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Core.Utilities;
using System.Reflection;

namespace RaptorSheets.Core.Mappers;

/// <summary>
/// Generic mapper that uses Column attributes to map between entities and Google Sheets data.
/// Eliminates the need for manual mapper implementations for each entity type.
/// </summary>
public static class GenericSheetMapper<T> where T : class, new()
{
    private static readonly List<(PropertyInfo Property, ColumnAttribute Column)> _columnProperties = 
        TypedFieldUtils.GetColumnProperties<T>();

    private static readonly List<(PropertyInfo Property, ColumnAttribute Column)> _inputColumnProperties = 
        _columnProperties.Where(p => p.Column.IsInput).ToList();

    private static readonly List<(PropertyInfo Property, ColumnAttribute Column)> _outputColumnProperties = 
        _columnProperties.Where(p => p.Column.IsOutput).ToList();

    /// <summary>
    /// Gets all column properties (both input and output)
    /// </summary>
    public static IReadOnlyList<(PropertyInfo Property, ColumnAttribute Column)> GetAllColumnProperties() => 
        _columnProperties.AsReadOnly();

    /// <summary>
    /// Gets only input column properties (user-entered data that should be written to sheets)
    /// </summary>
    public static IReadOnlyList<(PropertyInfo Property, ColumnAttribute Column)> GetInputColumnProperties() => 
        _inputColumnProperties.AsReadOnly();

    /// <summary>
    /// Gets only output column properties (formula/calculated fields that should NOT be written)
    /// </summary>
    public static IReadOnlyList<(PropertyInfo Property, ColumnAttribute Column)> GetOutputColumnProperties() => 
        _outputColumnProperties.AsReadOnly();

    /// <summary>
    /// Maps Google Sheets range data to a list of entities.
    /// Uses Column attributes to determine how to parse each field.
    /// </summary>
    /// <param name="values">Raw data from Google Sheets</param>
    /// <returns>List of typed entities</returns>
    public static List<T> MapFromRangeData(IList<IList<object>> values)
    {
        var entities = new List<T>();
        var headers = new Dictionary<int, string>();
        
        // Filter out empty rows
        values = values?.Where(x => x.Count > 0 && !string.IsNullOrEmpty(x[0]?.ToString())).ToList() ?? new List<IList<object>>();
        
        var rowId = 0;

        foreach (var row in values)
        {
            rowId++;
            
            // First row is headers
            if (rowId == 1)
            {
                headers = HeaderHelpers.ParserHeader(row);
                continue;
            }

            var entity = new T();
            
            // Set RowId if the entity has this property
            var rowIdProperty = typeof(T).GetProperty("RowId");
            if (rowIdProperty != null && rowIdProperty.CanWrite)
            {
                rowIdProperty.SetValue(entity, rowId);
            }

            // Map each property based on Column attribute
            foreach (var (property, columnAttr) in _columnProperties)
            {
                var headerName = columnAttr.GetEffectiveHeaderName();
                var cellValue = GetValueFromSheet(headerName, row, headers, columnAttr.FieldType);
                
                if (cellValue != null && property.CanWrite)
                {
                    property.SetValue(entity, cellValue);
                }
            }
            
            // Set Saved property if it exists
            var savedProperty = typeof(T).GetProperty("Saved");
            if (savedProperty != null && savedProperty.CanWrite && savedProperty.PropertyType == typeof(bool))
            {
                savedProperty.SetValue(entity, true);
            }

            entities.Add(entity);
        }

        return entities;
    }

    /// <summary>
    /// Maps entities to Google Sheets range data (simple object arrays).
    /// Uses Column attributes to determine how to format each field.
    /// Only writes INPUT columns to prevent breaking array formulas on output columns.
    /// </summary>
    /// <param name="entities">List of entities to map</param>
    /// <param name="headers">Header row from the sheet</param>
    /// <returns>Range data suitable for Google Sheets API</returns>
    public static IList<IList<object?>> MapToRangeData(List<T> entities, IList<object> headers)
    {
        var rangeData = new List<IList<object?>>();

        foreach (var entity in entities)
        {
            var objectList = new List<object?>();

            // Map headers explicitly using .Select(...) before iterating
            var mappedHeaders = headers.Select(header => header.ToString()!.Trim());

            foreach (var headerName in mappedHeaders)
            {
                var propertyInfo = _columnProperties.FirstOrDefault(
                    p => p.Column.GetEffectiveHeaderName() == headerName);

                // Only write values for INPUT columns (isInput: true)
                // Output columns (formulas) should remain empty to preserve array formulas
                if (propertyInfo.Property != null && propertyInfo.Column.IsInput)
                {
                    var value = GetValueForSheet(entity, headerName);
                    objectList.Add(value);
                }
                else
                {
                    // Output column or unknown column - write null
                    objectList.Add(null);
                }
            }

            rangeData.Add(objectList);
        }

        return rangeData;
    }

    /// <summary>
    /// Maps entities to Google Sheets RowData (structured format with types).
    /// Uses Column attributes to determine proper data types for Google Sheets.
    /// Only writes INPUT columns to prevent breaking array formulas on output columns.
    /// </summary>
    /// <param name="entities">List of entities to map</param>
    /// <param name="headers">Header row from the sheet</param>
    /// <returns>RowData suitable for batch update operations</returns>
    public static IList<RowData> MapToRowData(List<T> entities, IList<object> headers)
    {
        var rows = new List<RowData>();

        foreach (var entity in entities)
        {
            var rowData = new RowData();
            var cells = new List<CellData>();

            // Map headers explicitly using .Select(...) before iterating
            var mappedHeadersForRowData = headers.Select(header => header.ToString()!.Trim());
            foreach (var headerName in mappedHeadersForRowData)
            {
                var propertyInfo = _columnProperties.FirstOrDefault(
                    p => p.Column.GetEffectiveHeaderName() == headerName);

                // Only write values for INPUT columns (isInput: true)
                // Output columns (formulas) should remain empty to preserve array formulas
                if (propertyInfo.Property != null && propertyInfo.Column.IsInput)
                {
                    var cellData = GetCellDataForSheet(entity, headerName);
                    cells.Add(cellData);
                }
                else
                {
                    // Output column or unknown column - write empty CellData to preserve position
                    // IMPORTANT: Use empty CellData (not null) - Google Sheets API needs explicit
                    // placeholder to maintain column positions. Null would be skipped/optimized away.
                    cells.Add(new CellData());
                }
            }

            rowData.Values = cells;
            rows.Add(rowData);
        }

        return rows;
    }

    /// <summary>
    /// Creates a format row for applying number formats to a sheet.
    /// Uses Column attributes to determine appropriate formats.
    /// </summary>
    /// <param name="headers">Header row from the sheet</param>
    /// <returns>RowData with format information only</returns>
    public static RowData MapToRowFormat(IList<object> headers)
    {
        var rowData = new RowData();
        var cells = new List<CellData>();

        // Map headers explicitly using .Select(...) before iterating
        var mappedHeadersForRowFormat = headers.Select(header => header.ToString()!.Trim());
        cells.AddRange(mappedHeadersForRowFormat.Select(GetFormatForHeader));

        rowData.Values = cells;
        return rowData;
    }

    /// <summary>
    /// Gets the sheet model with headers automatically configured from the entity.
    /// This is the primary entry point that replaces manual mapper GetSheet() methods.
    /// </summary>
    /// <param name="sheetModel">The base sheet model from SheetsConfig</param>
    /// <param name="configureFormulas">Optional action to configure formulas and validation</param>
    /// <returns>Configured sheet model ready for use</returns>
    public static SheetModel GetSheet(SheetModel sheetModel, Action<SheetModel>? configureFormulas = null)
    {
        // Headers are already generated from entity in SheetsConfig
        sheetModel.Headers.UpdateColumns();
        
        // Apply any formula/validation configuration
        configureFormulas?.Invoke(sheetModel);
        
        return sheetModel;
    }

    #region Private Helper Methods

    /// <summary>
    /// Extracts a value from sheet data and converts it to the appropriate type.
    /// </summary>
    private static object? GetValueFromSheet(
        string headerName, 
        IList<object> values, 
        Dictionary<int, string> headers, 
        FieldTypeEnum fieldType)
    {
        return fieldType switch
        {
            FieldTypeEnum.String => HeaderHelpers.GetStringValue(headerName, values, headers),
            FieldTypeEnum.Integer => HeaderHelpers.GetIntValue(headerName, values, headers),
            FieldTypeEnum.Number => HeaderHelpers.GetDecimalValueOrNull(headerName, values, headers),
            FieldTypeEnum.Currency => HeaderHelpers.GetDecimalValueOrNull(headerName, values, headers),
            FieldTypeEnum.Percentage => HeaderHelpers.GetDecimalValueOrNull(headerName, values, headers),
            FieldTypeEnum.Boolean => HeaderHelpers.GetBoolValue(headerName, values, headers),
            FieldTypeEnum.DateTime => HeaderHelpers.GetDateValue(headerName, values, headers),
            FieldTypeEnum.Time => HeaderHelpers.GetStringValue(headerName, values, headers),
            FieldTypeEnum.Duration => HeaderHelpers.GetStringValue(headerName, values, headers),
            FieldTypeEnum.PhoneNumber => HeaderHelpers.GetStringValue(headerName, values, headers),
            FieldTypeEnum.Email => HeaderHelpers.GetStringValue(headerName, values, headers),
            FieldTypeEnum.Url => HeaderHelpers.GetStringValue(headerName, values, headers),
            _ => HeaderHelpers.GetStringValue(headerName, values, headers)
        };
    }

    /// <summary>
    /// Gets a value from an entity for a specific header, formatted for simple range data.
    /// </summary>
    private static object? GetValueForSheet(T entity, string headerName)
    {
        var propertyInfo = _columnProperties.FirstOrDefault(
            p => p.Column.GetEffectiveHeaderName() == headerName);

        if (propertyInfo.Property == null)
        {
            return null;
        }

        var value = propertyInfo.Property.GetValue(entity);
        
        if (value == null)
        {
            return "";
        }

        // For nullable types, return empty string if null
        if (propertyInfo.Property.PropertyType.IsGenericType &&
            propertyInfo.Property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            return value?.ToString() ?? "";
        }

        // For boolean, return the value directly (not as string)
        if (propertyInfo.Column.FieldType == FieldTypeEnum.Boolean)
        {
            return value;
        }

        return value.ToString();
    }

    /// <summary>
    /// Gets CellData with proper type information for structured updates.
    /// </summary>
    private static CellData GetCellDataForSheet(T entity, string headerName)
    {
        var propertyInfo = _columnProperties.FirstOrDefault(
            p => p.Column.GetEffectiveHeaderName() == headerName);

        if (propertyInfo.Property == null)
        {
            return new CellData();
        }

        var value = propertyInfo.Property.GetValue(entity);
        var fieldType = propertyInfo.Column.FieldType;

        return CreateCellData(value, fieldType);
    }

    /// <summary>
    /// Creates CellData with appropriate ExtendedValue based on field type.
    /// </summary>
    private static CellData CreateCellData(object? value, FieldTypeEnum fieldType)
    {
        if (value == null)
        {
            return new CellData { UserEnteredValue = new ExtendedValue { StringValue = null } };
        }

        return fieldType switch
        {
            FieldTypeEnum.Boolean => new CellData 
            { 
                UserEnteredValue = new ExtendedValue { BoolValue = (bool)value } 
            },
            FieldTypeEnum.Integer => new CellData 
            { 
                UserEnteredValue = new ExtendedValue { NumberValue = Convert.ToDouble(value) } 
            },
            FieldTypeEnum.Number or FieldTypeEnum.Currency or FieldTypeEnum.Percentage => new CellData 
            { 
                UserEnteredValue = new ExtendedValue { NumberValue = Convert.ToDouble(value) } 
            },
            FieldTypeEnum.DateTime => new CellData 
            { 
                UserEnteredValue = new ExtendedValue { NumberValue = value.ToString()!.ToSerialDate() } 
            },
            FieldTypeEnum.Time => new CellData 
            { 
                UserEnteredValue = new ExtendedValue { NumberValue = value.ToString()!.ToSerialTime() } 
            },
            FieldTypeEnum.Duration => new CellData 
            { 
                UserEnteredValue = new ExtendedValue { NumberValue = value.ToString()!.ToSerialDuration() } 
            },
            _ => new CellData 
            { 
                UserEnteredValue = new ExtendedValue { StringValue = value.ToString() } 
            }
        };
    }

    /// <summary>
    /// Gets format information for a specific header.
    /// </summary>
    private static CellData GetFormatForHeader(string headerName)
    {
        var propertyInfo = _columnProperties.FirstOrDefault(
            p => p.Column.GetEffectiveHeaderName() == headerName);

        if (propertyInfo.Property == null)
        {
            return new CellData();
        }

        var format = TypedFieldUtils.GetFormatFromFieldType(propertyInfo.Column.FieldType);
        
        if (!format.HasValue || format.Value == FormatEnum.DEFAULT)
        {
            return new CellData();
        }

        return new CellData { UserEnteredFormat = SheetHelpers.GetCellFormat(format.Value) };
    }

    #endregion
}
