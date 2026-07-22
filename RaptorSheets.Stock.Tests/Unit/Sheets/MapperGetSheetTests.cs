using System.Text.RegularExpressions;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Stock.Sheets;
using Xunit;

namespace RaptorSheets.Stock.Tests.Unit.Sheets;

public class MapperGetSheetTests
{
    public static IEnumerable<object[]> Sheets =>
    new List<object[]>
    {
        new object[] { AccountSheet.GetSheet(), AccountSheet.BaseSheet },
        new object[] { StockSheet.GetSheet(), StockSheet.BaseSheet },
        new object[] { TickerSheet.GetSheet(), TickerSheet.BaseSheet },
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

    [Fact]
    public void StockSheet_CrossSheetFormulas_ShouldReferenceARealColumn()
    {
        // Regression test: StockSheet.GetSheet() builds several formulas via tickerSheet.GetRange(...)
        // (CurrentPrice/PeRatio/52-week High-Low/MaxHigh/MinLow). That only resolves to a real column
        // if tickerSheet.Headers.UpdateColumns() has already run - StockSheet.GetSheet() used to only
        // call UpdateColumns() on its own Stocks sheet, so every tickerSheet.GetRange(...) call
        // resolved to a bare "'Tickers'!" (no column), producing invalid formula syntax (#ERROR! in
        // Sheets) on every column that referenced it.
        var sheet = StockSheet.GetSheet();

        var crossSheetFormulaHeaders = sheet.Headers.Where(h => h.Formula?.Contains("'Tickers'!") == true);

        Assert.NotEmpty(crossSheetFormulaHeaders);
        Assert.All(crossSheetFormulaHeaders, header =>
            Assert.True(Regex.IsMatch(header.Formula!, @"'Tickers'![A-Z]+\d*:[A-Z]+"),
                $"'{header.Name}' formula references Tickers! without a real column: {header.Formula}"));
    }

    //GetDataValidation

    //GetSheetForRange

    //GetCommonShiftGroupSheetHeaders

    //GetCommonTripGroupSheetHeaders
}