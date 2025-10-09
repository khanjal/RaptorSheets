using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models;

namespace RaptorSheets.Core.Validators;

/// <summary>
/// Validates Google Sheets schema against entity definitions
/// Provides functionality similar to GoogleSheetsWrapper's schema validation
/// </summary>
public static class SchemaValidator
{
    /// <summary>
    /// Validates that a sheet's header row matches the expected entity schema
    /// </summary>
    /// <typeparam name="T">The entity type to validate against</typeparam>
    /// <param name="headerRow">The actual header row from Google Sheets</param>
    /// <returns>Schema validation result</returns>
    public static SchemaValidationResult ValidateSheet<T>(IList<object> headerRow)
    {
        var result = new SchemaValidationResult();
        
        if (headerRow == null)
        {
            result.AddError("Header row is null");
            return result;
        }

        var expectedHeaders = EntitySheetConfigHelper.GenerateHeadersFromEntity<T>();
        var actualHeaders = headerRow.Select(h => h?.ToString() ?? "").ToList();

        // Validate entity configuration first
        var entityValidation = ValidateEntityConfiguration<T>();
        if (!entityValidation.IsValid)
        {
            result.Merge(entityValidation);
            return result;
        }

        // Check if all expected headers are present
        foreach (var expectedHeader in expectedHeaders)
        {
            if (!actualHeaders.Contains(expectedHeader.Name, StringComparer.OrdinalIgnoreCase))
            {
                result.AddError($"Missing expected header: '{expectedHeader.Name}'");
            }
        }

        // Check for unexpected headers (warnings only)
        var expectedHeaderNames = expectedHeaders.Select(h => h.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var actualHeader in actualHeaders.Where(h => !string.IsNullOrWhiteSpace(h)))
        {
            if (!expectedHeaderNames.Contains(actualHeader))
            {
                result.AddWarning($"Unexpected header found: '{actualHeader}'");
            }
        }

        // Validate column order if strict ordering is required
        ValidateColumnOrder(expectedHeaders, actualHeaders, result);

        return result;
    }

    /// <summary>
    /// Validates the entity configuration for sheet generation
    /// </summary>
    /// <typeparam name="T">The entity type to validate</typeparam>
    /// <returns>Schema validation result</returns>
    public static SchemaValidationResult ValidateEntityConfiguration<T>()
    {
        var result = new SchemaValidationResult();
        var validationErrors = EntitySheetConfigHelper.ValidateEntityForSheetGeneration<T>();

        foreach (var error in validationErrors)
        {
            result.AddError(error);
        }

        return result;
    }

    /// <summary>
    /// Validates that required headers are present in the entity
    /// </summary>
    /// <typeparam name="T">The entity type to validate</typeparam>
    /// <param name="requiredHeaders">List of required header names</param>
    /// <returns>Schema validation result</returns>
    public static SchemaValidationResult ValidateRequiredHeaders<T>(IEnumerable<string> requiredHeaders)
    {
        var result = new SchemaValidationResult();
        var validationErrors = EntitySheetConfigHelper.ValidateEntityForSheetGeneration<T>(requiredHeaders);

        foreach (var error in validationErrors)
        {
            result.AddError(error);
        }

        return result;
    }

    /// <summary>
    /// Validates that the sheet has the correct structure for batch operations
    /// </summary>
    /// <typeparam name="T">The entity type to validate</typeparam>
    /// <param name="sheetData">The complete sheet data including headers</param>
    /// <param name="expectedMinRows">Minimum expected number of data rows</param>
    /// <returns>Schema validation result</returns>
    public static SchemaValidationResult ValidateSheetStructure<T>(IList<IList<object>> sheetData, int expectedMinRows = 0)
    {
        var result = new SchemaValidationResult();

        if (sheetData == null || sheetData.Count == 0)
        {
            result.AddError("Sheet data is empty");
            return result;
        }

        // Validate headers
        var headerValidation = ValidateSheet<T>(sheetData[0]);
        result.Merge(headerValidation);

        // Validate minimum row count
        var dataRowCount = sheetData.Count - 1; // Subtract header row
        if (dataRowCount < expectedMinRows)
        {
            result.AddError($"Sheet has {dataRowCount} data rows, but expected at least {expectedMinRows}");
        }

        // Validate consistent column count
        if (sheetData.Count > 1)
        {
            var expectedColumnCount = sheetData[0].Count;
            for (int i = 1; i < sheetData.Count; i++)
            {
                if (sheetData[i].Count != expectedColumnCount)
                {
                    result.AddWarning($"Row {i + 1} has {sheetData[i].Count} columns, expected {expectedColumnCount}");
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Validates column order if the entity specifies explicit ordering
    /// </summary>
    private static void ValidateColumnOrder(List<Models.Google.SheetCellModel> expectedHeaders, List<string> actualHeaders, SchemaValidationResult result)
    {
        // This could be enhanced to validate strict column ordering if needed
        // For now, we just ensure all expected headers are present (already done above)
        
        // Future enhancement: validate that columns appear in the expected order
        // when ColumnOrder attributes specify explicit positioning
    }

    /// <summary>
    /// Validates that data types in the sheet are compatible with entity properties
    /// </summary>
    /// <typeparam name="T">The entity type to validate</typeparam>
    /// <param name="sampleDataRow">A sample data row to validate</param>
    /// <param name="headers">Header to column mapping</param>
    /// <returns>Schema validation result</returns>
    public static SchemaValidationResult ValidateDataTypes<T>(IList<object> sampleDataRow, Dictionary<int, string> headers)
    {
        var result = new SchemaValidationResult();

        if (sampleDataRow == null)
        {
            result.AddWarning("No sample data row provided for type validation");
            return result;
        }

        var columnProperties = Utilities.TypedFieldUtils.GetColumnProperties<T>();
        
        foreach (var (property, columnAttr) in columnProperties)
        {
            var headerName = columnAttr.GetEffectiveHeaderName();
            var columnIndex = headers.FirstOrDefault(kvp => kvp.Value == headerName).Key;
            
            if (columnIndex < sampleDataRow.Count)
            {
                var cellValue = sampleDataRow[columnIndex];
                if (cellValue != null && !string.IsNullOrWhiteSpace(cellValue.ToString()))
                {
                    try
                    {
                        var convertedValue = Utilities.TypedFieldUtils.ConvertFromSheetValue(cellValue, property.PropertyType, columnAttr);
                        if (convertedValue == null && property.PropertyType.IsValueType && Nullable.GetUnderlyingType(property.PropertyType) == null)
                        {
                            result.AddWarning($"Column '{headerName}' contains data that may not be compatible with property type '{property.PropertyType.Name}'");
                        }
                    }
                    catch (Exception)
                    {
                        result.AddWarning($"Column '{headerName}' contains data that cannot be converted to property type '{property.PropertyType.Name}'");
                    }
                }
            }
        }

        return result;
    }
}