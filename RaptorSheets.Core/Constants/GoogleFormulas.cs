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
    /// </summary>
    public const string ArrayLiteralUniqueCombined = "={\"{header}\";SORT(UNIQUE({{range1};{range2}}))}";

    /// <summary>
    /// Simple array literal for unique filtered values: ="{header}";SORT(UNIQUE(IFERROR(FILTER({sourceRange}, {sourceRange}<>""))))
    /// Placeholders: {header}, {sourceRange}
    /// More efficient than ArrayFormulaBase for filtered unique values
    /// Sorted alphabetically/numerically
    /// </summary>
    public const string ArrayLiteralUniqueFilteredSorted = "={\"{header}\";SORT(UNIQUE(IFERROR(FILTER({sourceRange}, {sourceRange}<>\"\"))))}";

    /// <summary>
    /// Simple array literal for unique filtered values: ="{header}";UNIQUE(IFERROR(FILTER({sourceRange}, {sourceRange}<>"")))
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
    /// ARRAYFORMULA for unique filtered values: =ARRAYFORMULA(IFS(ROW({keyRange})=1,"{header}",ISBLANK({keyRange}), "", true, SORT(UNIQUE(IFERROR(FILTER({sourceRange}, {sourceRange}<>"")), 1)))
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