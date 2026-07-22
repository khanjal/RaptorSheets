using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Stock.Entities;
using RaptorSheets.Stock.Mappers;

namespace RaptorSheets.Stock.Helpers;

/// <summary>
/// Stock-specific wiring on top of Core's generic entity-change request builders
/// (<see cref="GoogleRequestHelpers.ChangeSheetData{T}"/>/<see cref="GoogleRequestHelpers.CreateUpdateCellRequests{T}"/>) -
/// same pattern as Gig's GigRequestHelpers. Only the Stocks sheet is wired for writes (Ticker/
/// Account/Shares - see <see cref="StockMapper.MapToRowData"/>): Accounts and Tickers are fully
/// formula/GOOGLEFINANCE-driven rollups with nothing for a user to insert directly.
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
        return GoogleRequestHelpers.CreateUpdateCellRequests(stocks, sheetProperties, StockMapper.MapToRowData);
    }
}
