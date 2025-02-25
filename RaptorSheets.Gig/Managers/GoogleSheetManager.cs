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
    public Task<SheetEntity> ChangeSheetData(List<SheetEnum> sheets, SheetEntity sheetEntity);
    public Task<SheetEntity> ChangeSheetData(List<SheetEnum> sheets, SheetEntity sheetEntity, ActionTypeEnum actionType);
    public Task<SheetEntity> CreateSheets();
    public Task<SheetEntity> CreateSheets(List<SheetEnum> sheets);
    public Task<SheetEntity> GetSheet(string sheet);
    public Task<SheetEntity> GetSheets();
    public Task<SheetEntity> GetSheets(List<SheetEnum> sheets);
    public Task<List<PropertyEntity>> GetSheetProperties(List<string> sheets);
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

    public async Task<SheetEntity> ChangeSheetData(List<SheetEnum> sheets, SheetEntity sheetEntity)
    {
        var changes = new Dictionary<SheetEnum, object>();

        // Pull out all changes into a single object to iterate through.
        foreach (var sheet in sheets)
        {
            switch (sheet)
            {
                case SheetEnum.SHIFTS:
                    if (sheetEntity.Shifts.Count > 0)
                        changes.Add(sheet, sheetEntity.Shifts);
                    break;

                case SheetEnum.TRIPS:
                    if (sheetEntity.Trips.Count > 0)
                        changes.Add(sheet, sheetEntity.Trips);
                    break;
                default:
                    // Unsupported sheet.
                    sheetEntity.Messages.Add(MessageHelpers.CreateErrorMessage($"{ActionTypeEnum.LOOKUP} data: {sheet.UpperName()} not supported", MessageTypeEnum.GENERAL));
                    break;
            }
        }

        if (changes.Count == 0)
        {
            sheetEntity.Messages.Add(MessageHelpers.CreateWarningMessage("No data to change", MessageTypeEnum.GENERAL));
            return sheetEntity;
        }

        var sheetInfo = await GetSheetProperties(sheets.Select(t => t.GetDescription()).ToList());
        var batchUpdateSpreadsheetRequest = new BatchUpdateSpreadsheetRequest
        {
            Requests = []
        };

        foreach (var change in changes)
        {
            switch (change.Key)
            {
                case SheetEnum.SHIFTS:
                    var shiftProperties = sheetInfo.FirstOrDefault(x => x.Name == change.Key.GetDescription());
                    batchUpdateSpreadsheetRequest.Requests.AddRange(ChangeShiftSheetData(change.Value as List<ShiftEntity> ?? [], shiftProperties));
                    break;
                case SheetEnum.TRIPS:
                    var tripPropertes = sheetInfo.FirstOrDefault(x => x.Name == change.Key.GetDescription());
                    batchUpdateSpreadsheetRequest.Requests.AddRange(ChangeTripSheetData(change.Value as List<TripEntity> ?? [], tripPropertes));
                    break;
            }
        }

        var success = (await _googleSheetService.BatchUpdateSpreadsheet(batchUpdateSpreadsheetRequest)) != null;

        return sheetEntity;
    }

    private List<Request> ChangeShiftSheetData(List<ShiftEntity> shifts, PropertyEntity? sheetProperties)
    {
        var requests = new List<Request>();

        // Add requests
        var addShifts = shifts?.Where(x => x.Action == ActionTypeEnum.APPEND.GetDescription()).ToList() ?? [];
        requests.AddRange(CreateUpdateCellShiftRequests(addShifts, sheetProperties));

        // Update requests
        var updateShifts = shifts?.Where(x => x.Action == ActionTypeEnum.UPDATE.GetDescription()).ToList() ?? [];
        requests.AddRange(CreateUpdateCellShiftRequests(updateShifts, sheetProperties));

        // Delete requests
        var deleteShifts = shifts?.Where(x => x.Action == ActionTypeEnum.DELETE.GetDescription()).ToList() ?? [];
        requests.AddRange(CreateDeleteShiftRequests(updateShifts, sheetProperties));

        return requests;
    }

    private IEnumerable<Request> CreateUpdateCellShiftRequests(List<ShiftEntity> shifts, PropertyEntity? sheetProperties)
    {
        var headers = sheetProperties?.Attributes[PropertyEnum.HEADERS.GetDescription()]?.Split(",").Cast<object>().ToList();
        var maxRowValue = int.Parse(sheetProperties?.Attributes[PropertyEnum.MAX_ROW_VALUE.GetDescription()] ?? "0");
        int sheetId = int.TryParse(sheetProperties?.Id, out var id) ? id : 0;

        if (shifts.Count == 0 || sheetProperties == null || headers?.Count == 0 || sheetId == 0)
        {
            return [];
        }

        var requests = new List<Request>();

        foreach (var shift in shifts)
        {
            var rowData = ShiftMapper.MapToRowData([shift], headers!);

            // TODO: Look into making this an add dimension incase the difference is more than one.
            if (shift.RowId > maxRowValue)
            {
                // Create an append dimension to add more rows to sheet
                requests.Add(GoogleRequestHelpers.GenerateAppendCells(sheetId, rowData));
            }
            else
            {
                requests.Add(GoogleRequestHelpers.GenerateUpdateCellsRequest(sheetId, shift.RowId, rowData));
            }
        }

        return requests;
    }

    private IEnumerable<Request> CreateDeleteShiftRequests(List<ShiftEntity> shifts, PropertyEntity? sheetProperties)
    {
        var rowIds = shifts.Select(x => x.RowId).ToList();
        int sheetId = int.TryParse(sheetProperties?.Id, out var id) ? id : 0;

        if (shifts.Count == 0 || sheetProperties == null ||  sheetId == 0)        {
            return [];
        }

        var requests = GoogleRequestHelpers.GenerateDeleteRequests(sheetId, rowIds);

        return requests;
    }

    private List<Request> ChangeTripSheetData(List<TripEntity> trips, PropertyEntity? sheetProperties)
    {
        var requests = new List<Request>();

        // Add requests
        var addTrips = trips?.Where(x => x.Action == ActionTypeEnum.APPEND.GetDescription()).ToList() ?? [];
        requests.AddRange(CreateUpdateCellTripRequests(addTrips, sheetProperties));

        // Update requests
        var updateTrips = trips?.Where(x => x.Action == ActionTypeEnum.UPDATE.GetDescription()).ToList() ?? [];
        requests.AddRange(CreateUpdateCellTripRequests(updateTrips, sheetProperties));

        // Delete requests
        var deleteTrips = trips?.Where(x => x.Action == ActionTypeEnum.DELETE.GetDescription()).ToList() ?? [];
        requests.AddRange(CreateDeleteTripRequests(updateTrips, sheetProperties));

        return requests;
    }

    private IEnumerable<Request> CreateUpdateCellTripRequests(List<TripEntity> trips, PropertyEntity? sheetProperties)
    {
        var headers = sheetProperties?.Attributes[PropertyEnum.HEADERS.GetDescription()]?.Split(",").Cast<object>().ToList();
        var maxRowValue = int.Parse(sheetProperties?.Attributes[PropertyEnum.MAX_ROW_VALUE.GetDescription()] ?? "0");
        int sheetId = int.TryParse(sheetProperties?.Id, out var id) ? id : 0;

        if (trips.Count == 0 || sheetProperties == null || headers?.Count == 0 || sheetId == 0)
        {
            return [];
        }

        var requests = new List<Request>();

        foreach (var trip in trips)
        {
            var rowData = TripMapper.MapToRowData([trip], headers!);

            // TODO: Look into making this an add dimension incase the difference is more than one.
            if (trip.RowId > maxRowValue)
            {
                // Create an append dimension to add more rows to sheet
                requests.Add(GoogleRequestHelpers.GenerateAppendCells(sheetId, rowData));
            }
            else
            {
                requests.Add(GoogleRequestHelpers.GenerateUpdateCellsRequest(sheetId, trip.RowId, rowData));
            }
        }

        return requests;
    }

    private IEnumerable<Request> CreateDeleteTripRequests(List<TripEntity> trips, PropertyEntity? sheetProperties)
    {
        var rowIds = trips.Select(x => x.RowId).ToList();
        int sheetId = int.TryParse(sheetProperties?.Id, out var id) ? id : 0;

        if (trips.Count == 0 || sheetProperties == null || sheetId == 0)
        {
            return [];
        }

        var requests = GoogleRequestHelpers.GenerateDeleteRequests(sheetId, rowIds);

        return requests;
    }

    //public async Task<SheetEntity> ChangeSheetDataOld(List<SheetEnum> sheets, SheetEntity sheetEntity)
    //{
    //    var changes = new Dictionary<SheetEnum, object>();

    //    // Pull out all changes into a single object to iterate through.
    //    foreach (var sheet in sheets)
    //    {
    //        switch (sheet)
    //        {
    //            case SheetEnum.SHIFTS:
    //                if (sheetEntity.Shifts.Count > 0)
    //                    changes.Add(sheet, sheetEntity.Shifts);
    //                break;

    //            case SheetEnum.TRIPS:
    //                if (sheetEntity.Trips.Count > 0)
    //                    changes.Add(sheet, sheetEntity.Trips);
    //                break;
    //            default:
    //                // Unsupported sheet.
    //                sheetEntity.Messages.Add(MessageHelpers.CreateErrorMessage($"{ActionTypeEnum.LOOKUP} data: {sheet.UpperName()} not supported", MessageTypeEnum.GENERAL));
    //                break;
    //        }
    //    }

    //    if (changes.Count == 0)
    //    {
    //        sheetEntity.Messages.Add(MessageHelpers.CreateWarningMessage("No data to change", MessageTypeEnum.GENERAL));
    //        return sheetEntity;
    //    }

    //    var sheetInfo = await GetSheetProperties(sheets.Select(t => t.GetDescription()).ToList());
    //    //var batchRequests = new Dictionary<ActionTypeEnum, IList<IList<object?>>>();
    //    var batchUpdateSpreadsheetRequest = new BatchUpdateSpreadsheetRequest
    //    {
    //        Requests = []
    //    };

    //    foreach (var change in changes)
    //    {
    //        switch (change.Key)
    //        {
    //            case SheetEnum.SHIFTS:

    //                // TODO: Create a function to return requests.
    //                var sheetProperties = sheetInfo.FirstOrDefault(x => x.Name == change.Key.GetDescription());
    //                var headers = sheetProperties?.Attributes[PropertyEnum.HEADERS.GetDescription()]?.Split(",").Cast<object>().ToList();
    //                var maxRow = int.Parse(sheetProperties?.Attributes[PropertyEnum.MAX_ROW.GetDescription()] ?? "0");
    //                var maxRowValue = int.Parse(sheetProperties?.Attributes[PropertyEnum.MAX_ROW_VALUE.GetDescription()] ?? "0");
    //                int sheetId = int.TryParse(sheetProperties?.Id, out var id) ? id : 0;

    //                if (sheetProperties == null || headers?.Count == 0 || sheetId == 0)
    //                {
    //                    break;
    //                }
    //                var rowValues = new Dictionary<int, IList<IList<object?>>>();
    //                var addShifts = (change.Value as List<ShiftEntity>)?.Where(x => x.Action == ActionTypeEnum.APPEND.GetDescription()).ToList();

    //                // Check to see if more rows need to be added to the sheet.
    //                if (maxRowValue + addShifts?.Count >= maxRow && addShifts?.Count > 0)
    //                {
    //                    batchUpdateSpreadsheetRequest.Requests.AddRange(GoogleRequestHelpers.GenerateAppendDimension(sheetId, addShifts.Count));
    //                }

    //                foreach (var shift in (List<ShiftEntity>)change.Value)
    //                {
    //                    ActionTypeEnum action = shift.Action.GetValueFromName<ActionTypeEnum>();
    //                    switch (action)
    //                    {
    //                        case ActionTypeEnum.APPEND:
    //                            var rowData = ShiftMapper.MapToRowData([shift], headers!);
    //                            if (shift.RowId > maxRow)
    //                            {
    //                                // Create an append dimension to add more rows to sheet
    //                            }


    //                            rowValues.Add(shift.RowId, rowData);
    //                            var batchRequest = GoogleRequestHelpers.GenerateUpdateRequest(change.Key.GetDescription(), rowData);
    //                            var appendCellsRequest = new AppendCellsRequest();
    //                            appendCellsRequest.SheetId = sheetId;
    //                            appendCellsRequest.Rows = rowData;
    //                            appendCellsRequest.Fields = "*";
    //                            batchUpdateSpreadsheetRequest.Requests.Add(new Request { AppendCells = appendCellsRequest });
    //                            break;
    //                        case ActionTypeEnum.UPDATE:
    //                            var rowValues = new Dictionary<int, IList<IList<object?>>>();
    //                            rowValues.Add(shift.RowId, ShiftMapper.MapToRangeData([shift], headers));
    //                            var batchRequest = GoogleRequestHelpers.GenerateUpdateRequest(change.Key.GetDescription(), rowValues);
    //                            batchUpdateSpreadsheetRequest.Requests.Add(new Request { UpdateCells = batchRequest });
    //                            break;
    //                        case ActionTypeEnum.DELETE:
    //                            var rowIds = [shift.RowId];
    //                            var deleteRequests = GoogleRequestHelpers.GenerateDeleteRequest((int)sheetId, rowIds);
    //                            batchUpdateSpreadsheetRequest.Requests.AddRange(deleteRequests);
    //                            break;
    //                        default:
    //                            break;
    //                    }
    //                }

    //                var addShifts = (change.Value as List<ShiftEntity>)?.Where(x => x.Action == ActionTypeEnum.APPEND.GetDescription()).ToList();
    //                if (addShifts?.Count > 0 && headers?.Count > 0)
    //                {
    //                    var rowData = ShiftMapper.MapToRowData(addShifts!, headers);
    //                    //var appendCellRequest = new AppendCellsRequest();
    //                    //var valueRange = new ValueRange { Values = values };
    //                    //var request = _googleSheetService.Spreadsheets.Values.Append(valueRange, _spreadsheetId, range);
    //                    //request.ValueInputOption = AppendRequest.ValueInputOptionEnum.USERENTERED;
    //                    var appendCellsRequest = new AppendCellsRequest();
    //                    appendCellsRequest.SheetId = sheetId;
    //                    // appendCellsRequest.Rows = rowData;
    //                    appendCellsRequest.Fields = "*";
    //                    //var batchUpdateRequest = new BatchUpdateValuesRequest();
    //                    //batchUpdateRequest.Data.Add(valueRange);
    //                    //batchUpdateRequest.ValueInputOption = "USER_ENTERED";
    //                    batchUpdateSpreadsheetRequest.Requests.Add(new Request { AppendCells = appendCellsRequest });
    //                    //batchUpdateSpreadsheetRequest.Requests.Add(AppendRequest(valueRange, $"{change.Key.GetDescription()}!{GoogleConfig.Range}"));
    //                }

    //                var updateShifts = (change.Value as List<ShiftEntity>)?.Where(x => x.Action == ActionTypeEnum.UPDATE.GetDescription()).ToList();

    //                if (updateShifts?.Count > 0 && headers?.Count > 0)
    //                {
    //                    var rowValues = new Dictionary<int, IList<IList<object?>>>();
    //                    foreach (var shift in updateShifts)
    //                    {
    //                        // rowValues.Add(shift.RowId, ShiftMapper.MapToRangeData([shift], headers));
    //                    }
    //                    var batchRequest = GoogleRequestHelpers.GenerateUpdateRequest(change.Key.GetDescription(), rowValues);
    //                    // batchUpdateSpreadsheetRequest.Requests.Add(new Request { UpdateCells = batchRequest });
    //                    // sheetEntity.Shifts = ShiftMapper.MapToEntities((List<IList<object?>>)change.Value, 
    //                }

    //                var deleteShifts = (change.Value as List<ShiftEntity>)?.Where(x => x.Action == ActionTypeEnum.DELETE.GetDescription()).ToList();

    //                if (deleteShifts?.Count > 0)
    //                {
    //                    var rowIds = deleteShifts.Select(x => x.RowId).ToList();
    //                    var deleteRequests = GoogleRequestHelpers.GenerateDeleteRequest((int)sheetId, rowIds);
    //                    batchUpdateSpreadsheetRequest.Requests.AddRange(deleteRequests);
    //                }
    //                break;
    //            case SheetEnum.TRIPS:
    //                // sheetEntity.Trips = TripMapper.MapToEntities((List<IList<object?>>)change.Value);
    //                break;
    //            default:
    //                // Unsupported sheet.
    //                sheetEntity.Messages.Add(MessageHelpers.CreateErrorMessage($"{ActionTypeEnum.LOOKUP} data: {change.Key.UpperName()} not supported", MessageTypeEnum.GENERAL));
    //                break;
    //        }
    //    }

    //    var success = (await _googleSheetService.BatchUpdateSpreadsheet(batchUpdateSpreadsheetRequest)) != null;

    //    return sheetEntity;
    //}

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
        var valueRange = new ValueRange {  Values = [] };
        var ranges = sheets.Select(sheet => $"{sheet.GetDescription()}!{GoogleConfig.HeaderRange}").ToList();
        var sheetInfo = await _googleSheetService.GetSheetInfo(ranges);

        foreach (var sheet in sheets)
        {
            var sheetProperties = sheetInfo?.Sheets.FirstOrDefault(x => x.Properties.Title == sheet.GetDescription());
            var sheetHeaderValues = sheetProperties?.Data?[0]?.RowData?[0]?.Values;
            var headers = HeaderHelpers.GetHeadersFromCellData(sheetHeaderValues);

            if (!headers.Any())
                continue;

            switch (sheet)
            {
                case SheetEnum.SHIFTS:
                    valueRange.Values.AddRange(ShiftMapper.MapToRangeData(sheetEntity.Shifts, headers));
                    break;

                case SheetEnum.TRIPS:
                    valueRange.Values.AddRange(TripMapper.MapToRangeData(sheetEntity.Trips, headers));
                    break;
                default:
                    // Unsupported sheet.
                    sheetEntity.Messages.Add(MessageHelpers.CreateErrorMessage($"{actionType} data: {sheet.UpperName()} not supported", messageType));
                    break;
            }

            if (valueRange.Values.Any())
            {
                var success = (await _googleSheetService.AppendData(valueRange, $"{sheet.GetDescription()}!{GoogleConfig.Range}")) != null;
                sheetEntity.Messages.Add(MessageHelpers.CreateInfoMessage($"{actionType} data: {sheet.UpperName()}", messageType));
            }
            else
            {
                sheetEntity.Messages.Add(MessageHelpers.CreateWarningMessage($"No data to {actionType}", messageType));
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
                        rowIds.Add(shift.RowId);
                    }
                    sheetEntity.Messages.Add(MessageHelpers.CreateInfoMessage($"{actionType} data: {sheet.UpperName()}", messageType));
                    break;

                case SheetEnum.TRIPS:
                    foreach (var trip in sheetEntity.Trips)
                    {
                        rowIds.Add(trip.RowId);
                    }
                    sheetEntity.Messages.Add(MessageHelpers.CreateInfoMessage($"{actionType} data: {sheet.UpperName()}", messageType));
                    break;
                default:
                    // Unsupported sheet.
                    sheetEntity.Messages.Add(MessageHelpers.CreateErrorMessage($"{actionType} data: {sheet.UpperName()} not supported", messageType));
                    break;
            }

            if (rowIds.Count != 0)
            {
                var success = false;
                var batchRequest = new BatchUpdateSpreadsheetRequest { Requests = [] };
                batchRequest.Requests = GoogleRequestHelpers.GenerateDeleteRequests((int)sheetId, rowIds);
                success = (await _googleSheetService.BatchUpdateSpreadsheet(batchRequest)) != null;

                if (success)
                    sheetEntity.Messages.Add(MessageHelpers.CreateInfoMessage($"{actionType} data successful: {sheet.UpperName()}", messageType));
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

            var rowValues = new Dictionary<int, IList<IList<object?>>>();

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
                var batchRequest = GoogleRequestHelpers.GenerateUpdateValueRequest(sheet.GetDescription(), rowValues);

                success = (await _googleSheetService.BatchUpdateData(batchRequest)) != null;

                if (success)
                    sheetEntity.Messages.Add(MessageHelpers.CreateInfoMessage($"{actionType} data successful: {sheet.UpperName()}", messageType));
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
                    headerMessages.AddRange(HeaderHelpers.CheckSheetHeaders(sheetHeader, TripMapper.GetSheet()));
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

    public async Task<List<PropertyEntity>> GetSheetProperties(List<string> sheets) // TODO: Look into moving this to a common area
    {
        var ranges = new List<string>();
        var properties = new List<PropertyEntity>();

        sheets.ForEach(sheet => ranges.Add($"{sheet}!{GoogleConfig.HeaderRange}")); // Get headers for each sheet.
        sheets.ForEach(sheet => ranges.Add($"{sheet}!{GoogleConfig.RowRange}")); // Get max row for each sheet.

        var sheetInfo = await _googleSheetService.GetSheetInfo(ranges);

        foreach (var sheet in sheets)
        {
            var property = new PropertyEntity();
            var sheetProperties = sheetInfo?.Sheets.FirstOrDefault(x => x.Properties.Title == sheet);
            var sheetHeaderValues = string.Join(",", sheetProperties?.Data?[0]?.RowData?[0]?.Values?.Where(x => x.FormattedValue != null).Select(x => x.FormattedValue).ToList() ?? []);
            var maxRow = (sheetProperties?.Data?[1]?.RowData ?? []).Count;
            var maxRowValue = (sheetProperties?.Data?[1]?.RowData.Where(x => x.Values?[0]?.FormattedValue != null).Select(x => x.Values?[0]?.FormattedValue).ToList() ?? []).Count;
            var sheetId = sheetProperties?.Properties.SheetId.ToString() ?? "";

            property.Id = sheetId;
            property.Name = sheet;

            property.Attributes.Add(PropertyEnum.HEADERS.GetDescription(),sheetHeaderValues);
            property.Attributes.Add(PropertyEnum.MAX_ROW.GetDescription(), maxRow.ToString());
            property.Attributes.Add(PropertyEnum.MAX_ROW_VALUE.GetDescription(), maxRowValue.ToString());

            properties.Add(property);
        }

        return properties;
    }

    public async Task<string?> GetSpreadsheetName() // TODO: Look into moving this to a common area
    {
        var response = await _googleSheetService.GetSheetInfo();

        if (response == null)
            return null;

        return response.Properties.Title;
    }

}
