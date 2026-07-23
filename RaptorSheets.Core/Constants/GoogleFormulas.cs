using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Core.Constants;

/// <summary>
/// Generic Google Sheets formula templates for consistent reuse across all domain packages
/// All formulas use placeholder tokens that can be replaced with actual ranges/values
/// Domain-specific formulas should be in their respective domain packages
/// </summary>
[ExcludeFromCodeCoverage]
public static class GoogleFormulas
{
    #region ARRAYFORMULA Base Templates
    
    /// <summary>
    /// Basic ARRAYFORMULA with header and blank check: =ARRAYFORMULA(IFS(ROW({keyRange})=1,"{header}",ISBLANK({keyRange}), "", true, {formula}))
    /// Placeholders: {keyRange}, {header}, {formula}
    /// </summary>
    public const string ArrayFormulaBase = "=ARRAYFORMULA(IFS(ROW({keyRange})=1,\"{header}\",ISBLANK({keyRange}), \"\", true, {formula}))";

    /// <summary>
    /// Simple array literal for unique values: ="{header}";SORT(UNIQUE({sourceRange}))
    /// Placeholders: {header}, {sourceRange}
    /// This is more efficient than ArrayFormulaBase for simple unique value lists
    /// </summary>
    public const string ArrayLiteralUnique = "={\"{header}\";SORT(UNIQUE({sourceRange}))}";

    /// <summary>
    /// Simple array literal for unique values with combined ranges: ="{header}";SORT(UNIQUE({range1};{range2}))
    /// Placeholders: {header}, {range1}, {range2}
    /// Used when combining multiple source ranges for unique values
    /// Note: This includes empty values. Use ArrayLiteralUniqueCombinedFiltered to exclude empty values.
    /// </summary>
    public const string ArrayLiteralUniqueCombined = "={\"{header}\";SORT(UNIQUE({{range1};{range2}}))}";

    /// <summary>
    /// Simple array literal for unique values with combined ranges (filtered):
    /// <![CDATA[ ="{header}";SORT(UNIQUE(IFERROR(FILTER({range1};{range2}},{range1};{range2}<>"")))) ]]>
    /// Placeholders: {header}, {range1}, {range2}
    /// Used when combining multiple source ranges for unique values, excluding empty values
    /// This is the recommended version for most use cases to avoid blank entries
    /// IFERROR handles cases where FILTER returns no results
    /// </summary>
    public const string ArrayLiteralUniqueCombinedFiltered = "={\"{header}\";SORT(UNIQUE(IFERROR(FILTER({{range1};{range2}},{{range1};{range2}}<>\"\"))))}";

    /// <summary>
    /// Simple array literal for unique filtered values:
    /// <![CDATA[ ="{header}";SORT(UNIQUE(IFERROR(FILTER({sourceRange}, {sourceRange}<>"")))) ]]>
    /// Placeholders: {header}, {sourceRange}
    /// More efficient than ArrayFormulaBase for filtered unique values
    /// Sorted alphabetically/numerically
    /// </summary>
    public const string ArrayLiteralUniqueFilteredSorted = "={\"{header}\";SORT(UNIQUE(IFERROR(FILTER({sourceRange}, {sourceRange}<>\"\"))))}";

    /// <summary>
    /// Simple array literal for unique filtered values:
    /// <![CDATA[ ="{header}";UNIQUE(IFERROR(FILTER({sourceRange}, {sourceRange}<>""))) ]]>
    /// Placeholders: {header}, {sourceRange}
    /// Preserves source order - useful for chronological data like weeks, months
    /// This is the default - use ArrayLiteralUniqueFilteredSorted for alphabetical sorting
    /// </summary>
    public const string ArrayLiteralUniqueFiltered = "={\"{header}\";UNIQUE(IFERROR(FILTER({sourceRange}, {sourceRange}<>\"\")))}";

    /// <summary>
    /// ARRAYFORMULA for unique values: =ARRAYFORMULA(IFS(ROW({keyRange})=1,"{header}",ISBLANK({keyRange}), "", true, SORT(UNIQUE({sourceRange}))))
    /// Placeholders: {keyRange}, {header}, {sourceRange}
    /// </summary>
    public const string ArrayFormulaUnique = "=ARRAYFORMULA(IFS(ROW({keyRange})=1,\"{header}\",ISBLANK({keyRange}), \"\", true, SORT(UNIQUE({sourceRange}))))";

    /// <summary>
    /// ARRAYFORMULA for unique filtered values:
    /// <![CDATA[ =ARRAYFORMULA(IFS(ROW({keyRange})=1,"{header}",ISBLANK({keyRange}), "", true, SORT(UNIQUE(IFERROR(FILTER({sourceRange}, {sourceRange}<>"")), 1))) ]]>
    /// Placeholders: {keyRange}, {header}, {sourceRange}
    /// </summary>
    public const string ArrayFormulaUniqueFiltered = "=ARRAYFORMULA(IFS(ROW({keyRange})=1,\"{header}\",ISBLANK({keyRange}), \"\", true, SORT(UNIQUE(IFERROR(FILTER({sourceRange}, {sourceRange}<>\"\")), 1))))";

    /// <summary>
    /// ARRAYFORMULA for weekday text formatting from dates: =ARRAYFORMULA(IFS(ROW({keyRange})=1,"{header}",ISBLANK({keyRange}), "", true,TEXT({keyRange}{offset},"ddd")))
    /// Placeholders: {keyRange}, {header}, {offset}
    /// Used for converting date ranges to weekday abbreviations (Mon, Tue, Wed, etc.)
    /// </summary>
    public const string ArrayFormulaWeekdayText = "=ARRAYFORMULA(IFS(ROW({keyRange})=1,\"{header}\",ISBLANK({keyRange}), \"\", true,TEXT({keyRange}+{offset},\"ddd\")))";

    /// <summary>
    /// ARRAYFORMULA with separate blank check range: =ARRAYFORMULA(IFS(ROW({keyRange})=1,"{header}",ISBLANK({blankCheckRange}), "", true, {formula}))
    /// Placeholders: {keyRange}, {blankCheckRange}, {header}, {formula}
    /// Use when the row check and blank check need different columns
    /// </summary>
    public const string ArrayFormulaBaseWithBlankCheck = "=ARRAYFORMULA(IFS(ROW({keyRange})=1,\"{header}\",ISBLANK({keyRange}), \"\", true, IF(ISBLANK({blankCheckRange}), \"\", {formula})))";

    #endregion

    #region Generic Aggregation Formulas

    /// <summary>
    /// SUMIF aggregation: SUMIF({lookupRange},{keyRange},{sumRange})
    /// Placeholders: {lookupRange}, {keyRange}, {sumRange}
    /// </summary>
    public const string SumIfAggregation = "SUMIF({lookupRange},{keyRange},{sumRange})";

    /// <summary>
    /// COUNTIF aggregation: COUNTIF({lookupRange},{keyRange})
    /// Placeholders: {lookupRange}, {keyRange}
    /// </summary>
    public const string CountIfAggregation = "COUNTIF({lookupRange},{keyRange})";

    /// <summary>
    /// Generic division with zero protection: {numerator}/IF({denominator}=0,1,{denominator})
    /// Placeholders: {numerator}, {denominator}
    /// </summary>
    public const string SafeDivisionFormula = "{numerator}/IF({denominator}=0,1,{denominator})";

    #endregion

    #region Generic Lookup Formulas

    /// <summary>
    /// VLOOKUP with error handling: IFERROR(VLOOKUP({searchKey},{searchRange},{columnIndex},false),"")
    /// Placeholders: {searchKey}, {searchRange}, {columnIndex}
    /// </summary>
    public const string SafeVLookup = "IFERROR(VLOOKUP({searchKey},{searchRange},{columnIndex},false),\"\")";

    /// <summary>
    /// VLOOKUP for first/last values with sorting: IFERROR(VLOOKUP({keyRange},SORT(QUERY({sourceSheet}!{dateColumn}:{keyColumn},"SELECT {keyColumn}, {dateColumn}"),2,{isFirst}),2,0),"")
    /// Placeholders: {keyRange}, {sourceSheet}, {dateColumn}, {keyColumn}, {isFirst} (true/false)
    /// </summary>
    public const string SortedVLookup = "IFERROR(VLOOKUP({keyRange},SORT(QUERY({sourceSheet}!{dateColumn}:{keyColumn},\"SELECT {keyColumn}, {dateColumn}\"),2,{isFirst}),2,0),\"\")";

    #endregion

    #region Generic Date and Time Formulas

    /// <summary>
    /// Weekday number calculation: WEEKDAY({dateRange},2)
    /// Placeholders: {dateRange}
    /// </summary>
    public const string WeekdayNumber = "WEEKDAY({dateRange},2)";

    /// <summary>
    /// Split string by delimiter and get index: IFERROR(INDEX(SPLIT({sourceRange}, "{delimiter}"), 0, {index}), 0)
    /// Placeholders: {sourceRange}, {delimiter}, {index}
    /// </summary>
    public const string SplitStringByIndex = "IFERROR(INDEX(SPLIT({sourceRange}, \"{delimiter}\"), 0, {index}), 0)";

    /// <summary>
    /// Week number with year:
    /// <![CDATA[ WEEKNUM({dateRange},2)&"-"&YEAR({dateRange}) ]]>
    /// Placeholders: {dateRange}
    /// </summary>
    public const string WeekNumberWithYear = "WEEKNUM({dateRange},2)&\"-\"&YEAR({dateRange})";

    /// <summary>
    /// Month number with year:
    /// <![CDATA[ MONTH({dateRange})&"-"&YEAR({dateRange}) ]]>
    /// Placeholders: {dateRange}
    /// </summary>
    public const string MonthNumberWithYear = "MONTH({dateRange})&\"-\"&YEAR({dateRange})";

    /// <summary>
    /// Date range calculation for week begin: DATE({yearRange},1,1)+(({weekRange}-1)*7)-WEEKDAY(DATE({yearRange},1,1),3)
    /// Placeholders: {yearRange}, {weekRange}
    /// </summary>
    public const string WeekBeginDate = "DATE({yearRange},1,1)+(({weekRange}-1)*7)-WEEKDAY(DATE({yearRange},1,1),3)";

    /// <summary>
    /// Date range calculation for week end: DATE({yearRange},1,7)+(({weekRange}-1)*7)-WEEKDAY(DATE({yearRange},1,1),3)
    /// Placeholders: {yearRange}, {weekRange}
    /// </summary>
    public const string WeekEndDate = "DATE({yearRange},1,7)+(({weekRange}-1)*7)-WEEKDAY(DATE({yearRange},1,1),3)";

    /// <summary>
    /// Complex rolling average for time series analysis using DAVERAGE with transpose matrix.
    /// Formula:
    /// <![CDATA[ DAVERAGE(transpose({{totalRange},TRANSPOSE(if(ROW({totalRange}) <= TRANSPOSE(ROW({totalRange})),{totalRange},))}),sequence(rows({totalRange}),1),{if(,,);if(,,)}) ]]>
    /// Creates a cumulative average by building a growing dataset for each row; the DAVERAGE
    /// function with empty criteria averages all rows up to the current position.
    /// Placeholders: {totalRange}
    /// </summary>
    public const string RollingAverageFormula = "DAVERAGE(transpose({{totalRange},TRANSPOSE(if(ROW({totalRange}) <= TRANSPOSE(ROW({totalRange})),{totalRange},))}),sequence(rows({totalRange}),1),{if(,,);if(,,)})";

    #endregion

    #region Conditional Logic

    /// <summary>
    /// Zero division protection: IF({divisorRange}=0,1,{divisorRange})
    /// Placeholders: {divisorRange}
    /// </summary>
    public const string ZeroDivisionProtection = "IF({divisorRange}=0,1,{divisorRange})";

    /// <summary>
    /// Zero check for calculations: {valueRange} = 0, 0, true, {calculation}
    /// Placeholders: {valueRange}, {calculation}
    /// </summary>
    public const string ZeroCheckCondition = "{valueRange} = 0, 0, true, {calculation}";

    #endregion
}