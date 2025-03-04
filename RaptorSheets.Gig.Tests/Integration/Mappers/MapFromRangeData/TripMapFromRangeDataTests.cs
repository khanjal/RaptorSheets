using RaptorSheets.Core.Extensions;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Mappers;
using RaptorSheets.Gig.Tests.Data;
using RaptorSheets.Gig.Tests.Data.Attributes;
using RaptorSheets.Test.Common.Helpers;

namespace RaptorSheets.Gig.Tests.Integration.Mappers.MapFromRangeData;

[Collection("Google Data collection")]
public class TripMapFromRangeDataTests
{
    readonly GoogleDataFixture fixture;
    private static IList<IList<object>>? _values;
    private static List<TripEntity>? _entities;

    public TripMapFromRangeDataTests(GoogleDataFixture fixture)
    {
        this.fixture = fixture;
        _values = this.fixture.ValueRanges?.Where(x => x.DataFilters[0].A1Range == SheetEnum.TRIPS.GetDescription()).First().ValueRange.Values;
        _entities = TripMapper.MapFromRangeData(_values!);
    }

    [FactCheckUserSecrets]
    public void GivenTripSheetData_ThenReturnRangeData()
    {
        var nonEmptyValues = _values!.Where(x => !string.IsNullOrEmpty(x[0].ToString())).ToList();
        Assert.Equal(nonEmptyValues.Count - 1, _entities?.Count);

        foreach (var entity in _entities!)
        {
            Assert.NotEqual(0, entity.RowId);
            Assert.False(string.IsNullOrEmpty(entity.Date));
            Assert.False(string.IsNullOrEmpty(entity.Service));
            Assert.True(entity.Number >= 0);
            Assert.NotNull(entity.Type);
            Assert.NotNull(entity.Place);
            Assert.NotNull(entity.Pickup);
            Assert.NotNull(entity.Dropoff);
            Assert.NotNull(entity.Duration);
            // Assert.NotNull(entity.Pay);
            // Assert.NotNull(entity.Tip);
            // Assert.NotNull(entity.Bonus);
            // Assert.NotNull(entity.Total);
            // Assert.NotNull(entity.Cash);
            // Assert.True(entity.OdometerStart >= 0);
            // Assert.NotNull(entity.OdometerStart);
            // Assert.True(entity.Distance >= 0);
            Assert.NotNull(entity.Name);
            Assert.NotNull(entity.StartAddress);
            Assert.NotNull(entity.EndAddress);
            Assert.NotNull(entity.EndUnit);
            Assert.NotNull(entity.OrderNumber);
            Assert.NotNull(entity.Region);
            Assert.NotNull(entity.Note);
            Assert.NotNull(entity.Key);
            // Assert.True(entity.AmountPerTime >= 0);
            // Assert.True(entity.AmountPerDistance >= 0);
        }
    }

    [FactCheckUserSecrets]
    public void GivenTripSheetDataColumnOrderRandomized_ThenReturnSameRangeData()
    {
        var sheetOrder = new int[] { 0 }.Concat(RandomHelpers.GetRandomOrder(1, _values![0].Count - 1)).ToArray();
        var randomValues = RandomHelpers.RandomizeValues(_values, sheetOrder);

        var randomEntities = TripMapper.MapFromRangeData(randomValues);
        var nonEmptyRandomValues = randomValues!.Where(x => !string.IsNullOrEmpty(x[0].ToString())).ToList();
        Assert.Equal(nonEmptyRandomValues.Count - 1, randomEntities.Count);

        for (int i = 0; i < randomEntities.Count; i++)
        {
            var entity = _entities![i];
            var randomEntity = randomEntities[i];

            Assert.Equal(entity.RowId, randomEntity.RowId);
            Assert.Equal(entity.Date, randomEntity.Date);
            Assert.Equal(entity.Service, randomEntity.Service);
            Assert.Equal(entity.Number, randomEntity.Number);
            Assert.Equal(entity.Exclude, randomEntity.Exclude);
            Assert.Equal(entity.Type, randomEntity.Type);
            Assert.Equal(entity.Place, randomEntity.Place);
            Assert.Equal(entity.Pickup, randomEntity.Pickup);
            Assert.Equal(entity.Dropoff, randomEntity.Dropoff);
            Assert.Equal(entity.Duration, randomEntity.Duration);
            Assert.Equal(entity.Pay, randomEntity.Pay);
            Assert.Equal(entity.Tip, randomEntity.Tip);
            Assert.Equal(entity.Bonus, randomEntity.Bonus);
            Assert.Equal(entity.Total, randomEntity.Total);
            Assert.Equal(entity.Cash, randomEntity.Cash);
            Assert.Equal(entity.OdometerStart, randomEntity.OdometerStart);
            Assert.Equal(entity.OdometerEnd, randomEntity.OdometerEnd);
            Assert.Equal(entity.Distance, randomEntity.Distance);
            Assert.Equal(entity.Name, randomEntity.Name);
            Assert.Equal(entity.StartAddress, randomEntity.StartAddress);
            Assert.Equal(entity.EndAddress, randomEntity.EndAddress);
            Assert.Equal(entity.EndUnit, randomEntity.EndUnit);
            Assert.Equal(entity.OrderNumber, randomEntity.OrderNumber);
            Assert.Equal(entity.Region, randomEntity.Region);
            Assert.Equal(entity.Note, randomEntity.Note);
            Assert.Equal(entity.Key, randomEntity.Key);
            Assert.Equal(entity.AmountPerTime, randomEntity.AmountPerTime);
            Assert.Equal(entity.AmountPerDistance, randomEntity.AmountPerDistance);
        }
    }
}