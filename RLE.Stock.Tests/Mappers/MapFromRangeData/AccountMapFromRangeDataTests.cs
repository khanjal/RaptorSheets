using FluentAssertions;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Stock.Entities;
using RaptorSheets.Stock.Enums;
using RaptorSheets.Stock.Mappers;
using RaptorSheets.Stock.Tests.Data;
using RaptorSheets.Test.Helpers;
using Xunit;

namespace RaptorSheets.Stock.Tests.Mappers.MapFromRangeData;

[Collection("Google Data collection")]
public class AddressMapFromRangeDataTests
{
    readonly GoogleDataFixture fixture;
    private static IList<IList<object>>? _values;
    private static List<AccountEntity>? _entities;

    public AddressMapFromRangeDataTests(GoogleDataFixture fixture)
    {
        this.fixture = fixture;
        _values = this.fixture.valueRanges?.Where(x => x.DataFilters[0].A1Range == SheetEnum.ACCOUNTS.GetDescription()).First().ValueRange.Values;
        _entities = AccountMapper.MapFromRangeData(_values!);
    }

    [Fact]
    public void GivenAccountSheetData_ThenReturnRangeData()
    {
        var nonEmptyValues = _values!.Where(x => !string.IsNullOrEmpty(x[0].ToString())).ToList();
        _entities.Should().HaveCount(nonEmptyValues.Count - 1);

        foreach (var entity in _entities!)
        {
            entity.Id.Should().NotBe(0);
            entity.Account.Should().NotBeNullOrEmpty();
            entity.Stocks.Should().BeGreaterThanOrEqualTo(0);
            entity.Shares.Should().BeGreaterThanOrEqualTo(0);
            entity.AverageCost.Should().BeGreaterThanOrEqualTo(0);
            entity.CostTotal.Should().BeGreaterThanOrEqualTo(0);
            entity.CurrentTotal.Should().BeGreaterThanOrEqualTo(0);
        }
    }

    [Fact]
    public void GivenAccountSheetDataColumnOrderRandomized_ThenReturnSameRangeData()
    {
        var sheetOrder = new int[] { 0 }.Concat([.. RandomHelpers.GetRandomOrder(1, _values![0].Count - 1)]).ToArray();
        var randomValues = RandomHelpers.RandomizeValues(_values, sheetOrder);

        var randomEntities = AccountMapper.MapFromRangeData(randomValues);
        var nonEmptyRandomValues = randomValues!.Where(x => !string.IsNullOrEmpty(x[0].ToString())).ToList();
        randomEntities.Should().HaveCount(nonEmptyRandomValues.Count - 1);

        for (int i = 0; i < randomEntities.Count; i++)
        {
            var entity = _entities![i];
            var randomEntity = randomEntities[i];

            entity.Id.Should().Be(randomEntity.Id);
            entity.Account.Should().BeEquivalentTo(randomEntity.Account);
            entity.Stocks.Should().Be(randomEntity.Stocks);
            entity.Shares.Should().Be(randomEntity.Shares);
            entity.AverageCost.Should().Be(randomEntity.AverageCost);
            entity.CostTotal.Should().Be(randomEntity.CostTotal);
            entity.CurrentTotal.Should().Be(randomEntity.CurrentTotal);
        }
    }
}
