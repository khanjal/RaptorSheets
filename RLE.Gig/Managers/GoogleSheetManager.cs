using Google.Apis.Sheets.v4.Data;
using RLE.Core.Entities;
using RLE.Gig.Enums;
using RLE.Gig.Mappers;
using RLE.Core.Constants;
using RLE.Gig.Entities;
using RLE.Core.Services;
using RLE.Core.Enums;
using RLE.Core.Extensions;
using RLE.Core.Interfaces;
using RLE.Gig.Helpers;
using RLE.Core.Helpers;

namespace RLE.Gig.Managers;

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
                case SheetEnum.SHIFTS:
                    values = ShiftMapper.MapToRangeData(sheetEntity.Shifts, headers);
                    sheetEntity.Messages.Add(MessageHelper.CreateInfoMessage($"Adding data to {sheet.UpperName()}", MessageTypeEnum.AddData));
                    break;

                case SheetEnum.TRIPS:
                    values = TripMapper.MapToRangeData(sheetEntity.Trips, headers);
                    sheetEntity.Messages.Add(MessageHelper.CreateInfoMessage($"Adding data to {sheet.UpperName()}", MessageTypeEnum.AddData));
                    break;
                default:
                    // Unsupported sheet.
                    sheetEntity.Messages.Add(MessageHelper.CreateErrorMessage($"Adding data to {sheet.UpperName()} not supported", MessageTypeEnum.AddData));
                    break;
            }

            if (values.Any())
            {
                var valueRange = new ValueRange { Values = values };
                var result = await _googleSheetService.AppendData(valueRange, $"{sheet.GetDescription()}!{GoogleConfig.Range}");

                if (result == null)
                    sheetEntity.Messages.Add(MessageHelper.CreateErrorMessage($"Unable to add data to {sheet.UpperName()}", MessageTypeEnum.AddData));
                else
                    sheetEntity.Messages.Add(MessageHelper.CreateInfoMessage($"Added data to {sheet.UpperName()}", MessageTypeEnum.AddData));
            }
            else
            {
                sheetEntity.Messages.Add(MessageHelper.CreateWarningMessage($"No data to add to {sheet.UpperName()}", MessageTypeEnum.AddData));
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
        var sheetInfoResponse = await _googleSheetService.GetSheetInfo();

        if (sheetInfoResponse == null)
        {
            messages.Add(MessageHelper.CreateErrorMessage($"Unable to find spreadsheet", MessageTypeEnum.CheckSheet));
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
                missingSheetMessages.Add(MessageHelper.CreateErrorMessage($"Unable to find sheet {name}", MessageTypeEnum.CheckSheet));
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
            messages.Add(MessageHelper.CreateInfoMessage("All sheets found", MessageTypeEnum.CheckSheet));
        }

        if (!checkHeaders)
            return messages;

        messages.AddRange(await CheckSheetHeaders(sheets.Select(x => x.GetDescription()).ToList()));

        return messages;
    }

    public async Task<List<MessageEntity>> CheckSheetHeaders(List<string> sheets)
    {
        var messages = new List<MessageEntity>();
        // Get sheet headers
        var stringSheetList = string.Join(", ", sheets.Select(t => t.ToString()));
        var batchDataResponse = await _googleSheetService.GetBatchData(sheets, GoogleConfig.HeaderRange);

        if (batchDataResponse == null)
        {
            messages.Add(MessageHelper.CreateErrorMessage($"Unable to retrieve sheet(s): {stringSheetList}", MessageTypeEnum.GetSheets));
            return messages;
        }

        var headerMessages = new List<MessageEntity>();
        // Loop through sheets to check headers.
        foreach (var valueRange in batchDataResponse.ValueRanges)
        {
            var sheetRange = valueRange.ValueRange.Range;
            var sheet = sheetRange.Split("!")[0];
            var sheetEnum = (SheetEnum)Enum.Parse(typeof(SheetEnum), sheet.ToUpper());

            var sheetHeader = valueRange.ValueRange.Values;
            switch (sheetEnum)
            {
                case SheetEnum.ADDRESSES:
                    headerMessages.AddRange(HeaderHelper.CheckSheetHeaders(sheetHeader, AddressMapper.GetSheet()));
                    break;
                case SheetEnum.DAILY:
                    headerMessages.AddRange(HeaderHelper.CheckSheetHeaders(sheetHeader, DailyMapper.GetSheet()));
                    break;
                case SheetEnum.MONTHLY:
                    headerMessages.AddRange(HeaderHelper.CheckSheetHeaders(sheetHeader, MonthlyMapper.GetSheet()));
                    break;
                case SheetEnum.NAMES:
                    headerMessages.AddRange(HeaderHelper.CheckSheetHeaders(sheetHeader, NameMapper.GetSheet()));
                    break;
                case SheetEnum.PLACES:
                    headerMessages.AddRange(HeaderHelper.CheckSheetHeaders(sheetHeader, PlaceMapper.GetSheet()));
                    break;
                case SheetEnum.REGIONS:
                    headerMessages.AddRange(HeaderHelper.CheckSheetHeaders(sheetHeader, RegionMapper.GetSheet()));
                    break;
                case SheetEnum.SERVICES:
                    headerMessages.AddRange(HeaderHelper.CheckSheetHeaders(sheetHeader, ServiceMapper.GetSheet()));
                    break;
                case SheetEnum.SHIFTS:
                    headerMessages.AddRange(HeaderHelper.CheckSheetHeaders(sheetHeader, ShiftMapper.GetSheet()));
                    break;
                case SheetEnum.TRIPS:
                    headerMessages.AddRange(HeaderHelper.CheckSheetHeaders(sheetHeader, TripMapper.GetSheet()));
                    break;
                case SheetEnum.TYPES:
                    headerMessages.AddRange(HeaderHelper.CheckSheetHeaders(sheetHeader, TypeMapper.GetSheet()));
                    break;
                case SheetEnum.WEEKDAYS:
                    headerMessages.AddRange(HeaderHelper.CheckSheetHeaders(sheetHeader, WeekdayMapper.GetSheet()));
                    break;
                case SheetEnum.WEEKLY:
                    headerMessages.AddRange(HeaderHelper.CheckSheetHeaders(sheetHeader, WeeklyMapper.GetSheet()));
                    break;
                case SheetEnum.YEARLY:
                    headerMessages.AddRange(HeaderHelper.CheckSheetHeaders(sheetHeader, YearlyMapper.GetSheet()));
                    break;
                default:
                    break;
            }
        }

        if (headerMessages.Count > 0)
        {
            messages.Add(MessageHelper.CreateWarningMessage($"Found sheet header issue(s)", MessageTypeEnum.CheckSheet));
            messages.AddRange(headerMessages);
        }
        else
        {
            messages.Add(MessageHelper.CreateInfoMessage($"No sheet header issues found", MessageTypeEnum.CheckSheet));
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
        var batchUpdateSpreadsheetRequest = GenerateSheetsHelpers.Generate(sheets);
        var response = await _googleSheetService.CreateSheets(batchUpdateSpreadsheetRequest);

        var sheetEntity = new SheetEntity();

        // No sheets created if null.
        if (response == null)
        {
            foreach (var sheet in sheets)
            {
                sheetEntity.Messages.Add(MessageHelper.CreateErrorMessage($"{sheet.UpperName()} not created", MessageTypeEnum.CreateSheet));
            }

            return sheetEntity;
        }

        var sheetTitles = response.Replies.Where(x => x.AddSheet != null).Select(x => x.AddSheet.Properties.Title).ToList();

        foreach (var sheetTitle in sheetTitles)
        {
            sheetEntity.Messages.Add(MessageHelper.CreateInfoMessage($"{sheetTitle.GetValueFromName<SheetEnum>()} created", MessageTypeEnum.CreateSheet));
        }

        return sheetEntity;
    }

    public async Task<SheetEntity> GetSheet(string sheet)
    {
        var sheetExists = Enum.TryParse(sheet.ToUpper(), out SheetEnum sheetEnum) && Enum.IsDefined(typeof(SheetEnum), sheetEnum);

        if (!sheetExists)
        {
            return new SheetEntity { Messages = [MessageHelper.CreateErrorMessage($"Sheet {sheet.ToUpperInvariant()} does not exist", MessageTypeEnum.GetSheets)] };
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
            messages.Add(MessageHelper.CreateErrorMessage($"Unable to retrieve sheet(s): {stringSheetList}", MessageTypeEnum.GetSheets));
        }
        else
        {
            messages.Add(MessageHelper.CreateInfoMessage($"Retrieved sheet(s): {stringSheetList}", MessageTypeEnum.GetSheets));
            data = GigSheetHelpers.MapData(response);
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
            messages.Add(MessageHelper.CreateErrorMessage("Unable to get spreadsheet name", MessageTypeEnum.General));
        }
        else
        {
            messages.Add(MessageHelper.CreateInfoMessage($"Retrieved spreadsheet name: {spreadsheetName}", MessageTypeEnum.General));
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
