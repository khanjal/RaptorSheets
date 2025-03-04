using RaptorSheets.Core.Extensions;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Mappers;
using RaptorSheets.Gig.Tests.Data;
using RaptorSheets.Gig.Tests.Data.Attributes;
using RaptorSheets.Test.Common.Helpers;

namespace RaptorSheets.Gig.Tests.Integration.Mappers.MapFromRangeData;

[Collection("Google Data collection")]
public class ShiftMapFromRangeDataTests
{
    readonly GoogleDataFixture fixture;
    private static IList<IList<object>>? _values;
    private static List<ShiftEntity>? _entities;

    public ShiftMapFromRangeDataTests(GoogleDataFixture fixture)
    {
        this.fixture = fixture;
        _values = this.fixture.ValueRanges?.Where(x => x.DataFilters[0].A1Range == SheetEnum.SHIFTS.GetDescription()).First().ValueRange.Values;
        _entities = ShiftMapper.MapFromRangeData(_values!);
    }

    [FactCheckUserSecrets]
    public void GivenShiftSheetData_ThenReturnRangeData()
    {
        var nonEmptyValues = _values!.Where(x => !string.IsNullOrEmpty(x[0].ToString())).ToList();
        Assert.Equal(nonEmptyValues.Count - 1, _entities?.Count);

        foreach (var entity in _entities!)
        {
            Assert.NotEqual(0, entity.RowId);
            Assert.False(string.IsNullOrEmpty(entity.Date));
            Assert.NotNull(entity.Start);
            Assert.NotNull(entity.Finish);
            Assert.False(string.IsNullOrEmpty(entity.Service));
            Assert.True(entity.Number >= 0);
            Assert.NotNull(entity.Active);
            Assert.NotNull(entity.Time);
            Assert.True(entity.Trips >= 0);
            // Assert.NotNull(entity.Pay);
            // Assert.NotNull(entity.Tip);
            // Assert.NotNull(entity.Bonus);
            // Assert.NotNull(entity.Cash);
            // Assert.NotNull(entity.Distance);
            Assert.NotNull(entity.Region);
            Assert.NotNull(entity.Note);
            Assert.NotNull(entity.Key);
            Assert.True(entity.TotalTrips >= 0);
            // Assert.True(entity.TotalDistance >= 0);
            // Assert.True(entity.TotalPay >= 0);
            // Assert.True(entity.TotalTips >= 0);
            // Assert.True(entity.TotalBonus >= 0);
            // Assert.True(entity.GrandTotal >= 0);
            // Assert.True(entity.TotalCash >= 0);
            // Assert.True(entity.AmountPerTime >= 0);
            // Assert.True(entity.AmountPerDistance >= 0);
            // Assert.True(entity.AmountPerTrip >= 0);
        }
    }

    [FactCheckUserSecrets]
    public void GivenShiftSheetDataColumnOrderRandomized_ThenReturnSameRangeData()
    {
        var sheetOrder = new int[] { 0 }.Concat(RandomHelpers.GetRandomOrder(1, _values![0].Count - 1)).ToArray();
        var randomValues = RandomHelpers.RandomizeValues(_values, sheetOrder);

        var randomEntities = ShiftMapper.MapFromRangeData(randomValues);
        var nonEmptyRandomValues = randomValues!.Where(x => !string.IsNullOrEmpty(x[0].ToString())).ToList();
        Assert.Equal(nonEmptyRandomValues.Count - 1, randomEntities.Count);

        for (int i = 0; i < randomEntities.Count; i++)
        {
            var entity = _entities![i];
            var randomEntity = randomEntities[i];

            Assert.Equal(entity.RowId, randomEntity.RowId);
            Assert.Equal(entity.Date, randomEntity.Date);
            Assert.Equal(entity.Start, randomEntity.Start);
            Assert.Equal(entity.Finish, randomEntity.Finish);
            Assert.Equal(entity.Service, randomEntity.Service);
            Assert.Equal(entity.Number, randomEntity.Number);
            Assert.Equal(entity.Active, randomEntity.Active);
            Assert.Equal(entity.Time, randomEntity.Time);
            Assert.Equal(entity.Omit, randomEntity.Omit);
            Assert.Equal(entity.Trips, randomEntity.Trips);
            Assert.Equal(entity.Pay, randomEntity.Pay);
            Assert.Equal(entity.Tip, randomEntity.Tip);
            Assert.Equal(entity.Bonus, randomEntity.Bonus);
            Assert.Equal(entity.Cash, randomEntity.Cash);
            Assert.Equal(entity.Distance, randomEntity.Distance);
            Assert.Equal(entity.Region, randomEntity.Region);
            Assert.Equal(entity.Note, randomEntity.Note);
            Assert.Equal(entity.Key, randomEntity.Key);
            Assert.Equal(entity.TotalTrips, randomEntity.TotalTrips);
            Assert.Equal(entity.TotalDistance, randomEntity.TotalDistance);
            Assert.Equal(entity.TotalPay, randomEntity.TotalPay);
            Assert.Equal(entity.TotalTips, randomEntity.TotalTips);
            Assert.Equal(entity.TotalBonus, randomEntity.TotalBonus);
            Assert.Equal(entity.GrandTotal, randomEntity.GrandTotal);
            Assert.Equal(entity.TotalCash, randomEntity.TotalCash);
            Assert.Equal(entity.AmountPerTime, randomEntity.AmountPerTime);
            Assert.Equal(entity.AmountPerDistance, randomEntity.AmountPerDistance);
            Assert.Equal(entity.AmountPerTrip, randomEntity.AmountPerTrip);
        }
    }
}