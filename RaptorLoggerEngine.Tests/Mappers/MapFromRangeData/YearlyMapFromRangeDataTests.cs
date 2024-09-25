using FluentAssertions;
using RaptorLoggerEngine.Entities;
using RaptorLoggerEngine.Enums;
using RaptorLoggerEngine.Mappers;
using RaptorLoggerEngine.Tests.Data;
using RaptorLoggerEngine.Tests.Data.Helpers;
using RaptorLoggerEngine.Utilities.Extensions;

namespace RaptorLoggerEngine.Tests.Mappers.MapFromRangeData;

[Collection("Google Data collection")]
public class YearlyMapFromRangeDataTests
{
    readonly GoogleDataFixture fixture;
    private static IList<IList<object>>? _values;
    private static List<YearlyEntity>? _entities;

    public YearlyMapFromRangeDataTests(GoogleDataFixture fixture)
    {
        this.fixture = fixture;
        _values = this.fixture.valueRanges?.Where(x => x.DataFilters[0].A1Range == SheetEnum.YEARLY.DisplayName()).First().ValueRange.Values;
        _entities = YearlyMapper.MapFromRangeData(_values!);
    }

    [Fact]
    public void GivenWeekdaySheetData_ThenReturnRangeData()
    {
        var nonEmptyValues = _values!.Where(x => !string.IsNullOrEmpty(x[0].ToString())).ToList();
        _entities.Should().HaveCount(nonEmptyValues.Count - 1);

        foreach (var entity in _entities!)
        {
            entity.Id.Should().NotBe(0);
            entity.Year.Should().BeGreaterThanOrEqualTo(0);
            entity.Trips.Should().BeGreaterThanOrEqualTo(0);
            entity.Days.Should().BeGreaterThanOrEqualTo(0);
            entity.Pay.Should().NotBeNull();
            entity.Tip.Should().NotBeNull();
            entity.Bonus.Should().NotBeNull();
            entity.Total.Should().NotBeNull();
            entity.Cash.Should().NotBeNull();
            entity.Distance.Should().BeGreaterThanOrEqualTo(0);
            entity.Time.Should().NotBeNull();
            entity.AmountPerTrip.Should().BeGreaterThanOrEqualTo(0);
            entity.AmountPerDistance.Should().BeGreaterThanOrEqualTo(0);
            entity.AmountPerTime.Should().BeGreaterThanOrEqualTo(0);
            entity.AmountPerDay.Should().BeGreaterThanOrEqualTo(0);
            entity.Average.Should().BeGreaterThanOrEqualTo(0);
        }
    }

    [Fact]
    public void GivenWeekdaySheetDataColumnOrderRandomized_ThenReturnSameRangeData()
    {
        var sheetOrder = new int[] { 0 }.Concat([.. RandomHelpers.GetRandomOrder(1, _values![0].Count - 1)]).ToArray();
        var randomValues = RandomHelpers.RandomizeValues(_values, sheetOrder);

        var randomEntities = YearlyMapper.MapFromRangeData(randomValues);
        var nonEmptyRandomValues = randomValues!.Where(x => !string.IsNullOrEmpty(x[0].ToString())).ToList();
        randomEntities.Should().HaveCount(nonEmptyRandomValues.Count - 1);

        for (int i = 0; i < randomEntities.Count; i++)
        {
            var entity = _entities![i];
            var randomEntity = randomEntities[i];

            entity.Id.Should().Be(randomEntity.Id);
            entity.Year.Should().Be(randomEntity.Year);
            entity.Trips.Should().Be(randomEntity.Trips);
            entity.Days.Should().Be(randomEntity.Days);
            entity.Pay.Should().Be(randomEntity.Pay);
            entity.Tip.Should().Be(randomEntity.Tip);
            entity.Bonus.Should().Be(randomEntity.Bonus);
            entity.Total.Should().Be(randomEntity.Total);
            entity.Cash.Should().Be(randomEntity.Cash);
            entity.Distance.Should().Be(randomEntity.Distance);
            entity.Time.Should().BeEquivalentTo(randomEntity.Time);
            entity.AmountPerTrip.Should().Be(randomEntity.AmountPerTrip);
            entity.AmountPerDistance.Should().Be(randomEntity.AmountPerDistance);
            entity.AmountPerTime.Should().Be(randomEntity.AmountPerTime);
            entity.AmountPerDay.Should().Be(randomEntity.AmountPerDay);
            entity.Average.Should().Be(randomEntity.Average);
        }
    }
}
