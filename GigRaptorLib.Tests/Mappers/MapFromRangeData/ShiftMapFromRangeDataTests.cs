using FluentAssertions;
using GigRaptorLib.Entities;
using GigRaptorLib.Enums;
using GigRaptorLib.Mappers;
using GigRaptorLib.Tests.Data;
using GigRaptorLib.Tests.Data.Helpers;
using GigRaptorLib.Utilities.Extensions;

namespace GigRaptorLib.Tests.Mappers.MapFromRangeData;

[Collection("Google Data collection")]
public class ShiftMapFromRangeDataTests
{
    readonly GoogleDataFixture fixture;
    private static IList<IList<object>>? _values;
    private static List<ShiftEntity>? _entities;

    public ShiftMapFromRangeDataTests(GoogleDataFixture fixture)
    {
        this.fixture = fixture;
        _values = this.fixture.valueRanges?.Where(x => x.DataFilters[0].A1Range == SheetEnum.SHIFTS.DisplayName()).First().ValueRange.Values;
        _entities = ShiftMapper.MapFromRangeData(_values!);
    }

    [Fact]
    public void GivenShiftSheetData_ThenReturnRangeData()
    {
        var nonEmptyValues = _values!.Where(x => !string.IsNullOrEmpty(x[0].ToString())).ToList();
        _entities.Should().HaveCount(nonEmptyValues.Count - 1);

        foreach (var entity in _entities!)
        {
            entity.Id.Should().NotBe(0);
            entity.Date.Should().NotBeNullOrEmpty();
            entity.Start.Should().NotBeNull();
            entity.Finish.Should().NotBeNull();
            entity.Service.Should().NotBeNullOrEmpty();
            entity.Number.Should().BeGreaterThanOrEqualTo(0);
            entity.Active.Should().NotBeNull();
            entity.Time.Should().NotBeNull();
            entity.Trips.Should().BeGreaterThanOrEqualTo(0);
            entity.Pay.Should().NotBeNull();
            entity.Tip.Should().NotBeNull();
            entity.Bonus.Should().NotBeNull();
            entity.Cash.Should().NotBeNull();
            entity.Distance.Should().NotBeNull();
            entity.Region.Should().NotBeNull();
            entity.Note.Should().NotBeNull();
            entity.Key.Should().NotBeNull();
            entity.TotalTrips.Should().BeGreaterThanOrEqualTo(0);
            entity.TotalDistance.Should().BeGreaterThanOrEqualTo(0);
            entity.TotalPay.Should().BeGreaterThanOrEqualTo(0);
            entity.TotalTips.Should().BeGreaterThanOrEqualTo(0);
            entity.TotalBonus.Should().BeGreaterThanOrEqualTo(0);
            entity.GrandTotal.Should().BeGreaterThanOrEqualTo(0);
            entity.TotalCash.Should().BeGreaterThanOrEqualTo(0);
            entity.TotalTrips.Should().BeGreaterThanOrEqualTo(0);
            entity.AmountPerTime.Should().BeGreaterThanOrEqualTo(0);
            entity.AmountPerDistance.Should().BeGreaterThanOrEqualTo(0);
            entity.AmountPerTrip.Should().BeGreaterThanOrEqualTo(0);
        }
    }

    [Fact]
    public void GivenShiftSheetDataColumnOrderRandomized_ThenReturnSameRangeData()
    {
        var sheetOrder = new int[] { 0 }.Concat([.. RandomHelpers.GetRandomOrder(1, _values![0].Count - 1)]).ToArray();
        var randomValues = RandomHelpers.RandomizeValues(_values, sheetOrder);

        var randomEntities = ShiftMapper.MapFromRangeData(randomValues);
        var nonEmptyRandomValues = randomValues!.Where(x => !string.IsNullOrEmpty(x[0].ToString())).ToList();
        randomEntities.Should().HaveCount(nonEmptyRandomValues.Count - 1);

        for (int i = 0; i < randomEntities.Count; i++)
        {
            var entity = _entities![i];
            var randomEntity = randomEntities[i];

            entity.Id.Should().Be(randomEntity.Id);
            entity.Date.Should().BeEquivalentTo(randomEntity.Date);
            entity.Start.Should().BeEquivalentTo(randomEntity.Start);
            entity.Finish.Should().BeEquivalentTo(randomEntity.Finish);
            entity.Service.Should().BeEquivalentTo(randomEntity.Service);
            entity.Number.Should().Be(randomEntity.Number);
            entity.Active.Should().BeEquivalentTo(randomEntity.Active);
            entity.Time.Should().BeEquivalentTo(randomEntity.Time);
            entity.Omit.Should().Be(randomEntity.Omit);
            entity.Trips.Should().Be(randomEntity.Trips);
            entity.Pay.Should().Be(randomEntity.Pay);
            entity.Tip.Should().Be(randomEntity.Tip);
            entity.Bonus.Should().Be(randomEntity.Bonus);
            entity.Cash.Should().Be(randomEntity.Cash);
            entity.Distance.Should().Be(randomEntity.Distance);
            entity.Region.Should().BeEquivalentTo(randomEntity.Region);
            entity.Note.Should().BeEquivalentTo(randomEntity.Note);
            entity.Key.Should().BeEquivalentTo(randomEntity.Key);
            entity.TotalTrips.Should().Be(randomEntity.TotalTrips);
            entity.TotalTrips.Should().Be(randomEntity.TotalTrips);
            entity.TotalDistance.Should().Be(randomEntity.TotalDistance);
            entity.TotalPay.Should().Be(randomEntity.TotalPay);
            entity.TotalTips.Should().Be(randomEntity.TotalTips);
            entity.TotalBonus.Should().Be(randomEntity.TotalBonus);
            entity.GrandTotal.Should().Be(randomEntity.GrandTotal);
            entity.TotalCash.Should().Be(randomEntity.TotalCash);
            entity.AmountPerTime.Should().Be(randomEntity.AmountPerTime);
            entity.AmountPerDistance.Should().Be(randomEntity.AmountPerDistance);
            entity.AmountPerTrip.Should().Be(randomEntity.AmountPerTrip);
        }
    }
}
