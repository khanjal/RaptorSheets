using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Stock.Constants;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace RaptorSheets.Stock.Entities;

/// <summary>
/// The Tickers sheet is entirely formula-driven (ProtectSheet = true, see TickerSheet.BaseSheet) -
/// Ticker itself is a SORT/UNIQUE pull from the Stocks sheet, everything else is either a GOOGLEFINANCE
/// lookup or a cross-sheet aggregate of the Stocks sheet's holdings (see TickerSheet.GetSheet) - every
/// property is isInput: false. See AccountEntity's doc comment for why this isn't shared with
/// StockEntity via a common base class.
/// </summary>
[ExcludeFromCodeCoverage]
public class TickerEntity : SheetRowEntityBase
{
    [Column(SheetsConfig.HeaderNames.Ticker, isInput: false)]
    [JsonPropertyName("ticker")]
    public string Ticker { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Name, isInput: false)]
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Accounts, isInput: false)]
    [JsonPropertyName("accounts")]
    public int Accounts { get; set; }

    [Column(SheetsConfig.HeaderNames.Shares, isInput: false, formatType: Format.ACCOUNTING)]
    [JsonPropertyName("shares")]
    public decimal Shares { get; set; }

    [Column(SheetsConfig.HeaderNames.AverageCost, isInput: false, formatType: Format.ACCOUNTING, note: ColumnNotes.AverageCost)]
    [JsonPropertyName("averageCost")]
    public decimal AverageCost { get; set; }

    [Column(SheetsConfig.HeaderNames.CostTotal, isInput: false, formatType: Format.ACCOUNTING)]
    [JsonPropertyName("costTotal")]
    public decimal CostTotal { get; set; }

    [Column(SheetsConfig.HeaderNames.CurrentPrice, isInput: false, formatType: Format.ACCOUNTING)]
    [JsonPropertyName("currentPrice")]
    public decimal CurrentPrice { get; set; }

    [Column(SheetsConfig.HeaderNames.CurrentTotal, isInput: false, formatType: Format.ACCOUNTING)]
    [JsonPropertyName("currentTotal")]
    public decimal CurrentTotal { get; set; }

    [Column(SheetsConfig.HeaderNames.Return, isInput: false, formatType: Format.ACCOUNTING)]
    [JsonPropertyName("return")]
    public decimal Return { get; set; }

    [Column(SheetsConfig.HeaderNames.PeRatio, isInput: false, formatType: Format.ACCOUNTING)]
    [JsonPropertyName("peRatio")]
    public decimal PeRatio { get; set; }

    [Column(SheetsConfig.HeaderNames.WeekHigh52, isInput: false, formatType: Format.ACCOUNTING)]
    [JsonPropertyName("52WeekHigh")]
    public decimal WeekHigh52 { get; set; }

    [Column(SheetsConfig.HeaderNames.WeekLow52, isInput: false, formatType: Format.ACCOUNTING)]
    [JsonPropertyName("52WeekLow")]
    public decimal WeekLow52 { get; set; }

    [Column(SheetsConfig.HeaderNames.MaxHigh, isInput: false, formatType: Format.ACCOUNTING)]
    [JsonPropertyName("maxHigh")]
    public decimal MaxHigh { get; set; }

    [Column(SheetsConfig.HeaderNames.MinLow, isInput: false, formatType: Format.ACCOUNTING)]
    [JsonPropertyName("minLow")]
    public decimal MinLow { get; set; }
}
