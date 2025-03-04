using RaptorSheets.Core.Models.Google;
using RaptorSheets.Stock.Constants;
using RaptorSheets.Stock.Mappers;
using Xunit;

namespace RaptorSheets.Stock.Tests.Unit.Mappers;

public class MapperGetSheetTests
{
    public static IEnumerable<object[]> Sheets =>
    new List<object[]>
    {
        new object[] { AccountMapper.GetSheet(), SheetsConfig.AccountSheet },
        new object[] { StockMapper.GetSheet(), SheetsConfig.StockSheet },
        new object[] { TickerMapper.GetSheet(), SheetsConfig.TickerSheet },
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