using FluentAssertions;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Stock.Constants;
using RaptorSheets.Stock.Mappers;
using Xunit;

namespace RaptorSheets.Stock.Tests.Integration.Mappers;

public class MapperGetSheetTests
{
    public static IEnumerable<object[]> Sheets =>
    [
        [AccountMapper.GetSheet(), SheetsConfig.AccountSheet],
        [StockMapper.GetSheet(), SheetsConfig.StockSheet],
        [TickerMapper.GetSheet(), SheetsConfig.TickerSheet],
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
