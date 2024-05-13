using GigRaptorLib.Constants;
using GigRaptorLib.Enums;
using GigRaptorLib.Models;
using GigRaptorLib.Utilities.Extensions;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using static Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource;

namespace GigRaptorLib.Utilities.Google;

public interface IGoogleSheetService
{
    public Task<BatchGetValuesByDataFilterResponse?> GetBatchData(string spreadsheetId);
    public Task<ValueRange?> GetSheetData(string spreadsheetId, SheetEnum sheetEnum);
    public Task<Spreadsheet?> GetSheetInfo(string spreadsheetId);
    public Task<AppendValuesResponse?> AppendData(string spreadsheetId, ValueRange valueRange, string range);
    public Task<BatchUpdateSpreadsheetResponse?> CreateSheets(string spreadsheetId, List<SheetModel> sheets);

}


public class GoogleSheetService : IGoogleSheetService
{
    private SheetsService _sheetsService = new();
    private readonly string _range = GoogleConfig.Range;


    public GoogleSheetService(string accessToken)
    {
        var credential = GoogleCredential.FromAccessToken(accessToken);

        InitializeService(credential);
    }

    public GoogleSheetService(Dictionary<string, string> parameters)
    {
        var jsonCredential = new JsonCredentialParameters
        {
            Type = parameters["type"],
            ProjectId = parameters["projectId"],
            PrivateKeyId = parameters["privateKeyId"],
            PrivateKey = parameters["privateKey"],
            ClientEmail = parameters["clientEmail"],
            ClientId = parameters["clientId"],
            TokenUrl = parameters["tokenUrl"]
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

    public async Task<BatchGetValuesByDataFilterResponse?> GetBatchData(string spreadsheetId)
    {
        var body = new BatchGetValuesByDataFilterRequest
        {
            DataFilters = []
        };

        var sheets = Enum.GetValues(typeof(SheetEnum)).Cast<SheetEnum>();

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
            var request = _sheetsService.Spreadsheets.Values.BatchGetByDataFilter(body, spreadsheetId);
            var response = await request.ExecuteAsync();

            return response;
        }
        catch (Exception)
        {
            // TooManyRequests(429) "Quota exceeded for quota metric 'Read requests' and limit 'Read requests per minute per user' of service ..."
            return null;
        }
    }

    public async Task<ValueRange?> GetSheetData(string spreadsheetId, SheetEnum sheetEnum)
    {
        try
        {
            var request = _sheetsService.Spreadsheets.Values.Get(spreadsheetId, $"{sheetEnum.DisplayName()}!{_range}");
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

    public async Task<Spreadsheet?> GetSheetInfo(string spreadsheetId)
    {
        try
        {
            var request = _sheetsService.Spreadsheets.Get(spreadsheetId);
            var response = await request.ExecuteAsync();

            return response;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<AppendValuesResponse?> AppendData(string spreadsheetId, ValueRange valueRange, string range)
    {
        try
        {
            var request = _sheetsService.Spreadsheets.Values.Append(valueRange, spreadsheetId, range);
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

    public async Task<BatchUpdateSpreadsheetResponse?> CreateSheets(string spreadsheetId, List<SheetModel> sheets)
    {
        var batchUpdateSpreadsheetRequest = GenerateSheets.Generate(sheets);

        try
        {
            var request = _sheetsService.Spreadsheets.BatchUpdate(batchUpdateSpreadsheetRequest, spreadsheetId);
            var response = await request.ExecuteAsync();

            return response;
        }
        catch (Exception)
        {
            // Log or return an error?
            return null;
        }
    }
}
