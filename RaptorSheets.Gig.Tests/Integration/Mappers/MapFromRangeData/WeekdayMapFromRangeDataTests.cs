﻿using FluentAssertions;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Mappers;
using RaptorSheets.Gig.Tests.Data;
using RaptorSheets.Gig.Tests.Data.Attributes;
using RaptorSheets.Test.Common.Helpers;

namespace RaptorSheets.Gig.Tests.Unit.Mappers.MapFromRangeData;

[Collection("Google Data collection")]
public class WeekdayMapFromRangeDataTests
{
    readonly GoogleDataFixture fixture;
    private static IList<IList<object>>? _values;
    private static List<WeekdayEntity>? _entities;

    public WeekdayMapFromRangeDataTests(GoogleDataFixture fixture)
    {
        this.fixture = fixture;
        _values = this.fixture.ValueRanges?.Where(x => x.DataFilters[0].A1Range == SheetEnum.WEEKDAYS.GetDescription()).First().ValueRange.Values;
        _entities = WeekdayMapper.MapFromRangeData(_values!);
    }

    [FactCheckUserSecrets]
    public void GivenWeekdaySheetData_ThenReturnRangeData()
    {
        var nonEmptyValues = _values!.Where(x => !string.IsNullOrEmpty(x[0].ToString())).ToList();
        _entities.Should().HaveCount(nonEmptyValues.Count - 1);

        foreach (var entity in _entities!)
        {
            entity.RowId.Should().NotBe(0);
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

    [FactCheckUserSecrets]
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

            entity.RowId.Should().Be(randomEntity.RowId);
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
