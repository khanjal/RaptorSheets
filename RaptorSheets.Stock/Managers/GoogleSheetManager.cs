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
using SheetName = RaptorSheets.Stock.Enums.SheetName;

namespace RaptorSheets.Stock.Managers;

/// <summary>
/// Main interface for Google Sheet operations in the Stock domain. Shape matches Gig's
/// IGoogleSheetManager (string-based sheet names throughout) to keep the two domains' surfaces
/// comparable while more of RaptorSheets.Job/Home gets built on the same generic base.
/// </summary>
public interface IGoogleSheetManager
{
    // CRUD Operations
    Task<SheetEntity> ChangeSheetData(List<string> sheets, SheetEntity sheetEntity);
    Task<SheetEntity> CreateAllSheets();
    Task<SheetEntity> CreateSheets(List<string> sheets);
    Task<SheetEntity> DeleteAllSheets();
    Task<SheetEntity> DeleteSheets(List<string> sheets);
    Task<SheetEntity> GetSheet(string sheet);
    Task<SheetEntity> GetAllSheets();
    Task<SheetEntity> GetSheets(List<string> sheets);

    // Header Management
    SheetModel? GetSheetLayout(string sheet);
    List<SheetModel> GetSheetLayouts(List<string> sheets);
    Task<SheetEntity> InsertMissingColumns(Dictionary<string, List<ColumnInsertionInfo>> missingColumns);
}

public class GoogleSheetManager : GoogleSheetManagerBase<SheetEntity>, IGoogleSheetManager
{
    private static List<string> CanonicalSheetNames()
        => Enum.GetValues<SheetName>().Select(e => e.GetDescription()).ToList();

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
    /// self-heal, delegating straight to the base's string-keyed, index-ordered creation.
    /// </summary>
    protected override async Task<SheetEntity> CreateMissingSheetsAsync(Dictionary<string, int> missingIndexMap)
    {
        return await CreateSheets(missingIndexMap.Keys.ToList(), missingIndexMap);
    }

    /// <summary>
    /// Backs <see cref="GoogleSheetManagerBase{TEntity}.CreateSheets"/> and
    /// <see cref="GoogleSheetManagerBase{TEntity}.DeleteSheets"/> (for temp-sheet creation) with
    /// Stock's fully-configured AddSheet requests.
    /// </summary>
    protected override BatchUpdateSpreadsheetRequest GenerateSheetsRequest(List<string> sheetNames)
    {
        return GenerateSheetHelpers.Generate(sheetNames);
    }

    // This 1-arg overload exists because C# requires exact arity to implicitly satisfy
    // IGoogleSheetManager's single-parameter CreateSheets(List<string>) - an inherited method's
    // optional parameter doesn't count for interface matching the way it does for ordinary callers.
    public async Task<SheetEntity> CreateSheets(List<string> sheets)
    {
        return await CreateSheets(sheets, null);
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

    public async Task<SheetEntity> GetSheet(string sheet)
    {
        var sheetExists = CanonicalSheetNames().Any(name => string.Equals(name, sheet, StringComparison.OrdinalIgnoreCase));

        if (!sheetExists)
        {
            return new SheetEntity { Messages = [MessageHelpers.CreateErrorMessage($"Sheet {sheet.ToUpperInvariant()} does not exist", MessageType.GET_SHEETS)] };
        }

        return await GetSheets([sheet]);
    }

    // Only the Stocks sheet's Shares column is genuinely user-editable today - Accounts and Tickers
    // are fully formula/GOOGLEFINANCE-driven rollups, so they get no accessor entry (same as Gig's
    // read-only summary sheets - Daily/Weekly/Monthly/Yearly - having none).
    private static readonly Dictionary<string, GoogleRequestHelpers.SheetChangeAccessor<SheetEntity>> _sheetAccessors =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [SheetName.STOCKS.GetDescription()] = new(
                entity => entity.Sheets.Stocks.Count,
                entity => entity.Sheets.Stocks,
                (data, properties) => StockRequestHelpers.ChangeStockSheetData(data as List<StockEntity> ?? [], properties))
        };

    public async Task<SheetEntity> ChangeSheetData(List<string> sheets, SheetEntity sheetEntity)
    {
        var (sheetsWithData, resolveMessages) = GoogleRequestHelpers.ResolveSheetsWithData(sheets, sheetEntity, _sheetAccessors);
        sheetEntity.Messages.AddRange(resolveMessages);

        if (sheetsWithData.Count == 0)
        {
            sheetEntity.Messages.Add(MessageHelpers.CreateWarningMessage("No data to change", MessageType.GENERAL));
            return sheetEntity;
        }

        var sheetInfo = await GetSheetProperties(sheets);
        var (requests, buildMessages) = GoogleRequestHelpers.BuildChangeRequests(sheetsWithData, sheetEntity, _sheetAccessors, sheetInfo);
        sheetEntity.Messages.AddRange(buildMessages);

        var batchUpdateSpreadsheetRequest = new BatchUpdateSpreadsheetRequest { Requests = requests };
        var batchUpdateSpreadsheetResponse = await _googleSheetService.BatchUpdateSpreadsheet(batchUpdateSpreadsheetRequest);

        if (batchUpdateSpreadsheetResponse == null)
        {
            sheetEntity.Messages.Add(MessageHelpers.CreateErrorMessage($"Unable to save data", MessageType.SAVE_DATA));
        }

        return sheetEntity;
    }
}
