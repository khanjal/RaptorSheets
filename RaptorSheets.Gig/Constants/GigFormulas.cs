using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Gig.Constants;

/// <summary>
/// Gig-specific Google Sheets formula templates for business logic calculations
/// These formulas contain domain-specific logic for gig work tracking
/// </summary>
[ExcludeFromCodeCoverage]
public static class GigFormulas
{
    #region Financial Calculation Formulas

    /// <summary>
    /// Total income calculation (pay + tips + bonus): {payRange}+{tipsRange}+{bonusRange}
    /// Placeholders: {payRange}, {tipsRange}, {bonusRange}
    /// </summary>
    public const string TotalIncomeFormula = "{payRange}+{tipsRange}+{bonusRange}";

    /// <summary>
    /// Amount per trip calculation: {totalRange}/IF({tripsRange}=0,1,{tripsRange})
    /// Placeholders: {totalRange}, {tripsRange}
    /// </summary>
    public const string AmountPerTripFormula = "{totalRange}/IF({tripsRange}=0,1,{tripsRange})";

    /// <summary>
    /// Amount per distance calculation: {totalRange}/IF({distanceRange}=0,1,{distanceRange})
    /// Placeholders: {totalRange}, {distanceRange}
    /// </summary>
    public const string AmountPerDistanceFormula = "{totalRange}/IF({distanceRange}=0,1,{distanceRange})";

    /// <summary>
    /// Amount per time calculation (hourly rate): {totalRange}/IF({timeRange}=0,1,{timeRange}*24)
    /// Placeholders: {totalRange}, {timeRange}
    /// </summary>
    public const string AmountPerTimeFormula = "{totalRange}/IF({timeRange}=0,1,{timeRange}*24)";

    /// <summary>
    /// Amount per day calculation: {totalRange}/IF({daysRange}=0,1,{daysRange})
    /// Placeholders: {totalRange}, {daysRange}
    /// </summary>
    public const string AmountPerDayFormula = "{totalRange}/IF({daysRange}=0,1,{daysRange})";

    #endregion

    #region Gig-Specific Lookup Formulas

    /// <summary>
    /// Current amount lookup for weekday analysis: IFERROR(VLOOKUP(TODAY()-WEEKDAY(TODAY(),2)+{dayRange},{dailySheet}!{dateColumn}:{totalColumn},{totalIndex}+1,false),0)
    /// Placeholders: {dayRange}, {dailySheet}, {dateColumn}, {totalColumn}, {totalIndex}
    /// </summary>
    public const string CurrentAmountLookup = "IFERROR(VLOOKUP(TODAY()-WEEKDAY(TODAY(),2)+{dayRange},{dailySheet}!{dateColumn}:{totalColumn},{totalIndex}+1,false),0)";

    /// <summary>
    /// Previous amount lookup for weekday analysis: IFERROR(VLOOKUP(TODAY()-WEEKDAY(TODAY(),2)+{dayRange}-7,{dailySheet}!{dateColumn}:{totalColumn},{totalIndex}+1,false),0)
    /// Placeholders: {dayRange}, {dailySheet}, {dateColumn}, {totalColumn}, {totalIndex}
    /// </summary>
    public const string PreviousAmountLookup = "IFERROR(VLOOKUP(TODAY()-WEEKDAY(TODAY(),2)+{dayRange}-7,{dailySheet}!{dateColumn}:{totalColumn},{totalIndex}+1,false),0)";

    /// <summary>
    /// Previous day average calculation: ({totalRange}-{previousRange})/IF({daysRange}=0,1,{daysRange}-IF({previousRange}=0,0,-1))
    /// Placeholders: {totalRange}, {previousRange}, {daysRange}
    /// </summary>
    public const string PreviousDayAverage = "({totalRange}-{previousRange})/IF({daysRange}=0,1,{daysRange}-IF({previousRange}=0,0,-1))";

    /// <summary>
    /// Multiple field visit lookup for addresses: IFERROR(MIN(IF({sourceSheet}!{keyColumn1}:{keyColumn1}={keyRange},IF({sourceSheet}!{keyColumn2}:{keyColumn2}={keyRange},{sourceSheet}!{dateColumn}:{dateColumn}))),\"\")
    /// Placeholders: {keyRange}, {sourceSheet}, {keyColumn1}, {keyColumn2}, {dateColumn}
    /// </summary>
    public const string MultipleFieldVisitLookup = "IFERROR(MIN(IF({sourceSheet}!{keyColumn1}:{keyColumn1}={keyRange},IF({sourceSheet}!{keyColumn2}:{keyColumn2}={keyRange},{sourceSheet}!{dateColumn}:{dateColumn}))),\"\")";

    /// <summary>
    /// Dual field visit lookup using VLOOKUP with QUERY and SORT for addresses
    /// Formula: IFERROR(VLOOKUP({keyRange},SORT(QUERY({sourceSheet}!A:{keyColumn1Letter},"SELECT {keyColumn1Letter}, A"),{sortColumn},{sortOrder}),2,0),IFERROR(VLOOKUP({keyRange},SORT(QUERY({sourceSheet}!A:{keyColumn2Letter},"SELECT {keyColumn2Letter}, A"),{sortColumn},{sortOrder}),2,0),""))
    /// Placeholders: {keyRange}, {sourceSheet}, {keyColumn1Letter}, {keyColumn2Letter}, {sortColumn}, {sortOrder}
    /// Note: sortColumn is typically 1 (sort by key) for first visit or 2 (sort by date) for last visit
    /// </summary>
    public const string DualFieldVisitLookup = "IFERROR(VLOOKUP({keyRange},SORT(QUERY({sourceSheet}!A:{keyColumn1Letter},\"SELECT {keyColumn1Letter}, A\"),{sortColumn},{sortOrder}),2,0),IFERROR(VLOOKUP({keyRange},SORT(QUERY({sourceSheet}!A:{keyColumn2Letter},\"SELECT {keyColumn2Letter}, A\"),{sortColumn},{sortOrder}),2,0),\"\"))";

    #endregion

    #region Shift-Specific Business Logic

    /// <summary>
    /// Shift key generation: IFS(ROW({keyRange})=1,"{header}",ISBLANK({serviceRange}), "",true,IF(ISBLANK({numberRange}), {dateRange} & "-0-" & {serviceRange}, {dateRange} & "-" & {numberRange} & "-" & {serviceRange}))
    /// Placeholders: {keyRange}, {header}, {serviceRange}, {numberRange}, {dateRange}
    /// </summary>
    public const string ShiftKeyGeneration = "IFS(ROW({keyRange})=1,\"{header}\",ISBLANK({serviceRange}), \"\",true,IF(ISBLANK({numberRange}), {dateRange} & \"-0-\" & {serviceRange}, {dateRange} & \"-\" & {numberRange} & \"-\" & {serviceRange}))";

    /// <summary>
    /// Trip key generation with exclude: IF({excludeRange},{dateRange} & "-X-" & {serviceRange},IF(ISBLANK({numberRange}), {dateRange} & "-0-" & {serviceRange}, {dateRange} & "-" & {numberRange} & "-" & {serviceRange}))
    /// Placeholders: {excludeRange}, {dateRange}, {serviceRange}, {numberRange}
    /// </summary>
    public const string TripKeyGeneration = "IF({excludeRange},{dateRange} & \"-X-\" & {serviceRange},IF(ISBLANK({numberRange}), {dateRange} & \"-0-\" & {serviceRange}, {dateRange} & \"-\" & {numberRange} & \"-\" & {serviceRange}))";

    /// <summary>
    /// Total time active with fallback: IF(ISBLANK({activeTimeRange}),SUMIF({tripKeyRange},{shiftKeyRange},{tripDurationRange}),{activeTimeRange})
    /// Placeholders: {activeTimeRange}, {tripKeyRange}, {shiftKeyRange}, {tripDurationRange}
    /// </summary>
    public const string TotalTimeActiveWithFallback = "IF(ISBLANK({activeTimeRange}),SUMIF({tripKeyRange},{shiftKeyRange},{tripDurationRange}),{activeTimeRange})";

    /// <summary>
    /// Total time with omit logic: IF({omitRange}=false,IF(ISBLANK({totalTimeRange}),{totalActiveRange},{totalTimeRange}),0)
    /// Placeholders: {omitRange}, {totalTimeRange}, {totalActiveRange}
    /// </summary>
    public const string TotalTimeWithOmit = "IF({omitRange}=false,IF(ISBLANK({totalTimeRange}),{totalActiveRange},{totalTimeRange}),0)";

    /// <summary>
    /// Shift total addition pattern: {localRange} + SUMIF({tripKeyRange},{shiftKeyRange},{tripSumRange})
    /// Placeholders: {localRange}, {tripKeyRange}, {shiftKeyRange}, {tripSumRange}
    /// </summary>
    public const string ShiftTotalWithTripSum = "{localRange} + SUMIF({tripKeyRange},{shiftKeyRange},{tripSumRange})";

    /// <summary>
    /// Shift total trips pattern: {localTripsRange} + COUNTIF({tripKeyRange},{shiftKeyRange})
    /// Placeholders: {localTripsRange}, {tripKeyRange}, {shiftKeyRange}
    /// </summary>
    public const string ShiftTotalTrips = "{localTripsRange} + COUNTIF({tripKeyRange},{shiftKeyRange})";

    #endregion
}