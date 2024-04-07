using FluentAssertions;
using GigRaptorLib.Mappers;
using GigRaptorLib.Tests.Data.Helpers;

namespace GigRaptorLib.Tests.Mappers.MapFromRangeData;

public class AddressMapFromRangeDataTests
{
    [Fact]
    public void GivenAddressSheetData_ThenReturnRangeData()
    {
        var values = JsonHelpers.LoadJson("Address");
        values.Should().NotBeNull();

        var entities = AddressMapper.MapFromRangeData(values!);
        var nonEmptyValues = values!.Where(x => !string.IsNullOrEmpty(x[0].ToString())).ToList(); // First column should not be empty
        entities.Should().HaveCount(nonEmptyValues.Count - 1);

        foreach (var entity in entities)
        {
            entity.Id.Should().NotBe(0);
            entity.Address.Should().NotBeNullOrEmpty();
            entity.Visits.Should().BeGreaterThan(0);
            entity.Pay.Should().NotBeNull();
            entity.Tip.Should().NotBeNull();
            entity.Bonus.Should().NotBeNull();
            entity.Total.Should().NotBeNull();
            entity.Cash.Should().NotBeNull();
            entity.Distance.Should().BeGreaterThanOrEqualTo(0);
        }
    }

    [Fact]
    public void GivenAddressSheetDataColumnOrderRandomized_ThenReturnSameRangeData()
    {
        var values = JsonHelpers.LoadJson("Address");
        var entities = AddressMapper.MapFromRangeData(values!);

        var sheetOrder = new int[] { 0 }.Concat([.. RandomHelpers.GetRandomOrder(1, values![0].Count - 1)]).ToArray();
        var randomValues = RandomHelpers.RandomizeValues(values, sheetOrder);

        var randomEntities = AddressMapper.MapFromRangeData(randomValues);
        var nonEmptyRandomValues = randomValues!.Where(x => !string.IsNullOrEmpty(x[0].ToString())).ToList();
        randomEntities.Should().HaveCount(nonEmptyRandomValues.Count - 1);

        for (int i = 0; i < randomEntities.Count; i++)
        {
            var entity = entities[i];
            var randomEntity = randomEntities[i];

            entity.Id.Should().Be(randomEntity.Id);
            entity.Address.Should().BeEquivalentTo(randomEntity.Address);
            entity.Visits.Should().Be(randomEntity.Visits);
            entity.Pay.Should().Be(randomEntity.Pay);
            entity.Tip.Should().Be(randomEntity.Tip);
            entity.Bonus.Should().Be(randomEntity.Bonus);
            entity.Total.Should().Be(randomEntity.Total);
            entity.Cash.Should().Be(randomEntity.Cash);
            entity.Distance.Should().Be(randomEntity.Distance);
        }
    }
}
