using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Gig.Entities;

namespace RaptorSheets.Gig.Helpers;

/// <summary>
/// Gig-specific wiring on top of Core's generic entity-change request builders
/// (<see cref="GoogleRequestHelpers.ChangeSheetData{T}"/>/<see cref="GoogleRequestHelpers.CreateUpdateCellRequests{T}"/>) -
/// each pair below just names a concrete entity type and its <see cref="GenericSheetMapper{T}"/>.
/// </summary>
public static class GigRequestHelpers
{
    // TRIP
    public static List<Request> ChangeTripSheetData(List<TripEntity> trips, PropertyEntity? sheetProperties)
    {
        return GoogleRequestHelpers.ChangeSheetData(trips, sheetProperties, (entities, props) => CreateUpdateCellTripRequests(entities, props));
    }

    public static IEnumerable<Request> CreateUpdateCellTripRequests(List<TripEntity> trips, PropertyEntity? sheetProperties)
    {
        return GoogleRequestHelpers.CreateUpdateCellRequests(trips, sheetProperties, GenericSheetMapper<TripEntity>.MapToRowData);
    }

    // SHIFT
    public static List<Request> ChangeShiftSheetData(List<ShiftEntity> shifts, PropertyEntity? sheetProperties)
    {
        return GoogleRequestHelpers.ChangeSheetData(shifts, sheetProperties, (entities, props) => CreateUpdateCellShiftRequests(entities, props));
    }

    public static IEnumerable<Request> CreateUpdateCellShiftRequests(List<ShiftEntity> shifts, PropertyEntity? sheetProperties)
    {
        return GoogleRequestHelpers.CreateUpdateCellRequests(shifts, sheetProperties, GenericSheetMapper<ShiftEntity>.MapToRowData);
    }

    // SETUP
    public static List<Request> ChangeSetupSheetData(List<SetupEntity> setup, PropertyEntity? sheetProperties)
    {
        return GoogleRequestHelpers.ChangeSheetData(setup, sheetProperties, (entities, props) => CreateUpdateCellSetupRequests(entities, props));
    }

    public static IEnumerable<Request> CreateUpdateCellSetupRequests(List<SetupEntity> setup, PropertyEntity? sheetProperties)
    {
        return GoogleRequestHelpers.CreateUpdateCellRequests(setup, sheetProperties, GenericSheetMapper<SetupEntity>.MapToRowData);
    }

    // EXPENSE
    public static List<Request> ChangeExpensesSheetData(List<ExpenseEntity> expenses, PropertyEntity? sheetProperties)
    {
        return GoogleRequestHelpers.ChangeSheetData(expenses, sheetProperties, (entities, props) => CreateUpdateCellExpenseRequests(entities, props));
    }

    public static IEnumerable<Request> CreateUpdateCellExpenseRequests(List<ExpenseEntity> expenses, PropertyEntity? sheetProperties)
    {
        return GoogleRequestHelpers.CreateUpdateCellRequests(expenses, sheetProperties, GenericSheetMapper<ExpenseEntity>.MapToRowData);
    }
}
