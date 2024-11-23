namespace RLE.Core.Constants;

public static class ColumnFormulas
{
    public static string ArrayFormula(string columnTitle, string keyRange, string formula)
    {
        return $"=ARRAYFORMULA(IFS(ROW({keyRange})=1,\"{columnTitle}\",ISBLANK({keyRange}), \"\",true,{formula})";
    }

    public static string MultiplyRanges(string columnTitle, string keyRange, string firstRange, string secondRange)
    {
        return ArrayFormula(columnTitle, keyRange, $"{firstRange}*{secondRange}");
    }

    public static string SubtractRanges(string columnTitle, string keyRange, string firstRange, string secondRange)
    {
        return ArrayFormula(columnTitle, keyRange, $"{firstRange}-{secondRange}");
    }

    public static string SumIf(string columnTitle, string keyRange, string range, string criterion, string sumRange)
    {
        return ArrayFormula(columnTitle, keyRange, $"SUMIF({range},{criterion},{sumRange})");
    }

    public static string SumIfBlank(string columnTitle, string keyRange, string range, string criterion, string sumRange)
    {
        return ArrayFormula(columnTitle, keyRange, $"IF(SUMIF({range},{criterion},{sumRange})=0,\"\",SUMIF({range},{criterion},{sumRange})");
    }
}
