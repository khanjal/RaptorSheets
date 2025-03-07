using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Tests.Data.Helpers;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Mappers;
using RaptorSheets.Gig.Tests.Data.Helpers;
using System.ComponentModel;

namespace RaptorSheets.Gig.Tests.Unit.Mappers;

[Category("Unit Tests")]
public class MapToRangeDataTests
{
    private static SheetEntity? _sheetData;

    public MapToRangeDataTests()
    {
        _sheetData = TestGigHelpers.LoadSheetJson();
    }

    [Fact]
    public void GivenSheetData_ThenReturnShiftData()
    {
        var shiftHeaders = JsonHelpers.LoadJsonSheetData("Shift")![0];
        var shifts = ShiftMapper.MapToRangeData(_sheetData!.Shifts, shiftHeaders);
        var headers = HeaderHelpers.ParserHeader(shiftHeaders);

        Assert.NotNull(shifts);
        Assert.Equal(3, shifts.Count);

        for (int i = 0; i < shifts.Count; i++)
        {
            var shift = shifts[i];
            if (shift == null) continue;
            var shiftData = _sheetData.Shifts[i];

#pragma warning disable CS8602 // Rethrow to preserve stack details
            Assert.Equal(shift[shiftHeaders.IndexOf(HeaderEnum.DATE.GetDescription())].ToString(), shiftData.Date);
            Assert.Equal(shift[shiftHeaders.IndexOf(HeaderEnum.TIME_START.GetDescription())].ToString(), shiftData.Start);
            Assert.Equal(shift[shiftHeaders.IndexOf(HeaderEnum.TIME_END.GetDescription())].ToString(), shiftData.Finish);
            Assert.Equal(shift[shiftHeaders.IndexOf(HeaderEnum.SERVICE.GetDescription())].ToString(), shiftData.Service);
            Assert.Equal(shift[shiftHeaders.IndexOf(HeaderEnum.TIME_ACTIVE.GetDescription())].ToString(), shiftData.Active);
            Assert.Equal(shift[shiftHeaders.IndexOf(HeaderEnum.TIME_TOTAL.GetDescription())].ToString(), shiftData.Time);
            Assert.Equal(shift[shiftHeaders.IndexOf(HeaderEnum.REGION.GetDescription())].ToString(), shiftData.Region);
            Assert.Equal(shift[shiftHeaders.IndexOf(HeaderEnum.NOTE.GetDescription())].ToString(), shiftData.Note);
#pragma warning restore CS8602 // Rethrow to preserve stack details

            if (shiftData.Number == null)
                Assert.Equal(0, HeaderHelpers.GetIntValue(HeaderEnum.NUMBER.GetDescription(), shift!, headers));
            else
                Assert.Equal(HeaderHelpers.GetIntValue(HeaderEnum.NUMBER.GetDescription(), shift!, headers), shiftData.Number);

            if (shiftData.Omit == null)
                Assert.False(HeaderHelpers.GetBoolValue(HeaderEnum.TIME_OMIT.GetDescription(), shift!, headers));
            else
                Assert.Equal(HeaderHelpers.GetBoolValue(HeaderEnum.TIME_OMIT.GetDescription(), shift!, headers), shiftData.Omit);

            // TODO: Future support of shift only would use this.
            // Assert.Equal(HeaderParser.GetDecimalValue(HeaderEnum.PAY.DisplayName(), shift, headers), shiftData.Pay);
            // Assert.Equal(HeaderParser.GetDecimalValue(HeaderEnum.TIPS.DisplayName(), shift, headers), shiftData.Tip);
            // Assert.Equal(HeaderParser.GetDecimalValue(HeaderEnum.BONUS.DisplayName(), shift, headers), shiftData.Bonus);
            // Assert.Equal(HeaderParser.GetDecimalValue(HeaderEnum.TOTAL.DisplayName(), shift, headers), shiftData.Total);
            // Assert.Equal(HeaderParser.GetDecimalValue(HeaderEnum.CASH.DisplayName(), shift, headers), shiftData.Cash);
        }
    }

    [Fact]
    public void GivenSheetData_ThenReturnTripData()
    {
        var tripHeaders = JsonHelpers.LoadJsonSheetData("Trip")![0];
        var trips = TripMapper.MapToRangeData(_sheetData!.Trips, tripHeaders);
        var headers = HeaderHelpers.ParserHeader(tripHeaders);

        Assert.NotNull(trips);
        Assert.Equal(4, trips.Count);

        for (int i = 0; i < trips.Count; i++)
        {
            var trip = trips[i];
            if (trip == null) continue;
            var tripData = _sheetData.Trips[i];

#pragma warning disable CS8602 // Rethrow to preserve stack details
            Assert.Equal(trip[tripHeaders.IndexOf(HeaderEnum.DATE.GetDescription())].ToString(), tripData.Date);
            Assert.Equal(trip[tripHeaders.IndexOf(HeaderEnum.SERVICE.GetDescription())].ToString(), tripData.Service);
            Assert.Equal(trip[tripHeaders.IndexOf(HeaderEnum.PLACE.GetDescription())].ToString(), tripData.Place);
            Assert.Equal(trip[tripHeaders.IndexOf(HeaderEnum.PICKUP.GetDescription())].ToString(), tripData.Pickup);
            Assert.Equal(trip[tripHeaders.IndexOf(HeaderEnum.DROPOFF.GetDescription())].ToString(), tripData.Dropoff);
            Assert.Equal(trip[tripHeaders.IndexOf(HeaderEnum.DURATION.GetDescription())].ToString(), tripData.Duration);
            Assert.Equal(trip[tripHeaders.IndexOf(HeaderEnum.NAME.GetDescription())].ToString(), tripData.Name);
            Assert.Equal(trip[tripHeaders.IndexOf(HeaderEnum.ADDRESS_START.GetDescription())].ToString(), tripData.StartAddress);
            Assert.Equal(trip[tripHeaders.IndexOf(HeaderEnum.ADDRESS_END.GetDescription())].ToString(), tripData.EndAddress);
            Assert.Equal(trip[tripHeaders.IndexOf(HeaderEnum.UNIT_END.GetDescription())].ToString(), tripData.EndUnit);
            Assert.Equal(trip[tripHeaders.IndexOf(HeaderEnum.ORDER_NUMBER.GetDescription())].ToString(), tripData.OrderNumber);
            Assert.Equal(trip[tripHeaders.IndexOf(HeaderEnum.NOTE.GetDescription())].ToString(), tripData.Note);
#pragma warning restore CS8602 // Rethrow to preserve stack details

            // Number
            if (tripData.Number == null)
                Assert.Equal(0, HeaderHelpers.GetIntValue(HeaderEnum.NUMBER.GetDescription(), trip!, headers));
            else
                Assert.Equal(HeaderHelpers.GetIntValue(HeaderEnum.NUMBER.GetDescription(), trip!, headers), tripData.Number);

            // Odometer Start
            if (tripData.OdometerStart == null)
                Assert.Equal(0, HeaderHelpers.GetDecimalValue(HeaderEnum.ODOMETER_START.GetDescription(), trip!, headers));
            else
                Assert.Equal(HeaderHelpers.GetDecimalValue(HeaderEnum.ODOMETER_START.GetDescription(), trip!, headers), tripData.OdometerStart);

            // Odometer End
            if (tripData.OdometerEnd == null)
                Assert.Equal(0, HeaderHelpers.GetDecimalValue(HeaderEnum.ODOMETER_END.GetDescription(), trip!, headers));
            else
                Assert.Equal(HeaderHelpers.GetDecimalValue(HeaderEnum.ODOMETER_END.GetDescription(), trip!, headers), tripData.OdometerEnd);

            // Distance
            if (tripData.Distance == null)
                Assert.Equal(0, HeaderHelpers.GetDecimalValue(HeaderEnum.DISTANCE.GetDescription(), trip!, headers));
            else
                Assert.Equal(HeaderHelpers.GetDecimalValue(HeaderEnum.DISTANCE.GetDescription(), trip!, headers), tripData.Distance);

            // Pay
            if (tripData.Pay == null)
                Assert.Equal(0, HeaderHelpers.GetDecimalValue(HeaderEnum.PAY.GetDescription(), trip!, headers));
            else
                Assert.Equal(HeaderHelpers.GetDecimalValue(HeaderEnum.PAY.GetDescription(), trip!, headers), tripData.Pay);

            // Tip
            if (tripData.Tip == null)
                Assert.Equal(0, HeaderHelpers.GetDecimalValue(HeaderEnum.TIPS.GetDescription(), trip!, headers));
            else
                Assert.Equal(HeaderHelpers.GetDecimalValue(HeaderEnum.TIPS.GetDescription(), trip!, headers), tripData.Tip);

            // Bonus
            if (tripData.Bonus == null)
                Assert.Equal(0, HeaderHelpers.GetDecimalValue(HeaderEnum.BONUS.GetDescription(), trip!, headers));
            else
                Assert.Equal(HeaderHelpers.GetDecimalValue(HeaderEnum.BONUS.GetDescription(), trip!, headers), tripData.Bonus);

            // Cash
            if (tripData.Cash == null)
                Assert.Equal(0, HeaderHelpers.GetDecimalValue(HeaderEnum.CASH.GetDescription(), trip!, headers));
            else
                Assert.Equal(HeaderHelpers.GetDecimalValue(HeaderEnum.CASH.GetDescription(), trip!, headers), tripData.Cash);
        }
    }
}