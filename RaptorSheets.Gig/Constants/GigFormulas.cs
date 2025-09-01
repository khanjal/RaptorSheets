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

    #region Gig-Specific Date Formulas

    /// <summary>
    /// Week number with year for gig tracking: WEEKNUM({dateRange},2)&"-"&YEAR({dateRange})
    /// Placeholders: {dateRange}
    /// </summary>
    public const string WeekNumberWithYear = "WEEKNUM({dateRange},2)&\"-\"&YEAR({dateRange})";

    /// <summary>
    /// Month number with year for gig tracking: MONTH({dateRange})&"-"&YEAR({dateRange})
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

    #endregion

    #region Shift-Specific Business Logic

    /// <summary>
    /// Shift key generation: IF(ISBLANK({numberRange}), {dateRange} & "-0-" & {serviceRange}, {dateRange} & "-" & {numberRange} & "-" & {serviceRange})
    /// Placeholders: {numberRange}, {dateRange}, {serviceRange}
    /// </summary>
    public const string ShiftKeyGeneration = "IF(ISBLANK({numberRange}), {dateRange} & \"-0-\" & {serviceRange}, {dateRange} & \"-\" & {numberRange} & \"-\" & {serviceRange})";

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

    #region Mapper-Specific Formulas

    /// <summary>
    /// Complex rolling average for time series analysis: DAVERAGE(transpose({{totalRange},TRANSPOSE(if(ROW({totalRange}) <= TRANSPOSE(ROW({totalRange})),{totalRange},))}),sequence(rows({totalRange}),1),{if(,,);if(,,)})
    /// Placeholders: {totalRange}
    /// </summary>
    public const string RollingAverageFormula = "DAVERAGE(transpose({{{totalRange},TRANSPOSE(if(ROW({totalRange}) <= TRANSPOSE(ROW({totalRange})),{totalRange},))}),sequence(rows({totalRange}),1),{if(,,);if(,,)})";

    #endregion
}