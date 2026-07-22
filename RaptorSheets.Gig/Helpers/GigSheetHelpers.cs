using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Models;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Core.Registries;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Mappers;

namespace RaptorSheets.Gig.Helpers;

/// <summary>
/// Helper methods for Google Sheets operations in the Gig domain.
///
/// HYBRID APPROACH:
/// Uses both constants and enums for optimal performance and maintainability:
/// - Switch statements use normalized strings for performance
/// - Enums provide type safety for API operations
/// - Constants ensure consistent string values throughout
///
/// Per-sheet dispatch (headers, row mapping, missing-sheet detection) is delegated to a shared
/// RaptorSheets.Core.Registries.SheetRegistry&lt;SheetEntity&gt; instead of hand-rolled dictionaries/loops,
/// so the same orchestration is reused by other domain packages (see RaptorSheets.Stock) without
/// requiring a generic Cell/Row entity model.
/// </summary>
public static class GigSheetHelpers
{
    public static List<SheetModel> GetSheets()
    {
        var sheets = new List<SheetModel>
        {
            ShiftMapper.GetSheet(),
            TripMapper.GetSheet()
        };

        return sheets;
    }

    public static List<string> GetSheetNames()
    {
        return SheetsConfig.SheetUtilities.GetAllSheetNames();
    }

    /// <summary>
    /// The shared registry backing this domain's header/row-mapping/missing-column orchestration.
    /// Exposed so <see cref="RaptorSheets.Core.Managers.GoogleSheetManagerBase"/>'s generic
    /// GetSheetsCoreAsync/AutoHealMissingColumnsAsync can operate on it directly.
    /// </summary>
    public static SheetRegistry<SheetEntity> Registry => s_registry;

    private static readonly SheetRegistry<SheetEntity> s_registry = BuildRegistry();

    private static SheetRegistry<SheetEntity> BuildRegistry()
    {
        var registry = new SheetRegistry<SheetEntity>();

        // dependsOn declares which other sheet(s) each mapper's GetSheet() cross-references via
        // GetRange/GetLocalRange/GetRangeBetweenColumns, so RefreshDependentSheetsAsync knows to
        // rewrite this sheet's header formulas whenever one of those sheets is created/healed/changed.
        registry.RegisterGeneric<SheetEntity, AddressEntity>(SheetsConfig.SheetNames.Addresses, AddressMapper.GetSheet, (se, rows) => se.Sheets.Addresses = rows, dependsOn: [SheetsConfig.SheetNames.Trips]);
        registry.RegisterGeneric<SheetEntity, DailyEntity>(SheetsConfig.SheetNames.Daily, DailyMapper.GetSheet, (se, rows) => se.Sheets.Daily = rows, dependsOn: [SheetsConfig.SheetNames.Shifts]);
        registry.RegisterGeneric<SheetEntity, ExpenseEntity>(SheetsConfig.SheetNames.Expenses, () => GenericSheetMapper<ExpenseEntity>.GetSheet(SheetsConfig.ExpenseSheet), (se, rows) => se.Sheets.Expenses = rows);
        registry.RegisterGeneric<SheetEntity, MonthlyEntity>(SheetsConfig.SheetNames.Monthly, MonthlyMapper.GetSheet, (se, rows) => se.Sheets.Monthly = rows, dependsOn: [SheetsConfig.SheetNames.Daily]);
        registry.RegisterGeneric<SheetEntity, NameEntity>(SheetsConfig.SheetNames.Names, NameMapper.GetSheet, (se, rows) => se.Sheets.Names = rows, dependsOn: [SheetsConfig.SheetNames.Trips]);
        registry.RegisterGeneric<SheetEntity, PlaceEntity>(SheetsConfig.SheetNames.Places, PlaceMapper.GetSheet, (se, rows) => se.Sheets.Places = rows, dependsOn: [SheetsConfig.SheetNames.Trips]);
        registry.RegisterGeneric<SheetEntity, DeliveryEntity>(SheetsConfig.SheetNames.Deliveries, DeliveryMapper.GetSheet, (se, rows) => se.Sheets.Deliveries = rows, dependsOn: [SheetsConfig.SheetNames.Trips]);
        registry.RegisterGeneric<SheetEntity, LocationEntity>(SheetsConfig.SheetNames.Locations, LocationMapper.GetSheet, (se, rows) => se.Sheets.Locations = rows, dependsOn: [SheetsConfig.SheetNames.Trips]);
        registry.RegisterGeneric<SheetEntity, RegionEntity>(SheetsConfig.SheetNames.Regions, RegionMapper.GetSheet, (se, rows) => se.Sheets.Regions = rows, dependsOn: [SheetsConfig.SheetNames.Trips, SheetsConfig.SheetNames.Shifts]);
        registry.RegisterGeneric<SheetEntity, SetupEntity>(SheetsConfig.SheetNames.Setup, () => GenericSheetMapper<SetupEntity>.GetSheet(SheetsConfig.SetupSheet), (se, rows) => se.Sheets.Setup = rows);
        registry.RegisterGeneric<SheetEntity, ServiceEntity>(SheetsConfig.SheetNames.Services, ServiceMapper.GetSheet, (se, rows) => se.Sheets.Services = rows, dependsOn: [SheetsConfig.SheetNames.Trips, SheetsConfig.SheetNames.Shifts]);
        registry.RegisterGeneric<SheetEntity, ShiftEntity>(SheetsConfig.SheetNames.Shifts, ShiftMapper.GetSheet, (se, rows) => se.Sheets.Shifts = rows, dependsOn: [SheetsConfig.SheetNames.Trips]);
        registry.RegisterGeneric<SheetEntity, TripEntity>(SheetsConfig.SheetNames.Trips, TripMapper.GetSheet, (se, rows) => se.Sheets.Trips = rows);
        registry.RegisterGeneric<SheetEntity, TypeEntity>(SheetsConfig.SheetNames.Types, TypeMapper.GetSheet, (se, rows) => se.Sheets.Types = rows, dependsOn: [SheetsConfig.SheetNames.Trips]);
        registry.RegisterGeneric<SheetEntity, WeekdayEntity>(SheetsConfig.SheetNames.Weekdays, WeekdayMapper.GetSheet, (se, rows) => se.Sheets.Weekdays = rows, dependsOn: [SheetsConfig.SheetNames.Daily]);
        registry.RegisterGeneric<SheetEntity, WeeklyEntity>(SheetsConfig.SheetNames.Weekly, WeeklyMapper.GetSheet, (se, rows) => se.Sheets.Weekly = rows, dependsOn: [SheetsConfig.SheetNames.Daily]);
        registry.RegisterGeneric<SheetEntity, YearlyEntity>(SheetsConfig.SheetNames.Yearly, YearlyMapper.GetSheet, (se, rows) => se.Sheets.Yearly = rows, dependsOn: [SheetsConfig.SheetNames.Monthly]);

        return registry;
    }

    public static List<SheetModel> GetMissingSheets(Spreadsheet spreadsheet)
    {
        return s_registry.GetMissingSheets(spreadsheet, GetSheetNames());
    }

    /// <summary>
    /// Checks a spreadsheet's tab names for sheets that don't correspond to any known Gig sheet.
    /// Only needs sheet tab metadata (no grid/cell data) - safe to call with a cheap, no-ranges
    /// spreadsheet fetch. Known-sheet header validation happens separately via <see cref="MapData(BatchGetValuesByDataFilterResponse)"/>.
    /// </summary>
    public static List<MessageEntity> CheckUnknownSheets(Spreadsheet spreadsheet)
    {
        return s_registry.CheckUnknownSheets(spreadsheet);
    }

    /// <summary>
    /// Full header validation against grid-data (IncludeGridData=true) spreadsheet metadata.
    /// </summary>
    public static List<MessageEntity> CheckSheetHeaders(Spreadsheet spreadsheet)
    {
        return s_registry.CheckSheetHeaders(spreadsheet);
    }

    /// <summary>
    /// Same as <see cref="CheckSheetHeaders(Spreadsheet)"/>, but also reports which columns are
    /// missing entirely and where they should be inserted, for use with
    /// <see cref="RaptorSheets.Core.Helpers.ColumnInsertionHelper"/>.
    /// </summary>
    public static List<MessageEntity> CheckSheetHeaders(Spreadsheet spreadsheet, out Dictionary<string, List<ColumnInsertionInfo>> missingColumns)
    {
        return s_registry.CheckSheetHeaders(spreadsheet, out missingColumns);
    }

    /// <summary>
    /// Detects columns missing entirely from a batchGet response, reusing the header row already
    /// present in each range - no extra API call. SheetId is left at 0; the caller fills it in.
    /// </summary>
    public static Dictionary<string, List<ColumnInsertionInfo>> DetectMissingColumns(BatchGetValuesByDataFilterResponse response)
    {
        return s_registry.DetectMissingColumns(response);
    }

    public static SheetModel? GetSheetLayout(string sheetName)
    {
        return s_registry.GetSheetLayout(sheetName);
    }

    public static List<SheetModel> GetSheetLayouts(IEnumerable<string> sheetNames)
    {
        return s_registry.GetSheetLayouts(sheetNames);
    }

    public static DataValidationRule GetDataValidation(Validation validation, string? range = "")
    {
        return validation switch
        {
            Validation.BOOLEAN => GoogleValidationHelper.CreateBooleanRule(),
            Validation.RANGE_ADDRESS or Validation.RANGE_NAME or Validation.RANGE_PLACE
                or Validation.RANGE_REGION or Validation.RANGE_SERVICE or Validation.RANGE_TYPE
                => GoogleValidationHelper.CreateOneOfRangeRule($"{GetSheetForRange(validation)}!A2:A"),
            Validation.RANGE_SELF => GoogleValidationHelper.CreateOneOfRangeRule($"{range}"),
            _ => new DataValidationRule()
        };
    }

    private static string? GetSheetForRange(Validation validationEnum)
    {
        return validationEnum switch
        {
            Validation.RANGE_ADDRESS => SheetsConfig.SheetNames.Addresses,
            Validation.RANGE_NAME => SheetsConfig.SheetNames.Names,
            Validation.RANGE_PLACE => SheetsConfig.SheetNames.Places,
            Validation.RANGE_REGION => SheetsConfig.SheetNames.Regions,
            Validation.RANGE_SERVICE => SheetsConfig.SheetNames.Services,
            Validation.RANGE_TYPE => SheetsConfig.SheetNames.Types,
            _ => null
        };
    }

    public static SheetEntity? MapData(Spreadsheet spreadsheet)
    {
        return s_registry.MapData(spreadsheet);
    }

    public static SheetEntity? MapData(BatchGetValuesByDataFilterResponse response)
    {
        return s_registry.MapData(response);
    }
}
