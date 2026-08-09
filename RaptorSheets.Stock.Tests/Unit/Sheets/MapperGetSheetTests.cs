using System.Text.RegularExpressions;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Stock.Sheets;
using Xunit;

namespace RaptorSheets.Stock.Tests.Unit.Sheets;

public partial class MapperGetSheetTests
{
    [GeneratedRegex(@"'Tickers'![A-Z]+\d*:[A-Z]+")]
    private static partial Regex TickersColumnReferenceRegex();


    public static TheoryData<string> Sheets =>
    new()
    {
        nameof(AccountSheet), nameof(StockSheet), nameof(TickerSheet),
    };

    // TheoryData rows must be natively serializable (xUnit1045) so Test Explorer can enumerate
    // individual rows - SheetModel isn't, so the theory data is a sheet-type name and the test
    // resolves the actual (result, config) pair here instead of carrying SheetModel instances.
    private static (SheetModel Result, SheetModel Config) ResolveSheet(string sheetName) => sheetName switch
    {
        nameof(AccountSheet) => (AccountSheet.GetSheet(), AccountSheet.BaseSheet),
        nameof(StockSheet) => (StockSheet.GetSheet(), StockSheet.BaseSheet),
        nameof(TickerSheet) => (TickerSheet.GetSheet(), TickerSheet.BaseSheet),
        _ => throw new ArgumentOutOfRangeException(nameof(sheetName), sheetName, "Unknown sheet type")
    };

    [Theory]
    [MemberData(nameof(Sheets))]
    public void GivenGetSheetConfig_ThenReturnSheet(string sheetName)
    {
        var (result, sheetConfig) = ResolveSheet(sheetName);

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
            Assert.True(TickersColumnReferenceRegex().IsMatch(header.Formula!),
                $"'{header.Name}' formula references Tickers! without a real column: {header.Formula}"));
    }

    //GetDataValidation

    //GetSheetForRange

    //GetCommonShiftGroupSheetHeaders

    //GetCommonTripGroupSheetHeaders
}