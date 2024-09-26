using Google.Apis.Sheets.v4.Data;
using RLE.Core.Entities;
using RLE.Core.Enums;
using RLE.Core.Utilities.Extensions;
using RLE.Core.Utilities;
using RLE.Gig.Enums;
using RLE.Gig.Mappers;
using RLE.Core.Interfaces;
using RLE.Core.Constants;
using RLE.Core.Utilities.Google;
using RLE.Gig.Entities;

namespace RLE.Gig.Utilities.Google;

public interface IGigSheetManager : ISheetManager
{
    public Task<GigSheetEntity> AddSheetData(List<GigSheetEnum> sheets, GigSheetEntity sheetEntity);
    public Task<GigSheetEntity> CreateSheets();
    public Task<GigSheetEntity> CreateSheets(List<GigSheetEnum> sheets);
    public Task<GigSheetEntity> GetSheet(string sheet);
    public Task<GigSheetEntity> GetSheets();
    public Task<GigSheetEntity> GetSheets(List<GigSheetEnum> sheets);
}

public class GigSheetManager : IGigSheetManager
{
    private readonly GoogleSheetService _googleSheetService;

    public GigSheetManager(string accessToken, string spreadsheetId)
    {
        _googleSheetService = new GoogleSheetService(accessToken, spreadsheetId);
    }

    public GigSheetManager(Dictionary<string, string> parameters, string spreadsheetId)
    {
        _googleSheetService = new GoogleSheetService(parameters, spreadsheetId);
    }

    public async Task<GigSheetEntity> AddSheetData(List<GigSheetEnum> sheets, GigSheetEntity sheetEntity)
    {
        foreach (var sheet in sheets)
        {
            var headers = (await _googleSheetService.GetSheetData(sheet.GetDescription()))?.Values[0];

            if (headers == null)
                continue;

            IList<IList<object?>> values = [];

            switch (sheet)
            {
                case GigSheetEnum.SHIFTS:
                    values = ShiftMapper.MapToRangeData(sheetEntity.Shifts, headers);
                    sheetEntity.Messages.Add(MessageHelper.CreateInfoMessage($"Adding data to {sheet.UpperName()}", MessageTypeEnum.AddData));
                    break;

                case GigSheetEnum.TRIPS:
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
        var sheets = new List<GigSheetEnum>();

        var missingSheetMessages = new List<MessageEntity>();
        // Loop through all sheets to see if they exist.
        foreach (var name in Enum.GetNames<GigSheetEnum>())
        {
            GigSheetEnum sheetEnum = (GigSheetEnum)Enum.Parse(typeof(GigSheetEnum), name);

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
            var sheetEnum = (GigSheetEnum)Enum.Parse(typeof(GigSheetEnum), sheet.ToUpper());

            var sheetHeader = valueRange.ValueRange.Values;
            switch (sheetEnum)
            {
                case GigSheetEnum.ADDRESSES:
                    headerMessages.AddRange(HeaderHelper.CheckSheetHeaders(sheetHeader, AddressMapper.GetSheet()));
                    break;
                case GigSheetEnum.DAILY:
                    headerMessages.AddRange(HeaderHelper.CheckSheetHeaders(sheetHeader, DailyMapper.GetSheet()));
                    break;
                case GigSheetEnum.MONTHLY:
                    headerMessages.AddRange(HeaderHelper.CheckSheetHeaders(sheetHeader, MonthlyMapper.GetSheet()));
                    break;
                case GigSheetEnum.NAMES:
                    headerMessages.AddRange(HeaderHelper.CheckSheetHeaders(sheetHeader, NameMapper.GetSheet()));
                    break;
                case GigSheetEnum.PLACES:
                    headerMessages.AddRange(HeaderHelper.CheckSheetHeaders(sheetHeader, PlaceMapper.GetSheet()));
                    break;
                case GigSheetEnum.REGIONS:
                    headerMessages.AddRange(HeaderHelper.CheckSheetHeaders(sheetHeader, RegionMapper.GetSheet()));
                    break;
                case GigSheetEnum.SERVICES:
                    headerMessages.AddRange(HeaderHelper.CheckSheetHeaders(sheetHeader, ServiceMapper.GetSheet()));
                    break;
                case GigSheetEnum.SHIFTS:
                    headerMessages.AddRange(HeaderHelper.CheckSheetHeaders(sheetHeader, ShiftMapper.GetSheet()));
                    break;
                case GigSheetEnum.TRIPS:
                    headerMessages.AddRange(HeaderHelper.CheckSheetHeaders(sheetHeader, TripMapper.GetSheet()));
                    break;
                case GigSheetEnum.TYPES:
                    headerMessages.AddRange(HeaderHelper.CheckSheetHeaders(sheetHeader, TypeMapper.GetSheet()));
                    break;
                case GigSheetEnum.WEEKDAYS:
                    headerMessages.AddRange(HeaderHelper.CheckSheetHeaders(sheetHeader, WeekdayMapper.GetSheet()));
                    break;
                case GigSheetEnum.WEEKLY:
                    headerMessages.AddRange(HeaderHelper.CheckSheetHeaders(sheetHeader, WeeklyMapper.GetSheet()));
                    break;
                case GigSheetEnum.YEARLY:
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

    public async Task<GigSheetEntity> CreateSheets()
    {
        var sheets = Enum.GetValues(typeof(GigSheetEnum)).Cast<GigSheetEnum>().ToList();
        return await CreateSheets(sheets);
    }

    public async Task<GigSheetEntity> CreateSheets(List<GigSheetEnum> sheets)
    {
        var batchUpdateSpreadsheetRequest = GenerateSheetHelper.Generate(sheets);
        var response = await _googleSheetService.CreateSheets(batchUpdateSpreadsheetRequest);

        var sheetEntity = new GigSheetEntity();

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
            sheetEntity.Messages.Add(MessageHelper.CreateInfoMessage($"{sheetTitle.GetValueFromName<GigSheetEnum>()} created", MessageTypeEnum.CreateSheet));
        }

        return sheetEntity;
    }

    public async Task<GigSheetEntity> GetSheet(string sheet)
    {
        var sheetExists = Enum.TryParse(sheet.ToUpper(), out GigSheetEnum sheetEnum) && Enum.IsDefined(typeof(GigSheetEnum), sheetEnum);

        if (!sheetExists)
        {
            return new GigSheetEntity { Messages = [MessageHelper.CreateErrorMessage($"Sheet {sheet.ToUpperInvariant()} does not exist", MessageTypeEnum.GetSheets)] };
        }

        return await GetSheets([sheetEnum]);
    }

    public async Task<GigSheetEntity> GetSheets()
    {
        // TODO Add check sheets here where it can add missing sheets.

        var sheets = Enum.GetValues(typeof(GigSheetEnum)).Cast<GigSheetEnum>().ToList();
        var response = await GetSheets(sheets);

        return response ?? new GigSheetEntity();
    }

    public async Task<GigSheetEntity> GetSheets(List<GigSheetEnum> sheets)
    {
        var data = new GigSheetEntity();
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
        if (sheets.Count < Enum.GetNames(typeof(GigSheetEnum)).Length)
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
