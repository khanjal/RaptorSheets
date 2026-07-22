using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Models.Google;
using System.Diagnostics.CodeAnalysis;
using Header = RaptorSheets.Stock.Enums.Header;

namespace RaptorSheets.Stock.Constants;

[ExcludeFromCodeCoverage]
public static class SheetsConfig
{
    public static SheetModel AccountSheet => new()
    {
        Name = Enums.SheetName.ACCOUNTS.GetDescription(),
        CellColor = SheetColor.LIGHT_GREEN,
        TabColor = SheetColor.GREEN,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = [
            new SheetCellModel { Name = Header.ACCOUNT.GetDescription() },
            new SheetCellModel { Name = Header.STOCKS.GetDescription() },
            .. CommonCostSheetHeaders,
            .. CommonReturnSheetHeaders
        ]
    };

    public static SheetModel StockSheet => new()
    {
        Name = Enums.SheetName.STOCKS.GetDescription(),
        CellColor = SheetColor.LIGHT_CYAN,
        TabColor = SheetColor.CYAN,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        Headers = [
            new SheetCellModel { Name = Header.TICKER.GetDescription() },
            new SheetCellModel { Name = Header.NAME.GetDescription() },
            new SheetCellModel { Name = Header.ACCOUNT.GetDescription() },
            .. CommonPriceSheetHeaders,
            .. CommonHistorySheetHeaders
    ]
    };

    public static SheetModel TickerSheet => new()
    {
        Name = Enums.SheetName.TICKERS.GetDescription(),
        CellColor = SheetColor.LIGHT_YELLOW,
        TabColor = SheetColor.ORANGE,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = [
            new SheetCellModel { Name = Header.TICKER.GetDescription() },
            new SheetCellModel { Name = Header.NAME.GetDescription() },
            new SheetCellModel { Name = Header.ACCOUNTS.GetDescription() },
            .. CommonPriceSheetHeaders,
            .. CommonHistorySheetHeaders
        ]
    };

    private static List<SheetCellModel> CommonCostSheetHeaders =>
    [
        new SheetCellModel { Name = Header.SHARES.GetDescription() },
        new SheetCellModel { Name = Header.AVERAGE_COST.GetDescription() },
        new SheetCellModel { Name = Header.COST_TOTAL.GetDescription() },
    ];

    private static List<SheetCellModel> CommonPriceSheetHeaders =>
    [
        .. CommonCostSheetHeaders,
        new SheetCellModel { Name = Header.CURRENT_PRICE.GetDescription() },
        .. CommonReturnSheetHeaders,
    ];

    private static List<SheetCellModel> CommonReturnSheetHeaders =>
    [
        new SheetCellModel { Name = Header.CURRENT_TOTAL.GetDescription() },
        new SheetCellModel { Name = Header.RETURN.GetDescription() },
    ];

    private static List<SheetCellModel> CommonHistorySheetHeaders =>
    [
        new SheetCellModel { Name = Header.PE_RATIO.GetDescription() },
        new SheetCellModel { Name = Header.WEEK_HIGH_52.GetDescription() },
        new SheetCellModel { Name = Header.WEEK_LOW_52.GetDescription() },
        new SheetCellModel { Name = Header.MAX_HIGH.GetDescription() },
        new SheetCellModel { Name = Header.MIN_LOW.GetDescription() }
    ];
}
