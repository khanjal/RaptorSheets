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
        try
        {
            var spreadsheetInfo = await GetSheetInfo();
            if (spreadsheetInfo != null && spreadsheetInfo.Sheets != null)
            {
                var existingTitles = spreadsheetInfo.Sheets
                    .Where(s => s?.Properties?.Title != null)
                    .Select(s => s.Properties.Title)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var missingSheets = sheets.Where(s => !existingTitles.Contains(s)).ToList();

                if (missingSheets.Count > 0)
                {
                    var requests = SheetOrderingHelper.BuildAddSheetRequests(spreadsheetInfo, sheets);
                    if (requests != null && requests.Count > 0)
                    {
                        var batchUpdate = new Google.Apis.Sheets.v4.Data.BatchUpdateSpreadsheetRequest
                        {
                            Requests = requests.ToList()
                        };

                        // Attempt to create missing sheets; log but continue regardless of failure
                        var createResponse = await BatchUpdateSpreadsheet(batchUpdate);
                        if (createResponse == null)
                        {
                            Console.WriteLine($"Warning: failed to create missing sheets: {string.Join(',', missingSheets)}");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Do not fail here; we'll still attempt the batch get which will be handled below
            Console.WriteLine($"Warning while ensuring sheets exist: {ex.Message}");
        }

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
            var response = await _sheetService.GetSpreadsheet(ranges);

            return response;
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
