using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Stock.Constants;
using RaptorSheets.Stock.Entities;
using Header = RaptorSheets.Stock.Enums.Header;

namespace RaptorSheets.Stock.Sheets;

public static class StockSheet
{
    /// <summary>
    /// Bare sheet definition (name/colors/freeze/headers, no formulas) - internal so
    /// AccountSheet/TickerSheet can resolve this sheet's column positions for their own cross-sheet
    /// formulas without recursing into this sheet's GetSheet(). Stocks and Tickers read each
    /// other's columns (Stocks pulls Tickers' pricing, Tickers aggregates Stocks' holdings), so
    /// both sides must stay on the bare accessor - calling each other's full GetSheet() here would
    /// recurse infinitely. External callers should use GetSheet() instead.
    /// </summary>
    internal static SheetModel BaseSheet => new()
    {
        Name = Enums.SheetName.STOCKS.GetDescription(),
        CellColor = SheetColor.LIGHT_CYAN_3,
        TabColor = SheetColor.CYAN,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<StockEntity>()
    };

    public static SheetModel GetSheet()
    {
        var sheet = BaseSheet;
        var tickerSheet = TickerSheet.BaseSheet;

        // Ensure column indexes are properly assigned - tickerSheet needs this too since several
        // headers below resolve cross-sheet ranges via tickerSheet.GetRange(...), which depends on
        // each header's Column having already been computed.
        sheet.Headers.UpdateColumns();
        tickerSheet.Headers.UpdateColumns();

        // Apply header-specific configurations
        for (int i = 0; i < sheet.Headers.Count; i++)
        {
            var header = sheet.Headers[i];
            var headerEnum = header!.Name.ToString()!.Trim().GetValueFromName<Header>();
            var keyRange = GoogleConfig.KeyRange;

            switch (headerEnum)
            {
                case Header.NAME:
                    // Ticker is the Stocks sheet's own key column (column A), so this resolves
                    // directly against each row's own Ticker value - same GOOGLEFINANCE lookup
                    // TickerSheet uses on the Tickers reference sheet, just applied locally.
                    header.Formula = ColumnFormulas.GoogleFinanceBasic(headerEnum.GetDescription(),
                                                                    keyRange,
                                                                    Header.TICKER.GetDescription(),
                                                                    GoogleFinanceAttributes.NAME.GetDescription());
                    break;
                case Header.AVERAGE_COST:
                    header.Note = ColumnNotes.AverageCost;
                    header.Format = Format.ACCOUNTING;
                    break;
                case Header.COST_TOTAL:
                    header.Format = Format.ACCOUNTING;
                    header.Formula = ColumnFormulas.MultiplyRanges(headerEnum.GetDescription(),
                                                                    keyRange,
                                                                    sheet.GetLocalRange(Header.SHARES.GetDescription()),
                                                                    sheet.GetLocalRange(Header.AVERAGE_COST.GetDescription()));
                    break;
                case Header.CURRENT_PRICE:
                case Header.MAX_HIGH:
                case Header.MIN_LOW:
                case Header.WEEK_HIGH_52:
                case Header.WEEK_LOW_52:
                    header.Format = Format.ACCOUNTING;
                    header.Formula = ColumnFormulas.SumIf(headerEnum.GetDescription(),
                                                                    keyRange,
                                                                    tickerSheet.GetRange(Header.TICKER.GetDescription()),
                                                                    keyRange,
                                                                    tickerSheet.GetRange(headerEnum.GetDescription()));
                    break;
                case Header.CURRENT_TOTAL:
                    header.Format = Format.ACCOUNTING;
                    header.Formula = ColumnFormulas.MultiplyRanges(headerEnum.GetDescription(),
                                                                    keyRange,
                                                                    sheet.GetLocalRange(Header.SHARES.GetDescription()),
                                                                    sheet.GetLocalRange(Header.CURRENT_PRICE.GetDescription()));
                    break;
                case Header.PE_RATIO:
                    header.Format = Format.ACCOUNTING;
                    header.Formula = ColumnFormulas.SumIfBlank(headerEnum.GetDescription(),
                                                                    keyRange,
                                                                    tickerSheet.GetRange(Header.TICKER.GetDescription()),
                                                                    keyRange,
                                                                    tickerSheet.GetRange(headerEnum.GetDescription()));
                    break;
                case Header.RETURN:
                    header.Format = Format.ACCOUNTING;
                    header.Formula = ColumnFormulas.SubtractRanges(headerEnum.GetDescription(),
                                                                    keyRange,
                                                                    sheet.GetLocalRange(Header.CURRENT_TOTAL.GetDescription()),
                                                                    sheet.GetLocalRange(Header.COST_TOTAL.GetDescription()));
                    break;
                case Header.SHARES:
                    header.Format = Format.ACCOUNTING;
                    break;
                default:
                    // Apply basic formatting based on header name patterns
                    ApplyBasicFormatting(header, header.Name);
                    break;
            }
        }

        return sheet;
    }

    /// <summary>
    /// Apply basic formatting patterns based on header content for Stock domain
    /// </summary>
    private static void ApplyBasicFormatting(SheetCellModel header, string headerName)
    {
        var lowerName = headerName.ToLowerInvariant();

        if (lowerName.Contains("cost") || lowerName.Contains("price") || lowerName.Contains("total") ||
            lowerName.Contains("return") || lowerName.Contains("high") || lowerName.Contains("low"))
            header.Format = Format.ACCOUNTING;
        else if (lowerName.Contains("ratio"))
            header.Format = Format.NUMBER;
        else
            header.Format = Format.TEXT;
    }
}
