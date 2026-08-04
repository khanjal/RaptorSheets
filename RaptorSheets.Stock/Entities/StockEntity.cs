using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Enums;
using RaptorSheets.Stock.Constants;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace RaptorSheets.Stock.Entities;

/// <summary>
/// The Stocks sheet is the only one of the three with genuine user input - Ticker/Account/Shares
/// are typed in directly; everything else is a header-row ARRAYFORMULA (GOOGLEFINANCE off this
/// row's own Ticker, or a cross-sheet pull from the Tickers reference sheet - see StockSheet.GetSheet)
/// that auto-extends over newly appended rows, so isInput: false for those to avoid writing over the
/// formula. The 10 formula-only columns (AverageCost onward) live on PriceRollupEntity, shared with
/// TickerEntity - see that class's doc comment before touching any of them or this class's `order`
/// values. AverageCost is isInput: false even though it has no formula on *this* sheet (CostTotal is
/// computed from it, not the other way around) - matches the pre-port hand-rolled MapToRowData,
/// which never wrote it via the API either; changing that is a separate decision.
/// </summary>
[ExcludeFromCodeCoverage]
public class StockEntity : PriceRollupEntity
{
    [Column(SheetsConfig.HeaderNames.Ticker, isInput: true, order: 0)]
    [JsonPropertyName("ticker")]
    public string Ticker { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Name, isInput: false, order: 1)]
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Account, isInput: true, order: 2)]
    [JsonPropertyName("account")]
    public string Account { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Shares, isInput: true, formatType: Format.ACCOUNTING, order: 3)]
    [JsonPropertyName("shares")]
    public decimal Shares { get; set; }
}
