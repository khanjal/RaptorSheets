using RaptorSheets.Core.Constants;
using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Core.Helpers;

/// <summary>
/// Helper class for building generic Google Sheets formulas using the GoogleFormulas constants
/// Provides methods to replace placeholders with actual values for common patterns
/// Domain-specific formula builders should be in their respective domain packages
/// </summary>
[ExcludeFromCodeCoverage]
public static class GoogleFormulaBuilder
{
    // Placeholder constants
    private const string PlaceholderHeader = "{header}";
    private const string PlaceholderSourceRange = "{sourceRange}";
    private const string PlaceholderKeyRange = "{keyRange}";
    private const string PlaceholderLookupRange = "{lookupRange}";
    private const string PlaceholderSumRange = "{sumRange}";
    private const string PlaceholderRange1 = "{range1}";
    private const string PlaceholderRange2 = "{range2}";
    private const string PlaceholderNumerator = "{numerator}";
    private const string PlaceholderDenominator = "{denominator}";
    private const string PlaceholderFormula = "{formula}";
    private const string PlaceholderSearchKey = "{searchKey}";
    private const string PlaceholderSearchRange = "{searchRange}";
    private const string PlaceholderColumnIndex = "{columnIndex}";
    private const string PlaceholderSourceSheet = "{sourceSheet}";
    private const string PlaceholderDateColumn = "{dateColumn}";
    private const string PlaceholderKeyColumn = "{keyColumn}";
    private const string PlaceholderIsFirst = "{isFirst}";
    private const string PlaceholderDateRange = "{dateRange}";
    private const string PlaceholderDelimiter = "{delimiter}";
    private const string PlaceholderIndex = "{index}";
    private const string PlaceholderOffset = "{offset}";
    private const string PlaceholderYearRange = "{yearRange}";
    private const string PlaceholderWeekRange = "{weekRange}";
    private const string PlaceholderTotalRange = "{totalRange}";

    #region Generic ARRAYFORMULA Builders

    /// <summary>
    /// Builds a complete ARRAYFORMULA with SUMIF aggregation
    /// </summary>
    public static string BuildArrayFormulaSumIf(string keyRange, string header, string lookupRange, string sumRange)
    {
        var sumIfFormula = GoogleFormulas.SumIfAggregation
            .Replace(PlaceholderLookupRange, lookupRange)
            .Replace(PlaceholderKeyRange, keyRange)
            .Replace(PlaceholderSumRange, sumRange);

        return GoogleFormulas.ArrayFormulaBase
            .Replace(PlaceholderKeyRange, keyRange)
            .Replace(PlaceholderHeader, header)
            .Replace(PlaceholderFormula, sumIfFormula);
    }

    /// <summary>
    /// Builds a complete ARRAYFORMULA with COUNTIF aggregation
    /// </summary>
    public static string BuildArrayFormulaCountIf(string keyRange, string header, string lookupRange)
    {
        var countIfFormula = GoogleFormulas.CountIfAggregation
            .Replace(PlaceholderLookupRange, lookupRange)
            .Replace(PlaceholderKeyRange, keyRange);

        return GoogleFormulas.ArrayFormulaBase
            .Replace(PlaceholderKeyRange, keyRange)
            .Replace(PlaceholderHeader, header)
            .Replace(PlaceholderFormula, countIfFormula);
    }

    /// <summary>
    /// Builds a simple array literal for unique values (more efficient than ARRAYFORMULA for simple lists)
    /// </summary>
    public static string BuildArrayLiteralUnique(string header, string sourceRange)
    {
        return GoogleFormulas.ArrayLiteralUnique
            .Replace(PlaceholderHeader, header)
            .Replace(PlaceholderSourceRange, sourceRange);
    }

    /// <summary>
    /// Builds a simple array literal for unique values from combined ranges
    /// </summary>
    public static string BuildArrayLiteralUniqueCombined(string header, string range1, string range2)
    {
        return GoogleFormulas.ArrayLiteralUniqueCombined
            .Replace(PlaceholderHeader, header)
            .Replace(PlaceholderRange1, range1)
            .Replace(PlaceholderRange2, range2);
    }

    /// <summary>
    /// Builds a simple array literal for unique values from combined ranges with empty value filtering
    /// This is the recommended version for most use cases to avoid blank entries
    /// </summary>
    public static string BuildArrayLiteralUniqueCombinedFiltered(string header, string range1, string range2)
    {
        return GoogleFormulas.ArrayLiteralUniqueCombinedFiltered
            .Replace(PlaceholderHeader, header)
            .Replace(PlaceholderRange1, range1)
            .Replace(PlaceholderRange2, range2);
    }

    /// <summary>
    /// Builds a simple array literal for unique filtered values
    /// </summary>
    public static string BuildArrayLiteralUniqueFiltered(string header, string sourceRange)
    {
        return GoogleFormulas.ArrayLiteralUniqueFiltered
            .Replace(PlaceholderHeader, header)
            .Replace(PlaceholderSourceRange, sourceRange);
    }

    /// <summary>
    /// Builds a simple array literal for unique filtered values with sorting
    /// </summary>
    public static string BuildArrayLiteralUniqueFilteredSorted(string header, string sourceRange)
    {
        return GoogleFormulas.ArrayLiteralUniqueFilteredSorted
            .Replace(PlaceholderHeader, header)
            .Replace(PlaceholderSourceRange, sourceRange);
    }

    /// <summary>
    /// Builds a QUERY formula that groups two parallel ranges by the first two columns
    /// and returns a header row plus a count column. This centralizes the common summary pattern.
    /// </summary>
    public static string BuildQueryGroupTwoColumns(string header1, string header2, string range1, string range2, string countHeader, bool countColumnIsSecond = false)
    {
        var countExpr = countColumnIsSecond ? "count(Col2)" : "count(Col1)";

        var ranges = "{" + range1 + "," + range2 + "}";

        var innerQuery = "\"select Col1, Col2, " + countExpr + " where Col1 is not null and Col2 is not null group by Col1, Col2 order by Col1 asc, " + countExpr + " desc label Col1 '" + header1 + "', Col2 '" + header2 + "', " + countExpr + " '" + countHeader + "'\",0";

        return "=QUERY(" + ranges + "," + innerQuery + ")";
    }

    /// <summary>
    /// Builds a QUERY formula that groups two parallel ranges by the first two columns
    /// and returns a header row, a count column, and one or more summed columns
    /// (e.g. Pay, Tips, Bonus, Total) aggregated per group.
    /// </summary>
    public static string BuildQueryGroupTwoColumns(string header1, string header2, string range1, string range2, string countHeader, IEnumerable<(string Header, string Range)> sumColumns, bool countColumnIsSecond = false)
    {
        return BuildQueryGroupTwoColumns(header1, header2, range1, range2, countHeader, sumColumns.Select(s => (s.Header, s.Range, AggregateFunction: "sum")), countColumnIsSecond);
    }

    /// <summary>
    /// Builds a QUERY formula that groups two parallel ranges by the first two columns
    /// and returns a header row, a count column, and one or more aggregated columns
    /// (e.g. sum(Pay), min(Date), max(Date)) per group.
    /// </summary>
    public static string BuildQueryGroupTwoColumns(string header1, string header2, string range1, string range2, string countHeader, IEnumerable<(string Header, string Range, string AggregateFunction)> aggregateColumns, bool countColumnIsSecond = false)
    {
        var countExpr = countColumnIsSecond ? "count(Col2)" : "count(Col1)";
        var aggregateColumnList = aggregateColumns.ToList();

        var ranges = "{" + range1 + "," + range2 + string.Concat(aggregateColumnList.Select(a => "," + a.Range)) + "}";

        var selectColumns = new List<string> { "Col1", "Col2", countExpr };
        var labelClauses = new List<string> { "Col1 '" + header1 + "'", "Col2 '" + header2 + "'", countExpr + " '" + countHeader + "'" };

        for (var i = 0; i < aggregateColumnList.Count; i++)
        {
            var aggregateExpr = $"{aggregateColumnList[i].AggregateFunction}(Col{i + 3})";
            selectColumns.Add(aggregateExpr);
            labelClauses.Add(aggregateExpr + " '" + aggregateColumnList[i].Header + "'");
        }

        var innerQuery = "\"select " + string.Join(", ", selectColumns) +
            " where Col1 is not null and Col2 is not null group by Col1, Col2 order by Col1 asc, " + countExpr +
            " desc label " + string.Join(", ", labelClauses) + "\",0";

        return "=QUERY(" + ranges + "," + innerQuery + ")";
    }

    /// <summary>
    /// Builds a complete ARRAYFORMULA for unique values
    /// </summary>
    public static string BuildArrayFormulaUnique(string keyRange, string header, string sourceRange)
    {
        return GoogleFormulas.ArrayFormulaUnique
            .Replace(PlaceholderKeyRange, keyRange)
            .Replace(PlaceholderHeader, header)
            .Replace(PlaceholderSourceRange, sourceRange);
    }

    /// <summary>
    /// Builds a complete ARRAYFORMULA for unique filtered values
    /// </summary>
    public static string BuildArrayFormulaUniqueFiltered(string keyRange, string header, string sourceRange)
    {
        return GoogleFormulas.ArrayFormulaUniqueFiltered
            .Replace(PlaceholderKeyRange, keyRange)
            .Replace(PlaceholderHeader, header)
            .Replace(PlaceholderSourceRange, sourceRange);
    }

    /// <summary>
    /// Builds ARRAYFORMULA with safe division (zero protection)
    /// </summary>
    public static string BuildArrayFormulaSafeDivision(string keyRange, string header, string numerator, string denominator)
    {
        var divisionFormula = GoogleFormulas.SafeDivisionFormula
            .Replace(PlaceholderNumerator, numerator)
            .Replace(PlaceholderDenominator, denominator);

        return GoogleFormulas.ArrayFormulaBase
            .Replace(PlaceholderKeyRange, keyRange)
            .Replace(PlaceholderHeader, header)
            .Replace(PlaceholderFormula, divisionFormula);
    }

    /// <summary>
    /// Builds a complete ARRAYFORMULA with number sequence
    /// </summary>
    public static string BuildArrayLiteralNumberSequence(string header, int count)
    {
        var numbers = string.Join(";", Enumerable.Range(1, count));
        return $"={{\"{header}\";SORT({{{numbers}}})}}";
    }

    #endregion

    #region Generic Lookup Formula Builders

    /// <summary>
    /// Builds ARRAYFORMULA for sorted lookup (first or last value)
    /// </summary>
    public static string BuildArrayFormulaSortedLookup(string keyRange, string header, string sourceSheet, string dateColumn, string keyColumn, bool isFirst)
    {
        var lookupFormula = GoogleFormulas.SortedVLookup
            .Replace(PlaceholderKeyRange, keyRange)
            .Replace(PlaceholderSourceSheet, sourceSheet)
            .Replace(PlaceholderDateColumn, dateColumn)
            .Replace(PlaceholderKeyColumn, keyColumn)
            .Replace(PlaceholderIsFirst, isFirst.ToString().ToLower());

        return GoogleFormulas.ArrayFormulaBase
            .Replace(PlaceholderKeyRange, keyRange)
            .Replace(PlaceholderHeader, header)
            .Replace(PlaceholderFormula, lookupFormula);
    }

    /// <summary>
    /// Builds safe VLOOKUP with error handling
    /// </summary>
    public static string BuildSafeVLookup(string searchKey, string searchRange, string columnIndex)
    {
        return GoogleFormulas.SafeVLookup
            .Replace(PlaceholderSearchKey, searchKey)
            .Replace(PlaceholderSearchRange, searchRange)
            .Replace(PlaceholderColumnIndex, columnIndex);
    }

    #endregion

    #region Generic Date/Time Formula Builders

    /// <summary>
    /// Builds weekday formula
    /// </summary>
    public static string BuildArrayFormulaWeekday(string keyRange, string header, string dateRange)
    {
        var weekdayFormula = GoogleFormulas.WeekdayNumber.Replace(PlaceholderDateRange, dateRange);

        return GoogleFormulas.ArrayFormulaBase
            .Replace(PlaceholderKeyRange, keyRange)
            .Replace(PlaceholderHeader, header)
            .Replace(PlaceholderFormula, weekdayFormula);
    }

    /// <summary>
    /// Builds week number with year formula (e.g. "32-2026")
    /// </summary>
    public static string BuildArrayFormulaWeekNumber(string keyRange, string header, string dateRange)
    {
        var weekFormula = GoogleFormulas.WeekNumberWithYear.Replace(PlaceholderDateRange, dateRange);

        return GoogleFormulas.ArrayFormulaBase
            .Replace(PlaceholderKeyRange, keyRange)
            .Replace(PlaceholderHeader, header)
            .Replace(PlaceholderFormula, weekFormula);
    }

    /// <summary>
    /// Builds month number with year formula (e.g. "7-2026")
    /// </summary>
    public static string BuildArrayFormulaMonthNumber(string keyRange, string header, string dateRange)
    {
        var monthFormula = GoogleFormulas.MonthNumberWithYear.Replace(PlaceholderDateRange, dateRange);

        return GoogleFormulas.ArrayFormulaBase
            .Replace(PlaceholderKeyRange, keyRange)
            .Replace(PlaceholderHeader, header)
            .Replace(PlaceholderFormula, monthFormula);
    }

    /// <summary>
    /// Builds week begin date calculation from a year and ISO week number
    /// </summary>
    public static string BuildArrayFormulaWeekBeginDate(string keyRange, string header, string yearRange, string weekRange)
    {
        var beginFormula = GoogleFormulas.WeekBeginDate
            .Replace(PlaceholderYearRange, yearRange)
            .Replace(PlaceholderWeekRange, weekRange);

        return GoogleFormulas.ArrayFormulaBase
            .Replace(PlaceholderKeyRange, keyRange)
            .Replace(PlaceholderHeader, header)
            .Replace(PlaceholderFormula, beginFormula);
    }

    /// <summary>
    /// Builds week end date calculation from a year and ISO week number
    /// </summary>
    public static string BuildArrayFormulaWeekEndDate(string keyRange, string header, string yearRange, string weekRange)
    {
        var endFormula = GoogleFormulas.WeekEndDate
            .Replace(PlaceholderYearRange, yearRange)
            .Replace(PlaceholderWeekRange, weekRange);

        return GoogleFormulas.ArrayFormulaBase
            .Replace(PlaceholderKeyRange, keyRange)
            .Replace(PlaceholderHeader, header)
            .Replace(PlaceholderFormula, endFormula);
    }

    /// <summary>
    /// Builds a rolling (cumulative) average formula for time series analysis
    /// </summary>
    public static string BuildArrayFormulaRollingAverage(string keyRange, string header, string totalRange)
    {
        var averageFormula = GoogleFormulas.RollingAverageFormula
            .Replace(PlaceholderTotalRange, totalRange);

        return GoogleFormulas.ArrayFormulaBase
            .Replace(PlaceholderKeyRange, keyRange)
            .Replace(PlaceholderHeader, header)
            .Replace(PlaceholderFormula, averageFormula);
    }

    /// <summary>
    /// Builds string split by delimiter formula
    /// </summary>
    public static string BuildArrayFormulaSplit(string keyRange, string header, string sourceRange, string delimiter, int index)
    {
        // Delegate to the indexed split implementation to avoid duplicate logic
        return BuildArrayFormulaSplitByIndex(keyRange, header, sourceRange, delimiter, index);
    }

    /// <summary>
    /// Builds ARRAYFORMULA for day extraction from dates
    /// </summary>
    public static string BuildArrayFormulaDay(string keyRange, string header, string dateRange)
    {
        var dayFormula = $"DAY({dateRange})";
        return GoogleFormulas.ArrayFormulaBase
            .Replace(PlaceholderKeyRange, keyRange)
            .Replace(PlaceholderHeader, header)
            .Replace(PlaceholderFormula, dayFormula);
    }

    /// <summary>
    /// Builds ARRAYFORMULA for month extraction from dates
    /// </summary>
    public static string BuildArrayFormulaMonth(string keyRange, string header, string dateRange)
    {
        var monthFormula = $"MONTH({dateRange})";
        return GoogleFormulas.ArrayFormulaBase
            .Replace(PlaceholderKeyRange, keyRange)
            .Replace(PlaceholderHeader, header)
            .Replace(PlaceholderFormula, monthFormula);
    }

    /// <summary>
    /// Builds ARRAYFORMULA for year extraction from dates
    /// </summary>
    public static string BuildArrayFormulaYear(string keyRange, string header, string dateRange)
    {
        var yearFormula = $"YEAR({dateRange})";
        return GoogleFormulas.ArrayFormulaBase
            .Replace(PlaceholderKeyRange, keyRange)
            .Replace(PlaceholderHeader, header)
            .Replace(PlaceholderFormula, yearFormula);
    }

    /// <summary>
    /// Builds ARRAYFORMULA for weekday text extraction from dates
    /// </summary>
    public static string BuildArrayFormulaWeekdayText(string keyRange, string header, string dateRange, int offset = 0)
    {
        var offsetStr = offset.ToString();
        return GoogleFormulas.ArrayFormulaWeekdayText
            .Replace(PlaceholderKeyRange, keyRange)
            .Replace(PlaceholderHeader, header)
            .Replace(PlaceholderOffset, offsetStr);
    }

    /// <summary>
    /// Builds ARRAYFORMULA for dual count (counts occurrences in two ranges)
    /// </summary>
    public static string BuildArrayFormulaDualCountIf(string keyRange, string header, string range1, string range2)
    {
        var dualCountFormula = $"COUNTIF({range1},{keyRange})+COUNTIF({range2},{keyRange})";
        return GoogleFormulas.ArrayFormulaBase
            .Replace(PlaceholderKeyRange, keyRange)
            .Replace(PlaceholderHeader, header)
            .Replace(PlaceholderFormula, dualCountFormula);
    }

    /// <summary>
    /// Builds ARRAYFORMULA for splitting strings and extracting parts by index
    /// </summary>
    public static string BuildArrayFormulaSplitByIndex(string keyRange, string header, string sourceRange, string delimiter, int index)
    {
        var splitFormula = GoogleFormulas.SplitStringByIndex
            .Replace(PlaceholderSourceRange, sourceRange)
            .Replace(PlaceholderDelimiter, delimiter)
            .Replace(PlaceholderIndex, index.ToString());

        return GoogleFormulas.ArrayFormulaBase
            .Replace(PlaceholderKeyRange, keyRange)
            .Replace(PlaceholderHeader, header)
            .Replace(PlaceholderFormula, splitFormula);
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Generic method to replace multiple placeholders in any formula template
    /// </summary>
    public static string BuildCustomFormula(string formulaTemplate, params (string placeholder, string value)[] replacements)
    {
        var result = formulaTemplate;
        foreach (var (placeholder, value) in replacements)
        {
            result = result.Replace(placeholder, value);
        }
        return result;
    }

    /// <summary>
    /// Wraps any formula with ARRAYFORMULA base structure
    /// </summary>
    public static string WrapWithArrayFormula(string keyRange, string header, string innerFormula)
    {
        return GoogleFormulas.ArrayFormulaBase
            .Replace(PlaceholderKeyRange, keyRange)
            .Replace(PlaceholderHeader, header)
            .Replace(PlaceholderFormula, innerFormula);
    }

    /// <summary>
    /// Builds zero-safe division formula
    /// </summary>
    public static string BuildSafeDivision(string numerator, string denominator)
    {
        return GoogleFormulas.SafeDivisionFormula
            .Replace(PlaceholderNumerator, numerator)
            .Replace(PlaceholderDenominator, denominator);
    }

    /// <summary>
    /// Builds ARRAYFORMULA for weekday amount current (custom logic for current week aggregation)
    /// </summary>
    public static string BuildArrayFormulaWeekdayAmountCurrent(string keyRange, string dailyRange)
    {
        return $"=ARRAYFORMULA(IFS(ROW({keyRange})=1,\"Curr Amt\",ISBLANK({keyRange}), \"\", true, IFERROR(VLOOKUP(TODAY()-WEEKDAY(TODAY(),2)+{keyRange},{dailyRange},6,false),0)))";
    }

    /// <summary>
    /// Builds ARRAYFORMULA for weekday amount (current/previous) with offset and custom column title
    /// </summary>
    public static string BuildArrayFormulaWeekdayAmount(string keyRange, string dailyRange, int offset, string columnTitle)
    {
        return $"=ARRAYFORMULA(IFS(ROW({keyRange})=1,\"{columnTitle}\",ISBLANK({keyRange}), \"\", true, IFERROR(VLOOKUP(TODAY()-WEEKDAY(TODAY(),2)+{keyRange}+{offset},{dailyRange},6,false),0)))";
    }

    #endregion
}