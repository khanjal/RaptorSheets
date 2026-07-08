using Google.Apis.Sheets.v4.Data;
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
    // Lightweight in-memory cache to avoid repeated GetSpreadsheet calls
    private Spreadsheet? _cachedSpreadsheet;
    private readonly object _cacheLock = new object();


    public GoogleSheetService(string accessToken, string spreadsheetId)
    {
        _sheetService = new SheetServiceWrapper(accessToken, spreadsheetId);
    }

    public GoogleSheetService(Dictionary<string, string> parameters, string spreadsheetId)
    {
        _sheetService = new SheetServiceWrapper(parameters, spreadsheetId);
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
            Console.WriteLine($"Error: {ex.Message}");
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
            Console.WriteLine($"Error: {ex.Message}");
            return null;
        }
    }

    public async Task<BatchUpdateSpreadsheetResponse?> BatchUpdateSpreadsheet(BatchUpdateSpreadsheetRequest batchUpdateSpreadsheetRequest)
    {
        try
        {
            var response = await _sheetService.BatchUpdateSpreadsheet(batchUpdateSpreadsheetRequest);

            // Invalidate cached spreadsheet information when modifications are made
            if (response != null)
            {
                lock (_cacheLock)
                {
                    _cachedSpreadsheet = null;
                }
            }

            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
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
        // Ensure missing sheets are created first to avoid invalid A1 range parsing errors
        await SheetInitializationHelper.EnsureMissingSheetsCreatedAsync(this, sheets);

        var request = GoogleRequestHelpers.GenerateBatchGetValuesByDataFilterRequest(sheets, range);

        try
        {
            var response = await _sheetService.BatchGetByDataFilter(request);

            return response;
        }
        catch (Exception ex)
        {
            // TooManyRequests(429) "Quota exceeded for quota metric 'Read requests' and limit 'Read requests per minute per user' of service ..."
            Console.WriteLine($"Error: {ex.Message}");
            return null;
        }
    }
   
    public async Task<ValueRange?> GetSheetData(string sheet)
    {
        try
        {
            var response = await _sheetService.GetValues($"{sheet}!{_range}");

            return response;
        }
        catch (Exception ex)
        {
            // NotFound (invalid spreadsheetId/range)
            // BadRequest (invalid sheet name)
            Console.WriteLine($"Error: {ex.Message}");
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
            // If no ranges are requested, return cached spreadsheet info when available
            if (ranges == null)
            {
                lock (_cacheLock)
                {
                    if (_cachedSpreadsheet != null)
                    {
                        return _cachedSpreadsheet;
                    }
                }

                var response = await _sheetService.GetSpreadsheet(ranges);
                if (response != null)
                {
                    lock (_cacheLock)
                    {
                        _cachedSpreadsheet = response;
                    }
                }

                return response;
            }

            // When specific ranges are requested, do not use the cached full-spreadsheet object
            var rangedResponse = await _sheetService.GetSpreadsheet(ranges);
            return rangedResponse;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
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
            Console.WriteLine($"Error: {ex.Message}");
            return null;
        }
    }
}
