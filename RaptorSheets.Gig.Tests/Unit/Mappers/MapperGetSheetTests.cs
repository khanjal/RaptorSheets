using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Mappers;
using System.ComponentModel;

namespace RaptorSheets.Gig.Tests.Unit.Mappers;

[Category("Unit Tests")]
public class MapperGetSheetTests
{
    public static IEnumerable<object[]> Sheets =>
    new List<object[]>
    {
        new object[] { AddressMapper.GetSheet(), SheetsConfig.AddressSheet },
        new object[] { DailyMapper.GetSheet(), SheetsConfig.DailySheet },
        new object[] { MonthlyMapper.GetSheet(), SheetsConfig.MonthlySheet },
        new object[] { NameMapper.GetSheet(), SheetsConfig.NameSheet },
        new object[] { PlaceMapper.GetSheet(), SheetsConfig.PlaceSheet },
        new object[] { RegionMapper.GetSheet(), SheetsConfig.RegionSheet },
        new object[] { ServiceMapper.GetSheet(), SheetsConfig.ServiceSheet },
        new object[] { ShiftMapper.GetSheet(), SheetsConfig.ShiftSheet },
        new object[] { TripMapper.GetSheet(), SheetsConfig.TripSheet },
        new object[] { TypeMapper.GetSheet(), SheetsConfig.TypeSheet },
        new object[] { WeekdayMapper.GetSheet(), SheetsConfig.WeekdaySheet },
        new object[] { WeeklyMapper.GetSheet(), SheetsConfig.WeeklySheet },
        new object[] { YearlyMapper.GetSheet(), SheetsConfig.YearlySheet },
    };

    [Theory]
    [MemberData(nameof(Sheets))]
    public void GivenGetSheetConfig_ThenReturnSheet(SheetModel result, SheetModel sheetConfig)
    {
        Assert.Equal(sheetConfig.CellColor, result.CellColor);
        Assert.Equal(sheetConfig.FreezeColumnCount, result.FreezeColumnCount);
        Assert.Equal(sheetConfig.FreezeRowCount, result.FreezeRowCount);
        Assert.Equal(sheetConfig.Headers.Count, result.Headers.Count);
        Assert.Equal(sheetConfig.Name, result.Name);
        Assert.Equal(sheetConfig.ProtectSheet, result.ProtectSheet);
        Assert.Equal(sheetConfig.TabColor, result.TabColor);

        foreach (var configHeader in sheetConfig.Headers)
        {
            var resultHeader = result.Headers.First(x => x.Name == configHeader.Name);
            Assert.False(string.IsNullOrWhiteSpace(resultHeader.Column));

            if (result.ProtectSheet)
                Assert.False(string.IsNullOrWhiteSpace(resultHeader.Formula));
        }
    }

    //GetDataValidation

    //GetSheetForRange

    //GetCommonShiftGroupSheetHeaders

    //GetCommonTripGroupSheetHeaders
}