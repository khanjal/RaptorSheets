using System.ComponentModel;

namespace RLE.Stock.Enums;

public enum HeaderEnum
{
    [Description("Account")]
    ACCOUNT,

    [Description("Accts")]
    ACCOUNTS,

    [Description("Avg Cost")]
    AVERAGE_COST,

    [Description("Cost Total")]
    COST_TOTAL,

    [Description("Current Price")]
    CURRENT_PRICE,

    [Description("Current Total")]
    CURRENT_TOTAL,

    [Description("Max High")]
    MAX_HIGH,

    [Description("Min Low")]
    MIN_LOW,

    [Description("Name")]
    NAME,

    [Description("P/E Ratio")]
    PE_RATIO,

    [Description("Return")]
    RETURN,

    [Description("Shares")]
    SHARES,

    [Description("Stocks")]
    STOCKS,

    [Description("Ticker")]
    TICKER,

    [Description("52 Wk High")]
    WEEK_HIGH_52,

    [Description("52 Wk Low")]
    WEEK_LOW_52,
}