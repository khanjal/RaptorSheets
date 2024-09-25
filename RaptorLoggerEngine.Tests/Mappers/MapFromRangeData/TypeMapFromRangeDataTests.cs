using FluentAssertions;
using RaptorLoggerEngine.Enums;
using RaptorLoggerEngine.Mappers;
using RaptorLoggerEngine.Tests.Data;
using RaptorLoggerEngine.Tests.Data.Helpers;
using RaptorLoggerEngine.Utilities.Extensions;
using RLE.Core.Entities;

namespace RaptorLoggerEngine.Tests.Mappers.MapFromRangeData;

[Collection("Google Data collection")]
public class TypeMapFromRangeDataTests
{
    readonly GoogleDataFixture fixture;
    private static IList<IList<object>>? _values;
    private static List<TypeEntity>? _entities;

    public TypeMapFromRangeDataTests(GoogleDataFixture fixture)
    {
        this.fixture = fixture;
        _values = this.fixture.valueRanges?.Where(x => x.DataFilters[0].A1Range == SheetEnum.TYPES.DisplayName()).First().ValueRange.Values;
        _entities = TypeMapper.MapFromRangeData(_values!);
    }

    [Fact]
    public void GivenTypeSheetData_ThenReturnRangeData()
    {
        var nonEmptyValues = _values!.Where(x => !string.IsNullOrEmpty(x[0].ToString())).ToList();
        _entities.Should().HaveCount(nonEmptyValues.Count - 1);

        foreach (var entity in _entities!)
        {
            entity.Id.Should().NotBe(0);
            entity.Type.Should().NotBeNullOrEmpty();
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
    public void GivenTypeSheetDataColumnOrderRandomized_ThenReturnSameRangeData()
    {
        var sheetOrder = new int[] { 0 }.Concat([.. RandomHelpers.GetRandomOrder(1, _values![0].Count - 1)]).ToArray();
        var randomValues = RandomHelpers.RandomizeValues(_values, sheetOrder);

        var randomEntities = TypeMapper.MapFromRangeData(randomValues);
        var nonEmptyRandomValues = randomValues!.Where(x => !string.IsNullOrEmpty(x[0].ToString())).ToList();
        randomEntities.Should().HaveCount(nonEmptyRandomValues.Count - 1);

        for (int i = 0; i < randomEntities.Count; i++)
        {
            var entity = _entities![i];
            var randomEntity = randomEntities[i];

            entity.Id.Should().Be(randomEntity.Id);
            entity.Type.Should().BeEquivalentTo(randomEntity.Type);
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

