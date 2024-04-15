﻿using FluentAssertions;
using GigRaptorLib.Entities;
using GigRaptorLib.Mappers;
using GigRaptorLib.Tests.Data.Helpers;

namespace GigRaptorLib.Tests.Mappers.MapFromRangeData;

public class WeekdayMapFromRangeDataTests
{
    private static IList<IList<object>>? _values;
    private static List<WeekdayEntity>? _entities;

    public WeekdayMapFromRangeDataTests()
    {
        _values = JsonHelpers.LoadJsonSheetData("Weekday");
        _entities = WeekdayMapper.MapFromRangeData(_values!);
    }

    [Fact]
    public void GivenWeekdaySheetData_ThenReturnRangeData()
    {
        var nonEmptyValues = _values!.Where(x => !string.IsNullOrEmpty(x[0].ToString())).ToList();
        _entities.Should().HaveCount(nonEmptyValues.Count - 1);

        foreach (var entity in _entities!)
        {
            entity.Id.Should().NotBe(0);
            entity.Day.Should().BeGreaterThanOrEqualTo(0);
            entity.Weekday.Should().NotBeNull();
            entity.Trips.Should().BeGreaterThanOrEqualTo(0);
            entity.Days.Should().BeGreaterThanOrEqualTo(0);
            entity.Pay.Should().NotBeNull();
            entity.Tip.Should().NotBeNull();
            entity.Bonus.Should().NotBeNull();
            entity.Total.Should().NotBeNull();
            entity.Cash.Should().NotBeNull();
            entity.Distance.Should().BeGreaterThanOrEqualTo(0);
            entity.Time.Should().NotBeNull();
            entity.CurrentAmount.Should().BeGreaterThanOrEqualTo(0);
            entity.PreviousAmount.Should().BeGreaterThanOrEqualTo(0);
            entity.DailyAverage.Should().BeGreaterThanOrEqualTo(0);
            entity.PreviousDailyAverage.Should().BeGreaterThanOrEqualTo(0);
        }
    }

    [Fact]
    public void GivenWeekdaySheetDataColumnOrderRandomized_ThenReturnSameRangeData()
    {
        var sheetOrder = new int[] { 0 }.Concat([.. RandomHelpers.GetRandomOrder(1, _values![0].Count - 1)]).ToArray();
        var randomValues = RandomHelpers.RandomizeValues(_values, sheetOrder);

        var randomEntities = WeekdayMapper.MapFromRangeData(randomValues);
        var nonEmptyRandomValues = randomValues!.Where(x => !string.IsNullOrEmpty(x[0].ToString())).ToList();
        randomEntities.Should().HaveCount(nonEmptyRandomValues.Count - 1);

        for (int i = 0; i < randomEntities.Count; i++)
        {
            var entity = _entities![i];
            var randomEntity = randomEntities[i];

            entity.Id.Should().Be(randomEntity.Id);
            entity.Day.Should().Be(randomEntity.Day);
            entity.Weekday.Should().BeEquivalentTo(randomEntity.Weekday);
            entity.Trips.Should().Be(randomEntity.Trips);
            entity.Days.Should().Be(randomEntity.Days);
            entity.Pay.Should().Be(randomEntity.Pay);
            entity.Tip.Should().Be(randomEntity.Tip);
            entity.Bonus.Should().Be(randomEntity.Bonus);
            entity.Total.Should().Be(randomEntity.Total);
            entity.Cash.Should().Be(randomEntity.Cash);
            entity.Distance.Should().Be(randomEntity.Distance);
            entity.Time.Should().BeEquivalentTo(randomEntity.Time);
            entity.CurrentAmount.Should().Be(randomEntity.CurrentAmount);
            entity.PreviousAmount.Should().Be(randomEntity.PreviousAmount);
            entity.DailyAverage.Should().Be(randomEntity.DailyAverage);
            entity.PreviousDailyAverage.Should().Be(randomEntity.PreviousDailyAverage);
        }
    }
}