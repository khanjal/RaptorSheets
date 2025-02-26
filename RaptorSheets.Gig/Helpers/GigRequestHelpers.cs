using Google.Apis.Sheets.v4.Data;
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

        var requests = GoogleRequestHelpers.GenerateDeleteRequests(sheetId, rowIds);

        return requests;
    }

    // TRIP

    public static List<Request> ChangeTripSheetData(List<TripEntity> trips, PropertyEntity? sheetProperties)
    {
        var requests = new List<Request>();

        // Add requests
        var addTrips = trips?.Where(x => x.Action == ActionTypeEnum.APPEND.GetDescription()).ToList() ?? [];
        requests.AddRange(CreateUpdateCellTripRequests(addTrips, sheetProperties));

        // Update requests
        var updateTrips = trips?.Where(x => x.Action == ActionTypeEnum.UPDATE.GetDescription()).ToList() ?? [];
        requests.AddRange(CreateUpdateCellTripRequests(updateTrips, sheetProperties));

        // Delete requests
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

        foreach (var trip in trips)
        {
            var rowData = TripMapper.MapToRowData([trip], headers!);
            var request = new Request();

            if (trip.RowId > maxRow)
            {
                requests.Add(GoogleRequestHelpers.GenerateAppendCells(sheetId, rowData));
                maxRow += trips.Count;
            }
            else
            {
                requests.Add(GoogleRequestHelpers.GenerateUpdateCellsRequest(sheetId, trip.RowId, rowData));
            }
        }

        return requests;
    }

    // SHIFT
    public static List<Request> ChangeShiftSheetData(List<ShiftEntity> shifts, PropertyEntity? sheetProperties)
    {
        var requests = new List<Request>();

        // Add requests
        var addShifts = shifts?.Where(x => x.Action == ActionTypeEnum.APPEND.GetDescription()).ToList() ?? [];
        requests.AddRange(CreateUpdateCellShiftRequests(addShifts, sheetProperties));

        // Update requests
        var updateShifts = shifts?.Where(x => x.Action == ActionTypeEnum.UPDATE.GetDescription()).ToList() ?? [];
        requests.AddRange(CreateUpdateCellShiftRequests(updateShifts, sheetProperties));

        // Delete requests
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

        foreach (var shift in shifts)
        {
            var rowData = ShiftMapper.MapToRowData([shift], headers!);

            if (shift.RowId > maxRow)
            {
                requests.Add(GoogleRequestHelpers.GenerateAppendDimension(sheetId, shifts.Count));
                maxRow += shifts.Count;
            }

            requests.Add(GoogleRequestHelpers.GenerateUpdateCellsRequest(sheetId, shift.RowId, rowData));
        }

        return requests;
    }
}
