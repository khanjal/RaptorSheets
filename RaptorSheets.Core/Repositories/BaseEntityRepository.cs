using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Models;
using RaptorSheets.Core.Services;
using RaptorSheets.Core.Validators;
using System.Text;

namespace RaptorSheets.Core.Repositories;

/// <summary>
/// Base repository providing automatic CRUD operations for typed entities
/// Similar to GoogleSheetsWrapper's BaseRepository but with RaptorSheets' enhanced features
/// </summary>
/// <typeparam name="T">The entity type that implements proper ColumnOrder and TypedField attributes</typeparam>
public abstract class BaseEntityRepository<T> where T : class, new()
{
    protected readonly IGoogleSheetService _sheetService;
    protected readonly string _sheetName;
    protected readonly bool _hasHeaderRow;
    protected List<string>? _cachedHeaders;

    /// <summary>
    /// Initializes a new instance of the BaseEntityRepository
    /// </summary>
    /// <param name="sheetService">The Google Sheets service</param>
    /// <param name="sheetName">The name of the sheet to operate on</param>
    /// <param name="hasHeaderRow">Whether the sheet has a header row (default: true)</param>
    public BaseEntityRepository(IGoogleSheetService sheetService, string sheetName, bool hasHeaderRow = true)
    {
        _sheetService = sheetService ?? throw new ArgumentNullException(nameof(sheetService));
        _sheetName = sheetName ?? throw new ArgumentNullException(nameof(sheetName));
        _hasHeaderRow = hasHeaderRow;
    }

    /// <summary>
    /// Gets all entities from the sheet
    /// </summary>
    /// <returns>List of typed entities</returns>
    public virtual async Task<List<T>> GetAllAsync()
    {
        var data = await _sheetService.GetSheetData(_sheetName);
        if (data?.Values == null || data.Values.Count == 0) 
        {
            return new List<T>();
        }

        if (_hasHeaderRow && data.Values.Count == 1)
        {
            // Only header row exists, no data
            return new List<T>();
        }

        var headers = _hasHeaderRow ? HeaderHelpers.ParserHeader(data.Values[0]) : GetDefaultHeaders();
        var dataRows = _hasHeaderRow ? data.Values.Skip(1).ToList() : data.Values.ToList();

        return TypedEntityMapper<T>.MapFromRangeData(dataRows, headers);
    }

    /// <summary>
    /// Adds a new entity to the sheet
    /// </summary>
    /// <param name="entity">The entity to add</param>
    /// <returns>True if successful, false otherwise</returns>
    public virtual async Task<bool> AddAsync(T entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        var headers = await GetHeadersAsync();
        var rowData = TypedEntityMapper<T>.MapToRangeData(new List<T> { entity }, headers);

        var valueRange = new ValueRange { Values = rowData };
        var result = await _sheetService.AppendData(valueRange, $"{_sheetName}!A:Z");

        return result != null;
    }

    /// <summary>
    /// Adds multiple entities to the sheet
    /// </summary>
    /// <param name="entities">The entities to add</param>
    /// <returns>True if successful, false otherwise</returns>
    public virtual async Task<bool> AddRangeAsync(IEnumerable<T> entities)
    {
        if (entities == null) throw new ArgumentNullException(nameof(entities));

        var entitiesList = entities.ToList();
        if (entitiesList.Count == 0) return true;

        var headers = await GetHeadersAsync();
        var rowData = TypedEntityMapper<T>.MapToRangeData(entitiesList, headers);

        var valueRange = new ValueRange { Values = rowData };
        var result = await _sheetService.AppendData(valueRange, $"{_sheetName}!A:Z");

        return result != null;
    }

    /// <summary>
    /// Updates an entity at a specific row index
    /// </summary>
    /// <param name="entity">The entity to update</param>
    /// <param name="rowIndex">The 1-based row index (excluding headers)</param>
    /// <returns>True if successful, false otherwise</returns>
    public virtual async Task<bool> UpdateAsync(T entity, int rowIndex)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (rowIndex < 1) throw new ArgumentOutOfRangeException(nameof(rowIndex), "Row index must be 1 or greater");

        var headers = await GetHeadersAsync();
        var rowData = TypedEntityMapper<T>.MapToRangeData(new List<T> { entity }, headers);

        if (rowData.Count == 0) return false;

        // Calculate actual row number (add 1 for header if present)
        var actualRowNumber = _hasHeaderRow ? rowIndex + 1 : rowIndex;
        var lastColumn = GetLastColumn(headers.Count);
        var range = $"{_sheetName}!A{actualRowNumber}:{lastColumn}{actualRowNumber}";

        var valueRange = new ValueRange { Values = rowData };
        var result = await _sheetService.UpdateData(valueRange, range);

        return result != null;
    }

    /// <summary>
    /// Deletes a row at the specified index
    /// Note: This requires additional Google Sheets API operations for row deletion
    /// </summary>
    /// <param name="rowIndex">The 1-based row index (excluding headers)</param>
    /// <returns>True if successful, false otherwise</returns>
    public virtual async Task<bool> DeleteAsync(int rowIndex)
    {
        // This would require implementing row deletion in GoogleSheetService
        // For now, we can clear the row content
        if (rowIndex < 1) throw new ArgumentOutOfRangeException(nameof(rowIndex), "Row index must be 1 or greater");

        var headers = await GetHeadersAsync();
        var actualRowNumber = _hasHeaderRow ? rowIndex + 1 : rowIndex;
        var lastColumn = GetLastColumn(headers.Count);
        var range = $"{_sheetName}!A{actualRowNumber}:{lastColumn}{actualRowNumber}";

        // Create empty row
        var emptyRow = new List<object?>();
        for (int i = 0; i < headers.Count; i++)
        {
            emptyRow.Add("");
        }

        var valueRange = new ValueRange { Values = new List<IList<object?>> { emptyRow } };
        var result = await _sheetService.UpdateData(valueRange, range);

        return result != null;
    }

    /// <summary>
    /// Validates the sheet schema against the entity definition
    /// </summary>
    /// <returns>Schema validation result</returns>
    public virtual async Task<SchemaValidationResult> ValidateSchemaAsync()
    {
        if (!_hasHeaderRow)
        {
            return new SchemaValidationResult
            {
                IsValid = false,
                Errors = { "Schema validation requires a header row" }
            };
        }

        var data = await _sheetService.GetSheetData(_sheetName);
        if (data?.Values == null || data.Values.Count == 0)
        {
            return new SchemaValidationResult
            {
                IsValid = false,
                Errors = { "Sheet is empty or cannot be accessed" }
            };
        }

        return SchemaValidator.ValidateSheet<T>(data.Values[0]);
    }

    /// <summary>
    /// Gets the count of data rows in the sheet
    /// </summary>
    /// <returns>Number of data rows</returns>
    public virtual async Task<int> GetCountAsync()
    {
        var data = await _sheetService.GetSheetData(_sheetName);
        if (data?.Values == null || data.Values.Count == 0)
        {
            return 0;
        }

        return _hasHeaderRow ? Math.Max(0, data.Values.Count - 1) : data.Values.Count;
    }

    /// <summary>
    /// Creates the sheet headers if they don't exist
    /// </summary>
    /// <returns>True if successful, false otherwise</returns>
    public virtual async Task<bool> InitializeSheetAsync()
    {
        if (!_hasHeaderRow) return true;

        var data = await _sheetService.GetSheetData(_sheetName);
        if (data?.Values != null && data.Values.Count > 0)
        {
            // Sheet already has data
            return true;
        }

        // Create header row
        var headers = TypedEntityMapper<T>.GetHeaderNames();
        var headerRow = headers.Cast<object>().ToList();
        var valueRange = new ValueRange { Values = new List<IList<object>> { headerRow } };

        var result = await _sheetService.UpdateData(valueRange, $"{_sheetName}!A1:Z1");
        return result != null;
    }

    /// <summary>
    /// Gets the format information for the entity's fields
    /// </summary>
    /// <returns>Dictionary of format information by header name</returns>
    public virtual Dictionary<string, (Enums.FormatEnum? Format, string? NumberPattern)> GetFormatInfo()
    {
        return TypedEntityMapper<T>.GetFormatInfo();
    }

    #region Protected Helper Methods

    /// <summary>
    /// Gets the headers for the sheet, either from the actual sheet or from entity definition
    /// </summary>
    protected virtual async Task<List<string>> GetHeadersAsync()
    {
        if (_cachedHeaders != null)
        {
            return _cachedHeaders;
        }

        if (_hasHeaderRow)
        {
            var data = await _sheetService.GetSheetData(_sheetName);
            if (data?.Values?.Count > 0)
            {
                var headers = HeaderHelpers.ParserHeader(data.Values[0]);
                _cachedHeaders = headers.Values.ToList();
                return _cachedHeaders;
            }
        }

        // Fall back to entity-defined headers
        _cachedHeaders = TypedEntityMapper<T>.GetHeaderNames();
        return _cachedHeaders;
    }

    /// <summary>
    /// Gets default headers based on entity definition
    /// </summary>
    protected virtual Dictionary<int, string> GetDefaultHeaders()
    {
        var headers = TypedEntityMapper<T>.GetHeaderNames();
        var result = new Dictionary<int, string>();
        for (int i = 0; i < headers.Count; i++)
        {
            result[i] = headers[i];
        }
        return result;
    }

    /// <summary>
    /// Converts column count to Excel-style column letter
    /// </summary>
    protected virtual string GetLastColumn(int columnCount)
    {
        var columnBuilder = new StringBuilder();
        while (columnCount > 0)
        {
            columnCount--;
            columnBuilder.Insert(0, (char)('A' + columnCount % 26));
            columnCount /= 26;
        }
        return columnBuilder.ToString();
    }

    #endregion
}