using RaptorSheets.Stock.Entities;
using RaptorSheets.Stock.Sheets;
using Xunit;

namespace RaptorSheets.Stock.Tests.Unit.Sheets;

public class StockSheetTests
{
    [Fact]
    public void MapToRowData_WritesTickerAccountShares_LeavesFormulaColumnsEmpty()
    {
        var entities = new List<StockEntity> { new() { RowId = 2, Shares = 10, Account = "Fidelity", Ticker = "AAPL" } };
        var headers = new List<object> { "Account", "Ticker", "Shares", "Avg Cost" };

        var result = StockSheet.MapToRowData(entities, headers);

        Assert.Single(result);
        var cells = result[0].Values;
        Assert.Equal(4, cells.Count);
        Assert.Equal("Fidelity", cells[0].UserEnteredValue?.StringValue); // Account - user-insertable
        Assert.Equal("AAPL", cells[1].UserEnteredValue?.StringValue); // Ticker - user-insertable
        Assert.Equal(10, cells[2].UserEnteredValue?.NumberValue); // Shares - user-insertable
        Assert.Null(cells[3].UserEnteredValue); // Avg Cost - formula/rollup column, not written
    }

    [Fact]
    public void MapToRowData_WithMultipleEntities_ReturnsOneRowPerEntity()
    {
        var entities = new List<StockEntity>
        {
            new() { RowId = 2, Shares = 5 },
            new() { RowId = 3, Shares = 10 }
        };
        var headers = new List<object> { "Shares" };

        var result = StockSheet.MapToRowData(entities, headers);

        Assert.Equal(2, result.Count);
        Assert.Equal(5, result[0].Values[0].UserEnteredValue?.NumberValue);
        Assert.Equal(10, result[1].Values[0].UserEnteredValue?.NumberValue);
    }

    [Fact]
    public void MapToRowData_WithUnrecognizedFormulaColumnHeader_WritesEmptyPlaceholder()
    {
        // "Avg Cost" is a real header, but it's a formula/rollup column (see AVERAGE_COST's case
        // in StockSheet.GetSheet()) - unlike Ticker/Account/Shares, MapToRowData must never write
        // to it, even though the entity has other fields populated.
        var entities = new List<StockEntity> { new() { RowId = 2, Shares = 3, Account = "Fidelity", Ticker = "AAPL" } };
        var headers = new List<object> { "Avg Cost" };

        var result = StockSheet.MapToRowData(entities, headers);

        Assert.Single(result[0].Values);
        Assert.Null(result[0].Values[0].UserEnteredValue);
    }
}
