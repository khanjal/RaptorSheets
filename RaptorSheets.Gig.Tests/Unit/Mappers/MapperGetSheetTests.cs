using FluentAssertions;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Mappers;
using System.ComponentModel;

namespace RaptorSheets.Gig.Tests.Unit.Mappers;

[Category("Unit Tests")]
public class MapperGetSheetTests
{
    public static IEnumerable<object[]> Sheets =>
    [
        [AddressMapper.GetSheet(), SheetsConfig.AddressSheet],
        [DailyMapper.GetSheet(), SheetsConfig.DailySheet],
        [MonthlyMapper.GetSheet(), SheetsConfig.MonthlySheet],
        [NameMapper.GetSheet(), SheetsConfig.NameSheet],
        [PlaceMapper.GetSheet(), SheetsConfig.PlaceSheet],
        [RegionMapper.GetSheet(), SheetsConfig.RegionSheet],
        [ServiceMapper.GetSheet(), SheetsConfig.ServiceSheet],
        [ShiftMapper.GetSheet(), SheetsConfig.ShiftSheet],
        [TripMapper.GetSheet(), SheetsConfig.TripSheet],
        [TypeMapper.GetSheet(), SheetsConfig.TypeSheet],
        [WeekdayMapper.GetSheet(), SheetsConfig.WeekdaySheet],
        [WeeklyMapper.GetSheet(), SheetsConfig.WeeklySheet],
        [YearlyMapper.GetSheet(), SheetsConfig.YearlySheet],
    ];

    [Theory]
    [MemberData(nameof(Sheets))]
    public void GivenGetSheetConfig_ThenReturnSheet(SheetModel result, SheetModel sheetConfig)
    {
        result.CellColor.Should().Be(sheetConfig.CellColor);
        result.FreezeColumnCount.Should().Be(sheetConfig.FreezeColumnCount);
        result.FreezeRowCount.Should().Be(sheetConfig.FreezeRowCount);
        result.Headers.Count.Should().Be(sheetConfig.Headers.Count);
        result.Name.Should().Be(sheetConfig.Name);
        result.ProtectSheet.Should().Be(sheetConfig.ProtectSheet);
        result.TabColor.Should().Be(sheetConfig.TabColor);

        foreach (var configHeader in sheetConfig.Headers)
        {
            var resultHeader = result.Headers.First(x => x.Name == configHeader.Name);
            resultHeader.Column.Should().NotBeNullOrWhiteSpace();

            if (result.ProtectSheet)
                resultHeader.Formula.Should().NotBeNullOrWhiteSpace();
        }
    }

    //GetDataValidation

    //GetSheetForRange

    //GetCommonShiftGroupSheetHeaders

    //GetCommonTripGroupSheetHeaders
}
