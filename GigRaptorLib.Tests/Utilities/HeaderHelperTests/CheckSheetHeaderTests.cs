using FluentAssertions;
using GigRaptorLib.Enums;
using GigRaptorLib.Mappers;
using GigRaptorLib.Models;
using GigRaptorLib.Tests.Data;
using GigRaptorLib.Tests.Data.Helpers;
using GigRaptorLib.Utilities;
using GigRaptorLib.Utilities.Extensions;
using Google.Apis.Sheets.v4.Data;

namespace GigRaptorLib.Tests.Utilities.HeaderHelperTests;

[Collection("Google Data collection")]
public class CheckSheetHeaderTests
{
    public static IEnumerable<object[]> Sheets =>
        [
            [AddressMapper.GetSheet(), SheetEnum.ADDRESSES],
            [DailyMapper.GetSheet(), SheetEnum.DAILY],
            [MonthlyMapper.GetSheet(), SheetEnum.MONTHLY],
            [NameMapper.GetSheet(), SheetEnum.NAMES],
            [PlaceMapper.GetSheet(), SheetEnum.PLACES],
            [RegionMapper.GetSheet(), SheetEnum.REGIONS],
            [ServiceMapper.GetSheet(), SheetEnum.SERVICES],
            [ShiftMapper.GetSheet(), SheetEnum.SHIFTS],
            [TripMapper.GetSheet(), SheetEnum.TRIPS],
            [TypeMapper.GetSheet(), SheetEnum.TYPES],
            [WeekdayMapper.GetSheet(), SheetEnum.WEEKDAYS],
            [WeeklyMapper.GetSheet(), SheetEnum.WEEKLY],
            [YearlyMapper.GetSheet(), SheetEnum.YEARLY],
        ];

    readonly GoogleDataFixture fixture;
    private static IList<MatchedValueRange>? _matchedValueRanges;

    public CheckSheetHeaderTests(GoogleDataFixture fixture)
    {
        this.fixture = fixture;
        _matchedValueRanges = this.fixture.valueRanges;
    }

    [Theory]
    [MemberData(nameof(Sheets))]
    public void GivenFullHeaders_ThenReturnNoMessages(SheetModel sheet, SheetEnum sheetEnum)
    {
        var values = _matchedValueRanges?.Where(x => x.DataFilters[0].A1Range == sheetEnum.DisplayName()).First().ValueRange.Values.ToList();
        var messages = HeaderHelper.CheckSheetHeaders(values!, sheet);

        messages.Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(Sheets))]
    public void GivenMissingHeaders_ThenReturnErrorMessages(SheetModel sheet, SheetEnum sheetEnum)
    {
        var values = _matchedValueRanges?.Where(x => x.DataFilters[0].A1Range == sheetEnum.DisplayName()).First().ValueRange.Values;

        var headerValues = new List<IList<object>>
        {
            values![0].ToList().GetRange(0, values[0].Count - 3)
        };

        var errorMessages = HeaderHelper.CheckSheetHeaders(headerValues!, sheet).Where(x => x.Type == MessageEnum.Error.UpperName());

        errorMessages.Should().NotBeNullOrEmpty();

    }

    [Theory]
    [MemberData(nameof(Sheets))]
    public void GivenMisorderedHeaders_ThenReturnWarningMessages(SheetModel sheet, SheetEnum sheetEnum)
    {
        var values = _matchedValueRanges?.Where(x => x.DataFilters[0].A1Range == sheetEnum.DisplayName()).First().ValueRange.Values;

        var headerValues = new List<IList<object>>
        {
            values![0].ToList().GetRange(0, values[0].Count - 1)
        };

        var headerOrder = new int[] { 0 }.Concat([.. RandomHelpers.GetRandomOrder(1, headerValues![0].Count - 1)]).ToArray();
        var randomValues = RandomHelpers.RandomizeValues(headerValues, headerOrder);

        var warningMessages = HeaderHelper.CheckSheetHeaders(randomValues!, sheet).Where(x => x.Type == MessageEnum.Warning.UpperName());

        warningMessages.Should().NotBeNullOrEmpty();
    }
}
