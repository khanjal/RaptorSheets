using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Home.Entities;

namespace RaptorSheets.Home.Helpers;

/// <summary>
/// Home-specific wiring on top of Core's generic entity-change request builders
/// (<see cref="GoogleRequestHelpers.ChangeSheetData{T}"/>/<see cref="GoogleRequestHelpers.CreateUpdateCellRequests{T}"/>).
/// Every Home sheet is user-entered, so each has a change wrapper.
/// </summary>
public static class HomeRequestHelpers
{
    // APPLIANCES
    public static List<Request> ChangeApplianceSheetData(List<ApplianceEntity> appliances, PropertyEntity? sheetProperties)
        => GoogleRequestHelpers.ChangeSheetData(appliances, sheetProperties, (entities, props) =>
            GoogleRequestHelpers.CreateUpdateCellRequests(entities, props, GenericSheetMapper<ApplianceEntity>.MapToRowData));

    // PROJECTS
    public static List<Request> ChangeProjectSheetData(List<ProjectEntity> projects, PropertyEntity? sheetProperties)
        => GoogleRequestHelpers.ChangeSheetData(projects, sheetProperties, (entities, props) =>
            GoogleRequestHelpers.CreateUpdateCellRequests(entities, props, GenericSheetMapper<ProjectEntity>.MapToRowData));

    // MAINTENANCE
    public static List<Request> ChangeMaintenanceSheetData(List<MaintenanceEntity> maintenance, PropertyEntity? sheetProperties)
        => GoogleRequestHelpers.ChangeSheetData(maintenance, sheetProperties, (entities, props) =>
            GoogleRequestHelpers.CreateUpdateCellRequests(entities, props, GenericSheetMapper<MaintenanceEntity>.MapToRowData));

    // DOORS & WINDOWS
    public static List<Request> ChangeDoorWindowSheetData(List<DoorWindowEntity> doorsWindows, PropertyEntity? sheetProperties)
        => GoogleRequestHelpers.ChangeSheetData(doorsWindows, sheetProperties, (entities, props) =>
            GoogleRequestHelpers.CreateUpdateCellRequests(entities, props, GenericSheetMapper<DoorWindowEntity>.MapToRowData));

    // PAINTS
    public static List<Request> ChangePaintSheetData(List<PaintEntity> paints, PropertyEntity? sheetProperties)
        => GoogleRequestHelpers.ChangeSheetData(paints, sheetProperties, (entities, props) =>
            GoogleRequestHelpers.CreateUpdateCellRequests(entities, props, GenericSheetMapper<PaintEntity>.MapToRowData));

    // POWER
    public static List<Request> ChangePowerSheetData(List<PowerEntity> power, PropertyEntity? sheetProperties)
        => GoogleRequestHelpers.ChangeSheetData(power, sheetProperties, (entities, props) =>
            GoogleRequestHelpers.CreateUpdateCellRequests(entities, props, GenericSheetMapper<PowerEntity>.MapToRowData));

    // ROOMS
    public static List<Request> ChangeRoomSheetData(List<RoomEntity> rooms, PropertyEntity? sheetProperties)
        => GoogleRequestHelpers.ChangeSheetData(rooms, sheetProperties, (entities, props) =>
            GoogleRequestHelpers.CreateUpdateCellRequests(entities, props, GenericSheetMapper<RoomEntity>.MapToRowData));

    // CONTACTS
    public static List<Request> ChangeContactSheetData(List<ContactEntity> contacts, PropertyEntity? sheetProperties)
        => GoogleRequestHelpers.ChangeSheetData(contacts, sheetProperties, (entities, props) =>
            GoogleRequestHelpers.CreateUpdateCellRequests(entities, props, GenericSheetMapper<ContactEntity>.MapToRowData));

    // STATS
    public static List<Request> ChangeStatSheetData(List<StatEntity> stats, PropertyEntity? sheetProperties)
        => GoogleRequestHelpers.ChangeSheetData(stats, sheetProperties, (entities, props) =>
            GoogleRequestHelpers.CreateUpdateCellRequests(entities, props, GenericSheetMapper<StatEntity>.MapToRowData));
}
