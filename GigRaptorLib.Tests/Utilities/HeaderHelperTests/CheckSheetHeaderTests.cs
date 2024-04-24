using FluentAssertions;
using GigRaptorLib.Enums;
using GigRaptorLib.Mappers;
using GigRaptorLib.Models;
using GigRaptorLib.Tests.Data;
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
    private static Dictionary<int, string>? _headers;

    public CheckSheetHeaderTests(GoogleDataFixture fixture)
    {
        this.fixture = fixture;
        _matchedValueRanges = this.fixture.valueRanges;
    }

    [Theory]
    [MemberData(nameof(Sheets))]
    public void GivenFullHeadersCheck_ThenReturnNoMessages(SheetModel sheet, SheetEnum sheetEnum)
    {
        var values = _matchedValueRanges?.Where(x => x.DataFilters[0].A1Range == sheetEnum.DisplayName()).First().ValueRange.Values;
        var messages = HeaderHelper.CheckSheetHeaders(values!, sheet);

        messages.Should().BeEmpty();
    }
}
