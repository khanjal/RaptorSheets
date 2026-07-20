using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Entities;
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

    private static readonly SheetRegistry<SheetEntity> s_registry = BuildRegistry();

    private static SheetRegistry<SheetEntity> BuildRegistry()
    {
        var registry = new SheetRegistry<SheetEntity>();

        registry.RegisterGeneric<SheetEntity, AddressEntity>(SheetsConfig.SheetNames.Addresses, AddressMapper.GetSheet, (se, rows) => se.Addresses = rows);
        registry.RegisterGeneric<SheetEntity, DailyEntity>(SheetsConfig.SheetNames.Daily, DailyMapper.GetSheet, (se, rows) => se.Daily = rows);
        registry.RegisterGeneric<SheetEntity, ExpenseEntity>(SheetsConfig.SheetNames.Expenses, () => GenericSheetMapper<ExpenseEntity>.GetSheet(SheetsConfig.ExpenseSheet), (se, rows) => se.Expenses = rows);
        registry.RegisterGeneric<SheetEntity, MonthlyEntity>(SheetsConfig.SheetNames.Monthly, MonthlyMapper.GetSheet, (se, rows) => se.Monthly = rows);
        registry.RegisterGeneric<SheetEntity, NameEntity>(SheetsConfig.SheetNames.Names, NameMapper.GetSheet, (se, rows) => se.Names = rows);
        registry.RegisterGeneric<SheetEntity, PlaceEntity>(SheetsConfig.SheetNames.Places, PlaceMapper.GetSheet, (se, rows) => se.Places = rows);
        registry.RegisterGeneric<SheetEntity, DeliveryEntity>(SheetsConfig.SheetNames.Deliveries, DeliveryMapper.GetSheet, (se, rows) => se.Deliveries = rows);
        registry.RegisterGeneric<SheetEntity, LocationEntity>(SheetsConfig.SheetNames.Locations, LocationMapper.GetSheet, (se, rows) => se.Locations = rows);
        registry.RegisterGeneric<SheetEntity, RegionEntity>(SheetsConfig.SheetNames.Regions, RegionMapper.GetSheet, (se, rows) => se.Regions = rows);
        registry.RegisterGeneric<SheetEntity, SetupEntity>(SheetsConfig.SheetNames.Setup, () => GenericSheetMapper<SetupEntity>.GetSheet(SheetsConfig.SetupSheet), (se, rows) => se.Setup = rows);
        registry.RegisterGeneric<SheetEntity, ServiceEntity>(SheetsConfig.SheetNames.Services, ServiceMapper.GetSheet, (se, rows) => se.Services = rows);
        registry.RegisterGeneric<SheetEntity, ShiftEntity>(SheetsConfig.SheetNames.Shifts, ShiftMapper.GetSheet, (se, rows) => se.Shifts = rows);
        registry.RegisterGeneric<SheetEntity, TripEntity>(SheetsConfig.SheetNames.Trips, TripMapper.GetSheet, (se, rows) => se.Trips = rows);
        registry.RegisterGeneric<SheetEntity, TypeEntity>(SheetsConfig.SheetNames.Types, TypeMapper.GetSheet, (se, rows) => se.Types = rows);
        registry.RegisterGeneric<SheetEntity, WeekdayEntity>(SheetsConfig.SheetNames.Weekdays, WeekdayMapper.GetSheet, (se, rows) => se.Weekdays = rows);
        registry.RegisterGeneric<SheetEntity, WeeklyEntity>(SheetsConfig.SheetNames.Weekly, WeeklyMapper.GetSheet, (se, rows) => se.Weekly = rows);
        registry.RegisterGeneric<SheetEntity, YearlyEntity>(SheetsConfig.SheetNames.Yearly, YearlyMapper.GetSheet, (se, rows) => se.Yearly = rows);

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

    public static SheetModel? GetSheetLayout(string sheetName)
    {
        return s_registry.GetSheetLayout(sheetName);
    }

    public static List<SheetModel> GetSheetLayouts(IEnumerable<string> sheetNames)
    {
        return s_registry.GetSheetLayouts(sheetNames);
    }

    public static DataValidationRule GetDataValidation(ValidationEnum validation, string? range = "")
    {
        var dataValidation = new DataValidationRule();

        switch (validation)
        {
            case ValidationEnum.BOOLEAN:
                dataValidation.Condition = new BooleanCondition { Type = "BOOLEAN" };
                break;
            case ValidationEnum.RANGE_ADDRESS:
            case ValidationEnum.RANGE_NAME:
            case ValidationEnum.RANGE_PLACE:
            case ValidationEnum.RANGE_REGION:
            case ValidationEnum.RANGE_SERVICE:
            case ValidationEnum.RANGE_TYPE:
                var values = new List<ConditionValue> { new() { UserEnteredValue = $"={GetSheetForRange(validation)}!A2:A" } };
                dataValidation.Condition = new BooleanCondition { Type = "ONE_OF_RANGE", Values = values };
                dataValidation.ShowCustomUi = true;
                dataValidation.Strict = false;
                break;
            case ValidationEnum.RANGE_SELF:
                var selfValues = new List<ConditionValue> { new() { UserEnteredValue = $"={range}" } };
                dataValidation.Condition = new BooleanCondition { Type = "ONE_OF_RANGE", Values = selfValues };
                dataValidation.ShowCustomUi = true;
                dataValidation.Strict = false;
                break;
        }

        return dataValidation;
    }

    private static string? GetSheetForRange(ValidationEnum validationEnum)
    {
        return validationEnum switch
        {
            ValidationEnum.RANGE_ADDRESS => SheetsConfig.SheetNames.Addresses,
            ValidationEnum.RANGE_NAME => SheetsConfig.SheetNames.Names,
            ValidationEnum.RANGE_PLACE => SheetsConfig.SheetNames.Places,
            ValidationEnum.RANGE_REGION => SheetsConfig.SheetNames.Regions,
            ValidationEnum.RANGE_SERVICE => SheetsConfig.SheetNames.Services,
            ValidationEnum.RANGE_TYPE => SheetsConfig.SheetNames.Types,
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
