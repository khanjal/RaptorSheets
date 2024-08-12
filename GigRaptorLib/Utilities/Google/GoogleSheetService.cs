using GigRaptorLib.Constants;
using GigRaptorLib.Enums;
using GigRaptorLib.Utilities.Extensions;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using static Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource;

namespace GigRaptorLib.Utilities.Google;

public interface IGoogleSheetService
{
    public Task<AppendValuesResponse?> AppendData(ValueRange valueRange, string range);
    public Task<BatchUpdateSpreadsheetResponse?> CreateSheets(BatchUpdateSpreadsheetRequest batchUpdateSpreadsheetRequest);
    public Task<BatchGetValuesByDataFilterResponse?> GetBatchData(List<SheetEnum> sheets);
    public Task<ValueRange?> GetSheetData(SheetEnum sheetEnum);
    public Task<Spreadsheet?> GetSheetInfo();
}

public class GoogleSheetService : IGoogleSheetService
{
    private readonly SheetsService _sheetsService = new();
    private readonly string _spreadsheetId = "";
    private readonly string _range = GoogleConfig.Range;


    public GoogleSheetService(string accessToken, string spreadsheetId)
    {
        _spreadsheetId = spreadsheetId;
        var credential = GoogleCredential.FromAccessToken(accessToken);

        InitializeService(credential);
    }

    public GoogleSheetService(Dictionary<string, string> parameters, string spreadsheetId)
    {
        _spreadsheetId = spreadsheetId;
        var jsonCredential = new JsonCredentialParameters
        {
            Type = parameters["type"],
            PrivateKeyId = parameters["privateKeyId"],
            PrivateKey = parameters["privateKey"],
            ClientEmail = parameters["clientEmail"],
            ClientId = parameters["clientId"],
        };

        var credential = GoogleCredential.FromJsonParameters(jsonCredential);

        InitializeService(credential);
    }

    private void InitializeService(GoogleCredential credential)
    {
        _sheetsService = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = GoogleConfig.AppName
        });
    }

    public async Task<AppendValuesResponse?> AppendData(ValueRange valueRange, string range)
    {
        try
        {
            var request = _sheetsService.Spreadsheets.Values.Append(valueRange, _spreadsheetId, range);
            request.ValueInputOption = AppendRequest.ValueInputOptionEnum.USERENTERED;
            var response = await request.ExecuteAsync();

            return response;
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
            var request = _sheetsService.Spreadsheets.BatchUpdate(batchUpdateSpreadsheetRequest, _spreadsheetId);
            var response = await request.ExecuteAsync();

            return response;
        }
        catch (Exception)
        {
            // Log or return an error?
            return null;
        }
    }

    public async Task<BatchGetValuesByDataFilterResponse?> GetBatchData(List<SheetEnum> sheets)
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
                A1Range = sheet.DisplayName()
            };
            body.DataFilters.Add(filter);
        }

        try
        {
            var request = _sheetsService.Spreadsheets.Values.BatchGetByDataFilter(body, _spreadsheetId);
            var response = await request.ExecuteAsync();

            return response;
        }
        catch (Exception)
        {
            // TooManyRequests(429) "Quota exceeded for quota metric 'Read requests' and limit 'Read requests per minute per user' of service ..."
            return null;
        }
    }

    public async Task<ValueRange?> GetSheetData(SheetEnum sheetEnum)
    {
        try
        {
            var request = _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, $"{sheetEnum.DisplayName()}!{_range}");
            var response = await request.ExecuteAsync();

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
            var request = _sheetsService.Spreadsheets.Get(_spreadsheetId);
            var response = await request.ExecuteAsync();

            return response;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
