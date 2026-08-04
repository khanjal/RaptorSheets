using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Stock.Constants;

/// <summary>
/// Header name string constants for use in [Column(...)] attributes on Stock's entities -
/// attribute arguments must be compile-time constants, so these mirror (and must stay in sync
/// with) RaptorSheets.Stock.Enums.Header's [Description] text. Each sheet's own SheetModel
/// definition (headers generated from its entity, formulas/row-mapping applied on top) lives in
/// RaptorSheets.Stock.Sheets (AccountSheet/StockSheet/TickerSheet).
/// </summary>
[ExcludeFromCodeCoverage]
public static class SheetsConfig
{
    public static class HeaderNames
    {
        public const string Account = "Account";
        public const string Accounts = "Accts";
        public const string AverageCost = "Avg Cost";
        public const string CostTotal = "Cost Total";
        public const string CurrentPrice = "Current Price";
        public const string CurrentTotal = "Current Total";
        public const string MaxHigh = "Max High";
        public const string MinLow = "Min Low";
        public const string Name = "Name";
        public const string PeRatio = "P/E Ratio";
        public const string Return = "Return";
        public const string Shares = "Shares";
        public const string Stocks = "Stocks";
        public const string Ticker = "Ticker";
        public const string WeekHigh52 = "52 Wk High";
        public const string WeekLow52 = "52 Wk Low";
    }
}
