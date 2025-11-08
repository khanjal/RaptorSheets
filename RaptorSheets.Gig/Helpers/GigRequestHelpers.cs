using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Common.Mappers;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Gig.Entities;

namespace RaptorSheets.Gig.Helpers;

public static class GigRequestHelpers
{
    // COMMON
    public static IEnumerable<Request> CreateDeleteRequests(List<int> rowIds, PropertyEntity? sheetProperties)
    {
        int sheetId = int.TryParse(sheetProperties?.Id, out var id) ? id : 0;

        if (rowIds.Count == 0 || sheetProperties == null || sheetId == 0)
        {
            return [];
        }

        // Use the efficient range-based approach for better performance
        var indexRanges = GoogleRequestHelpers.GenerateIndexRanges(rowIds);
        var requests = GoogleRequestHelpers.GenerateDeleteRequests(sheetId, indexRanges);

        return requests;
    }

    // GENERIC METHODS
    
    /// <summary>
    /// Generic method to handle sheet data changes for any entity type
    /// </summary>
    public static List<Request> ChangeSheetData<T>(List<T> entities, PropertyEntity? sheetProperties, Func<List<T>, PropertyEntity?, IEnumerable<Request>> createUpdateRequests) 
        where T : class
    {
        var requests = new List<Request>();

        // Append/Update requests FIRST - this ensures row IDs are correct before any deletions
        var saveEntities = entities?.Where(x => GetEntityAction(x) != ActionTypeEnum.DELETE.GetDescription()).ToList() ?? [];
        requests.AddRange(createUpdateRequests(saveEntities, sheetProperties));

        // Delete requests AFTER updates - delete from highest row ID to lowest to prevent shifting issues
        var deleteEntities = entities?.Where(x => GetEntityAction(x) == ActionTypeEnum.DELETE.GetDescription()).ToList() ?? [];
        var rowIds = deleteEntities.Select(x => GetEntityRowId(x)).ToList();
        requests.AddRange(CreateDeleteRequests(rowIds, sheetProperties));

        return requests;
    }

    /// <summary>
    /// Generic method to create update cell requests for any entity type
    /// </summary>
    public static IEnumerable<Request> CreateUpdateCellRequests<T>(List<T> entities, PropertyEntity? sheetProperties, Func<List<T>, IList<object>, IList<RowData>> mapToRowData) 
        where T : class
    {
        var headers = sheetProperties?.Attributes[PropertyEnum.HEADERS.GetDescription()]?.Split(",").Cast<object>().ToList();
        var maxRow = int.Parse(sheetProperties?.Attributes[PropertyEnum.MAX_ROW.GetDescription()] ?? "0");
        int sheetId = int.TryParse(sheetProperties?.Id, out var id) ? id : 0;

        if (entities.Count == 0 || sheetProperties == null || headers?.Count == 0 || sheetId == 0)
        {
            return [];
        }

        var requests = new List<Request>();

        var appendEntities = entities.Where(x => GetEntityRowId(x) > maxRow).ToList();
        if (appendEntities.Count > 0)
        {
            var appendData = mapToRowData(appendEntities, headers!);
            requests.Add(GoogleRequestHelpers.GenerateAppendCells(sheetId, appendData));
        }

        var updateEntities = entities.Where(x => GetEntityRowId(x) <= maxRow).ToList();
        foreach (var entity in updateEntities)
        {
            var rowData = mapToRowData([entity], headers!);
            requests.Add(GoogleRequestHelpers.GenerateUpdateCellsRequest(sheetId, GetEntityRowId(entity) - 1, rowData));
        }

        return requests;
    }

    /// <summary>
    /// Helper method to get Action property from any entity using reflection
    /// </summary>
    public static string GetEntityAction<T>(T entity) where T : class
    {
        var actionProperty = typeof(T).GetProperty("Action");
        return actionProperty?.GetValue(entity)?.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Helper method to get RowId property from any entity using reflection
    /// </summary>
    public static int GetEntityRowId<T>(T entity) where T : class
    {
        var rowIdProperty = typeof(T).GetProperty("RowId");
        return (int)(rowIdProperty?.GetValue(entity) ?? 0);
    }

    // SPECIFIC IMPLEMENTATIONS USING GENERIC METHODS

    // TRIP
    public static List<Request> ChangeTripSheetData(List<TripEntity> trips, PropertyEntity? sheetProperties)
    {
        return ChangeSheetData(trips, sheetProperties, (entities, props) => CreateUpdateCellTripRequests(entities, props));
    }

    public static IEnumerable<Request> CreateUpdateCellTripRequests(List<TripEntity> trips, PropertyEntity? sheetProperties)
    {
        return CreateUpdateCellRequests(trips, sheetProperties, GenericSheetMapper<TripEntity>.MapToRowData);
    }

    // SHIFT
    public static List<Request> ChangeShiftSheetData(List<ShiftEntity> shifts, PropertyEntity? sheetProperties)
    {
        return ChangeSheetData(shifts, sheetProperties, (entities, props) => CreateUpdateCellShiftRequests(entities, props));
    }

    public static IEnumerable<Request> CreateUpdateCellShiftRequests(List<ShiftEntity> shifts, PropertyEntity? sheetProperties)
    {
        return CreateUpdateCellRequests(shifts, sheetProperties, GenericSheetMapper<ShiftEntity>.MapToRowData);
    }

    // SETUP
    public static List<Request> ChangeSetupSheetData(List<SetupEntity> setup, PropertyEntity? sheetProperties)
    {
        return ChangeSheetData(setup, sheetProperties, (entities, props) => CreateUpdateCellSetupRequests(entities, props));
    }

    public static IEnumerable<Request> CreateUpdateCellSetupRequests(List<SetupEntity> setup, PropertyEntity? sheetProperties)
    {
        return CreateUpdateCellRequests(setup, sheetProperties, SetupMapper.MapToRowData);
    }

    // EXPENSE
    public static List<Request> ChangeExpensesSheetData(List<ExpenseEntity> expenses, PropertyEntity? sheetProperties)
    {
        return ChangeSheetData(expenses, sheetProperties, (entities, props) => CreateUpdateCellExpenseRequests(entities, props));
    }

    public static IEnumerable<Request> CreateUpdateCellExpenseRequests(List<ExpenseEntity> expenses, PropertyEntity? sheetProperties)
    {
        return CreateUpdateCellRequests(expenses, sheetProperties, GenericSheetMapper<ExpenseEntity>.MapToRowData);
    }
}
