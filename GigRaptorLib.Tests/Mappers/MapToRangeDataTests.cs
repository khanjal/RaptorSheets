using FluentAssertions;
using GigRaptorLib.Entities;
using GigRaptorLib.Mappers;
using GigRaptorLib.Tests.Data.Helpers;

namespace GigRaptorLib.Tests.Mappers;

public class MapToRangeDataTests
{
    private static SheetEntity? _sheetData;

    public MapToRangeDataTests()
    {
        _sheetData = JsonHelpers.LoadSheetJson();
    }

    [Fact]
    public void GivenSheetData_ThenReturnShiftData()
    {
        var shiftHeaders = JsonHelpers.LoadJsonSheetData("Shift")![0];
        var shifts = ShiftMapper.MapToRangeData(_sheetData!.Shifts, shiftHeaders);

        Assert.NotNull(shifts);
        shifts.Count.Should().Be(3);

        foreach (var shift in shifts)
        {

        }
    }

    [Fact]
    public void GivenSheetData_ThenReturnTripData()
    {
        var tripHeaders = JsonHelpers.LoadJsonSheetData("Trip")![0];
        var trips = TripMapper.MapToRangeData(_sheetData!.Trips, tripHeaders);

        Assert.NotNull(trips);
        trips.Count.Should().Be(3);

        foreach (var trip in trips)
        {

        }
    }
}
