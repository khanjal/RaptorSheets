using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Entities;
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
/// formula. AverageCost is isInput: false too even though it has no formula on *this* sheet
/// (CostTotal is computed from it, not the other way around) - matches the pre-port hand-rolled
/// MapToRowData, which never wrote it via the API either; changing that is a separate decision; see
/// ColumnFormulas.MultiplyRanges (CostTotal).
/// </summary>
[ExcludeFromCodeCoverage]
public class StockEntity : SheetRowEntityBase
{
    [Column(SheetsConfig.HeaderNames.Ticker, isInput: true)]
    [JsonPropertyName("ticker")]
    public string Ticker { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Name, isInput: false)]
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Account, isInput: true)]
    [JsonPropertyName("account")]
    public string Account { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Shares, isInput: true, formatType: Format.ACCOUNTING)]
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
