using RLE.Core.Enums;
using RLE.Core.Extensions;
using RLE.Core.Models.Google;
using RLE.Stock.Enums;
using System.Diagnostics.CodeAnalysis;

namespace RLE.Stock.Constants;

[ExcludeFromCodeCoverage]
public static class SheetsConfig
{
    public static SheetModel AccountSheet => new()
    {
        Name = SheetEnum.ACCOUNTS.GetDescription(),
        CellColor = ColorEnum.LIGHT_GREEN,
        TabColor = ColorEnum.GREEN,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = [
            new SheetCellModel { Name = HeaderEnum.ACCOUNT.GetDescription() },
            .. CommonCostSheetHeaders,
        ]
    };

    public static SheetModel StockSheet => new()
    {
        Name = SheetEnum.STOCKS.GetDescription(),
        CellColor = ColorEnum.LIGHT_CYAN,
        TabColor = ColorEnum.CYAN,
        FreezeColumnCount = 2,
        FreezeRowCount = 1,
        Headers = [
            new SheetCellModel { Name = HeaderEnum.ACCOUNT.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.TICKER.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.NAME.GetDescription() },
            .. CommonCostSheetHeaders,
            .. CommonPriceSheetHeaders
    ]
    };

    public static SheetModel TickerSheet => new()
    {
        Name = SheetEnum.TICKERS.GetDescription(),
        CellColor = ColorEnum.LIGHT_YELLOW,
        TabColor = ColorEnum.ORANGE,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = [
            new SheetCellModel { Name = HeaderEnum.TICKER.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.NAME.GetDescription() },
            new SheetCellModel { Name = HeaderEnum.ACCOUNTS.GetDescription() },
            .. CommonCostSheetHeaders,
            .. CommonPriceSheetHeaders
        ]
    };

    private static List<SheetCellModel> CommonCostSheetHeaders =>
    [
        new SheetCellModel { Name = HeaderEnum.SHARES.GetDescription() },
        new SheetCellModel { Name = HeaderEnum.AVERAGE_COST.GetDescription() },
        new SheetCellModel { Name = HeaderEnum.COST_TOTAL.GetDescription() },
        new SheetCellModel { Name = HeaderEnum.CURRENT_PRICE.GetDescription() },
        new SheetCellModel { Name = HeaderEnum.CURRENT_TOTAL.GetDescription() },
        new SheetCellModel { Name = HeaderEnum.RETURN.GetDescription() },
    ];

    private static List<SheetCellModel> CommonPriceSheetHeaders =>
    [
        new SheetCellModel { Name = HeaderEnum.PE_RATIO.GetDescription() },
        new SheetCellModel { Name = HeaderEnum.WEEK_HIGH_52.GetDescription() },
        new SheetCellModel { Name = HeaderEnum.WEEK_LOW_52.GetDescription() },
        new SheetCellModel { Name = HeaderEnum.MAX_HIGH.GetDescription() },
        new SheetCellModel { Name = HeaderEnum.MIN_LOW.GetDescription() }
    ];
}
