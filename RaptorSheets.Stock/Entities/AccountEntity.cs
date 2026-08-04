using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Stock.Constants;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace RaptorSheets.Stock.Entities;

/// <summary>
/// The Accounts sheet is entirely formula-driven (ProtectSheet = true, see AccountSheet.BaseSheet) -
/// every column here, including Account itself (a SORT/UNIQUE pull from the Stocks sheet), is
/// isInput: false. No property is shared with StockEntity/TickerEntity via a common base class even
/// though some names/types match - the same header name is genuinely user-input on one sheet and
/// formula-output on another (e.g. Shares), which [Column]'s isInput can't express conditionally on
/// a shared base. Note there is deliberately no CurrentPrice property - the Accounts sheet has no
/// such column (unlike Stock/Ticker).
/// </summary>
[ExcludeFromCodeCoverage]
public class AccountEntity : SheetRowEntityBase
{
    [Column(SheetsConfig.HeaderNames.Account, isInput: false)]
    [JsonPropertyName("account")]
    public string Account { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Stocks, isInput: false)]
    [JsonPropertyName("stocks")]
    public decimal Stocks { get; set; }

    [Column(SheetsConfig.HeaderNames.Shares, isInput: false, formatType: Format.ACCOUNTING)]
    [JsonPropertyName("shares")]
    public decimal Shares { get; set; }

    [Column(SheetsConfig.HeaderNames.AverageCost, isInput: false, formatType: Format.ACCOUNTING)]
    [JsonPropertyName("averageCost")]
    public decimal AverageCost { get; set; }

    [Column(SheetsConfig.HeaderNames.CostTotal, isInput: false, formatType: Format.ACCOUNTING)]
    [JsonPropertyName("costTotal")]
    public decimal CostTotal { get; set; }

    [Column(SheetsConfig.HeaderNames.CurrentTotal, isInput: false, formatType: Format.ACCOUNTING)]
    [JsonPropertyName("currentTotal")]
    public decimal CurrentTotal { get; set; }

    [Column(SheetsConfig.HeaderNames.Return, isInput: false, formatType: Format.ACCOUNTING)]
    [JsonPropertyName("return")]
    public decimal Return { get; set; }
}
