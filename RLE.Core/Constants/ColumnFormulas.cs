namespace RLE.Core.Constants;

public static class ColumnFormulas
{
    // Array Formulas
    public static string ArrayFormula(string columnTitle, string keyRange, string formula)
    {
        return $"=ARRAYFORMULA(IFS(ROW({keyRange})=1,\"{columnTitle}\",ISBLANK({keyRange}), \"\",true,{formula})";
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
        return ArrayFormula(columnTitle, keyRange, $"IF(SUMIF({range},{criterion},{sumRange})=0,\"\",SUMIF({range},{criterion},{sumRange})");
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

    public static string GoogleFinanceMax(string columnTitle, string mapArray, string lambdaName, string attributeName)
    {
        return MapLambda(columnTitle, mapArray, lambdaName, $"MAX(INDEX(GOOGLEFINANCE({lambdaName}, \"{attributeName}\", DATE(1980,1,2), TODAY(), \"DAILY\"),,2))");
    }

    public static string GoogleFinanceMin(string columnTitle, string mapArray, string lambdaName, string attributeName)
    {
        return MapLambda(columnTitle, mapArray, lambdaName, $"MIN(INDEX(GOOGLEFINANCE({lambdaName}, \"{attributeName}\", DATE(1980,1,2), TODAY(), \"DAILY\"),,2))");
    }
}
