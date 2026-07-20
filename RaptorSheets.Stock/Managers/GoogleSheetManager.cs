using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Logging;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Services;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Managers;
using RaptorSheets.Core.Models;
using RaptorSheets.Stock.Entities;
using RaptorSheets.Stock.Helpers;
using RaptorSheets.Core.Models.Google;
using SheetEnum = RaptorSheets.Stock.Enums.SheetEnum;

namespace RaptorSheets.Stock.Managers;

public interface IGoogleSheetManager
{
    public Task<SheetEntity> AddSheetData(List<Enums.SheetEnum> sheets, SheetEntity sheetEntity);
    public Task<SheetEntity> CreateSheets();
    public Task<SheetEntity> CreateSheets(List<Enums.SheetEnum> sheets);
    public Task<SheetEntity> GetSheet(string sheet);
    public Task<SheetEntity> GetSheets();
    public Task<SheetEntity> GetSheets(List<Enums.SheetEnum> sheets);
    public SheetModel? GetSheetLayout(string sheet);
    public List<SheetModel> GetSheetLayouts(List<string> sheets);
    public Task<SheetEntity> InsertMissingColumns(Dictionary<string, List<ColumnInsertionInfo>> missingColumns);
}

public class GoogleSheetManager : GoogleSheetManagerBase<SheetEntity>, IGoogleSheetManager
{
    private static List<string> CanonicalSheetNames()
        => Enum.GetValues<SheetEnum>().Select(e => e.GetDescription()).ToList();

    public GoogleSheetManager(IGoogleSheetService googleSheetService, ILogger? logger = null)
        : base(googleSheetService, StockSheetHelpers.Registry, CanonicalSheetNames(), logger)
    {
    }

    public GoogleSheetManager(string accessToken, string spreadsheetId, ILogger? logger = null)
        : base(accessToken, spreadsheetId, StockSheetHelpers.Registry, CanonicalSheetNames(), logger)
    {
    }

    public GoogleSheetManager(Dictionary<string, string> parameters, string spreadsheetId, ILogger? logger = null)
        : base(parameters, spreadsheetId, StockSheetHelpers.Registry, CanonicalSheetNames(), logger)
    {
    }

    /// <summary>
    /// Restores sheets found missing entirely during <see cref="GoogleSheetManagerBase{TEntity}.GetSheets"/>
    /// self-heal. Stock doesn't support Gig's index-ordered creation, so the desired-index map's
    /// ordering is not preserved - only which sheets need to exist.
    /// </summary>
    protected override async Task<SheetEntity> CreateMissingSheetsAsync(Dictionary<string, int> missingIndexMap)
    {
        var missingSheets = missingIndexMap.Keys
            .Select(name => name.GetValueFromName<Enums.SheetEnum>())
            .ToList();
        return await CreateSheets(missingSheets);
    }

    public async Task<SheetEntity> AddSheetData(List<Enums.SheetEnum> sheets, SheetEntity sheetEntity)
    {
        foreach (var sheet in sheets)
        {
            var headers = (await _googleSheetService.GetSheetData(sheet.GetDescription()))?.Values[0];

            if (headers == null)
            {
                // Add error message if headers are missing (sheet not supported or not found)
                sheetEntity.Messages.Add(MessageHelpers.CreateErrorMessage($"Adding data to {sheet.UpperName()} not supported (headers not found)", MessageTypeEnum.ADD_DATA));
                continue;
            }

            IList<IList<object?>> values = [];

            switch (sheet)
            {
                case Enums.SheetEnum.ACCOUNTS:
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

    /// <summary>
    /// Checks a spreadsheet's tab names for sheets that don't correspond to any known Stock sheet.
    /// Only needs sheet tab metadata (no grid/cell data). Static so callers can use it off the type
    /// without a manager instance; thin shim over <see cref="StockSheetHelpers"/>.
    /// </summary>
    public static List<MessageEntity> CheckUnknownSheets(Spreadsheet sheetInfoResponse)
    {
        return StockSheetHelpers.CheckUnknownSheets(sheetInfoResponse);
    }

    public static List<MessageEntity> CheckSheetHeaders(Spreadsheet sheetInfoResponse)
    {
        return StockSheetHelpers.CheckSheetHeaders(sheetInfoResponse);
    }

    /// <summary>
    /// Same as <see cref="CheckSheetHeaders(Spreadsheet)"/>, but also reports which columns are
    /// missing entirely and where they should be inserted, for use with
    /// <see cref="GoogleSheetManagerBase{TEntity}.InsertMissingColumns"/>.
    /// </summary>
    public static List<MessageEntity> CheckSheetHeaders(Spreadsheet sheetInfoResponse, out Dictionary<string, List<ColumnInsertionInfo>> missingColumns)
    {
        return StockSheetHelpers.CheckSheetHeaders(sheetInfoResponse, out missingColumns);
    }

    public async Task<SheetEntity> CreateSheets()
    {
        var sheets = Enum.GetValues(typeof(SheetEnum)).Cast<SheetEnum>().ToList();
        return await CreateSheets(sheets);
    }

    public async Task<SheetEntity> CreateSheets(List<Enums.SheetEnum> sheets)
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
            sheetEntity.Messages.Add(MessageHelpers.CreateInfoMessage($"{sheetTitle.GetValueFromName<Enums.SheetEnum>()} created", MessageTypeEnum.CREATE_SHEET));
        }

        return sheetEntity;
    }

    public async Task<SheetEntity> GetSheet(string sheet)
    {
        var sheetExists = Enum.TryParse(sheet.ToUpper(), out Enums.SheetEnum sheetEnum) && Enum.IsDefined(typeof(Enums.SheetEnum), sheetEnum);

        if (!sheetExists)
        {
            return new SheetEntity { Messages = [MessageHelpers.CreateErrorMessage($"Sheet {sheet.ToUpperInvariant()} does not exist", MessageTypeEnum.GET_SHEETS)] };
        }

        return await GetSheets([sheetEnum]);
    }

    public async Task<SheetEntity> GetSheets()
    {
        return await GetAllSheets();
    }

    public async Task<SheetEntity> GetSheets(List<Enums.SheetEnum> sheets)
    {
        // Orchestration (batchGet -> self-heal missing sheets -> unknown-tab detection -> map data ->
        // auto-heal missing columns -> spreadsheet name) is inherited from
        // GoogleSheetManagerBase<SheetEntity>.GetSheets(List<string>). Missing sheets are restored via
        // this domain's CreateMissingSheetsAsync override (enum-based CreateSheets).
        var sheetNames = sheets.Select(x => x.GetDescription()).ToList();
        return await GetSheets(sheetNames);
    }
}
