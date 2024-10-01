using FluentAssertions;
using RLE.Core.Extensions;
using RLE.Gig.Entities;
using RLE.Gig.Enums;
using RLE.Gig.Mappers;
using RLE.Gig.Tests.Data;
using RLE.Gig.Tests.Data.Helpers;

namespace RLE.Gig.Tests.Mappers.MapFromRangeData;

[Collection("Google Data collection")]
public class TripMapFromRangeDataTests
{
    readonly GoogleDataFixture fixture;
    private static IList<IList<object>>? _values;
    private static List<TripEntity>? _entities;

    public TripMapFromRangeDataTests(GoogleDataFixture fixture)
    {
        this.fixture = fixture;
        _values = this.fixture.valueRanges?.Where(x => x.DataFilters[0].A1Range == SheetEnum.TRIPS.GetDescription()).First().ValueRange.Values;
        _entities = TripMapper.MapFromRangeData(_values!);
    }

    [Fact]
    public void GivenTripSheetData_ThenReturnRangeData()
    {
        var nonEmptyValues = _values!.Where(x => !string.IsNullOrEmpty(x[0].ToString())).ToList();
        _entities.Should().HaveCount(nonEmptyValues.Count - 1);

        foreach (var entity in _entities!)
        {
            entity.Id.Should().NotBe(0);
            entity.Date.Should().NotBeNullOrEmpty();
            entity.Service.Should().NotBeNullOrEmpty();
            entity.Number.Should().BeGreaterThanOrEqualTo(0);
            entity.Type.Should().NotBeNull();
            entity.Place.Should().NotBeNull();
            entity.Pickup.Should().NotBeNull();
            entity.Dropoff.Should().NotBeNull();
            entity.Duration.Should().NotBeNull();
            entity.Pay.Should().NotBeNull();
            entity.Tip.Should().NotBeNull();
            entity.Bonus.Should().NotBeNull();
            entity.Total.Should().NotBeNull();
            entity.Cash.Should().NotBeNull();
            entity.OdometerStart.Should().BeGreaterThanOrEqualTo(0);
            entity.OdometerStart.Should().NotBeNull();
            entity.Distance.Should().BeGreaterThanOrEqualTo(0);
            entity.Name.Should().NotBeNull();
            entity.StartAddress.Should().NotBeNull();
            entity.EndAddress.Should().NotBeNull();
            entity.EndUnit.Should().NotBeNull();
            entity.OrderNumber.Should().NotBeNull();
            entity.Region.Should().NotBeNull();
            entity.Note.Should().NotBeNull();
            entity.Key.Should().NotBeNull();
            entity.AmountPerTime.Should().BeGreaterThanOrEqualTo(0);
            entity.AmountPerDistance.Should().BeGreaterThanOrEqualTo(0);
        }
    }

    [Fact]
    public void GivenTripSheetDataColumnOrderRandomized_ThenReturnSameRangeData()
    {
        var sheetOrder = new int[] { 0 }.Concat([.. RandomHelpers.GetRandomOrder(1, _values![0].Count - 1)]).ToArray();
        var randomValues = RandomHelpers.RandomizeValues(_values, sheetOrder);

        var randomEntities = TripMapper.MapFromRangeData(randomValues);
        var nonEmptyRandomValues = randomValues!.Where(x => !string.IsNullOrEmpty(x[0].ToString())).ToList();
        randomEntities.Should().HaveCount(nonEmptyRandomValues.Count - 1);

        for (int i = 0; i < randomEntities.Count; i++)
        {
            var entity = _entities![i];
            var randomEntity = randomEntities[i];

            entity.Id.Should().Be(randomEntity.Id);
            entity.Date.Should().BeEquivalentTo(randomEntity.Date);
            entity.Service.Should().BeEquivalentTo(randomEntity.Service);
            entity.Number.Should().Be(randomEntity.Number);
            entity.Exclude.Should().Be(randomEntity.Exclude);
            entity.Type.Should().BeEquivalentTo(randomEntity.Type);
            entity.Place.Should().BeEquivalentTo(randomEntity.Place);
            entity.Pickup.Should().BeEquivalentTo(randomEntity.Pickup);
            entity.Dropoff.Should().BeEquivalentTo(randomEntity.Dropoff);
            entity.Duration.Should().BeEquivalentTo(randomEntity.Duration);
            entity.Pay.Should().Be(randomEntity.Pay);
            entity.Tip.Should().Be(randomEntity.Tip);
            entity.Bonus.Should().Be(randomEntity.Bonus);
            entity.Total.Should().Be(randomEntity.Total);
            entity.Cash.Should().Be(randomEntity.Cash);
            entity.OdometerStart.Should().Be(randomEntity.OdometerStart);
            entity.OdometerEnd.Should().Be(randomEntity.OdometerEnd);
            entity.Distance.Should().Be(randomEntity.Distance);
            entity.Name.Should().BeEquivalentTo(randomEntity.Name);
            entity.StartAddress.Should().BeEquivalentTo(randomEntity.StartAddress);
            entity.EndAddress.Should().BeEquivalentTo(randomEntity.EndAddress);
            entity.EndUnit.Should().BeEquivalentTo(randomEntity.EndUnit);
            entity.OrderNumber.Should().BeEquivalentTo(randomEntity.OrderNumber);
            entity.Region.Should().BeEquivalentTo(randomEntity.Region);
            entity.Note.Should().BeEquivalentTo(randomEntity.Note);
            entity.Key.Should().BeEquivalentTo(randomEntity.Key);
            entity.AmountPerTime.Should().Be(randomEntity.AmountPerTime);
            entity.AmountPerDistance.Should().Be(randomEntity.AmountPerDistance);
        }
    }
}
