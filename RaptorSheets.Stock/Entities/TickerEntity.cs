using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Enums;
using RaptorSheets.Stock.Constants;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace RaptorSheets.Stock.Entities;

/// <summary>
/// The Tickers sheet is entirely formula-driven (ProtectSheet = true, see TickerSheet.BaseSheet) -
/// Ticker itself is a SORT/UNIQUE pull from the Stocks sheet, everything else is either a GOOGLEFINANCE
/// lookup or a cross-sheet aggregate of the Stocks sheet's holdings (see TickerSheet.GetSheet) - every
/// property is isInput: false. The 10 formula-only columns (AverageCost onward) live on
/// PriceRollupEntity, shared with StockEntity - see that class's doc comment before touching any of
/// them or this class's `order` values. AccountEntity is NOT part of this hierarchy (see
/// PriceRollupEntity's doc comment for why).
/// </summary>
[ExcludeFromCodeCoverage]
public class TickerEntity : PriceRollupEntity
{
    [Column(SheetsConfig.HeaderNames.Ticker, isInput: false, order: 0)]
    [JsonPropertyName("ticker")]
    public string Ticker { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Name, isInput: false, order: 1)]
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Accounts, isInput: false, order: 2)]
    [JsonPropertyName("accounts")]
    public int Accounts { get; set; }

    [Column(SheetsConfig.HeaderNames.Shares, isInput: false, formatType: Format.ACCOUNTING, order: 3)]
    [JsonPropertyName("shares")]
    public decimal Shares { get; set; }
}
