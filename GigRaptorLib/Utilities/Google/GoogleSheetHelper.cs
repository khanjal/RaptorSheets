using GigRaptorLib.Enums;
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
    public async Task<IList<MatchedValueRange>?> GetAllData(string spreadsheetId)
    {
        var body = new BatchGetValuesByDataFilterRequest
        {
            DataFilters = []
        };

        var sheets = Enum.GetValues(typeof(SheetEnum)).Cast<SheetEnum>();

        foreach (var sheet in sheets)
        {
            var filter = new DataFilter();
            filter.A1Range = sheet.DisplayName();
            body.DataFilters.Add(filter);
        }

        try
        {
            var batchGetRequest = _sheetsService.Spreadsheets.Values.BatchGetByDataFilter(body, spreadsheetId);
            var batchResponse = await batchGetRequest.ExecuteAsync();
            var values = batchResponse.ValueRanges;

            return values;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            var message = ex.Message.Split(". ");
            // "Quota exceeded for quota metric 'Read requests' and limit 'Read requests per minute per user' of service ..."
            // _sheet.Errors.Add($"{message.LastOrDefault().Trim()}");
            return null;
        }
    }

    public async Task<IList<IList<object>>?> GetSheetData(string spreadsheetId, SheetEnum sheetEnum)
    {
        var getRequest = _sheetsService.Spreadsheets.Values.Get(spreadsheetId, $"{sheetEnum.DisplayName()}!{_range}");

        try
        {
            var getResponse = await getRequest.ExecuteAsync();
            IList<IList<Object>> values = getResponse.Values;

            return values;
        }
        catch (Exception ex)
        {
            // _sheet.Errors.Add($"Failed to find sheet: {sheetRange}");
            return null;
        }
    }

    public async Task<SpreadsheetProperties?> GetSheetProperties(string spreadsheetId)
    {
        try
        {
            var getRequest = _sheetsService.Spreadsheets.Get(spreadsheetId);

            var getResponse = await getRequest.ExecuteAsync();
            var properties = getResponse.Properties;

            return properties;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public void AppendData(string spreadsheetId, ValueRange valueRange, string range)
    {
        try
        {
            var appendRequest = _sheetsService.Spreadsheets.Values.Append(valueRange, spreadsheetId, range);
            appendRequest.ValueInputOption = AppendRequest.ValueInputOptionEnum.USERENTERED;
            appendRequest.Execute();
        }
        catch (Exception ex)
        {
            // Log or return an error?
        }
    }
}
