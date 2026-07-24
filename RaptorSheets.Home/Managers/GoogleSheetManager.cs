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
    Task<SheetEntity> ChangeSheetData(List<string> sheets, SheetEntity sheetEntity, CancellationToken cancellationToken = default);
    Task<SheetEntity> CreateAllSheets(CancellationToken cancellationToken = default);
    Task<SheetEntity> CreateSheets(List<string> sheets, CancellationToken cancellationToken = default);
    Task<SheetEntity> DeleteAllSheets(CancellationToken cancellationToken = default);
    Task<SheetEntity> DeleteSheets(List<string> sheets, CancellationToken cancellationToken = default);
    Task<SheetEntity> GetSheet(string sheet, CancellationToken cancellationToken = default);
    Task<SheetEntity> GetAllSheets(CancellationToken cancellationToken = default);
    Task<SheetEntity> GetSheets(List<string> sheets, CancellationToken cancellationToken = default);

    // Metadata & Properties
    Task<List<PropertyEntity>> GetAllSheetProperties(CancellationToken cancellationToken = default);
    Task<List<PropertyEntity>> GetSheetProperties(List<string> sheets, CancellationToken cancellationToken = default);
    Task<List<string>> GetAllSheetTabNames(CancellationToken cancellationToken = default);
    Task<Spreadsheet?> GetSpreadsheetInfo(List<string>? ranges = null, CancellationToken cancellationToken = default);
    Task<BatchGetValuesByDataFilterResponse?> GetBatchData(List<string> sheets, CancellationToken cancellationToken = default);
    SheetModel? GetSheetLayout(string sheet);
    List<SheetModel> GetSheetLayouts(List<string> sheets);

    // Header Management
    Task<SheetEntity> InsertMissingColumns(Dictionary<string, List<ColumnInsertionInfo>> missingColumns, CancellationToken cancellationToken = default);

    // Demo Data Generation
    Task<SheetEntity> SetupDemo(int? seed = null, CancellationToken cancellationToken = default);
    Task<SheetEntity> PopulateDemoData(int? seed = null, CancellationToken cancellationToken = default);
    SheetEntity GenerateDemoData(int? seed = null);
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

    protected override Task<SheetEntity> CreateMissingSheetsAsync(Dictionary<string, int> missingIndexMap, CancellationToken cancellationToken = default)
    {
        return CreateSheets(missingIndexMap, cancellationToken);
    }

    protected override BatchUpdateSpreadsheetRequest GenerateSheetsRequest(List<string> sheetNames)
    {
        return GenerateSheetsHelpers.Generate(sheetNames);
    }

    #endregion

    #region Create Operations

    // 1-arg overload to satisfy IGoogleSheetManager's exact arity.
    public async Task<SheetEntity> CreateSheets(List<string> sheets, CancellationToken cancellationToken = default)
    {
        return await CreateSheets(sheets, null, cancellationToken);
    }

    /// <summary>
    /// Creates sheets using a title->desiredIndex map. The map's keys are the sheet titles to create.
    /// </summary>
    public async Task<SheetEntity> CreateSheets(Dictionary<string, int> sheetsWithIndices, CancellationToken cancellationToken = default)
    {
        if (sheetsWithIndices == null || sheetsWithIndices.Count == 0)
        {
            return await CreateSheets(new List<string>(), cancellationToken);
        }

        var sheets = SheetOrderingHelper.OrderSheetTitlesByIndex(sheetsWithIndices);

        return await CreateSheets(sheets, sheetsWithIndices, cancellationToken);
    }

    #endregion

    #region Read Operations

    public async Task<SheetEntity> GetSheet(string sheet, CancellationToken cancellationToken = default)
    {
        var sheetExists = GenerateSheetsHelpers.GetSheetNames()
            .Any(name => string.Equals(name, sheet, StringComparison.OrdinalIgnoreCase));

        if (!sheetExists)
        {
            return new SheetEntity { Messages = [MessageHelpers.CreateErrorMessage($"Sheet {sheet.ToUpperInvariant()} does not exist", MessageType.GET_SHEETS)] };
        }

        return await GetSheets([sheet], cancellationToken);
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
            [SheetsConfig.SheetNames.DoorsWindows] = new(
                entity => entity.Sheets.DoorsWindows.Count,
                entity => entity.Sheets.DoorsWindows,
                (data, properties) => HomeRequestHelpers.ChangeDoorWindowSheetData(data as List<DoorWindowEntity> ?? [], properties)),
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

    public async Task<SheetEntity> ChangeSheetData(List<string> sheets, SheetEntity sheetEntity, CancellationToken cancellationToken = default)
    {
        var (sheetsWithData, resolveMessages) = GoogleRequestHelpers.ResolveSheetsWithData(sheets, sheetEntity, _sheetAccessors);
        sheetEntity.Messages.AddRange(resolveMessages);

        if (sheetsWithData.Count == 0)
        {
            sheetEntity.Messages.Add(MessageHelpers.CreateWarningMessage("No data to change", MessageType.GENERAL));
            return sheetEntity;
        }

        var sheetInfo = await GetSheetProperties(sheets, cancellationToken);
        var (requests, buildMessages) = GoogleRequestHelpers.BuildChangeRequests(sheetsWithData, sheetEntity, _sheetAccessors, sheetInfo);
        sheetEntity.Messages.AddRange(buildMessages);

        var batchUpdateSpreadsheetRequest = new BatchUpdateSpreadsheetRequest { Requests = requests };
        var batchUpdateSpreadsheetResponse = await _googleSheetService.BatchUpdateSpreadsheet(batchUpdateSpreadsheetRequest, cancellationToken);

        if (batchUpdateSpreadsheetResponse == null)
        {
            sheetEntity.Messages.Add(MessageHelpers.CreateErrorMessage($"Unable to save data", MessageType.SAVE_DATA));
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

    #region Demo Data Generation

    /// <summary>
    /// Creates all sheets and then fills every sheet with a realistic sample household's worth of
    /// demo data.
    /// </summary>
    public async Task<SheetEntity> SetupDemo(int? seed = null, CancellationToken cancellationToken = default)
    {
        await CreateAllSheets(cancellationToken);
        await Task.Delay(1500, cancellationToken); // let freshly-created sheets become writable
        return await PopulateDemoData(seed, cancellationToken);
    }

    /// <summary>
    /// Writes generated demo data into every Home sheet in a single batch. Unlike Stock/Gig/Job,
    /// every Home sheet is directly user-entered (none are formula-derived from another), so all
    /// nine get written here rather than just one or two input sheets.
    /// </summary>
    public async Task<SheetEntity> PopulateDemoData(int? seed = null, CancellationToken cancellationToken = default)
    {
        var demoData = GenerateDemoData(seed);

        var sheetsToWrite = new List<string>
        {
            SheetsConfig.SheetNames.Rooms,
            SheetsConfig.SheetNames.Contacts,
            SheetsConfig.SheetNames.Appliances,
            SheetsConfig.SheetNames.Projects,
            SheetsConfig.SheetNames.Maintenance,
            SheetsConfig.SheetNames.DoorsWindows,
            SheetsConfig.SheetNames.Paints,
            SheetsConfig.SheetNames.Power,
            SheetsConfig.SheetNames.Stats
        };

        await ChangeSheetData(sheetsToWrite, demoData, cancellationToken);
        return demoData;
    }

    /// <summary>
    /// Generates a small, realistic sample household's worth of data across every Home sheet,
    /// without writing it anywhere. RowIds start at 2 (per sheet) so a subsequent write lands rows
    /// below the header row. Deliberately a fixed, curated set rather than randomly generated volume
    /// (unlike Gig/Job's demo data) - the goal is a clean example a user can look at and edit, not a
    /// large synthetic dataset. <paramref name="seed"/> is accepted for API consistency with the
    /// other domains' demo-data methods but isn't currently used.
    /// </summary>
    public SheetEntity GenerateDemoData(int? seed = null)
    {
        const string livingRoom = "Living Room";
        const string kitchen = "Kitchen";
        const string garage = "Garage";

        var sheetEntity = new SheetEntity();

        sheetEntity.Sheets.Rooms.AddRange(
        [
            new RoomEntity { RowId = 2, Room = livingRoom, Length = 15, Width = 12, Level = "Main" },
            new RoomEntity { RowId = 3, Room = kitchen, Length = 12, Width = 10, Level = "Main" },
            new RoomEntity { RowId = 4, Room = "Primary Bedroom", Length = 14, Width = 13, Level = "Upper" },
            new RoomEntity { RowId = 5, Room = "Bathroom", Length = 8, Width = 6, Level = "Upper" },
            new RoomEntity { RowId = 6, Room = garage, Length = 20, Width = 20, Level = "Main" },
            // Detached structures use Level to say so, instead of a floor like Main/Upper
            new RoomEntity { RowId = 7, Room = "Shed", Length = 10, Width = 8, Level = "Shed" }
        ]);

        sheetEntity.Sheets.Contacts.AddRange(
        [
            new ContactEntity { RowId = 2, Name = "Ace Plumbing", Number = "555-0100", Description = "Plumber" },
            new ContactEntity { RowId = 3, Name = "Bright Spark Electric", Number = "555-0111", Description = "Electrician" },
            new ContactEntity { RowId = 4, Name = "Cool Breeze HVAC", Number = "555-0122", Description = "HVAC Technician" }
        ]);

        sheetEntity.Sheets.Appliances.AddRange(
        [
            new ApplianceEntity { RowId = 2, Type = "Refrigerator", Location = kitchen, Manufacturer = "LG", Model = "LRFVS3006S", FilterDate = "2026-01-15", ReplacementMonths = 6, OriginalPrice = 1899.99m },
            new ApplianceEntity { RowId = 3, Type = "Washer", Location = garage, Manufacturer = "Samsung", Model = "WF45T6000AW", OriginalPrice = 749.99m },
            new ApplianceEntity { RowId = 4, Type = "Furnace", Location = garage, Manufacturer = "Carrier", Model = "59SC5", FilterDate = "2026-02-01", ReplacementMonths = 3, OriginalPrice = 3200.00m },
            // Whole-property energy systems fit the same generic shape (EnergySource/Capacity cover
            // what Filter/FilterDate cover for appliances that need it instead)
            new ApplianceEntity { RowId = 5, Type = "Solar Panel System", Manufacturer = "SunPower", Model = "X22-370", EnergySource = "Solar", Capacity = "6.5 kW", OriginalPrice = 18500.00m },
            new ApplianceEntity { RowId = 6, Type = "Generator", Location = garage, Manufacturer = "Generac", Model = "Guardian 22kW", EnergySource = "Propane", Capacity = "22 kW", OriginalPrice = 4500.00m }
        ]);

        sheetEntity.Sheets.Projects.AddRange(
        [
            new ProjectEntity { RowId = 2, Task = "Repaint Living Room", Area = livingRoom, Started = "2026-03-01", Completed = "2026-03-05", ApproximateCost = 250.00m },
            new ProjectEntity { RowId = 3, Task = "Install Ceiling Fan", Area = "Primary Bedroom", Started = "2026-04-10", ApproximateCost = 180.00m }
        ]);

        sheetEntity.Sheets.Maintenance.AddRange(
        [
            new MaintenanceEntity { RowId = 2, Date = "2026-02-14", Problem = "Leaky kitchen faucet", CompanyPerson = "Ace Plumbing", Solution = "Replaced cartridge", Amount = 145.00m },
            new MaintenanceEntity { RowId = 3, Date = "2026-05-02", Problem = "Outlet not working", CompanyPerson = "Bright Spark Electric", Solution = "Replaced GFI outlet", Amount = 95.00m }
        ]);

        sheetEntity.Sheets.DoorsWindows.AddRange(
        [
            new DoorWindowEntity { RowId = 2, Location = livingRoom, Type = "Front Door", Brand = "Therma-Tru", Installed = "2018-06-01" },
            new DoorWindowEntity { RowId = 3, Location = kitchen, Type = "Sliding Door", Brand = "Andersen", Installed = "2018-06-01" },
            new DoorWindowEntity { RowId = 4, Location = livingRoom, Type = "Bay Window", Brand = "Pella", Installed = "2015-09-12" }
        ]);

        sheetEntity.Sheets.Paints.AddRange(
        [
            new PaintEntity { RowId = 2, Brand = "Sherwin-Williams", Type = "Eggshell", Color = "Agreeable Gray", Location = livingRoom, Remaining = "3/4 gallon", Size = "1 gallon" },
            new PaintEntity { RowId = 3, Brand = "Benjamin Moore", Type = "Satin", Color = "White Dove", Location = kitchen, Remaining = "1/2 gallon", Size = "1 gallon" }
        ]);

        sheetEntity.Sheets.Power.AddRange(
        [
            new PowerEntity { RowId = 2, Location = kitchen, Type = "Outlet", Position = "Countertop", Amps = 20, Grounded = true, GFI = true },
            new PowerEntity { RowId = 3, Location = garage, Type = "Outlet", Position = "Workbench", Amps = 20, Grounded = true, GFI = true }
        ]);

        sheetEntity.Sheets.Stats.AddRange(
        [
            new StatEntity { RowId = 2, Name = "Beds", Value = "3" },
            new StatEntity { RowId = 3, Name = "Baths", Value = "2" },
            new StatEntity { RowId = 4, Name = "Built", Value = "1998" },
            new StatEntity { RowId = 5, Name = "Square Footage", Value = "2400" },
            new StatEntity { RowId = 6, Name = "Roof Type", Value = "Asphalt Shingle" },
            new StatEntity { RowId = 7, Name = "Roof Installed", Value = "2015" },
            // A second structure's roof is just another name/value pair - Stats isn't limited to one
            // of each fact
            new StatEntity { RowId = 8, Name = "Shed Roof Type", Value = "Metal" }
        ]);

        return sheetEntity;
    }

    #endregion
}
