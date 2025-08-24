using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Common.Mappers;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Mappers;

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

    // TRIP
    public static List<Request> ChangeTripSheetData(List<TripEntity> trips, PropertyEntity? sheetProperties)
    {
        var requests = new List<Request>();

        // Append/Update requests FIRST - this ensures row IDs are correct before any deletions
        var saveTrips = trips?.Where(x => x.Action != ActionTypeEnum.DELETE.GetDescription()).ToList() ?? [];
        requests.AddRange(CreateUpdateCellTripRequests(saveTrips, sheetProperties));

        // Delete requests AFTER updates - delete from highest row ID to lowest to prevent shifting issues
        var deleteTrips = trips?.Where(x => x.Action == ActionTypeEnum.DELETE.GetDescription()).ToList() ?? [];
        var rowIds = deleteTrips.Select(x => x.RowId).ToList();
        requests.AddRange(CreateDeleteRequests(rowIds, sheetProperties));

        return requests;
    }

    public static IEnumerable<Request> CreateUpdateCellTripRequests(List<TripEntity> trips, PropertyEntity? sheetProperties)
    {
        var headers = sheetProperties?.Attributes[PropertyEnum.HEADERS.GetDescription()]?.Split(",").Cast<object>().ToList();
        var maxRow = int.Parse(sheetProperties?.Attributes[PropertyEnum.MAX_ROW.GetDescription()] ?? "0");
        int sheetId = int.TryParse(sheetProperties?.Id, out var id) ? id : 0;

        if (trips.Count == 0 || sheetProperties == null || headers?.Count == 0 || sheetId == 0)
        {
            return [];
        }

        var requests = new List<Request>();

        var appendTrips = trips.Where(x => x.RowId > maxRow).ToList();
        if (appendTrips.Count > 0)
        {
            var appendData = TripMapper.MapToRowData(appendTrips, headers!);
            requests.Add(GoogleRequestHelpers.GenerateAppendCells(sheetId, appendData));
        }

        var updateTrips = trips.Where(x => x.RowId <= maxRow).ToList();
        foreach (var trip in updateTrips)
        {
            var rowData = TripMapper.MapToRowData([trip], headers!);
            var request = new Request();

            requests.Add(GoogleRequestHelpers.GenerateUpdateCellsRequest(sheetId, trip.RowId - 1, rowData));
        }

        return requests;
    }

    // SHIFT
    public static List<Request> ChangeShiftSheetData(List<ShiftEntity> shifts, PropertyEntity? sheetProperties)
    {
        var requests = new List<Request>();

        // Append/Update requests FIRST - this ensures row IDs are correct before any deletions
        var saveShifts = shifts?.Where(x => x.Action != ActionTypeEnum.DELETE.GetDescription()).ToList() ?? [];
        requests.AddRange(CreateUpdateCellShiftRequests(saveShifts, sheetProperties));

        // Delete requests AFTER updates - delete from highest row ID to lowest to prevent shifting issues
        var deleteShifts = shifts?.Where(x => x.Action == ActionTypeEnum.DELETE.GetDescription()).ToList() ?? [];
        var rowIds = deleteShifts.Select(x => x.RowId).ToList();
        requests.AddRange(CreateDeleteRequests(rowIds, sheetProperties));

        return requests;
    }

    public static IEnumerable<Request> CreateUpdateCellShiftRequests(List<ShiftEntity> shifts, PropertyEntity? sheetProperties)
    {
        var headers = sheetProperties?.Attributes[PropertyEnum.HEADERS.GetDescription()]?.Split(",").Cast<object>().ToList();
        var maxRow = int.Parse(sheetProperties?.Attributes[PropertyEnum.MAX_ROW.GetDescription()] ?? "0");
        int sheetId = int.TryParse(sheetProperties?.Id, out var id) ? id : 0;

        if (shifts.Count == 0 || sheetProperties == null || headers?.Count == 0 || sheetId == 0)
        {
            return [];
        }

        var requests = new List<Request>();

        var appendShifts = shifts.Where(x => x.RowId > maxRow).ToList();
        if (appendShifts.Count > 0)
        {
            var appendData = ShiftMapper.MapToRowData(appendShifts, headers!);
            requests.Add(GoogleRequestHelpers.GenerateAppendCells(sheetId, appendData));
        }

        var updateShifts = shifts.Where(x => x.RowId <= maxRow).ToList();
        foreach (var shift in updateShifts)
        {
            var rowData = ShiftMapper.MapToRowData([shift], headers!);
            var request = new Request();

            requests.Add(GoogleRequestHelpers.GenerateUpdateCellsRequest(sheetId, shift.RowId - 1, rowData));
        }

        return requests;
    }

    // SETUP
    public static List<Request> ChangeSetupSheetData(List<SetupEntity> setup, PropertyEntity? sheetProperties)
    {
        var requests = new List<Request>();

        // Append/Update requests FIRST - this ensures row IDs are correct before any deletions
        var saveSetup = setup?.Where(x => x.Action != ActionTypeEnum.DELETE.GetDescription()).ToList() ?? [];
        requests.AddRange(CreateUpdateCellSetupRequests(saveSetup, sheetProperties));

        // Delete requests AFTER updates - delete from highest row ID to lowest to prevent shifting issues
        var deleteSetup = setup?.Where(x => x.Action == ActionTypeEnum.DELETE.GetDescription()).ToList() ?? [];
        var rowIds = deleteSetup.Select(x => x.RowId).ToList();
        requests.AddRange(CreateDeleteRequests(rowIds, sheetProperties));

        return requests;
    }

    public static IEnumerable<Request> CreateUpdateCellSetupRequests(List<SetupEntity> setup, PropertyEntity? sheetProperties)
    {
        var headers = sheetProperties?.Attributes[PropertyEnum.HEADERS.GetDescription()]?.Split(",").Cast<object>().ToList();
        var maxRow = int.Parse(sheetProperties?.Attributes[PropertyEnum.MAX_ROW.GetDescription()] ?? "0");
        int sheetId = int.TryParse(sheetProperties?.Id, out var id) ? id : 0;

        if (setup.Count == 0 || sheetProperties == null || headers?.Count == 0 || sheetId == 0)
        {
            return [];
        }

        var requests = new List<Request>();

        var appendSetup = setup.Where(x => x.RowId > maxRow).ToList();
        if (appendSetup.Count > 0)
        {
            var appendData = SetupMapper.MapToRowData(appendSetup, headers!);
            requests.Add(GoogleRequestHelpers.GenerateAppendCells(sheetId, appendData));
        }

        var updateSetup = setup.Where(x => x.RowId <= maxRow).ToList();
        foreach (var item in updateSetup)
        {
            var rowData = SetupMapper.MapToRowData([item], headers!);
            var request = new Request();

            requests.Add(GoogleRequestHelpers.GenerateUpdateCellsRequest(sheetId, item.RowId - 1, rowData));
        }

        return requests;
    }
}
