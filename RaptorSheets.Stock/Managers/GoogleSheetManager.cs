using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Services;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Interfaces;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Stock.Entities;
using RaptorSheets.Stock.Enums;
using RaptorSheets.Stock.Mappers;
using RaptorSheets.Stock.Helpers;

namespace RaptorSheets.Stock.Managers;

public interface IGoogleSheetManager : ISheetManager
{
    public Task<SheetEntity> AddSheetData(List<SheetEnum> sheets, SheetEntity sheetEntity);
    public Task<SheetEntity> CreateSheets();
    public Task<SheetEntity> CreateSheets(List<SheetEnum> sheets);
    public Task<SheetEntity> GetSheet(string sheet);
    public Task<SheetEntity> GetSheets();
    public Task<SheetEntity> GetSheets(List<SheetEnum> sheets);
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
            var headers = (await _googleSheetService.GetSheetData(sheet.GetDescription()))?.Values[0];

            if (headers == null)
                continue;

            IList<IList<object?>> values = [];

            switch (sheet)
            {
                case SheetEnum.ACCOUNTS:
                    //values = ShiftMapper.MapToRangeData(sheetEntity.Shifts, headers);
                    sheetEntity.Messages.Add(MessageHelpers.CreateInfoMessage($"Adding data to {sheet.UpperName()}", MessageTypeEnum.ADD_DATA));
                    break;

                default:
                    // Unsupported sheet.
                    sheetEntity.Messages.Add(MessageHelpers.CreateErrorMessage($"Adding data to {sheet.UpperName()} not supported", MessageTypeEnum.ADD_DATA));
                    break;
            }

            if (values.Any())
            {
                var valueRange = new ValueRange { Values = values };
                var result = await _googleSheetService.AppendData(valueRange, $"{sheet.GetDescription()}!{GoogleConfig.Range}");

                if (result == null)
                    sheetEntity.Messages.Add(MessageHelpers.CreateErrorMessage($"Unable to add data to {sheet.UpperName()}", MessageTypeEnum.ADD_DATA));
                else
                    sheetEntity.Messages.Add(MessageHelpers.CreateInfoMessage($"Added data to {sheet.UpperName()}", MessageTypeEnum.ADD_DATA));
            }
            else
            {
                sheetEntity.Messages.Add(MessageHelpers.CreateWarningMessage($"No data to add to {sheet.UpperName()}", MessageTypeEnum.ADD_DATA));
            }
        }

        return sheetEntity;
    }

    public async Task<List<MessageEntity>> CheckSheets()
    {
        return await CheckSheets(false);
    }

    public async Task<List<MessageEntity>> CheckSheets(bool checkHeaders)
    {
        var messages = new List<MessageEntity>();

        var sheetTitles = Enum.GetNames(typeof(SheetEnum)).ToList();
        var ranges = sheetTitles.Select(title => $"{title}!{GoogleConfig.HeaderRange}").ToList();

        var sheetInfoResponse = await _googleSheetService.GetSheetInfo(ranges);

        if (sheetInfoResponse == null)
        {
            messages.Add(MessageHelpers.CreateErrorMessage($"Unable to find spreadsheet", MessageTypeEnum.CHECK_SHEET));
            return messages;
        }

        var spreadsheetSheets = sheetInfoResponse.Sheets.Select(x => x.Properties.Title.ToUpper()).ToList();
        var sheets = new List<SheetEnum>();

        var missingSheetMessages = new List<MessageEntity>();
        // Loop through all sheets to see if they exist.
        foreach (var name in Enum.GetNames<SheetEnum>())
        {
            SheetEnum sheetEnum = (SheetEnum)Enum.Parse(typeof(SheetEnum), name);

            if (!spreadsheetSheets.Contains(name))
            {
                missingSheetMessages.Add(MessageHelpers.CreateErrorMessage($"Unable to find sheet {name}", MessageTypeEnum.CHECK_SHEET));
                continue;
            }

            sheets.Add(sheetEnum);
        }

        if (missingSheetMessages.Count > 0)
        {
            messages.AddRange(missingSheetMessages);
        }
        else
        {
            messages.Add(MessageHelpers.CreateInfoMessage("All sheets found", MessageTypeEnum.CHECK_SHEET));
        }

        if (!checkHeaders)
            return messages;

        messages.AddRange(CheckSheetHeaders(sheetInfoResponse));

        return messages;
    }

    public List<MessageEntity> CheckSheetHeaders(Spreadsheet sheetInfoResponse)
    {
        var messages = new List<MessageEntity>();

        if (sheetInfoResponse == null)
        {
            messages.Add(MessageHelpers.CreateErrorMessage($"Unable to retrieve sheet(s)", MessageTypeEnum.GENERAL));
            return messages;
        }

        var headerMessages = new List<MessageEntity>();
        // Loop through sheets to check headers.
        foreach (var sheet in sheetInfoResponse.Sheets)
        {
            var sheetEnum = (SheetEnum)Enum.Parse(typeof(SheetEnum), sheet.Properties.Title.ToUpper());
            var sheetHeader = HeaderHelpers.GetHeadersFromCellData(sheet.Data?[0]?.RowData?[0]?.Values);

            switch (sheetEnum)
            {
                case SheetEnum.ACCOUNTS:
                    // headerMessages.AddRange(HeaderHelper.CheckSheetHeaders(sheetHeader, AccountMapper.GetSheet()));
                    break;
                case SheetEnum.STOCKS:
                    headerMessages.AddRange(HeaderHelpers.CheckSheetHeaders(sheetHeader, StockMapper.GetSheet()));
                    break;
                case SheetEnum.TICKERS:
                    // headerMessages.AddRange(HeaderHelper.CheckSheetHeaders(sheetHeader, TickerMapper.GetSheet()));
                    break;
                default:
                    break;
            }
        }

        if (headerMessages.Count > 0)
        {
            messages.Add(MessageHelpers.CreateWarningMessage($"Found sheet header issue(s)", MessageTypeEnum.CHECK_SHEET));
            messages.AddRange(headerMessages);
        }
        else
        {
            messages.Add(MessageHelpers.CreateInfoMessage($"No sheet header issues found", MessageTypeEnum.CHECK_SHEET));
        }

        return messages;
    }

    public async Task<SheetEntity> CreateSheets()
    {
        var sheets = Enum.GetValues(typeof(SheetEnum)).Cast<SheetEnum>().ToList();
        return await CreateSheets(sheets);
    }

    public async Task<SheetEntity> CreateSheets(List<SheetEnum> sheets)
    {
        var batchUpdateSpreadsheetRequest = GenerateSheetHelpers.Generate(sheets);
        var response = await _googleSheetService.BatchUpdateSpreadsheet(batchUpdateSpreadsheetRequest);

        var sheetEntity = new SheetEntity();

        // No sheets created if null.
        if (response == null)
        {
            foreach (var sheet in sheets)
            {
                sheetEntity.Messages.Add(MessageHelpers.CreateErrorMessage($"{sheet.UpperName()} not created", MessageTypeEnum.CREATE_SHEET));
            }

            return sheetEntity;
        }

        var sheetTitles = response.Replies.Where(x => x.AddSheet != null).Select(x => x.AddSheet.Properties.Title).ToList();

        foreach (var sheetTitle in sheetTitles)
        {
            sheetEntity.Messages.Add(MessageHelpers.CreateInfoMessage($"{sheetTitle.GetValueFromName<SheetEnum>()} created", MessageTypeEnum.CREATE_SHEET));
        }

        return sheetEntity;
    }

    public async Task<SheetEntity> GetSheet(string sheet)
    {
        var sheetExists = Enum.TryParse(sheet.ToUpper(), out SheetEnum sheetEnum) && Enum.IsDefined(typeof(SheetEnum), sheetEnum);

        if (!sheetExists)
        {
            return new SheetEntity { Messages = [MessageHelpers.CreateErrorMessage($"Sheet {sheet.ToUpperInvariant()} does not exist", MessageTypeEnum.GET_SHEETS)] };
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

        var response = await _googleSheetService.GetBatchData(sheets.Select(x => x.GetDescription()).ToList());

        if (response == null)
        {
            messages.Add(MessageHelpers.CreateErrorMessage($"Unable to retrieve sheet(s): {stringSheetList}", MessageTypeEnum.GET_SHEETS));
        }
        else
        {
            messages.Add(MessageHelpers.CreateInfoMessage($"Retrieved sheet(s): {stringSheetList}", MessageTypeEnum.GET_SHEETS));
            data = StockSheetHelpers.MapData(response);
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
            messages.Add(MessageHelpers.CreateErrorMessage("Unable to get spreadsheet name", MessageTypeEnum.GENERAL));
        }
        else
        {
            messages.Add(MessageHelpers.CreateInfoMessage($"Retrieved spreadsheet name: {spreadsheetName}", MessageTypeEnum.GENERAL));
            data!.Properties.Name = spreadsheetName;
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
