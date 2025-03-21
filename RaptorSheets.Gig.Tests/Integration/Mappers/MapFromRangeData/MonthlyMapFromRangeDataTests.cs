using RaptorSheets.Core.Extensions;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Mappers;
using RaptorSheets.Gig.Tests.Data;
using RaptorSheets.Gig.Tests.Data.Attributes;
using RaptorSheets.Test.Common.Helpers;

namespace RaptorSheets.Gig.Tests.Integration.Mappers.MapFromRangeData;

[Collection("Google Data collection")]
public class MonthlyMapFromRangeDataTests
{
    readonly GoogleDataFixture fixture;
    private static IList<IList<object>>? _values;
    private static List<MonthlyEntity>? _entities;

    public MonthlyMapFromRangeDataTests(GoogleDataFixture fixture)
    {
        this.fixture = fixture;
        _values = this.fixture.ValueRanges?.Where(x => x.DataFilters[0].A1Range == SheetEnum.MONTHLY.GetDescription()).First().ValueRange.Values;
        _entities = MonthlyMapper.MapFromRangeData(_values!);
    }

    [FactCheckUserSecrets]
    public void GivenWeekdaySheetData_ThenReturnRangeData()
    {
        var nonEmptyValues = _values!.Where(x => !string.IsNullOrEmpty(x[0]?.ToString())).ToList();
        Assert.Equal(nonEmptyValues.Count - 1, _entities?.Count);

        foreach (var entity in _entities!)
        {
            Assert.NotEqual(0, entity.RowId);
            Assert.False(string.IsNullOrEmpty(entity.Month));
            Assert.True(entity.Trips >= 0);
            Assert.True(entity.Days >= 0);
            Assert.NotNull(entity.Pay);
            Assert.NotNull(entity.Tip);
            Assert.NotNull(entity.Bonus);
            Assert.NotNull(entity.Total);
            Assert.NotNull(entity.Cash);
            Assert.True(entity.Distance >= 0);
            Assert.NotNull(entity.Time);
            Assert.True(entity.AmountPerTrip >= 0);
            Assert.True(entity.AmountPerDistance >= 0);
            Assert.True(entity.AmountPerTime >= 0);
            Assert.True(entity.AmountPerDay >= 0);
            Assert.True(entity.Average >= 0);
            Assert.True(entity.Number >= 0);
            Assert.True(entity.Year >= 0);
        }
    }

    [FactCheckUserSecrets]
    public void GivenWeekdaySheetDataColumnOrderRandomized_ThenReturnSameRangeData()
    {
        var sheetOrder = new int[] { 0 }.Concat(RandomHelpers.GetRandomOrder(1, _values![0].Count - 1)).ToArray();
        var randomValues = RandomHelpers.RandomizeValues(_values, sheetOrder);

        var randomEntities = MonthlyMapper.MapFromRangeData(randomValues);
        var nonEmptyRandomValues = randomValues!.Where(x => !string.IsNullOrEmpty(x[0]?.ToString())).ToList();
        Assert.Equal(nonEmptyRandomValues.Count - 1, randomEntities.Count);

        for (int i = 0; i < randomEntities.Count; i++)
        {
            var entity = _entities![i];
            var randomEntity = randomEntities[i];

            Assert.Equal(entity.RowId, randomEntity.RowId);
            Assert.Equal(entity.Month, randomEntity.Month);
            Assert.Equal(entity.Trips, randomEntity.Trips);
            Assert.Equal(entity.Days, randomEntity.Days);
            Assert.Equal(entity.Pay, randomEntity.Pay);
            Assert.Equal(entity.Tip, randomEntity.Tip);
            Assert.Equal(entity.Bonus, randomEntity.Bonus);
            Assert.Equal(entity.Total, randomEntity.Total);
            Assert.Equal(entity.Cash, randomEntity.Cash);
            Assert.Equal(entity.Distance, randomEntity.Distance);
            Assert.Equal(entity.Time, randomEntity.Time);
            Assert.Equal(entity.AmountPerTrip, randomEntity.AmountPerTrip);
            Assert.Equal(entity.AmountPerDistance, randomEntity.AmountPerDistance);
            Assert.Equal(entity.AmountPerTime, randomEntity.AmountPerTime);
            Assert.Equal(entity.AmountPerDay, randomEntity.AmountPerDay);
            Assert.Equal(entity.Average, randomEntity.Average);
            Assert.Equal(entity.Number, randomEntity.Number);
            Assert.Equal(entity.Year, randomEntity.Year);
        }
    }
}