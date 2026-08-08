using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Stock.Constants;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace RaptorSheets.Stock.Entities;

/// <summary>
/// Shared by StockEntity and TickerEntity ONLY - verified byte-for-byte identical [Column]
/// configuration on both (every property here is isInput: false on every sheet that has it, formula/
/// GOOGLEFINANCE-driven on both Stocks and Tickers). Do NOT add AccountEntity to this hierarchy:
/// its AverageCost lacks the Note these entities have, and it has no CurrentPrice/PeRatio/
/// WeekHigh52/WeekLow52/MaxHigh/MinLow columns at all - a shorter, genuinely different shape.
///
/// If a future change ever needs a *different* isInput/Format for one of these properties on just
/// one of Stock/Ticker (not both), that property must move out of this base and back into the
/// specific entity that needs to diverge - do not try to make isInput conditional here, the same
/// mistake that caused the original CostEntity/PriceEntity split to be abandoned (see #83).
///
/// Explicit `order` on every property here (4-13) is required, not cosmetic:
/// TypedFieldUtils.GetPropertiesInInheritanceOrder walks base classes first, so without an explicit
/// order these would sort BEFORE StockEntity/TickerEntity's own Ticker/Name/Account(s)/Shares -
/// silently reordering every live sheet's columns. Order 0-3 is reserved for those subclass-specific
/// properties; keep this file's next addition (if any) at 14+.
/// </summary>
[ExcludeFromCodeCoverage]
public class PriceRollupEntity : SheetRowEntityBase
{
    [Column(SheetsConfig.HeaderNames.AverageCost, isInput: false, formatType: Format.ACCOUNTING, note: ColumnNotes.AverageCost, order: 4)]
    [JsonPropertyName("averageCost")]
    public decimal AverageCost { get; set; }

    [Column(SheetsConfig.HeaderNames.CostTotal, isInput: false, formatType: Format.ACCOUNTING, order: 5)]
    [JsonPropertyName("costTotal")]
    public decimal CostTotal { get; set; }

    [Column(SheetsConfig.HeaderNames.CurrentPrice, isInput: false, formatType: Format.ACCOUNTING, order: 6)]
    [JsonPropertyName("currentPrice")]
    public decimal CurrentPrice { get; set; }

    [Column(SheetsConfig.HeaderNames.CurrentTotal, isInput: false, formatType: Format.ACCOUNTING, order: 7)]
    [JsonPropertyName("currentTotal")]
    public decimal CurrentTotal { get; set; }

    [Column(SheetsConfig.HeaderNames.Return, isInput: false, formatType: Format.ACCOUNTING, order: 8)]
    [JsonPropertyName("return")]
    public decimal Return { get; set; }

    [Column(SheetsConfig.HeaderNames.PeRatio, isInput: false, formatType: Format.ACCOUNTING, order: 9)]
    [JsonPropertyName("peRatio")]
    public decimal PeRatio { get; set; }

    [Column(SheetsConfig.HeaderNames.WeekHigh52, isInput: false, formatType: Format.ACCOUNTING, order: 10)]
    [JsonPropertyName("52WeekHigh")]
    public decimal WeekHigh52 { get; set; }

    [Column(SheetsConfig.HeaderNames.WeekLow52, isInput: false, formatType: Format.ACCOUNTING, order: 11)]
    [JsonPropertyName("52WeekLow")]
    public decimal WeekLow52 { get; set; }

    [Column(SheetsConfig.HeaderNames.MaxHigh, isInput: false, formatType: Format.ACCOUNTING, order: 12)]
    [JsonPropertyName("maxHigh")]
    public decimal MaxHigh { get; set; }

    [Column(SheetsConfig.HeaderNames.MinLow, isInput: false, formatType: Format.ACCOUNTING, order: 13)]
    [JsonPropertyName("minLow")]
    public decimal MinLow { get; set; }
}
