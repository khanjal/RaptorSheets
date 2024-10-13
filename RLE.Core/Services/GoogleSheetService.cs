using Google.Apis.Sheets.v4.Data;
using RLE.Core.Constants;
using RLE.Core.Wrappers;

namespace RLE.Core.Services;

public interface IGoogleSheetService
{
    public Task<AppendValuesResponse?> AppendData(ValueRange valueRange, string range);
    public Task<BatchUpdateSpreadsheetResponse?> CreateSheets(BatchUpdateSpreadsheetRequest batchUpdateSpreadsheetRequest);
    public Task<BatchGetValuesByDataFilterResponse?> GetBatchData(List<string> sheets, string? range);
    public Task<ValueRange?> GetSheetData(string sheet);
    public Task<Spreadsheet?> GetSheetInfo();
}

public class GoogleSheetService : IGoogleSheetService
{
    private SheetServiceWrapper _sheetService;
    private readonly string _range = GoogleConfig.Range;

    public GoogleSheetService(SheetServiceWrapper sheetService)
    {
        _sheetService = sheetService;
    }

    public void InitializeService(string accessToken, string spreadsheetId)
    {
        _sheetService.InitializeService(accessToken, spreadsheetId);
    }

    public void InitializeService(Dictionary<string, string> parameters, string spreadsheetId)
    {
        _sheetService.InitializeService(parameters, spreadsheetId);
    }

    public async Task<AppendValuesResponse?> AppendData(ValueRange valueRange, string range)
    {
        try
        {
            return await _sheetService.AppendValues(range, valueRange);
        }
        catch (Exception)
        {
            // Log or return an error?
            return null;
        }
    }

    public async Task<BatchUpdateSpreadsheetResponse?> CreateSheets(BatchUpdateSpreadsheetRequest batchUpdateSpreadsheetRequest)
    {
        try
        {
            return await _sheetService.BatchUpdate(batchUpdateSpreadsheetRequest);
        }
        catch (Exception)
        {
            // Log or return an error?
            return null;
        }
    }

    public async Task<BatchGetValuesByDataFilterResponse?> GetBatchData(List<string> sheets, string? range = "")
    {
        if (sheets == null || sheets.Count < 1)
        {
            return null;
        }

        var body = new BatchGetValuesByDataFilterRequest
        {
            DataFilters = []
        };

        foreach (var sheet in sheets)
        {
            var filter = new DataFilter
            {
                A1Range = !string.IsNullOrWhiteSpace(range) ? $"{sheet}!{range}" : sheet
            };
            body.DataFilters.Add(filter);
        }

        try
        {
            var response = await _sheetService.BatchGetByDataFilter(body);

            return response;
        }
        catch (Exception)
        {
            // TooManyRequests(429) "Quota exceeded for quota metric 'Read requests' and limit 'Read requests per minute per user' of service ..."
            return null;
        }
    }

    public async Task<ValueRange?> GetSheetData(string sheet)
    {
        try
        {
            return await _sheetService.GetValues($"{sheet}!{_range}");
        }
        catch (Exception)
        {
            // NotFound (invalid spreadsheetId/range)
            // BadRequest (invalid sheet name)
            return null;
        }
    }

    public async Task<Spreadsheet?> GetSheetInfo()
    {
        try
        {
            var response = await _sheetService.GetSpreadsheet();

            return response;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
