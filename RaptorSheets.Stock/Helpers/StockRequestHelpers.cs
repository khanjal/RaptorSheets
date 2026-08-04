using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Stock.Entities;

namespace RaptorSheets.Stock.Helpers;

/// <summary>
/// Stock-specific wiring on top of Core's generic entity-change request builders
/// (<see cref="GoogleRequestHelpers.ChangeSheetData{T}"/>/<see cref="GoogleRequestHelpers.CreateUpdateCellRequests{T}"/>) -
/// same pattern as Gig's GigRequestHelpers. Only the Stocks sheet is wired for writes (Ticker/
/// Account/Shares are the only isInput: true columns on StockEntity - see its own doc comment):
/// Accounts and Tickers are fully formula/GOOGLEFINANCE-driven rollups with nothing for a user to
/// insert directly, and GenericSheetMapper&lt;T&gt;.MapToRowData already only writes isInput: true
/// columns generically, so no Stock-specific row-mapping code is needed here anymore.
/// </summary>
public static class StockRequestHelpers
{
    // STOCK
    public static List<Request> ChangeStockSheetData(List<StockEntity> stocks, PropertyEntity? sheetProperties)
    {
        return GoogleRequestHelpers.ChangeSheetData(stocks, sheetProperties, (entities, props) => CreateUpdateCellStockRequests(entities, props));
    }

    public static IEnumerable<Request> CreateUpdateCellStockRequests(List<StockEntity> stocks, PropertyEntity? sheetProperties)
    {
        return GoogleRequestHelpers.CreateUpdateCellRequests(stocks, sheetProperties, GenericSheetMapper<StockEntity>.MapToRowData);
    }
}
