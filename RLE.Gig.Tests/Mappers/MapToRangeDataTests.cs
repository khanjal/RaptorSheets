using FluentAssertions;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Tests.Data.Helpers;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Mappers;
using RaptorSheets.Gig.Tests.Data.Helpers;

namespace RaptorSheets.Gig.Tests.Mappers;

public class MapToRangeDataTests
{
    private static SheetEntity? _sheetData;

    public MapToRangeDataTests()
    {
        _sheetData = GigJsonHelpers.LoadSheetJson();
    }

    [Fact]
    public void GivenSheetData_ThenReturnShiftData()
    {
        var shiftHeaders = JsonHelpers.LoadJsonSheetData("Shift")![0];
        var shifts = ShiftMapper.MapToRangeData(_sheetData!.Shifts, shiftHeaders);
        var headers = HeaderHelpers.ParserHeader(shiftHeaders);

        Assert.NotNull(shifts);
        shifts.Count.Should().Be(3);

        for (int i = 0; i < shifts.Count; i++)
        {
            var shift = shifts[i];
            if (shift == null) continue;
            var shiftData = _sheetData.Shifts[i];

#pragma warning disable CS8602 // Rethrow to preserve stack details
            shiftData.Date.Should().BeEquivalentTo(shift[shiftHeaders.IndexOf(HeaderEnum.DATE.GetDescription())].ToString());
            shiftData.Start.Should().BeEquivalentTo(shift[shiftHeaders.IndexOf(HeaderEnum.TIME_START.GetDescription())].ToString());
            shiftData.Finish.Should().BeEquivalentTo(shift[shiftHeaders.IndexOf(HeaderEnum.TIME_END.GetDescription())].ToString());
            shiftData.Service.Should().BeEquivalentTo(shift[shiftHeaders.IndexOf(HeaderEnum.SERVICE.GetDescription())].ToString());
            shiftData.Active.Should().BeEquivalentTo(shift[shiftHeaders.IndexOf(HeaderEnum.TIME_ACTIVE.GetDescription())].ToString());
            shiftData.Time.Should().BeEquivalentTo(shift[shiftHeaders.IndexOf(HeaderEnum.TIME_TOTAL.GetDescription())].ToString());
            shiftData.Region.Should().BeEquivalentTo(shift[shiftHeaders.IndexOf(HeaderEnum.REGION.GetDescription())].ToString());
            shiftData.Note.Should().BeEquivalentTo(shift[shiftHeaders.IndexOf(HeaderEnum.NOTE.GetDescription())].ToString());
#pragma warning restore CS8602 // Rethrow to preserve stack details

            if (shiftData.Number == null)
                HeaderHelpers.GetIntValue(HeaderEnum.NUMBER.GetDescription(), shift!, headers).Should().Be(0);
            else
                shiftData.Number.Should().Be(HeaderHelpers.GetIntValue(HeaderEnum.NUMBER.GetDescription(), shift!, headers));

            if (shiftData.Omit == null)
                HeaderHelpers.GetBoolValue(HeaderEnum.TIME_OMIT.GetDescription(), shift!, headers).Should().Be(false);
            else
                shiftData.Omit.Should().Be(HeaderHelpers.GetBoolValue(HeaderEnum.TIME_OMIT.GetDescription(), shift!, headers));

            // TODO: Future support of shift only would use this.
            //shiftData.Pay.Should().Be(HeaderParser.GetDecimalValue(HeaderEnum.PAY.DisplayName(), shift, headers));
            //shiftData.Tip.Should().Be(HeaderParser.GetDecimalValue(HeaderEnum.TIPS.DisplayName(), shift, headers));
            //shiftData.Bonus.Should().Be(HeaderParser.GetDecimalValue(HeaderEnum.BONUS.DisplayName(), shift, headers));
            //shiftData.Total.Should().Be(HeaderParser.GetDecimalValue(HeaderEnum.TOTAL.DisplayName(), shift, headers));
            //shiftData.Cash.Should().Be(HeaderParser.GetDecimalValue(HeaderEnum.CASH.DisplayName(), shift, headers));
        }
    }

    [Fact]
    public void GivenSheetData_ThenReturnTripData()
    {
        var tripHeaders = JsonHelpers.LoadJsonSheetData("Trip")![0];
        var trips = TripMapper.MapToRangeData(_sheetData!.Trips, tripHeaders);
        var headers = HeaderHelpers.ParserHeader(tripHeaders);

        Assert.NotNull(trips);
        trips.Count.Should().Be(4);

        for (int i = 0; i < trips.Count; i++)
        {
            var trip = trips[i];
            if (trip == null) continue;
            var tripData = _sheetData.Trips[i];

#pragma warning disable CS8602 // Rethrow to preserve stack details
            tripData.Date.Should().BeEquivalentTo(trip[tripHeaders.IndexOf(HeaderEnum.DATE.GetDescription())].ToString());
            tripData.Service.Should().BeEquivalentTo(trip[tripHeaders.IndexOf(HeaderEnum.SERVICE.GetDescription())].ToString());
            tripData.Place.Should().BeEquivalentTo(trip[tripHeaders.IndexOf(HeaderEnum.PLACE.GetDescription())].ToString());
            tripData.Pickup.Should().BeEquivalentTo(trip[tripHeaders.IndexOf(HeaderEnum.PICKUP.GetDescription())].ToString());
            tripData.Dropoff.Should().BeEquivalentTo(trip[tripHeaders.IndexOf(HeaderEnum.DROPOFF.GetDescription())].ToString());
            tripData.Duration.Should().BeEquivalentTo(trip[tripHeaders.IndexOf(HeaderEnum.DURATION.GetDescription())].ToString());
            tripData.Name.Should().BeEquivalentTo(trip[tripHeaders.IndexOf(HeaderEnum.NAME.GetDescription())].ToString());
            tripData.StartAddress.Should().BeEquivalentTo(trip[tripHeaders.IndexOf(HeaderEnum.ADDRESS_START.GetDescription())].ToString());
            tripData.EndAddress.Should().BeEquivalentTo(trip[tripHeaders.IndexOf(HeaderEnum.ADDRESS_END.GetDescription())].ToString());
            tripData.EndUnit.Should().BeEquivalentTo(trip[tripHeaders.IndexOf(HeaderEnum.UNIT_END.GetDescription())].ToString());
            tripData.OrderNumber.Should().BeEquivalentTo(trip[tripHeaders.IndexOf(HeaderEnum.ORDER_NUMBER.GetDescription())].ToString());
            tripData.Note.Should().BeEquivalentTo(trip[tripHeaders.IndexOf(HeaderEnum.NOTE.GetDescription())].ToString());
#pragma warning restore CS8602 // Rethrow to preserve stack details

            // Number
            if (tripData.Number == null)
                HeaderHelpers.GetIntValue(HeaderEnum.NUMBER.GetDescription(), trip!, headers).Should().Be(0);
            else
                tripData.Number.Should().Be(HeaderHelpers.GetIntValue(HeaderEnum.NUMBER.GetDescription(), trip!, headers));

            // Odometer Start
            if (tripData.OdometerStart == null)
                HeaderHelpers.GetDecimalValue(HeaderEnum.ODOMETER_START.GetDescription(), trip!, headers).Should().Be(0);
            else
                tripData.OdometerStart.Should().Be(HeaderHelpers.GetDecimalValue(HeaderEnum.ODOMETER_START.GetDescription(), trip!, headers));

            // Odoemeter End
            if (tripData.OdometerEnd == null)
                HeaderHelpers.GetDecimalValue(HeaderEnum.ODOMETER_END.GetDescription(), trip!, headers).Should().Be(0);
            else
                tripData.OdometerEnd.Should().Be(HeaderHelpers.GetDecimalValue(HeaderEnum.ODOMETER_START.GetDescription(), trip!, headers));

            // Distance
            if (tripData.Distance == null)
                HeaderHelpers.GetDecimalValue(HeaderEnum.DISTANCE.GetDescription(), trip!, headers).Should().Be(0);
            else
                tripData.Distance.Should().Be(HeaderHelpers.GetDecimalValue(HeaderEnum.DISTANCE.GetDescription(), trip!, headers));

            // Pay
            if (tripData.Pay == null)
                HeaderHelpers.GetDecimalValue(HeaderEnum.PAY.GetDescription(), trip!, headers).Should().Be(0);
            else
                tripData.Pay.Should().Be(HeaderHelpers.GetDecimalValue(HeaderEnum.PAY.GetDescription(), trip!, headers));

            // Tip
            if (tripData.Tip == null)
                HeaderHelpers.GetDecimalValue(HeaderEnum.TIPS.GetDescription(), trip!, headers).Should().Be(0);
            else
                tripData.Tip.Should().Be(HeaderHelpers.GetDecimalValue(HeaderEnum.TIPS.GetDescription(), trip!, headers));

            // Bonus
            if (tripData.Bonus == null)
                HeaderHelpers.GetDecimalValue(HeaderEnum.BONUS.GetDescription(), trip!, headers).Should().Be(0);
            else
                tripData.Bonus.Should().Be(HeaderHelpers.GetDecimalValue(HeaderEnum.BONUS.GetDescription(), trip!, headers));

            // Cash
            if (tripData.Cash == null)
                HeaderHelpers.GetDecimalValue(HeaderEnum.CASH.GetDescription(), trip!, headers).Should().Be(0);
            else
                tripData.Cash.Should().Be(HeaderHelpers.GetDecimalValue(HeaderEnum.CASH.GetDescription(), trip!, headers));
        }
    }
}
