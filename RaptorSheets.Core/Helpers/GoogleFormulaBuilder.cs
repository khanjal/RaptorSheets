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
    #region Generic ARRAYFORMULA Builders

    /// <summary>
    /// Builds a complete ARRAYFORMULA with SUMIF aggregation
    /// </summary>
    public static string BuildArrayFormulaSumIf(string keyRange, string header, string lookupRange, string sumRange)
    {
        var sumIfFormula = GoogleFormulas.SumIfAggregation
            .Replace("{lookupRange}", lookupRange)
            .Replace("{keyRange}", keyRange)
            .Replace("{sumRange}", sumRange);

        return GoogleFormulas.ArrayFormulaBase
            .Replace("{keyRange}", keyRange)
            .Replace("{header}", header)
            .Replace("{formula}", sumIfFormula);
    }

    /// <summary>
    /// Builds a complete ARRAYFORMULA with COUNTIF aggregation
    /// </summary>
    public static string BuildArrayFormulaCountIf(string keyRange, string header, string lookupRange)
    {
        var countIfFormula = GoogleFormulas.CountIfAggregation
            .Replace("{lookupRange}", lookupRange)
            .Replace("{keyRange}", keyRange);

        return GoogleFormulas.ArrayFormulaBase
            .Replace("{keyRange}", keyRange)
            .Replace("{header}", header)
            .Replace("{formula}", countIfFormula);
    }

    /// <summary>
    /// Builds a complete ARRAYFORMULA for unique values
    /// </summary>
    public static string BuildArrayFormulaUnique(string keyRange, string header, string sourceRange)
    {
        return GoogleFormulas.ArrayFormulaUnique
            .Replace("{keyRange}", keyRange)
            .Replace("{header}", header)
            .Replace("{sourceRange}", sourceRange);
    }

    /// <summary>
    /// Builds a complete ARRAYFORMULA for unique filtered values
    /// </summary>
    public static string BuildArrayFormulaUniqueFiltered(string keyRange, string header, string sourceRange)
    {
        return GoogleFormulas.ArrayFormulaUniqueFiltered
            .Replace("{keyRange}", keyRange)
            .Replace("{header}", header)
            .Replace("{sourceRange}", sourceRange);
    }

    /// <summary>
    /// Builds ARRAYFORMULA with safe division (zero protection)
    /// </summary>
    public static string BuildArrayFormulaSafeDivision(string keyRange, string header, string numerator, string denominator)
    {
        var divisionFormula = GoogleFormulas.SafeDivisionFormula
            .Replace("{numerator}", numerator)
            .Replace("{denominator}", denominator);

        return GoogleFormulas.ArrayFormulaBase
            .Replace("{keyRange}", keyRange)
            .Replace("{header}", header)
            .Replace("{formula}", divisionFormula);
    }

    #endregion

    #region Generic Lookup Formula Builders

    /// <summary>
    /// Builds ARRAYFORMULA for sorted lookup (first or last value)
    /// </summary>
    public static string BuildArrayFormulaSortedLookup(string keyRange, string header, string sourceSheet, string dateColumn, string keyColumn, bool isFirst)
    {
        var lookupFormula = GoogleFormulas.SortedVLookup
            .Replace("{keyRange}", keyRange)
            .Replace("{sourceSheet}", sourceSheet)
            .Replace("{dateColumn}", dateColumn)
            .Replace("{keyColumn}", keyColumn)
            .Replace("{isFirst}", isFirst.ToString().ToLower());

        return GoogleFormulas.ArrayFormulaBase
            .Replace("{keyRange}", keyRange)
            .Replace("{header}", header)
            .Replace("{formula}", lookupFormula);
    }

    /// <summary>
    /// Builds safe VLOOKUP with error handling
    /// </summary>
    public static string BuildSafeVLookup(string searchKey, string searchRange, string columnIndex)
    {
        return GoogleFormulas.SafeVLookup
            .Replace("{searchKey}", searchKey)
            .Replace("{searchRange}", searchRange)
            .Replace("{columnIndex}", columnIndex);
    }

    #endregion

    #region Generic Date/Time Formula Builders

    /// <summary>
    /// Builds weekday formula
    /// </summary>
    public static string BuildArrayFormulaWeekday(string keyRange, string header, string dateRange)
    {
        var weekdayFormula = GoogleFormulas.WeekdayNumber.Replace("{dateRange}", dateRange);

        return GoogleFormulas.ArrayFormulaBase
            .Replace("{keyRange}", keyRange)
            .Replace("{header}", header)
            .Replace("{formula}", weekdayFormula);
    }

    /// <summary>
    /// Builds string split by delimiter formula
    /// </summary>
    public static string BuildArrayFormulaSplit(string keyRange, string header, string sourceRange, string delimiter, int index)
    {
        var splitFormula = GoogleFormulas.SplitStringByIndex
            .Replace("{sourceRange}", sourceRange)
            .Replace("{delimiter}", delimiter)
            .Replace("{index}", index.ToString());

        return GoogleFormulas.ArrayFormulaBase
            .Replace("{keyRange}", keyRange)
            .Replace("{header}", header)
            .Replace("{formula}", splitFormula);
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
            .Replace("{keyRange}", keyRange)
            .Replace("{header}", header)
            .Replace("{formula}", innerFormula);
    }

    /// <summary>
    /// Builds zero-safe division formula
    /// </summary>
    public static string BuildSafeDivision(string numerator, string denominator)
    {
        return GoogleFormulas.SafeDivisionFormula
            .Replace("{numerator}", numerator)
            .Replace("{denominator}", denominator);
    }

    #endregion
}