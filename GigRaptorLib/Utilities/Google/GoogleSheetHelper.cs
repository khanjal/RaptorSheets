using GigRaptorLib.Enums;
using GigRaptorLib.Models;
using GigRaptorLib.Utilities.Extensions;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Configuration;
using static Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource;

namespace GigRaptorLib.Utilities.Google;

public class GoogleSheetHelper
{
    private SheetsService _sheetsService;
    private readonly IConfiguration _configuration;
    private readonly string _range = "A1:Z1000";

    public GoogleSheetHelper()
    {
        _configuration = ConfigurationHelper.GetConfiguration();

        var jsonCredential = new JsonCredentialParameters
        {
            Type = _configuration.GetSection("google_credentials:type").Value,
            ProjectId = _configuration.GetSection("google_credentials:project_id").Value,
            PrivateKeyId = _configuration.GetSection("google_credentials:private_key_id").Value,
            PrivateKey = _configuration.GetSection("google_credentials:private_key").Value,
            ClientEmail = _configuration.GetSection("google_credentials:client_email").Value,
            ClientId = _configuration.GetSection("google_credentials:client_id").Value,
            TokenUrl = _configuration.GetSection("google_credentials:token_url").Value
        };

        var credential = GoogleCredential.FromJsonParameters(jsonCredential);

        _sheetsService = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "GigLogger"
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
            // "Quota exceeded for quota metric 'Read requests' and limit 'Read requests per minute per user' of service ..."
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

    public async Task<BatchUpdateSpreadsheetResponse?> BatchUpdate(string spreadsheetId, List<SheetModel> sheets)
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
