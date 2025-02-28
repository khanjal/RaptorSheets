using FluentAssertions;
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
public class TickerMapFromRangeDataTests
{
    readonly GoogleDataFixture fixture;
    private static IList<IList<object>>? _values;
    private static List<TickerEntity>? _entities;

    public TickerMapFromRangeDataTests(GoogleDataFixture fixture)
    {
        this.fixture = fixture;
        _values = this.fixture.valueRanges?.Where(x => x.DataFilters[0].A1Range == SheetEnum.TICKERS.GetDescription()).First().ValueRange.Values;
        _entities = TickerMapper.MapFromRangeData(_values!);
    }

    [FactCheckUserSecrets]
    public void GivenAccountSheetData_ThenReturnRangeData()
    {
        var nonEmptyValues = _values!.Where(x => !string.IsNullOrEmpty(x[0].ToString())).ToList();
        _entities.Should().HaveCount(nonEmptyValues.Count - 1);

        foreach (var entity in _entities!)
        {
            entity.RowId.Should().NotBe(0);
            entity.Ticker.Should().NotBeNullOrEmpty();
            entity.Name.Should().NotBeNullOrEmpty();
            entity.Shares.Should().BeGreaterThanOrEqualTo(0);
            entity.AverageCost.Should().BeGreaterThanOrEqualTo(0);
            entity.CostTotal.Should().BeGreaterThanOrEqualTo(0);
            entity.CurrentPrice.Should().BeGreaterThanOrEqualTo(0);
            entity.CurrentTotal.Should().BeGreaterThanOrEqualTo(0);
            entity.WeekHigh52.Should().BeGreaterThanOrEqualTo(0);
            entity.WeekLow52.Should().BeGreaterThanOrEqualTo(0);
            entity.MaxHigh.Should().BeGreaterThanOrEqualTo(0);
            entity.MinLow.Should().BeGreaterThanOrEqualTo(0);
        }
    }

    [FactCheckUserSecrets]
    public void GivenAccountSheetDataColumnOrderRandomized_ThenReturnSameRangeData()
    {
        var sheetOrder = new int[] { 0 }.Concat([.. RandomHelpers.GetRandomOrder(1, _values![0].Count - 1)]).ToArray();
        var randomValues = RandomHelpers.RandomizeValues(_values, sheetOrder);

        var randomEntities = TickerMapper.MapFromRangeData(randomValues);
        var nonEmptyRandomValues = randomValues!.Where(x => !string.IsNullOrEmpty(x[0].ToString())).ToList();
        randomEntities.Should().HaveCount(nonEmptyRandomValues.Count - 1);

        for (int i = 0; i < randomEntities.Count; i++)
        {
            var entity = _entities![i];
            var randomEntity = randomEntities[i];

            entity.RowId.Should().Be(randomEntity.RowId);
            entity.Ticker.Should().BeEquivalentTo(randomEntity.Ticker);
            entity.Name.Should().BeEquivalentTo(randomEntity.Name);
            entity.Shares.Should().Be(randomEntity.Shares);
            entity.AverageCost.Should().Be(randomEntity.AverageCost);
            entity.CostTotal.Should().Be(randomEntity.CostTotal);
            entity.CurrentPrice.Should().Be(randomEntity.CurrentPrice);
            entity.CurrentTotal.Should().Be(randomEntity.CurrentTotal);
            entity.Return.Should().Be(randomEntity.Return);
            entity.PeRatio.Should().Be(randomEntity.PeRatio);
            entity.WeekHigh52.Should().Be(randomEntity.WeekHigh52);
            entity.WeekLow52.Should().Be(randomEntity.WeekLow52);
            entity.MaxHigh.Should().Be(randomEntity.MaxHigh);
            entity.MinLow.Should().Be(randomEntity.MinLow);
        }
    }
}
