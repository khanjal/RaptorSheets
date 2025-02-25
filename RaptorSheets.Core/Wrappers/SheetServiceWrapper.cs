using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Constants;
using System.Diagnostics.CodeAnalysis;
using static Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource;

namespace RaptorSheets.Core.Wrappers;

public interface ISheetServiceWrapper
{
    Task<AppendValuesResponse> AppendValues(string range, IList<IList<object>> values);
    Task<AppendValuesResponse> AppendValues(string range, ValueRange valueRange);
    Task<BatchUpdateValuesResponse> BatchUpdateData(BatchUpdateValuesRequest batchUpdateValuesRequest);
    Task<BatchUpdateSpreadsheetResponse> BatchUpdateSpreadsheet(BatchUpdateSpreadsheetRequest batchUpdateSpreadsheetRequest);
    Task<BatchGetValuesByDataFilterResponse> BatchGetByDataFilter(BatchGetValuesByDataFilterRequest batchGetValuesByDataFilterRequest);
    Task<Spreadsheet> GetSpreadsheet(List<string>? ranges);
    Task<ValueRange> GetValues(string range);
    Task<UpdateValuesResponse> UpdateValues(string range, IList<IList<object>> values);
    Task<UpdateValuesResponse> UpdateValues(string range, ValueRange valueRange);
}

[ExcludeFromCodeCoverage]
public class SheetServiceWrapper : SheetsService, ISheetServiceWrapper
{
    private SheetsService _sheetsService = new();
    private string _spreadsheetId = "";

    public SheetServiceWrapper(string accessToken, string spreadsheetId)
    {
        _spreadsheetId = spreadsheetId;
        var credential = GoogleCredential.FromAccessToken(accessToken.Trim());

        InitializeService(credential);
    }

    public SheetServiceWrapper(Dictionary<string, string> parameters, string spreadsheetId)
    {
        _spreadsheetId = spreadsheetId;
        var jsonCredential = new JsonCredentialParameters
        {
            Type = parameters["type"].Trim(),
            PrivateKeyId = parameters["privateKeyId"].Trim(),
            PrivateKey = parameters["privateKey"].Trim(),
            ClientEmail = parameters["clientEmail"].Trim(),
            ClientId = parameters["clientId"].Trim(),
        };

        var credential = GoogleCredential.FromJsonParameters(jsonCredential);

        InitializeService(credential);
    }

    private SheetsService InitializeService(GoogleCredential credential)
    {
        _sheetsService = new SheetsService(new Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = GoogleConfig.AppName
        });

        return _sheetsService;
    }

    public async Task<AppendValuesResponse> AppendValues(string range, IList<IList<object>> values)
    {
        var valueRange = new ValueRange { Values = values };
        return await AppendValues(range, valueRange);
    }

    public async Task<AppendValuesResponse> AppendValues(string range, ValueRange valueRange)
    {
        var request = _sheetsService.Spreadsheets.Values.Append(valueRange, _spreadsheetId, range);
        request.ValueInputOption = AppendRequest.ValueInputOptionEnum.USERENTERED;
        return await request.ExecuteAsync();
    }

    public async Task<BatchGetValuesByDataFilterResponse> BatchGetByDataFilter(BatchGetValuesByDataFilterRequest batchGetValuesByDataFilterRequest)
    {
        var request = _sheetsService.Spreadsheets.Values.BatchGetByDataFilter(batchGetValuesByDataFilterRequest, _spreadsheetId);
        return await request.ExecuteAsync();
    }
    
    public async Task<BatchUpdateValuesResponse> BatchUpdateData(BatchUpdateValuesRequest batchUpdateValuesRequest)
    {
        var request = _sheetsService.Spreadsheets.Values.BatchUpdate(batchUpdateValuesRequest, _spreadsheetId);
        return await request.ExecuteAsync();
    }

    public async Task<BatchUpdateSpreadsheetResponse> BatchUpdateSpreadsheet(BatchUpdateSpreadsheetRequest batchUpdateSpreadsheetRequest)
    {
        var request = _sheetsService.Spreadsheets.BatchUpdate(batchUpdateSpreadsheetRequest, _spreadsheetId);
        return await request.ExecuteAsync();
    }

    public async Task<ValueRange> GetValues(string range)
    {
        var request = _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, range);
        var response = await request.ExecuteAsync();
        return response;
    }

    public async Task<Spreadsheet> GetSpreadsheet(List<string>? ranges = null)
    {
        var request = _sheetsService.Spreadsheets.Get(_spreadsheetId);
        if (ranges?.Count > 0)
        {
            request.IncludeGridData = true;
            request.Ranges = ranges;
        }
        return await request.ExecuteAsync();
    }

    public async Task<UpdateValuesResponse> UpdateValues(string range, IList<IList<object>> values)
    {
        var valueRange = new ValueRange { Values = values };
        return await UpdateValues(range, valueRange);
    }

    public async Task<UpdateValuesResponse> UpdateValues(string range, ValueRange valueRange)
    {
        var request = _sheetsService.Spreadsheets.Values.Update(valueRange, _spreadsheetId, range);
        request.ValueInputOption = UpdateRequest.ValueInputOptionEnum.USERENTERED;
        return await request.ExecuteAsync();
    }
}