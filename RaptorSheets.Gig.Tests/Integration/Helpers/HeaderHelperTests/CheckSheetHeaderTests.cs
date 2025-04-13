using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Gig.Mappers;
using RaptorSheets.Gig.Tests.Data;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Gig.Tests.Data.Attributes;
using RaptorSheets.Test.Common.Helpers;

namespace RaptorSheets.Gig.Tests.Integration.Helpers.HeaderHelperTests;

[Collection("Google Data collection")]
public class CheckSheetHeaderTests
{
    public static IEnumerable<object[]> Sheets =>
        new List<object[]>
        {
            new object[] { AddressMapper.GetSheet(), Enums.SheetEnum.ADDRESSES },
            new object[] { DailyMapper.GetSheet(), Enums.SheetEnum.DAILY },
            new object[] { MonthlyMapper.GetSheet(), Enums.SheetEnum.MONTHLY },
            new object[] { NameMapper.GetSheet(), Enums.SheetEnum.NAMES },
            new object[] { PlaceMapper.GetSheet(), Enums.SheetEnum.PLACES },
            new object[] { RegionMapper.GetSheet(), Enums.SheetEnum.REGIONS },
            new object[] { ServiceMapper.GetSheet(), Enums.SheetEnum.SERVICES },
            new object[] { ShiftMapper.GetSheet(), Enums.SheetEnum.SHIFTS },
            new object[] { TripMapper.GetSheet(), Enums.SheetEnum.TRIPS },
            new object[] { TypeMapper.GetSheet(), Enums.SheetEnum.TYPES },
            new object[] { WeekdayMapper.GetSheet(), Enums.SheetEnum.WEEKDAYS },
            new object[] { WeeklyMapper.GetSheet(), Enums.SheetEnum.WEEKLY },
            new object[] { YearlyMapper.GetSheet(), Enums.SheetEnum.YEARLY },
        };

    readonly GoogleDataFixture fixture;
    private static IList<MatchedValueRange>? _matchedValueRanges;

    public CheckSheetHeaderTests(GoogleDataFixture fixture)
    {
        this.fixture = fixture;
        _matchedValueRanges = this.fixture.ValueRanges;
    }

    [TheoryCheckUserSecrets]
    [MemberData(nameof(Sheets))]
    public void GivenFullHeaders_ThenReturnNoMessages(SheetModel sheet, Enums.SheetEnum sheetEnum)
    {
        var values = _matchedValueRanges?.Where(x => x.DataFilters[0].A1Range == sheetEnum.GetDescription()).First().ValueRange.Values.ToList();
        var messages = HeaderHelpers.CheckSheetHeaders(values![0], sheet);

        Assert.Empty(messages);
    }

    [TheoryCheckUserSecrets]
    [MemberData(nameof(Sheets))]
    public void GivenMissingHeaders_ThenReturnErrorMessages(SheetModel sheet, Enums.SheetEnum sheetEnum)
    {
        var values = _matchedValueRanges?.Where(x => x.DataFilters[0].A1Range == sheetEnum.GetDescription()).First().ValueRange.Values;

        var headerValues = new List<IList<object>>
        {
            values![0].ToList().GetRange(0, values[0].Count - 3)
        };

        var errorMessages = HeaderHelpers.CheckSheetHeaders(headerValues![0], sheet).Where(x => x.Level == MessageLevelEnum.ERROR.UpperName());

        Assert.NotEmpty(errorMessages);
    }

    [TheoryCheckUserSecrets]
    [MemberData(nameof(Sheets))]
    public void GivenMisorderedHeaders_ThenReturnWarningMessages(SheetModel sheet, Enums.SheetEnum sheetEnum)
    {
        var values = _matchedValueRanges?.Where(x => x.DataFilters[0].A1Range == sheetEnum.GetDescription()).First().ValueRange.Values;

        var headerValues = new List<IList<object>>
        {
            values![0].ToList().GetRange(0, values[0].Count - 1)
        };

        var headerOrder = new int[] { 0 }.Concat(RandomHelpers.GetRandomOrder(1, headerValues![0].Count - 1)).ToArray();
        var randomValues = RandomHelpers.RandomizeValues(headerValues, headerOrder);

        var warningMessages = HeaderHelpers.CheckSheetHeaders(randomValues![0], sheet).Where(x => x.Level == MessageLevelEnum.WARNING.UpperName());

        Assert.NotEmpty(warningMessages);
    }
}