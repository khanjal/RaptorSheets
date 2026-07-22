using System.ComponentModel;

namespace RaptorSheets.Stock.Enums;

public enum SheetName
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