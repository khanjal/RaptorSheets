using System.ComponentModel;

namespace RaptorSheets.Stock.Enums;

public enum SheetEnum
{
    // Core
    [Description("Stocks")]
    STOCKS,

    // Auxillary
    [Description("Accounts")]
    ACCOUNTS,

    [Description("Tickers")]
    TICKERS,
}