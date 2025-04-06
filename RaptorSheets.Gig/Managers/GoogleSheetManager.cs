using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Entities;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Mappers;
using RaptorSheets.Core.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Core.Services;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Gig.Helpers;
using RaptorSheets.Core.Helpers;
using System.Collections.Generic;

namespace RaptorSheets.Gig.Managers;

public interface IGoogleSheetManager
{
    public Task<SheetEntity> ChangeSheetData(List<SheetEnum> sheets, SheetEntity sheetEntity);
    public Task<SheetEntity> CreateSheets();
    public Task<SheetEntity> CreateSheets(List<SheetEnum> sheets);
    public Task<SheetEntity> GetSheet(string sheet);
    public Task<SheetEntity> GetSheets();
    public Task<SheetEntity> GetSheets(List<SheetEnum> sheets);
    public Task<List<PropertyEntity>> GetSheetProperties();
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
                    batchUpdateSpreadsheetRequest.Requests.AddRange(GigRequestHelpers.ChangeShiftSheetData(change.Value as List<ShiftEntity> ?? [], shiftProperties));
                    break;
                case SheetEnum.TRIPS:
                    var tripPropertes = sheetInfo.FirstOrDefault(x => x.Name == change.Key.GetDescription());
                    batchUpdateSpreadsheetRequest.Requests.AddRange(GigRequestHelpers.ChangeTripSheetData(change.Value as List<TripEntity> ?? [], tripPropertes));
                    break;
            }

            sheetEntity.Messages.Add(MessageHelpers.CreateInfoMessage($"Saving data: {change.Key.UpperName()}", MessageTypeEnum.SAVE_DATA));
        }

        // TODO: Look into returning data from the batch update.
        //batchUpdateSpreadsheetRequest.IncludeSpreadsheetInResponse = true;
        //batchUpdateSpreadsheetRequest.ResponseIncludeGridData = true;
        //batchUpdateSpreadsheetRequest.ResponseRanges = [SheetEnum.ADDRESSES.GetDescription(), SheetEnum.NAMES.GetDescription(), SheetEnum.PLACES.GetDescription(), SheetEnum.REGIONS.GetDescription(), SheetEnum.SERVICES.GetDescription(), SheetEnum.TYPES.GetDescription()];

        var batchUpdateSpreadsheetResponse = await _googleSheetService.BatchUpdateSpreadsheet(batchUpdateSpreadsheetRequest);

        //if (batchUpdateSpreadsheetResponse?.UpdatedSpreadsheet?.Sheets.Count > 0) { 
        //    var sheetData = SheetHelpers.GetSheetValues(batchUpdateSpreadsheetResponse.UpdatedSpreadsheet);
        //    var test = SheetEnum.ADDRESSES.GetDescription();

        //    foreach (var sheet in sheetData)
        //    {
        //        var sheetEnum = (SheetEnum)Enum.Parse(typeof(SheetEnum), sheet.Key.ToUpper());
        //        switch (sheetEnum)
        //        {
        //            case SheetEnum.ADDRESSES:
        //                sheetEntity.Addresses = AddressMapper.MapFromRangeData(sheet.Value);
        //                break;
        //            case SheetEnum.NAMES:
        //                sheetEntity.Names = NameMapper.MapFromRangeData(sheet.Value);
        //                break;
        //            case SheetEnum.PLACES:
        //                sheetEntity.Places = PlaceMapper.MapFromRangeData(sheet.Value);
        //                break;
        //            case SheetEnum.REGIONS:
        //                sheetEntity.Regions = RegionMapper.MapFromRangeData(sheet.Value);
        //                break;
        //            case SheetEnum.SERVICES:
        //                sheetEntity.Services = ServiceMapper.MapFromRangeData(sheet.Value);
        //                break;
        //            case SheetEnum.TYPES:
        //                sheetEntity.Types = TypeMapper.MapFromRangeData(sheet.Value);
        //                break;
        //        }
        //    }
        //}

        if (batchUpdateSpreadsheetResponse == null)
        {
            // Call sheet properties to check sheets
            var spreadsheetInfo = await _googleSheetService.GetSheetInfo();
            if (spreadsheetInfo != null)
            {
                sheetEntity.Messages.AddRange(SheetHelpers.CheckSheets(SheetHelpers.CheckSheets<SheetEnum>(spreadsheetInfo)));
            }

            sheetEntity.Messages.Add(MessageHelpers.CreateErrorMessage($"Unable to save data", MessageTypeEnum.SAVE_DATA));
        }

        return sheetEntity;
    }

    public static List<MessageEntity> CheckSheetHeaders(Spreadsheet sheetInfoResponse)
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
            if (!Enum.TryParse(sheet.Properties.Title.ToUpper(), out SheetEnum sheetEnum))
            {
                messages.Add(MessageHelpers.CreateWarningMessage($"Sheet {sheet.Properties.Title} does not match any known enum value", MessageTypeEnum.CHECK_SHEET));
                continue;
            }
            var sheetHeader = HeaderHelpers.GetHeadersFromCellData(sheet.Data?[0]?.RowData?[0]?.Values);

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

    public async Task<SheetEntity> CreateSheets(List<string> sheets)
    {
        var sheetEnums = sheets.Select(x => x.GetValueFromName<SheetEnum>()).ToList();
        return await CreateSheets(sheetEnums);
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
        var sheets = Enum.GetValues(typeof(SheetEnum)).Cast<SheetEnum>().ToList();
        var response = await GetSheets(sheets);

        return response ?? new SheetEntity();
    }

    public async Task<SheetEntity> GetSheets(List<SheetEnum> sheets)
    {
        var data = new SheetEntity();
        var messages = new List<MessageEntity>();
        var stringSheetList = string.Join(", ", sheets.Select(t => t.ToString()));
        var sheetsList = sheets.Select(x => x.GetDescription()).ToList();

        var response = await _googleSheetService.GetBatchData(sheets.Select(x => x.GetDescription()).ToList());
        Spreadsheet? spreadsheetInfo;

        if (response == null)
        {
            spreadsheetInfo = await _googleSheetService.GetSheetInfo();
            if (spreadsheetInfo != null)
            {
                var missingSheets = SheetHelpers.CheckSheets<SheetEnum>(spreadsheetInfo);

                if (missingSheets.Count != 0)
                {
                    messages.AddRange(SheetHelpers.CheckSheets(missingSheets));
                    messages.AddRange((await CreateSheets(missingSheets)).Messages);

                    // Reattempt to get the sheets once after creating them
                    response = await _googleSheetService.GetBatchData(sheets.Select(x => x.GetDescription()).ToList());
                    if (response == null)
                    {
                        messages.Add(MessageHelpers.CreateErrorMessage($"Unable to retrieve sheet(s) after creation", MessageTypeEnum.GET_SHEETS));
                    }
                }
            }
            else
            {
                messages.Add(MessageHelpers.CreateErrorMessage($"Unable to retrieve sheet(s)", MessageTypeEnum.GET_SHEETS));
            }
        }

        if (response != null)
        {
            messages.Add(MessageHelpers.CreateInfoMessage($"Retrieved sheet(s): {stringSheetList}", MessageTypeEnum.GET_SHEETS));
            
            var ranges = sheetsList.Select(sheet => $"{sheet}!{GoogleConfig.HeaderRange}").ToList();
            spreadsheetInfo = await _googleSheetService.GetSheetInfo(ranges);

            if (spreadsheetInfo != null)
            {
                messages.AddRange(CheckSheetHeaders(spreadsheetInfo));
            }

            data = GigSheetHelpers.MapData(response) ?? new SheetEntity();
        }

        data.Messages.AddRange(messages);

        return data;
    }

    public async Task<List<PropertyEntity>> GetSheetProperties() // TODO: Look into moving this to a common area
    {
        var sheets = Enum.GetValues(typeof(SheetEnum)).Cast<SheetEnum>().ToList();
        return await GetSheetProperties(sheets.Select(t => t.GetDescription()).ToList());
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
}
