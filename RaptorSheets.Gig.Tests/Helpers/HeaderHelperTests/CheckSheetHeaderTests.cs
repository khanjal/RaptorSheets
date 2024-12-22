using FluentAssertions;
using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Mappers;
using RaptorSheets.Gig.Tests.Data;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Test.Helpers;
using RaptorSheets.Gig.Tests.Data.Attributes;

namespace RaptorSheets.Gig.Tests.Helpers.HeaderHelperTests;

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
        _matchedValueRanges = this.fixture.ValueRanges;
    }

    [TheoryCheckUserSecrets]
    [MemberData(nameof(Sheets))]
    public void GivenFullHeaders_ThenReturnNoMessages(SheetModel sheet, SheetEnum sheetEnum)
    {
        var values = _matchedValueRanges?.Where(x => x.DataFilters[0].A1Range == sheetEnum.GetDescription()).First().ValueRange.Values.ToList();
        var messages = HeaderHelpers.CheckSheetHeaders(values!, sheet);

        messages.Should().BeEmpty();
    }

    [TheoryCheckUserSecrets]
    [MemberData(nameof(Sheets))]
    public void GivenMissingHeaders_ThenReturnErrorMessages(SheetModel sheet, SheetEnum sheetEnum)
    {
        var values = _matchedValueRanges?.Where(x => x.DataFilters[0].A1Range == sheetEnum.GetDescription()).First().ValueRange.Values;

        var headerValues = new List<IList<object>>
        {
            values![0].ToList().GetRange(0, values[0].Count - 3)
        };

        var errorMessages = HeaderHelpers.CheckSheetHeaders(headerValues!, sheet).Where(x => x.Level == MessageLevelEnum.ERROR.UpperName());

        errorMessages.Should().NotBeNullOrEmpty();

    }

    [TheoryCheckUserSecrets]
    [MemberData(nameof(Sheets))]
    public void GivenMisorderedHeaders_ThenReturnWarningMessages(SheetModel sheet, SheetEnum sheetEnum)
    {
        var values = _matchedValueRanges?.Where(x => x.DataFilters[0].A1Range == sheetEnum.GetDescription()).First().ValueRange.Values;

        var headerValues = new List<IList<object>>
        {
            values![0].ToList().GetRange(0, values[0].Count - 1)
        };

        var headerOrder = new int[] { 0 }.Concat([.. RandomHelpers.GetRandomOrder(1, headerValues![0].Count - 1)]).ToArray();
        var randomValues = RandomHelpers.RandomizeValues(headerValues, headerOrder);

        var warningMessages = HeaderHelpers.CheckSheetHeaders(randomValues!, sheet).Where(x => x.Level == MessageLevelEnum.WARNING.UpperName());

        warningMessages.Should().NotBeNullOrEmpty();
    }
}
