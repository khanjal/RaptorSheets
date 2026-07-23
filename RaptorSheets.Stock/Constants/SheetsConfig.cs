using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Models.Google;
using System.Diagnostics.CodeAnalysis;
using Header = RaptorSheets.Stock.Enums.Header;

namespace RaptorSheets.Stock.Constants;

/// <summary>
/// Header lists shared across more than one sheet - each sheet's own SheetModel definition lives
/// alongside its formulas/row-mapping in RaptorSheets.Stock.Sheets (AccountSheet/StockSheet/TickerSheet).
/// </summary>
[ExcludeFromCodeCoverage]
public static class SheetsConfig
{
    internal static List<SheetCellModel> CommonCostSheetHeaders =>
    [
        new SheetCellModel { Name = Header.SHARES.GetDescription() },
        new SheetCellModel { Name = Header.AVERAGE_COST.GetDescription() },
        new SheetCellModel { Name = Header.COST_TOTAL.GetDescription() },
    ];

    internal static List<SheetCellModel> CommonPriceSheetHeaders =>
    [
        .. CommonCostSheetHeaders,
        new SheetCellModel { Name = Header.CURRENT_PRICE.GetDescription() },
        .. CommonReturnSheetHeaders,
    ];

    internal static List<SheetCellModel> CommonReturnSheetHeaders =>
    [
        new SheetCellModel { Name = Header.CURRENT_TOTAL.GetDescription() },
        new SheetCellModel { Name = Header.RETURN.GetDescription() },
    ];

    internal static List<SheetCellModel> CommonHistorySheetHeaders =>
    [
        new SheetCellModel { Name = Header.PE_RATIO.GetDescription() },
        new SheetCellModel { Name = Header.WEEK_HIGH_52.GetDescription() },
        new SheetCellModel { Name = Header.WEEK_LOW_52.GetDescription() },
        new SheetCellModel { Name = Header.MAX_HIGH.GetDescription() },
        new SheetCellModel { Name = Header.MIN_LOW.GetDescription() }
    ];
}
