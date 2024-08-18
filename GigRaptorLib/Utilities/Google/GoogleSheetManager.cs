using GigRaptorLib.Constants;
using GigRaptorLib.Entities;
using GigRaptorLib.Enums;
using GigRaptorLib.Mappers;
using GigRaptorLib.Utilities.Extensions;
using Google.Apis.Sheets.v4.Data;

namespace GigRaptorLib.Utilities.Google;

public interface IGoogleSheetManager
{
    public Task<SheetEntity> AddSheetData(List<SheetEnum> sheets, SheetEntity sheetEntity);
    public Task<SheetEntity> CreateSheets();
    public Task<SheetEntity> CreateSheets(List<SheetEnum> sheets);
    public Task<SheetEntity> GetSheet(string sheet);
    public Task<SheetEntity> GetSheets();
    public Task<SheetEntity> GetSheets(List<SheetEnum> sheets);
    public Task<string?> GetSpreadsheetName();
}

public class GoogleSheetManager : IGoogleSheetManager
{
    private readonly GoogleSheetService _googleSheetService;

    public GoogleSheetManager(string accessToken, string spreadsheetId) 
    {
        _googleSheetService = new GoogleSheetService(accessToken, spreadsheetId);
    }

    public GoogleSheetManager(Dictionary<string, string> parameters, string spreadsheetId)
    {
        _googleSheetService = new GoogleSheetService(parameters, spreadsheetId);
    }

    public async Task<SheetEntity> AddSheetData(List<SheetEnum> sheets, SheetEntity sheetEntity)
    {
        foreach (var sheet in sheets)
        {
            var headers = (await _googleSheetService.GetSheetData(sheet))?.Values[0];

            if (headers == null)
                continue;

            IList<IList<object?>> values = [];

            switch (sheet) {
                case SheetEnum.SHIFTS:
                    values = ShiftMapper.MapToRangeData(sheetEntity.Shifts, headers);
                    sheetEntity.Messages.Add(MessageHelper.CreateInfoMessage($"Adding data to {sheet.UpperName()}"));
                    break;

                case SheetEnum.TRIPS:
                    values = TripMapper.MapToRangeData(sheetEntity.Trips, headers);
                    sheetEntity.Messages.Add(MessageHelper.CreateInfoMessage($"Adding data to {sheet.UpperName()}"));
                    break;
                default:
                    // Unsupported sheet.
                    sheetEntity.Messages.Add(MessageHelper.CreateErrorMessage($"Adding data to {sheet.UpperName()} not supported"));
                    break;
            }

            if (values.Any())
            {
                var valueRange = new ValueRange { Values = values };
                var result = await _googleSheetService.AppendData(valueRange, $"{sheet.DisplayName()}!{GoogleConfig.Range}");
                
                if (result == null)
                    sheetEntity.Messages.Add(MessageHelper.CreateErrorMessage($"Unable to add data to {sheet.UpperName()}"));
                else
                    sheetEntity.Messages.Add(MessageHelper.CreateInfoMessage($"Added data to {sheet.UpperName()}"));
            }
            else
            {
                sheetEntity.Messages.Add(MessageHelper.CreateWarningMessage($"No data to add to {sheet.UpperName()}"));
            }
        }

        return sheetEntity;
    }

    public async Task<SheetEntity> CreateSheets()
    {
        var sheets = Enum.GetValues(typeof(SheetEnum)).Cast<SheetEnum>().ToList();
        return await CreateSheets(sheets);
    }

    public async Task<SheetEntity> CreateSheets(List<SheetEnum> sheets)
    {
        var batchUpdateSpreadsheetRequest = GenerateSheetHelper.Generate(sheets);
        var response = await _googleSheetService.CreateSheets(batchUpdateSpreadsheetRequest);

        var sheetEntity = new SheetEntity();

        // No sheets created if null.
        if (response == null)
        {
            foreach (var sheet in sheets)
            {
                sheetEntity.Messages.Add(MessageHelper.CreateErrorMessage($"{sheet.UpperName()} not created"));
            }

            return sheetEntity;
        }

        var sheetTitles = response.Replies.Where(x => x.AddSheet != null).Select(x => x.AddSheet.Properties.Title).ToList();

        foreach (var sheetTitle in sheetTitles)
        {
            sheetEntity.Messages.Add(MessageHelper.CreateInfoMessage($"{sheetTitle.GetValueFromName<SheetEnum>()} created"));
        }

        return sheetEntity;
    }

    public async Task<SheetEntity> GetSheet(string sheet)
    {
        var sheetExists = Enum.TryParse(sheet.ToUpper(), out SheetEnum sheetEnum) && Enum.IsDefined(typeof(SheetEnum), sheetEnum);

        if (!sheetExists)
        {
            return new SheetEntity { Messages = [MessageHelper.CreateErrorMessage($"Sheet {sheet.ToUpperInvariant()} does not exist")] };
        }

        return await GetSheets([sheetEnum]);
    }

    public async Task<SheetEntity> GetSheets()
    {
        // TODO Add check sheets here where it can add missing sheets.

        var sheets = Enum.GetValues(typeof(SheetEnum)).Cast<SheetEnum>().ToList();
        var response = await GetSheets(sheets);

        return response ?? new SheetEntity();
    }

    public async Task<SheetEntity> GetSheets(List<SheetEnum> sheets)
    {
        var data = new SheetEntity();
        var messages = new List<MessageEntity>();
        var stringSheetList = string.Join(", ", sheets.Select(t => t.ToString()));

        var response = await _googleSheetService.GetBatchData(sheets);

        if (response == null)
        {
            messages.Add(MessageHelper.CreateErrorMessage($"Unable to retrieve sheet(s): {stringSheetList}"));
        }
        else
        {
            messages.Add(MessageHelper.CreateInfoMessage($"Retrieved sheet(s): {stringSheetList}"));
            data = SheetHelper.MapData(response);
        }

        // Only get spreadsheet name when all sheets are requested.
        if (sheets.Count < Enum.GetNames(typeof(SheetEnum)).Length)
        {
            data!.Messages = messages;
            return data;
        }

        var spreadsheetName = await GetSpreadsheetName();

        if (spreadsheetName == null)
        {
            messages.Add(MessageHelper.CreateErrorMessage("Unable to get spreadsheet name"));
            // data!.Name = spreadsheetId;
        }
        else
        {
            messages.Add(MessageHelper.CreateInfoMessage($"Retrieved spreadsheet name: {spreadsheetName}"));
            data!.Name = spreadsheetName;
        }

        data!.Messages = messages;
        return data;
    }

    public async Task<string?> GetSpreadsheetName()
    {
        var response = await _googleSheetService.GetSheetInfo();

        if (response == null)
            return null;

        return response.Properties.Title;
    }
}
