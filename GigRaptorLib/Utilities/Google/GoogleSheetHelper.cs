using GigRaptorLib.Enums;
using GigRaptorLib.Utilities.Extensions;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace GigRaptorLib.Utilities.Google;

public class GoogleSheetHelper
{
    private SheetsService _sheetsService;
    private readonly string? _spreadsheetId;
    private readonly IConfiguration _configuration;

    public GoogleSheetHelper()
    {
        _configuration = new ConfigurationBuilder()
            .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
            .Build();

        _spreadsheetId = _configuration.GetSection("spreadsheet_id").Value;

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
    public async Task GetAllData()
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

        var batchGetRequest = _sheetsService.Spreadsheets.Values.BatchGetByDataFilter(body, _spreadsheetId);
        var batchResponse = await batchGetRequest.ExecuteAsync();
        var values = batchResponse.ValueRanges;
    }
}
