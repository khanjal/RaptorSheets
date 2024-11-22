using System.ComponentModel;

namespace RLE.Stock.Enums;

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