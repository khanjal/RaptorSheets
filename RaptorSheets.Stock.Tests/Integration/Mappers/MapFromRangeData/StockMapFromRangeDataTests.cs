using RaptorSheets.Core.Extensions;
using RaptorSheets.Stock.Entities;
using RaptorSheets.Stock.Enums;
using RaptorSheets.Stock.Mappers;
using RaptorSheets.Stock.Tests.Data;
using RaptorSheets.Stock.Tests.Data.Attributes;
using RaptorSheets.Test.Common.Helpers;
using Xunit;

namespace RaptorSheets.Stock.Tests.Integration.Mappers.MapFromRangeData;

[Collection("Google Data collection")]
public class StockMapFromRangeDataTests
{
    readonly GoogleDataFixture fixture;
    private static IList<IList<object>>? _values;
    private static List<StockEntity>? _entities;

    public StockMapFromRangeDataTests(GoogleDataFixture fixture)
    {
        this.fixture = fixture;
        _values = this.fixture.valueRanges?.Where(x => x.DataFilters[0].A1Range == SheetEnum.STOCKS.GetDescription()).First().ValueRange.Values;
        _entities = StockMapper.MapFromRangeData(_values!);
    }

    [FactCheckUserSecrets]
    public void GivenAccountSheetData_ThenReturnRangeData()
    {
        var nonEmptyValues = _values!.Where(x => !string.IsNullOrEmpty(x[0].ToString())).ToList();
        Assert.Equal(nonEmptyValues.Count - 1, _entities?.Count);

        foreach (var entity in _entities!)
        {
            Assert.NotEqual(0, entity.RowId);
            Assert.False(string.IsNullOrEmpty(entity.Ticker));
            Assert.False(string.IsNullOrEmpty(entity.Name));
            Assert.False(string.IsNullOrEmpty(entity.Account));
            Assert.True(entity.Shares >= 0);
            Assert.True(entity.AverageCost >= 0);
            Assert.True(entity.CostTotal >= 0);
            Assert.True(entity.CurrentPrice >= 0);
            Assert.True(entity.CurrentTotal >= 0);
            Assert.True(entity.WeekHigh52 >= 0);
            Assert.True(entity.WeekLow52 >= 0);
            Assert.True(entity.MaxHigh >= 0);
            Assert.True(entity.MinLow >= 0);
        }
    }

    [FactCheckUserSecrets]
    public void GivenAccountSheetDataColumnOrderRandomized_ThenReturnSameRangeData()
    {
        var sheetOrder = new int[] { 0 }.Concat(RandomHelpers.GetRandomOrder(1, _values![0].Count - 1)).ToArray();
        var randomValues = RandomHelpers.RandomizeValues(_values, sheetOrder);

        var randomEntities = StockMapper.MapFromRangeData(randomValues);
        var nonEmptyRandomValues = randomValues!.Where(x => !string.IsNullOrEmpty(x[0].ToString())).ToList();
        Assert.Equal(nonEmptyRandomValues.Count - 1, randomEntities.Count);

        for (int i = 0; i < randomEntities.Count; i++)
        {
            var entity = _entities![i];
            var randomEntity = randomEntities[i];

            Assert.Equal(entity.RowId, randomEntity.RowId);
            Assert.Equal(entity.Ticker, randomEntity.Ticker);
            Assert.Equal(entity.Name, randomEntity.Name);
            Assert.Equal(entity.Account, randomEntity.Account);
            Assert.Equal(entity.Shares, randomEntity.Shares);
            Assert.Equal(entity.AverageCost, randomEntity.AverageCost);
            Assert.Equal(entity.CostTotal, randomEntity.CostTotal);
            Assert.Equal(entity.CurrentPrice, randomEntity.CurrentPrice);
            Assert.Equal(entity.CurrentTotal, randomEntity.CurrentTotal);
            Assert.Equal(entity.Return, randomEntity.Return);
            Assert.Equal(entity.PeRatio, randomEntity.PeRatio);
            Assert.Equal(entity.WeekHigh52, randomEntity.WeekHigh52);
            Assert.Equal(entity.WeekLow52, randomEntity.WeekLow52);
            Assert.Equal(entity.MaxHigh, randomEntity.MaxHigh);
            Assert.Equal(entity.MinLow, randomEntity.MinLow);
        }
    }
}