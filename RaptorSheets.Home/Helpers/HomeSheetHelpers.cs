using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Core.Registries;
using RaptorSheets.Home.Constants;
using RaptorSheets.Home.Entities;
using RaptorSheets.Home.Enums;
using RaptorSheets.Home.Sheets;

namespace RaptorSheets.Home.Helpers;

/// <summary>
/// Helper methods for Google Sheets operations in the Home domain.
///
/// Per-sheet dispatch (headers, row mapping, missing-column detection) is delegated to a shared
/// RaptorSheets.Core.Registries.SheetRegistry&lt;SheetEntity&gt;. Sheets that need no formulas map
/// directly through GenericSheetMapper&lt;T&gt;; Rooms and Appliances use their own sheet definitions
/// for their one calculated column each.
/// </summary>
public static class HomeSheetHelpers
{
    public static List<string> GetSheetNames()
    {
        return SheetsConfig.SheetUtilities.GetAllSheetNames();
    }

    /// <summary>
    /// The shared registry backing this domain's header/row-mapping/missing-column orchestration.
    /// </summary>
    public static SheetRegistry<SheetEntity> Registry => s_registry;

    private static readonly SheetRegistry<SheetEntity> s_registry = BuildRegistry();

    private static SheetRegistry<SheetEntity> BuildRegistry()
    {
        var registry = new SheetRegistry<SheetEntity>();

        registry.RegisterGeneric<SheetEntity, ApplianceEntity>(SheetsConfig.SheetNames.Appliances, ApplianceSheet.GetSheet, (se, rows) => se.Sheets.Appliances = rows);
        registry.RegisterGeneric<SheetEntity, ProjectEntity>(SheetsConfig.SheetNames.Projects, ProjectSheet.GetSheet, (se, rows) => se.Sheets.Projects = rows);
        registry.RegisterGeneric<SheetEntity, MaintenanceEntity>(SheetsConfig.SheetNames.Maintenance, MaintenanceSheet.GetSheet, (se, rows) => se.Sheets.Maintenance = rows);
        registry.RegisterGeneric<SheetEntity, DoorWindowEntity>(SheetsConfig.SheetNames.DoorsWindows, DoorWindowSheet.GetSheet, (se, rows) => se.Sheets.DoorsWindows = rows);
        registry.RegisterGeneric<SheetEntity, PaintEntity>(SheetsConfig.SheetNames.Paints, PaintSheet.GetSheet, (se, rows) => se.Sheets.Paints = rows);
        registry.RegisterGeneric<SheetEntity, PowerEntity>(SheetsConfig.SheetNames.Power, PowerSheet.GetSheet, (se, rows) => se.Sheets.Power = rows);
        registry.RegisterGeneric<SheetEntity, RoomEntity>(SheetsConfig.SheetNames.Rooms, RoomSheet.GetSheet, (se, rows) => se.Sheets.Rooms = rows);
        registry.RegisterGeneric<SheetEntity, ContactEntity>(SheetsConfig.SheetNames.Contacts, ContactSheet.GetSheet, (se, rows) => se.Sheets.Contacts = rows);
        registry.RegisterGeneric<SheetEntity, StatEntity>(SheetsConfig.SheetNames.Stats, StatSheet.GetSheet, (se, rows) => se.Sheets.Stats = rows);

        return registry;
    }

    public static List<SheetModel> GetMissingSheets(Spreadsheet spreadsheet)
    {
        return s_registry.GetMissingSheets(spreadsheet, GetSheetNames());
    }

    public static List<MessageEntity> CheckUnknownSheets(Spreadsheet spreadsheet)
    {
        return s_registry.CheckUnknownSheets(spreadsheet);
    }

    public static List<MessageEntity> CheckSheetHeaders(Spreadsheet spreadsheet)
    {
        return s_registry.CheckSheetHeaders(spreadsheet);
    }

    public static List<MessageEntity> CheckSheetHeaders(Spreadsheet spreadsheet, out Dictionary<string, List<ColumnInsertionInfo>> missingColumns)
    {
        return s_registry.CheckSheetHeaders(spreadsheet, out missingColumns);
    }

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
            Validation.RANGE_ROOM or Validation.RANGE_CONTACT
                => GoogleValidationHelper.CreateOneOfRangeRule($"{GetSheetForRange(validation)}!A2:A"),
            Validation.RANGE_SELF => GoogleValidationHelper.CreateOneOfRangeRule($"{range}"),
            _ => new DataValidationRule()
        };
    }

    private static string? GetSheetForRange(Validation validationEnum)
    {
        return validationEnum switch
        {
            Validation.RANGE_ROOM => SheetsConfig.SheetNames.Rooms,
            Validation.RANGE_CONTACT => SheetsConfig.SheetNames.Contacts,
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
