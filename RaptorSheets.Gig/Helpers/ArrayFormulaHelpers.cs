using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Gig.Helpers;
using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Gig.Helpers;

/// <summary>
/// Helper class for generating array formulas used in Google Sheets
/// NOTE: Consider using GoogleFormulaBuilder (generic) or GigFormulaBuilder (gig-specific) for new implementations
/// </summary>
[ExcludeFromCodeCoverage]
public static class ArrayFormulaHelpers
{
    #region Backward Compatibility Methods (Legacy)

    /// <summary>
    /// Generates ARRAYFORMULA with COUNTIF aggregation
    /// </summary>
    [Obsolete("Consider using GoogleFormulaBuilder.BuildArrayFormulaCountIf for better maintainability")]
    public static string ArrayFormulaCountIf(string keyRange, string header, string range)
    {
        return GoogleFormulaBuilder.BuildArrayFormulaCountIf(keyRange, header, range);
    }

    /// <summary>
    /// Generates ARRAYFORMULA with SUMIF aggregation
    /// </summary>
    [Obsolete("Consider using GoogleFormulaBuilder.BuildArrayFormulaSumIf for better maintainability")]
    public static string ArrayFormulaSumIf(string keyRange, string header, string range, string sumRange)
    {
        return GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, header, range, sumRange);
    }

    /// <summary>
    /// Generates ARRAYFORMULA for unique values
    /// </summary>
    [Obsolete("Consider using GoogleFormulaBuilder.BuildArrayFormulaUnique for better maintainability")]
    public static string ArrayForumlaUnique(string range, string header)
    {
        // Default to using A:A as key range for backward compatibility
        return GoogleFormulaBuilder.BuildArrayFormulaUnique("$A:$A", header, range);
    }

    /// <summary>
    /// Generates ARRAYFORMULA for unique filtered values
    /// </summary>
    [Obsolete("Consider using GoogleFormulaBuilder.BuildArrayFormulaUniqueFiltered for better maintainability")]
    public static string ArrayForumlaUniqueFilter(string range, string header)
    {
        // Default to using A:A as key range for backward compatibility
        return GoogleFormulaBuilder.BuildArrayFormulaUniqueFiltered("$A:$A", header, range);
    }

    /// <summary>
    /// Generates ARRAYFORMULA for unique filtered and sorted values
    /// </summary>
    [Obsolete("Consider using GoogleFormulaBuilder.BuildArrayFormulaUniqueFiltered for better maintainability")]
    public static string ArrayForumlaUniqueFilterSort(string range, string header)
    {
        // This was essentially the same as UniqueFilter
        return ArrayForumlaUniqueFilter(range, header);
    }

    /// <summary>
    /// Generates ARRAYFORMULA for total calculation (pay + tips + bonus) - GIG SPECIFIC
    /// </summary>
    [Obsolete("Consider using GigFormulaBuilder.BuildArrayFormulaTotal for better maintainability")]
    public static string ArrayFormulaTotal(string keyRange, string header, string payRange, string tipsRange, string bonusRange)
    {
        return GigFormulaBuilder.BuildArrayFormulaTotal(keyRange, header, payRange, tipsRange, bonusRange);
    }

    #endregion

    #region Visit Formula Methods (Legacy - Gig Specific)

    /// <summary>
    /// Generates ARRAYFORMULA for visit date lookup (first or last) - GIG SPECIFIC
    /// </summary>
    [Obsolete("Consider using GigFormulaBuilder.Common.BuildVisitDateLookup for better maintainability")]
    public static string ArrayFormulaVisit(string keyRange, string header, string referenceSheet, string columnStart, string columnEnd, bool first)
    {
        return GigFormulaBuilder.Common.BuildVisitDateLookup(keyRange, header, referenceSheet, columnStart, columnEnd, first);
    }

    /// <summary>
    /// Generates ARRAYFORMULA for multiple field visit lookup - GIG SPECIFIC
    /// </summary>
    [Obsolete("Consider using GigFormulaBuilder.BuildArrayFormulaMultipleFieldVisit for better maintainability")]
    public static string ArrayFormulaMultipleVisit(string keyRange, string header, string referenceSheet, string dateColumn, string keyColumn1, string keyColumn2, bool first)
    {
        return GigFormulaBuilder.BuildArrayFormulaMultipleFieldVisit(keyRange, header, referenceSheet, dateColumn, keyColumn1, keyColumn2, first);
    }

    #endregion

    #region New Implementation Using Domain-Specific Builders

    /// <summary>
    /// Generates ARRAYFORMULA with amount per trip calculation using new gig formula builder
    /// </summary>
    public static string ArrayFormulaAmountPerTrip(string keyRange, string header, string totalRange, string tripsRange)
    {
        return GigFormulaBuilder.BuildArrayFormulaAmountPerTrip(keyRange, header, totalRange, tripsRange);
    }

    /// <summary>
    /// Generates ARRAYFORMULA with amount per distance calculation using new gig formula builder
    /// </summary>
    public static string ArrayFormulaAmountPerDistance(string keyRange, string header, string totalRange, string distanceRange)
    {
        return GigFormulaBuilder.BuildArrayFormulaAmountPerDistance(keyRange, header, totalRange, distanceRange);
    }

    /// <summary>
    /// Generates ARRAYFORMULA with amount per time calculation using new gig formula builder
    /// </summary>
    public static string ArrayFormulaAmountPerTime(string keyRange, string header, string totalRange, string timeRange)
    {
        return GigFormulaBuilder.BuildArrayFormulaAmountPerTime(keyRange, header, totalRange, timeRange);
    }

    #endregion
}