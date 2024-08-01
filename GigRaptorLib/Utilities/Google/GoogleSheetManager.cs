using GigRaptorLib.Constants;
using GigRaptorLib.Entities;
using GigRaptorLib.Enums;
using GigRaptorLib.Mappers;
using GigRaptorLib.Models;
using GigRaptorLib.Utilities.Extensions;
using Google.Apis.Sheets.v4.Data;
using Newtonsoft.Json.Linq;

namespace GigRaptorLib.Utilities.Google;

public interface IGoogleSheetManger
{
    public Task<bool?> AddShiftData(string spreadsheetId, List<ShiftEntity> entities);
    public Task<bool?> AddTripData(string spreadsheetId, List<TripEntity> entities);
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
                await _googleSheetService.AppendData(spreadsheetId, valueRange, $"{sheet.DisplayName()}!{GoogleConfig.Range}");
                // TODO: Handle null from AppendData
            }
        }

        return true;
    }

    public async Task<bool?> AddShiftData(string spreadsheetId, List<ShiftEntity> entities)
    {
        var headers = (await _googleSheetService.GetSheetData(spreadsheetId, SheetEnum.SHIFTS))?.Values[0];

        if (headers == null)
        {
            return false;
        }

        var shifts = ShiftMapper.MapToRangeData(entities, headers);

        if (shifts.Count > 0)
        {
            var valueRange = new ValueRange { Values = shifts };
            var response = await _googleSheetService.AppendData(spreadsheetId, valueRange, $"{SheetEnum.SHIFTS.DisplayName()}!{GoogleConfig.Range}");
            
            if (response != null)
                return true;
            else
                return false;
        }

        return false;
    }

    public async Task<bool?> AddTripData(string spreadsheetId, List<TripEntity> entities)
    {
        var headers = (await _googleSheetService.GetSheetData(spreadsheetId, SheetEnum.TRIPS))?.Values[0];

        if (headers == null)
        {
            return false;
        }

        var trips = TripMapper.MapToRangeData(entities, headers);

        if (trips.Count > 0)
        {
            var valueRange = new ValueRange { Values = trips };
            var response = await _googleSheetService.AppendData(spreadsheetId, valueRange, $"{SheetEnum.TRIPS.DisplayName()}!{GoogleConfig.Range}");

            if (response != null)
                return true;
            else
                return false;
        }

        return false;
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
