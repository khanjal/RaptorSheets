using FluentAssertions;
using RLE.Core.Utilities.Extensions;
using RLE.Gig.Entities;
using RLE.Gig.Enums;
using RLE.Gig.Mappers;
using RLE.Gig.Tests.Data;
using RLE.Gig.Tests.Data.Helpers;

namespace RLE.Gig.Tests.Mappers.MapFromRangeData;

[Collection("Google Data collection")]
public class PlaceMapFromRangeDataTests
{
    readonly GoogleDataFixture fixture;
    private static IList<IList<object>>? _values;
    private static List<PlaceEntity>? _entities;

    public PlaceMapFromRangeDataTests(GoogleDataFixture fixture)
    {
        this.fixture = fixture;
        _values = this.fixture.valueRanges?.Where(x => x.DataFilters[0].A1Range == GigSheetEnum.PLACES.GetDescription()).First().ValueRange.Values;
        _entities = PlaceMapper.MapFromRangeData(_values!);
    }

    [Fact]
    public void GivenPlaceSheetData_ThenReturnRangeData()
    {
        var nonEmptyValues = _values!.Where(x => !string.IsNullOrEmpty(x[0].ToString())).ToList();
        _entities.Should().HaveCount(nonEmptyValues.Count - 1);

        foreach (var entity in _entities!)
        {
            entity.Id.Should().NotBe(0);
            entity.Place.Should().NotBeNullOrEmpty();
            entity.Trips.Should().BeGreaterThan(0);
            entity.Pay.Should().NotBeNull();
            entity.Tip.Should().NotBeNull();
            entity.Bonus.Should().NotBeNull();
            entity.Total.Should().NotBeNull();
            entity.Cash.Should().NotBeNull();
            entity.Distance.Should().BeGreaterThanOrEqualTo(0);
        }
    }

    [Fact]
    public void GivenPlaceSheetDataColumnOrderRandomized_ThenReturnSameRangeData()
    {
        var sheetOrder = new int[] { 0 }.Concat([.. RandomHelpers.GetRandomOrder(1, _values![0].Count - 1)]).ToArray();
        var randomValues = RandomHelpers.RandomizeValues(_values, sheetOrder);

        var randomEntities = PlaceMapper.MapFromRangeData(randomValues);
        var nonEmptyRandomValues = randomValues!.Where(x => !string.IsNullOrEmpty(x[0].ToString())).ToList();
        randomEntities.Should().HaveCount(nonEmptyRandomValues.Count - 1);

        for (int i = 0; i < randomEntities.Count; i++)
        {
            var entity = _entities![i];
            var randomEntity = randomEntities[i];

            entity.Id.Should().Be(randomEntity.Id);
            entity.Place.Should().BeEquivalentTo(randomEntity.Place);
            entity.Trips.Should().Be(randomEntity.Trips);
            entity.Pay.Should().Be(randomEntity.Pay);
            entity.Tip.Should().Be(randomEntity.Tip);
            entity.Bonus.Should().Be(randomEntity.Bonus);
            entity.Total.Should().Be(randomEntity.Total);
            entity.Cash.Should().Be(randomEntity.Cash);
            entity.Distance.Should().Be(randomEntity.Distance);
        }
    }
}
