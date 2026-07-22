namespace RaptorSheets.Core.Constants;

public static class ColumnFormulas
{
    // Array Formulas
    public static string ArrayFormula(string columnTitle, string keyRange, string formula)
    {
        return $"=ARRAYFORMULA(IFS(ROW({keyRange})=1,\"{columnTitle}\",ISBLANK({keyRange}), \"\",true,{formula}))";
    }

    public static string CountIf(string columnTitle, string keyRange, string range, string criterion)
    {
        return ArrayFormula(columnTitle, keyRange, $"COUNTIF({range},{criterion})");
    }

    public static string SortUnique(string columnTitle, string range)
    {
        return $"={{\"{columnTitle}\";SORT(UNIQUE({{{range}}}))}}";
    }

    public static string SumIf(string columnTitle, string keyRange, string range, string criterion, string sumRange)
    {
        return ArrayFormula(columnTitle, keyRange, $"SUMIF({range},{criterion},{sumRange})");
    }

    public static string SumIfDivide(string columnTitle, string keyRange, string range, string criterion, string sumRange, string divideRange)
    {
        return ArrayFormula(columnTitle, keyRange, $"SUMIF({range},{criterion},{sumRange})/{divideRange}");
    }

    public static string SumIfBlank(string columnTitle, string keyRange, string range, string criterion, string sumRange)
    {
        return ArrayFormula(columnTitle, keyRange, $"IF(SUMIF({range},{criterion},{sumRange})=0,\"\",SUMIF({range},{criterion},{sumRange}))");
    }

    // ArrayFormula - Simple Math
    public static string DivideRanges(string columnTitle, string keyRange, string firstRange, string secondRange)
    {
        return ArrayFormula(columnTitle, keyRange, $"IFERROR({firstRange}/{secondRange},0)");
    }

    public static string MultiplyRanges(string columnTitle, string keyRange, string firstRange, string secondRange)
    {
        return ArrayFormula(columnTitle, keyRange, $"{firstRange}*{secondRange}");
    }

    public static string SubtractRanges(string columnTitle, string keyRange, string firstRange, string secondRange)
    {
        return ArrayFormula(columnTitle, keyRange, $"{firstRange}-{secondRange}");
    }

    // GoogleFinance (Map w/ Lambda)
    public static string MapLambda(string columnTitle, string mapArray, string lambdaName, string formula)
    {
        return $"=MAP({mapArray},LAMBDA({lambdaName},IF(ROW({lambdaName})=1,\"{columnTitle}\",if(isblank({lambdaName}),,{formula}))))";
    }

    public static string GoogleFinanceBasic(string columnTitle, string mapArray, string lambdaName, string attributeName)
    {
        return MapLambda(columnTitle, mapArray, lambdaName, $"GOOGLEFINANCE({lambdaName},\"{attributeName}\")");
    }

    // Pulling GOOGLEFINANCE's full history back to 1980 for every ticker at once is slow/rate-limited
    // enough that it can take well over a minute to settle after first creation (unlike the other
    // GOOGLEFINANCE-driven columns here, which resolve in seconds) - a trailing 2-year window still
    // gives a meaningfully longer view than the 52-week high/low columns, without that cost.
    private const string RecentHistoryStartDate = "TODAY()-730";

    public static string GoogleFinanceMax(string columnTitle, string mapArray, string lambdaName, string attributeName)
    {
        return MapLambda(columnTitle, mapArray, lambdaName, $"MAX(INDEX(GOOGLEFINANCE({lambdaName}, \"{attributeName}\", {RecentHistoryStartDate}, TODAY(), \"DAILY\"),,2))");
    }

    public static string GoogleFinanceMin(string columnTitle, string mapArray, string lambdaName, string attributeName)
    {
        return MapLambda(columnTitle, mapArray, lambdaName, $"MIN(INDEX(GOOGLEFINANCE({lambdaName}, \"{attributeName}\", {RecentHistoryStartDate}, TODAY(), \"DAILY\"),,2))");
    }
}
