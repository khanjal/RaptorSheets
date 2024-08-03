using GigRaptorLib.Constants;
using GigRaptorLib.Entities;
using GigRaptorLib.Enums;
using GigRaptorLib.Mappers;
using GigRaptorLib.Models;
using GigRaptorLib.Utilities.Extensions;
using Google.Apis.Sheets.v4.Data;

namespace GigRaptorLib.Utilities.Google;

public interface IGoogleSheetManger
{
    public Task<bool?> AddSheetData(string spreadsheetId, List<SheetEnum> sheets, SheetEntity sheetEntity);
    public Task<bool?> CreateSheets(string spreadsheetId, List<SheetModel> sheets);
    public Task<SheetEntity?> GetSheets(string spreadsheetId);
    public Task<SheetEntity?> GetSheets(string spreadsheetId, List<SheetEnum> sheets);
    public Task<string?> GetSpreadsheetName(string spreadsheetId);
}

public class GoogleSheetManager : IGoogleSheetManger
{
    private readonly IGoogleSheetService _googleSheetService;

    public GoogleSheetManager(string accessToken) 
    {
        _googleSheetService = new GoogleSheetService(accessToken);
    }

    public GoogleSheetManager(Dictionary<string, string> parameters)
    {
        _googleSheetService = new GoogleSheetService(parameters);
    }

    public async Task<bool?> AddSheetData(string spreadsheetId, List<SheetEnum> sheets, SheetEntity sheetEntity)
    {
        var success = true;

        foreach (var sheet in sheets)
        {
            var headers = (await _googleSheetService.GetSheetData(spreadsheetId, sheet))?.Values[0];

            if (headers == null)
                continue;

            IList<IList<object?>> values = [];

            switch (sheet) {
                case SheetEnum.SHIFTS:
                    values = ShiftMapper.MapToRangeData(sheetEntity.Shifts, headers);
                    break;

                case SheetEnum.TRIPS:
                    values = TripMapper.MapToRangeData(sheetEntity.Trips, headers);
                    break;
                default:
                    // Unsupported sheet.
                    break;
            }

            if (values.Any())
            {
                var valueRange = new ValueRange { Values = values };
                var result = await _googleSheetService.AppendData(spreadsheetId, valueRange, $"{sheet.DisplayName()}!{GoogleConfig.Range}");
                
                if (result == null)
                {
                    success = false;
                }
            }
        }

        return success;
    }

    public async Task<bool?> CreateSheets(string spreadsheetId, List<SheetModel> sheets)
    {
        var batchUpdateSpreadsheetRequest = GenerateSheetHelper.Generate(sheets);
        var response = await _googleSheetService.CreateSheets(spreadsheetId, batchUpdateSpreadsheetRequest);

        if (response != null)
            return true;
        else
            return false;
    }

    public async Task<SheetEntity?> GetSheets(string spreadsheetId)
    {
        var sheets = Enum.GetValues(typeof(SheetEnum)).Cast<SheetEnum>().ToList();
        var response = await GetSheets(spreadsheetId, sheets);

        return response;
    }

    public async Task<SheetEntity?> GetSheets(string spreadsheetId, List<SheetEnum> sheets)
    {
        var response = await _googleSheetService.GetBatchData(spreadsheetId, sheets);

        if (response == null)
            return null;

        var data = SheetHelper.MapData(response);
        data!.Name = await GetSpreadsheetName(spreadsheetId) ?? spreadsheetId;
        return data;
    }

    public async Task<string?> GetSpreadsheetName(string spreadsheetId)
    {
        var response = await _googleSheetService.GetSheetInfo(spreadsheetId);

        if (response == null)
            return null;

        return response.Properties.Title;
    }
}
