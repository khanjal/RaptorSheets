using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Wrappers;
using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Core.Services;

public interface IGoogleSheetService
{
    public Task<AppendValuesResponse?> AppendData(ValueRange valueRange, string range);
    public Task<BatchUpdateValuesResponse?> BatchUpdateData(BatchUpdateValuesRequest batchUpdateValuesRequest);
    public Task<BatchUpdateSpreadsheetResponse?> BatchUpdateSpreadsheet(BatchUpdateSpreadsheetRequest batchUpdateSpreadsheetRequest);
    public Task<BatchGetValuesByDataFilterResponse?> GetBatchData(List<string> sheets);
    public Task<BatchGetValuesByDataFilterResponse?> GetBatchData(List<string> sheets, string? range);
    public Task<ValueRange?> GetSheetData(string sheet);
    public Task<Spreadsheet?> GetSheetInfo();
    public Task<Spreadsheet?> GetSheetInfo(List<string>? ranges);
    public Task<UpdateValuesResponse?> UpdateData(ValueRange valueRange, string range);
}

[ExcludeFromCodeCoverage]
public class GoogleSheetService : IGoogleSheetService
{
    private readonly SheetServiceWrapper _sheetService;
    private readonly string _range = GoogleConfig.Range;
    private readonly ILogger _logger;

    public GoogleSheetService(string accessToken, string spreadsheetId, ILogger? logger = null)
    {
        _sheetService = new SheetServiceWrapper(accessToken, spreadsheetId);
        _logger = logger ?? NullLogger.Instance;
    }

    public GoogleSheetService(Dictionary<string, string> parameters, string spreadsheetId, ILogger? logger = null)
    {
        _sheetService = new SheetServiceWrapper(parameters, spreadsheetId);
        _logger = logger ?? NullLogger.Instance;
    }

    public async Task<AppendValuesResponse?> AppendData(ValueRange valueRange, string range)
    {
        try
        {
            var response = await _sheetService.AppendValues(range, valueRange);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error appending data to range '{Range}'", range);
            return null;
        }
    }

    public async Task<BatchUpdateValuesResponse?> BatchUpdateData(BatchUpdateValuesRequest batchUpdateValuesRequest)
    {
        try
        {
            var response = await _sheetService.BatchUpdateData(batchUpdateValuesRequest);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error batch updating values");
            return null;
        }
    }

    public async Task<BatchUpdateSpreadsheetResponse?> BatchUpdateSpreadsheet(BatchUpdateSpreadsheetRequest batchUpdateSpreadsheetRequest)
    {
        try
        {
            var response = await _sheetService.BatchUpdateSpreadsheet(batchUpdateSpreadsheetRequest);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error batch updating spreadsheet");
            return null;
        }
    }

    public async Task<BatchGetValuesByDataFilterResponse?> GetBatchData(List<string> sheets)
    {
        return await GetBatchData(sheets, null);
    }

    public async Task<BatchGetValuesByDataFilterResponse?> GetBatchData(List<string> sheets, string? range)
    {
        if (sheets == null || sheets.Count < 1)
        {
            return null;
        }

        var request = GoogleRequestHelpers.GenerateBatchGetValuesByDataFilterRequest(sheets, range);

        try
        {
            var response = await _sheetService.BatchGetByDataFilter(request);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error batch getting data for sheets '{Sheets}'", string.Join(", ", sheets));
            return null;
        }
    }

    public async Task<ValueRange?> GetSheetData(string sheet)
    {
        if (string.IsNullOrWhiteSpace(sheet)) return null;

        try
        {
            var response = await _sheetService.GetValues($"{sheet}!{_range}");

            return response;
        }
        catch (Exception ex)
        {
            // NotFound (invalid spreadsheetId/range) or BadRequest (invalid sheet name)
            _logger.LogError(ex, "Error getting values for sheet '{Sheet}'", sheet);
            return null;
        }
    }

    public async Task<Spreadsheet?> GetSheetInfo()
    {
        return await GetSheetInfo(null);
    }

    public async Task<Spreadsheet?> GetSheetInfo(List<string>? ranges)
    {
        try
        {
            var response = await _sheetService.GetSpreadsheet(ranges);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sheet info for ranges '{Ranges}'", ranges == null ? "(none)" : string.Join(", ", ranges));
            return null;
        }
    }

    public async Task<UpdateValuesResponse?> UpdateData(ValueRange valueRange, string range)
    {
        try
        {
            var response = await _sheetService.UpdateValues(range, valueRange);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating data for range '{Range}'", range);
            return null;
        }
    }
}
