using RaptorSheets.Stock.Entities;
using RaptorSheets.Stock.Mappers;
using Xunit;

namespace RaptorSheets.Stock.Tests.Unit.Mappers;

public class StockMapperTests
{
    [Fact]
    public void MapToRowData_WritesSharesOnly_LeavesOtherColumnsEmpty()
    {
        var entities = new List<StockEntity> { new() { RowId = 2, Shares = 10, Account = "Fidelity", Ticker = "AAPL" } };
        var headers = new List<object> { "Account", "Ticker", "Shares", "Avg Cost" };

        var result = StockMapper.MapToRowData(entities, headers);

        Assert.Single(result);
        var cells = result[0].Values;
        Assert.Equal(4, cells.Count);
        Assert.Null(cells[0].UserEnteredValue); // Account - formula/rollup column, not written
        Assert.Null(cells[1].UserEnteredValue); // Ticker - formula/rollup column, not written
        Assert.Equal(10, cells[2].UserEnteredValue?.NumberValue); // Shares - the one editable column
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

        var result = StockMapper.MapToRowData(entities, headers);

        Assert.Equal(2, result.Count);
        Assert.Equal(5, result[0].Values[0].UserEnteredValue?.NumberValue);
        Assert.Equal(10, result[1].Values[0].UserEnteredValue?.NumberValue);
    }

    [Fact]
    public void MapToRowData_WithUnknownHeader_WritesEmptyPlaceholder()
    {
        var entities = new List<StockEntity> { new() { RowId = 2, Shares = 3 } };
        var headers = new List<object> { "Not A Real Header" };

        var result = StockMapper.MapToRowData(entities, headers);

        Assert.Single(result[0].Values);
        Assert.Null(result[0].Values[0].UserEnteredValue);
    }
}
