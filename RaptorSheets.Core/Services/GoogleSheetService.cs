using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Wrappers;
using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Core.Services;

public interface IGoogleSheetService
{
    public Task<AppendValuesResponse?> AppendData(ValueRange valueRange, string range);
    public Task<BatchUpdateValuesResponse?> BatchUpdateData(BatchUpdateValuesRequest batchUpdateValuesRequest);
    public Task<BatchUpdateSpreadsheetResponse?> BatchUpdateSpreadsheet(BatchUpdateSpreadsheetRequest batchUpdateSpreadsheetRequest);
    public Task<BatchGetValuesByDataFilterResponse?> GetBatchData(List<string> sheets, string? range);
    public Task<ValueRange?> GetSheetData(string sheet);
    public Task<Spreadsheet?> GetSheetInfo();
    public Task<UpdateValuesResponse?> UpdateData(ValueRange valueRange, string range);
}

[ExcludeFromCodeCoverage]
public class GoogleSheetService : IGoogleSheetService
{
    private SheetServiceWrapper _sheetService;
    private readonly string _range = GoogleConfig.Range;


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
        catch (Exception)
        {
            // Log or return an error?
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
        catch (Exception)
        {
            // Log or return an error?
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
            var response = await _sheetService.GetValues($"{sheet}!{_range}");

            return response;
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

    public async Task<UpdateValuesResponse?> UpdateData(ValueRange valueRange, string range)
    {
        try
        {
            var response = await _sheetService.UpdateValues(range, valueRange);

            return response;
        }
        catch (Exception)
        {
            // Log or return an error?
            return null;
        }
    }
}
