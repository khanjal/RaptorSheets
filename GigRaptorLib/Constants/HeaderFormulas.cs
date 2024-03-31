namespace GigRaptorLib.Constants
{
    public static class HeaderFormulas
    {
        public static string ArrayFormulaCountIf => "=ARRAYFORMULA(IFS(ROW($A:$A)=1,\"{0}\",ISBLANK($A:$A), \"\",true,COUNTIF({1},$A:$A)))";
        public static string ArrayFormulaSumIf => "=ARRAYFORMULA(IFS(ROW($A:$A)=1,\"{0}\",ISBLANK($A:$A), \"\",true,SUMIF({1},$A:$A, {2})))";
        public static string ArrayFormulaVisit => "=ARRAYFORMULA(IFS(ROW($A:$A)=1,\"{headerText}\",ISBLANK($A:$A), \"\", true, IFERROR(VLOOKUP($A:$A,SORT(QUERY({referenceSheet}!{columnStart}:{columnEnd},\"SELECT {columnEnd}, {columnStart}\"),2,{first}),2,0),\"\")))";
    }
}
