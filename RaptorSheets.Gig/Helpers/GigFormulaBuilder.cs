using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Gig.Constants;
using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Gig.Helpers;

/// <summary>
/// Helper class for building gig-specific Google Sheets formulas
/// Combines generic GoogleFormulaBuilder with gig business logic
/// </summary>
[ExcludeFromCodeCoverage]
public static class GigFormulaBuilder
{
    #region Gig Financial Formula Builders

    /// <summary>
    /// Builds ARRAYFORMULA with total income calculation (pay + tips + bonus)
    /// </summary>
    public static string BuildArrayFormulaTotal(string keyRange, string header, string payRange, string tipsRange, string bonusRange)
    {
        var totalFormula = GigFormulas.TotalIncomeFormula
            .Replace("{payRange}", payRange)
            .Replace("{tipsRange}", tipsRange)
            .Replace("{bonusRange}", bonusRange);

        return GoogleFormulas.ArrayFormulaBase
            .Replace("{keyRange}", keyRange)
            .Replace("{header}", header)
            .Replace("{formula}", totalFormula);
    }

    /// <summary>
    /// Builds ARRAYFORMULA with amount per trip calculation
    /// </summary>
    public static string BuildArrayFormulaAmountPerTrip(string keyRange, string header, string totalRange, string tripsRange)
    {
        var amountPerTripFormula = GigFormulas.AmountPerTripFormula
            .Replace("{totalRange}", totalRange)
            .Replace("{tripsRange}", tripsRange);

        return GoogleFormulas.ArrayFormulaBase
            .Replace("{keyRange}", keyRange)
            .Replace("{header}", header)
            .Replace("{formula}", amountPerTripFormula);
    }

    /// <summary>
    /// Builds ARRAYFORMULA with amount per distance calculation
    /// </summary>
    public static string BuildArrayFormulaAmountPerDistance(string keyRange, string header, string totalRange, string distanceRange)
    {
        var amountPerDistanceFormula = GigFormulas.AmountPerDistanceFormula
            .Replace("{totalRange}", totalRange)
            .Replace("{distanceRange}", distanceRange);

        return GoogleFormulas.ArrayFormulaBase
            .Replace("{keyRange}", keyRange)
            .Replace("{header}", header)
            .Replace("{formula}", amountPerDistanceFormula);
    }

    /// <summary>
    /// Builds ARRAYFORMULA with amount per time calculation (hourly rate)
    /// </summary>
    public static string BuildArrayFormulaAmountPerTime(string keyRange, string header, string totalRange, string timeRange)
    {
        var amountPerTimeFormula = GigFormulas.AmountPerTimeFormula
            .Replace("{totalRange}", totalRange)
            .Replace("{timeRange}", timeRange);

        return GoogleFormulas.ArrayFormulaBase
            .Replace("{keyRange}", keyRange)
            .Replace("{header}", header)
            .Replace("{formula}", amountPerTimeFormula);
    }

    /// <summary>
    /// Builds ARRAYFORMULA with amount per day calculation
    /// </summary>
    public static string BuildArrayFormulaAmountPerDay(string keyRange, string header, string totalRange, string daysRange)
    {
        var amountPerDayFormula = GigFormulas.AmountPerDayFormula
            .Replace("{totalRange}", totalRange)
            .Replace("{daysRange}", daysRange);

        return GoogleFormulas.ArrayFormulaBase
            .Replace("{keyRange}", keyRange)
            .Replace("{header}", header)
            .Replace("{formula}", amountPerDayFormula);
    }

    #endregion

    #region Gig Date/Time Formula Builders

    /// <summary>
    /// Builds week number with year formula for gig tracking
    /// </summary>
    public static string BuildArrayFormulaWeekNumber(string keyRange, string header, string dateRange)
    {
        var weekFormula = GigFormulas.WeekNumberWithYear.Replace("{dateRange}", dateRange);

        return GoogleFormulas.ArrayFormulaBase
            .Replace("{keyRange}", keyRange)
            .Replace("{header}", header)
            .Replace("{formula}", weekFormula);
    }

    /// <summary>
    /// Builds month number with year formula for gig tracking
    /// </summary>
    public static string BuildArrayFormulaMonthNumber(string keyRange, string header, string dateRange)
    {
        var monthFormula = GigFormulas.MonthNumberWithYear.Replace("{dateRange}", dateRange);

        return GoogleFormulas.ArrayFormulaBase
            .Replace("{keyRange}", keyRange)
            .Replace("{header}", header)
            .Replace("{formula}", monthFormula);
    }

    /// <summary>
    /// Builds week begin date calculation
    /// </summary>
    public static string BuildArrayFormulaWeekBegin(string keyRange, string header, string yearRange, string weekRange)
    {
        var beginFormula = GigFormulas.WeekBeginDate
            .Replace("{yearRange}", yearRange)
            .Replace("{weekRange}", weekRange);

        return GoogleFormulas.ArrayFormulaBase
            .Replace("{keyRange}", keyRange)
            .Replace("{header}", header)
            .Replace("{formula}", beginFormula);
    }

    /// <summary>
    /// Builds week end date calculation
    /// </summary>
    public static string BuildArrayFormulaWeekEnd(string keyRange, string header, string yearRange, string weekRange)
    {
        var endFormula = GigFormulas.WeekEndDate
            .Replace("{yearRange}", yearRange)
            .Replace("{weekRange}", weekRange);

        return GoogleFormulas.ArrayFormulaBase
            .Replace("{keyRange}", keyRange)
            .Replace("{header}", header)
            .Replace("{formula}", endFormula);
    }

    #endregion

    #region Gig Business Logic Formula Builders

    /// <summary>
    /// Builds current amount lookup for weekday analysis
    /// </summary>
    public static string BuildArrayFormulaCurrentAmount(string keyRange, string header, string dayRange, string dailySheet, string dateColumn, string totalColumn, string totalIndex)
    {
        var currentAmountFormula = GigFormulas.CurrentAmountLookup
            .Replace("{dayRange}", dayRange)
            .Replace("{dailySheet}", dailySheet)
            .Replace("{dateColumn}", dateColumn)
            .Replace("{totalColumn}", totalColumn)
            .Replace("{totalIndex}", totalIndex);

        return GoogleFormulas.ArrayFormulaBase
            .Replace("{keyRange}", keyRange)
            .Replace("{header}", header)
            .Replace("{formula}", currentAmountFormula);
    }

    /// <summary>
    /// Builds previous amount lookup for weekday analysis
    /// </summary>
    public static string BuildArrayFormulaPreviousAmount(string keyRange, string header, string dayRange, string dailySheet, string dateColumn, string totalColumn, string totalIndex)
    {
        var previousAmountFormula = GigFormulas.PreviousAmountLookup
            .Replace("{dayRange}", dayRange)
            .Replace("{dailySheet}", dailySheet)
            .Replace("{dateColumn}", dateColumn)
            .Replace("{totalColumn}", totalColumn)
            .Replace("{totalIndex}", totalIndex);

        return GoogleFormulas.ArrayFormulaBase
            .Replace("{keyRange}", keyRange)
            .Replace("{header}", header)
            .Replace("{formula}", previousAmountFormula);
    }

    /// <summary>
    /// Builds previous day average calculation
    /// </summary>
    public static string BuildArrayFormulaPreviousDayAverage(string keyRange, string header, string totalRange, string previousRange, string daysRange)
    {
        var averageFormula = GigFormulas.PreviousDayAverage
            .Replace("{totalRange}", totalRange)
            .Replace("{previousRange}", previousRange)
            .Replace("{daysRange}", daysRange);

        return GoogleFormulas.ArrayFormulaBase
            .Replace("{keyRange}", keyRange)
            .Replace("{header}", header)
            .Replace("{formula}", averageFormula);
    }

    /// <summary>
    /// Builds multiple field visit lookup for address tracking
    /// </summary>
    public static string BuildArrayFormulaMultipleFieldVisit(string keyRange, string header, string sourceSheet, string dateColumn, string keyColumn1, string keyColumn2, bool isFirst)
    {
        var functionName = isFirst ? "MIN" : "MAX";
        var multipleVisitFormula = GigFormulas.MultipleFieldVisitLookup
            .Replace("MIN", functionName)
            .Replace("{keyRange}", keyRange)
            .Replace("{sourceSheet}", sourceSheet)
            .Replace("{keyColumn1}", keyColumn1)
            .Replace("{keyColumn2}", keyColumn2)
            .Replace("{dateColumn}", dateColumn);

        return GoogleFormulas.ArrayFormulaBase
            .Replace("{keyRange}", keyRange)
            .Replace("{header}", header)
            .Replace("{formula}", multipleVisitFormula);
    }

    #endregion

    #region Convenient Combo Methods

    /// <summary>
    /// Builds complete gig summary formulas (wraps generic builders with gig logic)
    /// </summary>
    public static class Common
    {
        /// <summary>
        /// Builds sum aggregation using generic builder
        /// </summary>
        public static string BuildSumAggregation(string keyRange, string header, string lookupRange, string sumRange)
        {
            return GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, header, lookupRange, sumRange);
        }

        /// <summary>
        /// Builds count aggregation using generic builder
        /// </summary>
        public static string BuildCountAggregation(string keyRange, string header, string lookupRange)
        {
            return GoogleFormulaBuilder.BuildArrayFormulaCountIf(keyRange, header, lookupRange);
        }

        /// <summary>
        /// Builds visit date lookup using generic sorted lookup
        /// </summary>
        public static string BuildVisitDateLookup(string keyRange, string header, string sourceSheet, string dateColumn, string keyColumn, bool isFirst)
        {
            return GoogleFormulaBuilder.BuildArrayFormulaSortedLookup(keyRange, header, sourceSheet, dateColumn, keyColumn, isFirst);
        }

        /// <summary>
        /// Builds multiple field visit lookup for address tracking (checks multiple fields for matching)
        /// </summary>
        public static string BuildMultipleFieldVisitLookup(string keyRange, string header, string sourceSheet, string dateColumn, string keyColumn1, string keyColumn2, string dateIndex, bool isFirst)
        {
            var functionName = isFirst ? "MIN" : "MAX";
            var multipleVisitFormula = GigFormulas.MultipleFieldVisitLookup
                .Replace("MIN", functionName)
                .Replace("{keyRange}", keyRange)
                .Replace("{sourceSheet}", sourceSheet)
                .Replace("{keyColumn1}", keyColumn1)
                .Replace("{keyColumn2}", keyColumn2)
                .Replace("{dateColumn}", dateColumn);

            return GoogleFormulas.ArrayFormulaBase
                .Replace("{keyRange}", keyRange)
                .Replace("{header}", header)
                .Replace("{formula}", multipleVisitFormula);
        }
    }

    #endregion

    #region Shift-Specific Business Logic Builders

    /// <summary>
    /// Builds shift key generation formula
    /// </summary>
    public static string BuildArrayFormulaShiftKey(string keyRange, string header, string dateRange, string serviceRange, string numberRange)
    {
        // The GigFormulas.ShiftKeyGeneration already includes ARRAYFORMULA, so we don't wrap it
        return "=ARRAYFORMULA(" + GigFormulas.ShiftKeyGeneration
            .Replace("{keyRange}", keyRange)
            .Replace("{header}", header)
            .Replace("{dateRange}", dateRange)
            .Replace("{serviceRange}", serviceRange)
            .Replace("{numberRange}", numberRange) + ")";
    }

    /// <summary>
    /// Builds trip key generation formula with exclude logic
    /// </summary>
    public static string BuildArrayFormulaTripKey(string keyRange, string header, string dateRange, string serviceRange, string numberRange, string excludeRange)
    {
        var keyFormula = GigFormulas.TripKeyGeneration
            .Replace("{dateRange}", dateRange)
            .Replace("{serviceRange}", serviceRange)
            .Replace("{numberRange}", numberRange)
            .Replace("{excludeRange}", excludeRange);

        return GoogleFormulas.ArrayFormulaBase
            .Replace("{keyRange}", keyRange)
            .Replace("{header}", header)
            .Replace("{formula}", keyFormula);
    }

    /// <summary>
    /// Builds total time active with trip fallback
    /// </summary>
    public static string BuildArrayFormulaTotalTimeActive(string keyRange, string header, string activeTimeRange, string tripKeyRange, string shiftKeyRange, string tripDurationRange)
    {
        var timeActiveFormula = GigFormulas.TotalTimeActiveWithFallback
            .Replace("{activeTimeRange}", activeTimeRange)
            .Replace("{tripKeyRange}", tripKeyRange)
            .Replace("{shiftKeyRange}", shiftKeyRange)
            .Replace("{tripDurationRange}", tripDurationRange);

        return GoogleFormulas.ArrayFormulaBase
            .Replace("{keyRange}", keyRange)
            .Replace("{header}", header)
            .Replace("{formula}", timeActiveFormula);
    }

    /// <summary>
    /// Builds total time with omit logic
    /// </summary>
    public static string BuildArrayFormulaTotalTimeWithOmit(string keyRange, string header, string omitRange, string totalTimeRange, string totalActiveRange)
    {
        var totalTimeFormula = GigFormulas.TotalTimeWithOmit
            .Replace("{omitRange}", omitRange)
            .Replace("{totalTimeRange}", totalTimeRange)
            .Replace("{totalActiveRange}", totalActiveRange);

        return GoogleFormulas.ArrayFormulaBase
            .Replace("{keyRange}", keyRange)
            .Replace("{header}", header)
            .Replace("{formula}", totalTimeFormula);
    }

    /// <summary>
    /// Builds shift total with trip sum pattern
    /// </summary>
    public static string BuildArrayFormulaShiftTotalWithTripSum(string keyRange, string header, string localRange, string tripKeyRange, string shiftKeyRange, string tripSumRange)
    {
        var shiftTotalFormula = GigFormulas.ShiftTotalWithTripSum
            .Replace("{localRange}", localRange)
            .Replace("{tripKeyRange}", tripKeyRange)
            .Replace("{shiftKeyRange}", shiftKeyRange)
            .Replace("{tripSumRange}", tripSumRange);

        return GoogleFormulas.ArrayFormulaBase
            .Replace("{keyRange}", keyRange)
            .Replace("{header}", header)
            .Replace("{formula}", shiftTotalFormula);
    }

    /// <summary>
    /// Builds shift total trips pattern
    /// </summary>
    public static string BuildArrayFormulaShiftTotalTrips(string keyRange, string header, string localTripsRange, string tripKeyRange, string shiftKeyRange)
    {
        var totalTripsFormula = GigFormulas.ShiftTotalTrips
            .Replace("{localTripsRange}", localTripsRange)
            .Replace("{tripKeyRange}", tripKeyRange)
            .Replace("{shiftKeyRange}", shiftKeyRange);

        return GoogleFormulas.ArrayFormulaBase
            .Replace("{keyRange}", keyRange)
            .Replace("{header}", header)
            .Replace("{formula}", totalTripsFormula);
    }

    #endregion

    #region Advanced Time Series Formula Builders

    /// <summary>
    /// Builds rolling average formula for time series analysis
    /// </summary>
    public static string BuildArrayFormulaRollingAverage(string keyRange, string header, string totalRange)
    {
        var averageFormula = GigFormulas.RollingAverageFormula
            .Replace("{totalRange}", totalRange);

        return GoogleFormulas.ArrayFormulaBase
            .Replace("{keyRange}", keyRange)
            .Replace("{header}", header)
            .Replace("{formula}", averageFormula);
    }

    /// <summary>
    /// Builds week begin date calculation from week number
    /// </summary>
    public static string BuildArrayFormulaWeekBeginDate(string keyRange, string header, string weekRange)
    {
        var beginFormula = GigFormulas.WeekBeginDate
            .Replace("{weekRange}", weekRange);

        return GoogleFormulas.ArrayFormulaBase
            .Replace("{keyRange}", keyRange)
            .Replace("{header}", header)
            .Replace("{formula}", beginFormula);
    }

    /// <summary>
    /// Builds week end date calculation from week number
    /// </summary>
    public static string BuildArrayFormulaWeekEndDate(string keyRange, string header, string weekRange)
    {
        var endFormula = GigFormulas.WeekEndDate
            .Replace("{weekRange}", weekRange);

        return GoogleFormulas.ArrayFormulaBase
            .Replace("{keyRange}", keyRange)
            .Replace("{header}", header)
            .Replace("{formula}", endFormula);
    }

    #endregion
}