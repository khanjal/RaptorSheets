using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Entities;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Mappers;
using RaptorSheets.Core.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Core.Services;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Interfaces;
using RaptorSheets.Gig.Helpers;
using RaptorSheets.Core.Helpers;

namespace RaptorSheets.Gig.Managers;

public interface IGoogleSheetManager : ISheetManager
{
    public Task<SheetEntity> ChangeSheetData(List<SheetEnum> sheets, SheetEntity sheetEntity, ActionTypeEnum actionType);
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

    public async Task<SheetEntity> ChangeSheetData(List<SheetEnum> sheets, SheetEntity sheetEntity, ActionTypeEnum actionType)
    {
        switch (actionType)
        {
            case ActionTypeEnum.APPEND:
                return await AppendSheetData(sheets, sheetEntity, actionType);
            case ActionTypeEnum.DELETE:
                return await DeleteSheetData(sheets, sheetEntity, actionType);
            case ActionTypeEnum.UPDATE:
                return await UpdateSheetData(sheets, sheetEntity, actionType);
            default:
                sheetEntity.Messages.Add(MessageHelpers.CreateErrorMessage($"Action type {actionType} not supported", MessageTypeEnum.GENERAL));
                return sheetEntity;
        }
    }

    private async Task<SheetEntity> AppendSheetData(List<SheetEnum> sheets, SheetEntity sheetEntity, ActionTypeEnum actionType)
    {
        var messageType = MessageTypeEnum.ADD_DATA;

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
                    sheetEntity.Messages.Add(MessageHelpers.CreateInfoMessage($"{actionType} data: {sheet.UpperName()}", messageType));
                    break;

                case SheetEnum.TRIPS:
                    values = TripMapper.MapToRangeData(sheetEntity.Trips, headers);
                    sheetEntity.Messages.Add(MessageHelpers.CreateInfoMessage($"{actionType} data: {sheet.UpperName()}", messageType));
                    break;
                default:
                    // Unsupported sheet.
                    sheetEntity.Messages.Add(MessageHelpers.CreateErrorMessage($"{actionType} data: {sheet.UpperName()} not supported", messageType));
                    break;
            }

            if (values.Any())
            {
                var valueRange = new ValueRange { Values = values };
                var success = (await _googleSheetService.AppendData(valueRange, $"{sheet.GetDescription()}!{GoogleConfig.Range}")) != null;

                if (success)
                    sheetEntity.Messages.Add(MessageHelpers.CreateInfoMessage($"{actionType} data: {sheet.UpperName()}", messageType));
                else
                    sheetEntity.Messages.Add(MessageHelpers.CreateErrorMessage($"Unable to {actionType} data: {sheet.UpperName()}", messageType));
            }
            else
            {
                sheetEntity.Messages.Add(MessageHelpers.CreateWarningMessage($"No data to {actionType}: {sheet.UpperName()}", messageType));
            }
        }

        return sheetEntity;
    }

    private async Task<SheetEntity> DeleteSheetData(List<SheetEnum> sheets, SheetEntity sheetEntity, ActionTypeEnum actionType)
    {
        var messageType = MessageTypeEnum.DELETE_DATA;
        var sheetInfo = await _googleSheetService.GetSheetInfo();

        foreach (var sheet in sheets)
        {
            var headers = (await _googleSheetService.GetSheetData(sheet.GetDescription()))?.Values[0];
            var sheetId = sheetInfo?.Sheets?.FirstOrDefault(x => x.Properties.Title == sheet.GetDescription())?.Properties.SheetId;

            if (headers == null || sheetId == null)
                continue;

            List<int> rowIds = [];

            switch (sheet)
            {
                case SheetEnum.SHIFTS:
                    foreach (var shift in sheetEntity.Shifts)
                    {
                        rowIds.Add(shift.RowId-1);
                    }
                    sheetEntity.Messages.Add(MessageHelpers.CreateInfoMessage($"{actionType} data: {sheet.UpperName()}", messageType));
                    break;

                case SheetEnum.TRIPS:
                    foreach (var trip in sheetEntity.Trips)
                    {
                        rowIds.Add(trip.RowId-1);
                    }
                    sheetEntity.Messages.Add(MessageHelpers.CreateInfoMessage($"{actionType} data: {sheet.UpperName()}", messageType));
                    break;
                default:
                    // Unsupported sheet.
                    sheetEntity.Messages.Add(MessageHelpers.CreateErrorMessage($"{actionType} data: {sheet.UpperName()} not supported", messageType));
                    break;
            }

            if (rowIds.Any())
            {
                var success = false;
                var batchRequest = GoogleRequestHelpers.GenerateBatchDeleteRequest((int)sheetId, rowIds);
                success = (await _googleSheetService.BatchUpdateSpreadsheet(batchRequest)) != null;

                if (success)
                    sheetEntity.Messages.Add(MessageHelpers.CreateInfoMessage($"{actionType} data: {sheet.UpperName()}", messageType));
                else
                    sheetEntity.Messages.Add(MessageHelpers.CreateErrorMessage($"Unable to {actionType} data: {sheet.UpperName()}", messageType));
            }
            else
            {
                sheetEntity.Messages.Add(MessageHelpers.CreateWarningMessage($"No data to {actionType}: {sheet.UpperName()}", messageType));
            }
        }

        return sheetEntity;
    }

    private async Task<SheetEntity> UpdateSheetData(List<SheetEnum> sheets, SheetEntity sheetEntity, ActionTypeEnum actionType)
    {
        var messageType = MessageTypeEnum.UPDATE_DATA;

        foreach (var sheet in sheets)
        {
            var headers = (await _googleSheetService.GetSheetData(sheet.GetDescription()))?.Values[0];

            if (headers == null)
                continue;

            IDictionary<int, IList<IList<object?>>> rowValues = new Dictionary<int, IList<IList<object?>>>();

            switch (sheet)
            {
                case SheetEnum.SHIFTS:
                    foreach (var shift in sheetEntity.Shifts)
                    {
                        rowValues.Add(shift.RowId, ShiftMapper.MapToRangeData([shift], headers));
                    }
                    sheetEntity.Messages.Add(MessageHelpers.CreateInfoMessage($"{actionType} data: {sheet.UpperName()}", messageType));
                    break;

                case SheetEnum.TRIPS:
                    foreach (var trip in sheetEntity.Trips)
                    {
                        rowValues.Add(trip.RowId, TripMapper.MapToRangeData([trip], headers));
                    }
                    sheetEntity.Messages.Add(MessageHelpers.CreateInfoMessage($"{actionType} data: {sheet.UpperName()}", messageType));
                    break;
                default:
                    // Unsupported sheet.
                    sheetEntity.Messages.Add(MessageHelpers.CreateErrorMessage($"{actionType} data: {sheet.UpperName()} not supported", messageType));
                    break;
            }

            if (rowValues.Any())
            {
                var success = false;
                var batchRequest = GoogleRequestHelpers.GenerateUpdateRequest(sheet.GetDescription(), rowValues);

                success = (await _googleSheetService.BatchUpdateData(batchRequest)) != null;

                if (success)
                    sheetEntity.Messages.Add(MessageHelpers.CreateInfoMessage($"{actionType} data: {sheet.UpperName()}", messageType));
                else
                    sheetEntity.Messages.Add(MessageHelpers.CreateErrorMessage($"Unable to {actionType} data: {sheet.UpperName()}", messageType));
            }
            else
            {
                sheetEntity.Messages.Add(MessageHelpers.CreateWarningMessage($"No data to {actionType}: {sheet.UpperName()}", messageType));
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
            messages.Add(MessageHelpers.CreateErrorMessage($"Unable to retrieve sheet(s): {stringSheetList}", MessageTypeEnum.GENERAL));
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
                    headerMessages.AddRange(HeaderHelpers.CheckSheetHeaders(sheetHeader, AddressMapper.GetSheet()));
                    break;
                case SheetEnum.DAILY:
                    headerMessages.AddRange(HeaderHelpers.CheckSheetHeaders(sheetHeader, DailyMapper.GetSheet()));
                    break;
                case SheetEnum.MONTHLY:
                    headerMessages.AddRange(HeaderHelpers.CheckSheetHeaders(sheetHeader, MonthlyMapper.GetSheet()));
                    break;
                case SheetEnum.NAMES:
                    headerMessages.AddRange(HeaderHelpers.CheckSheetHeaders(sheetHeader, NameMapper.GetSheet()));
                    break;
                case SheetEnum.PLACES:
                    headerMessages.AddRange(HeaderHelpers.CheckSheetHeaders(sheetHeader, PlaceMapper.GetSheet()));
                    break;
                case SheetEnum.REGIONS:
                    headerMessages.AddRange(HeaderHelpers.CheckSheetHeaders(sheetHeader, RegionMapper.GetSheet()));
                    break;
                case SheetEnum.SERVICES:
                    headerMessages.AddRange(HeaderHelpers.CheckSheetHeaders(sheetHeader, ServiceMapper.GetSheet()));
                    break;
                case SheetEnum.SHIFTS:
                    headerMessages.AddRange(HeaderHelpers.CheckSheetHeaders(sheetHeader, ShiftMapper.GetSheet()));
                    break;
                case SheetEnum.TRIPS:
                    headerMessages.AddRange(HeaderHelpers.CheckSheetHeaders(sheetHeader, TripMapper.GetSheet()));
                    break;
                case SheetEnum.TYPES:
                    headerMessages.AddRange(HeaderHelpers.CheckSheetHeaders(sheetHeader, TypeMapper.GetSheet()));
                    break;
                case SheetEnum.WEEKDAYS:
                    headerMessages.AddRange(HeaderHelpers.CheckSheetHeaders(sheetHeader, WeekdayMapper.GetSheet()));
                    break;
                case SheetEnum.WEEKLY:
                    headerMessages.AddRange(HeaderHelpers.CheckSheetHeaders(sheetHeader, WeeklyMapper.GetSheet()));
                    break;
                case SheetEnum.YEARLY:
                    headerMessages.AddRange(HeaderHelpers.CheckSheetHeaders(sheetHeader, YearlyMapper.GetSheet()));
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
        var batchUpdateSpreadsheetRequest = GenerateSheetsHelpers.Generate(sheets);
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
