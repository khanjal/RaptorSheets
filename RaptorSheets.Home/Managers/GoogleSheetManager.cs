using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Logging;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Managers;
using RaptorSheets.Core.Models;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Home.Constants;
using RaptorSheets.Home.Entities;
using RaptorSheets.Home.Helpers;

namespace RaptorSheets.Home.Managers;

/// <summary>
/// Main interface for Google Sheet operations in the Home domain.
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

    // Metadata & Properties
    Task<List<PropertyEntity>> GetAllSheetProperties();
    Task<List<PropertyEntity>> GetSheetProperties(List<string> sheets);
    Task<List<string>> GetAllSheetTabNames();
    Task<Spreadsheet?> GetSpreadsheetInfo(List<string>? ranges = null);
    Task<BatchGetValuesByDataFilterResponse?> GetBatchData(List<string> sheets);
    SheetModel? GetSheetLayout(string sheet);
    List<SheetModel> GetSheetLayouts(List<string> sheets);

    // Header Management
    Task<SheetEntity> InsertMissingColumns(Dictionary<string, List<ColumnInsertionInfo>> missingColumns);
}

/// <summary>
/// Main Google Sheet Manager for the Home domain.
///
/// Domain-agnostic read/metadata/layout/heal orchestration is inherited from
/// <see cref="GoogleSheetManagerBase{TEntity}"/>. This class adds only the Home-specific pieces:
/// constructors, the CreateMissingSheetsAsync self-heal hook, the GenerateSheetsRequest override,
/// and the domain write operations (ordered CreateSheets, ChangeSheetData) plus static
/// header-check helpers.
/// </summary>
public class GoogleSheetManager : GoogleSheetManagerBase<SheetEntity>, IGoogleSheetManager
{
    #region Construction

    public GoogleSheetManager(RaptorSheets.Core.Services.IGoogleSheetService googleSheetService, ILogger? logger = null)
        : base(googleSheetService, HomeSheetHelpers.Registry, GenerateSheetsHelpers.GetSheetNames(), logger)
    {
    }

    public GoogleSheetManager(string accessToken, string spreadsheetId, ILogger? logger = null)
        : base(accessToken, spreadsheetId, HomeSheetHelpers.Registry, GenerateSheetsHelpers.GetSheetNames(), logger)
    {
    }

    public GoogleSheetManager(Dictionary<string, string> parameters, string spreadsheetId, ILogger? logger = null)
        : base(parameters, spreadsheetId, HomeSheetHelpers.Registry, GenerateSheetsHelpers.GetSheetNames(), logger)
    {
    }

    protected override Task<SheetEntity> CreateMissingSheetsAsync(Dictionary<string, int> missingIndexMap)
    {
        return CreateSheets(missingIndexMap);
    }

    protected override BatchUpdateSpreadsheetRequest GenerateSheetsRequest(List<string> sheetNames)
    {
        return GenerateSheetsHelpers.Generate(sheetNames);
    }

    #endregion

    #region Create Operations

    // 1-arg overload to satisfy IGoogleSheetManager's exact arity.
    public async Task<SheetEntity> CreateSheets(List<string> sheets)
    {
        return await CreateSheets(sheets, null);
    }

    /// <summary>
    /// Creates sheets using a title->desiredIndex map. The map's keys are the sheet titles to create.
    /// </summary>
    public async Task<SheetEntity> CreateSheets(Dictionary<string, int> sheetsWithIndices)
    {
        if (sheetsWithIndices == null || sheetsWithIndices.Count == 0)
        {
            return await CreateSheets(new List<string>());
        }

        var sheets = SheetOrderingHelper.OrderSheetTitlesByIndex(sheetsWithIndices);

        return await CreateSheets(sheets, sheetsWithIndices);
    }

    #endregion

    #region Read Operations

    public async Task<SheetEntity> GetSheet(string sheet)
    {
        var sheetExists = GenerateSheetsHelpers.GetSheetNames()
            .Any(name => string.Equals(name, sheet, StringComparison.OrdinalIgnoreCase));

        if (!sheetExists)
        {
            return new SheetEntity { Messages = [MessageHelpers.CreateErrorMessage($"Sheet {sheet.ToUpperInvariant()} does not exist", MessageTypeEnum.GET_SHEETS)] };
        }

        return await GetSheets([sheet]);
    }

    #endregion

    #region Update Operations

    private static readonly Dictionary<string, GoogleRequestHelpers.SheetChangeAccessor<SheetEntity>> _sheetAccessors =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [SheetsConfig.SheetNames.Appliances] = new(
                entity => entity.Sheets.Appliances.Count,
                entity => entity.Sheets.Appliances,
                (data, properties) => HomeRequestHelpers.ChangeApplianceSheetData(data as List<ApplianceEntity> ?? [], properties)),
            [SheetsConfig.SheetNames.Projects] = new(
                entity => entity.Sheets.Projects.Count,
                entity => entity.Sheets.Projects,
                (data, properties) => HomeRequestHelpers.ChangeProjectSheetData(data as List<ProjectEntity> ?? [], properties)),
            [SheetsConfig.SheetNames.Maintenance] = new(
                entity => entity.Sheets.Maintenance.Count,
                entity => entity.Sheets.Maintenance,
                (data, properties) => HomeRequestHelpers.ChangeMaintenanceSheetData(data as List<MaintenanceEntity> ?? [], properties)),
            [SheetsConfig.SheetNames.Doors] = new(
                entity => entity.Sheets.Doors.Count,
                entity => entity.Sheets.Doors,
                (data, properties) => HomeRequestHelpers.ChangeDoorSheetData(data as List<DoorEntity> ?? [], properties)),
            [SheetsConfig.SheetNames.Paints] = new(
                entity => entity.Sheets.Paints.Count,
                entity => entity.Sheets.Paints,
                (data, properties) => HomeRequestHelpers.ChangePaintSheetData(data as List<PaintEntity> ?? [], properties)),
            [SheetsConfig.SheetNames.Power] = new(
                entity => entity.Sheets.Power.Count,
                entity => entity.Sheets.Power,
                (data, properties) => HomeRequestHelpers.ChangePowerSheetData(data as List<PowerEntity> ?? [], properties)),
            [SheetsConfig.SheetNames.Rooms] = new(
                entity => entity.Sheets.Rooms.Count,
                entity => entity.Sheets.Rooms,
                (data, properties) => HomeRequestHelpers.ChangeRoomSheetData(data as List<RoomEntity> ?? [], properties)),
            [SheetsConfig.SheetNames.Contacts] = new(
                entity => entity.Sheets.Contacts.Count,
                entity => entity.Sheets.Contacts,
                (data, properties) => HomeRequestHelpers.ChangeContactSheetData(data as List<ContactEntity> ?? [], properties)),
            [SheetsConfig.SheetNames.Stats] = new(
                entity => entity.Sheets.Stats.Count,
                entity => entity.Sheets.Stats,
                (data, properties) => HomeRequestHelpers.ChangeStatSheetData(data as List<StatEntity> ?? [], properties))
        };

    public async Task<SheetEntity> ChangeSheetData(List<string> sheets, SheetEntity sheetEntity)
    {
        var (sheetsWithData, resolveMessages) = GoogleRequestHelpers.ResolveSheetsWithData(sheets, sheetEntity, _sheetAccessors);
        sheetEntity.Messages.AddRange(resolveMessages);

        if (sheetsWithData.Count == 0)
        {
            sheetEntity.Messages.Add(MessageHelpers.CreateWarningMessage("No data to change", MessageTypeEnum.GENERAL));
            return sheetEntity;
        }

        var sheetInfo = await GetSheetProperties(sheets);
        var (requests, buildMessages) = GoogleRequestHelpers.BuildChangeRequests(sheetsWithData, sheetEntity, _sheetAccessors, sheetInfo);
        sheetEntity.Messages.AddRange(buildMessages);

        var batchUpdateSpreadsheetRequest = new BatchUpdateSpreadsheetRequest { Requests = requests };
        var batchUpdateSpreadsheetResponse = await _googleSheetService.BatchUpdateSpreadsheet(batchUpdateSpreadsheetRequest);

        if (batchUpdateSpreadsheetResponse == null)
        {
            sheetEntity.Messages.Add(MessageHelpers.CreateErrorMessage($"Unable to save data", MessageTypeEnum.SAVE_DATA));
        }

        return sheetEntity;
    }

    #endregion

    #region Header Validation

    public static List<MessageEntity> CheckUnknownSheets(Spreadsheet sheetInfoResponse)
    {
        return HomeSheetHelpers.CheckUnknownSheets(sheetInfoResponse);
    }

    public static List<MessageEntity> CheckSheetHeaders(Spreadsheet sheetInfoResponse)
    {
        return HomeSheetHelpers.CheckSheetHeaders(sheetInfoResponse);
    }

    public static List<MessageEntity> CheckSheetHeaders(Spreadsheet sheetInfoResponse, out Dictionary<string, List<ColumnInsertionInfo>> missingColumns)
    {
        return HomeSheetHelpers.CheckSheetHeaders(sheetInfoResponse, out missingColumns);
    }

    #endregion
}
