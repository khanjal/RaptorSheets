using System.ComponentModel;
using RaptorSheets.Gig.Constants;

namespace RaptorSheets.Gig.Enums;

/// <summary>
/// Compatibility wrapper around SheetsConfig.HeaderNames constants.
/// This allows existing code to continue working while we migrate to the new constants approach.
/// </summary>
[Obsolete("Use SheetsConfig.HeaderNames constants directly instead of HeaderEnum.FIELD.GetDescription()")]
public enum HeaderEnum
{
    [Description(SheetsConfig.HeaderNames.Address)]
    ADDRESS,

    [Description(SheetsConfig.HeaderNames.AddressStart)]
    ADDRESS_START,

    [Description(SheetsConfig.HeaderNames.AddressEnd)]
    ADDRESS_END,

    [Description(SheetsConfig.HeaderNames.Amount)]
    AMOUNT,

    [Description(SheetsConfig.HeaderNames.AmountCurrent)]
    AMOUNT_CURRENT,

    [Description(SheetsConfig.HeaderNames.AmountPrevious)]
    AMOUNT_PREVIOUS,

    [Description(SheetsConfig.HeaderNames.AmountPerDay)]
    AMOUNT_PER_DAY,

    [Description(SheetsConfig.HeaderNames.AmountPerDistance)]
    AMOUNT_PER_DISTANCE,

    [Description(SheetsConfig.HeaderNames.AmountPerPreviousDay)]
    AMOUNT_PER_PREVIOUS_DAY,

    [Description(SheetsConfig.HeaderNames.AmountPerTime)]
    AMOUNT_PER_TIME,

    [Description(SheetsConfig.HeaderNames.AmountPerTrip)]
    AMOUNT_PER_TRIP,

    [Description(SheetsConfig.HeaderNames.Average)]
    AVERAGE,

    [Description(SheetsConfig.HeaderNames.Bonus)]
    BONUS,

    [Description(SheetsConfig.HeaderNames.Cash)]
    CASH,

    [Description(SheetsConfig.HeaderNames.Category)]
    CATEGORY,

    [Description(SheetsConfig.HeaderNames.Date)]
    DATE,

    [Description(SheetsConfig.HeaderNames.DateBegin)]
    DATE_BEGIN,

    [Description(SheetsConfig.HeaderNames.DateEnd)]
    DATE_END,

    [Description(SheetsConfig.HeaderNames.Day)]
    DAY,

    [Description(SheetsConfig.HeaderNames.Days)]
    DAYS,

    [Description("D/V")]
    DAYS_PER_VISIT,

    [Description("Since")]
    DAYS_SINCE_VISIT,

    [Description(SheetsConfig.HeaderNames.Description)]
    DESCRIPTION,

    [Description(SheetsConfig.HeaderNames.Distance)]
    DISTANCE,

    [Description(SheetsConfig.HeaderNames.Dropoff)]
    DROPOFF,

    [Description(SheetsConfig.HeaderNames.Duration)]
    DURATION,

    [Description(SheetsConfig.HeaderNames.Exclude)]
    EXCLUDE,

    [Description(SheetsConfig.HeaderNames.Key)]
    KEY,

    [Description(SheetsConfig.HeaderNames.Month)]
    MONTH,

    [Description(SheetsConfig.HeaderNames.Name)]
    NAME,

    [Description(SheetsConfig.HeaderNames.Note)]
    NOTE,

    [Description(SheetsConfig.HeaderNames.Number)]
    NUMBER,

    [Description("# Days")]
    NUMBER_OF_DAYS,

    [Description(SheetsConfig.HeaderNames.OdometerEnd)]
    ODOMETER_END,

    [Description(SheetsConfig.HeaderNames.OdometerStart)]
    ODOMETER_START,

    [Description(SheetsConfig.HeaderNames.OrderNumber)]
    ORDER_NUMBER,

    [Description(SheetsConfig.HeaderNames.Pay)]
    PAY,

    [Description(SheetsConfig.HeaderNames.Pickup)]
    PICKUP,

    [Description(SheetsConfig.HeaderNames.Place)]
    PLACE,

    [Description(SheetsConfig.HeaderNames.Region)]
    REGION,

    [Description(SheetsConfig.HeaderNames.Service)]
    SERVICE,

    [Description("Tax Deductible")]
    TAX_DEDUCTIBLE,

    [Description(SheetsConfig.HeaderNames.TimeActive)]
    TIME_ACTIVE,

    [Description(SheetsConfig.HeaderNames.TimeEnd)]
    TIME_END,

    [Description(SheetsConfig.HeaderNames.TimeOmit)]
    TIME_OMIT,

    [Description(SheetsConfig.HeaderNames.TimeStart)]
    TIME_START,

    [Description(SheetsConfig.HeaderNames.TimeTotal)]
    TIME_TOTAL,

    [Description("Tip")]
    TIP,

    [Description(SheetsConfig.HeaderNames.Tips)]
    TIPS,

    [Description(SheetsConfig.HeaderNames.Total)]
    TOTAL,

    [Description(SheetsConfig.HeaderNames.TotalBonus)]
    TOTAL_BONUS,

    [Description(SheetsConfig.HeaderNames.TotalCash)]
    TOTAL_CASH,

    [Description(SheetsConfig.HeaderNames.TotalDistance)]
    TOTAL_DISTANCE,

    [Description(SheetsConfig.HeaderNames.TotalGrand)]
    TOTAL_GRAND,

    [Description(SheetsConfig.HeaderNames.TotalPay)]
    TOTAL_PAY,

    [Description(SheetsConfig.HeaderNames.TotalTime)]
    TOTAL_TIME,

    [Description(SheetsConfig.HeaderNames.TotalTimeActive)]
    TOTAL_TIME_ACTIVE,

    [Description(SheetsConfig.HeaderNames.TotalTips)]
    TOTAL_TIPS,

    [Description(SheetsConfig.HeaderNames.TotalTrips)]
    TOTAL_TRIPS,

    [Description(SheetsConfig.HeaderNames.Trips)]
    TRIPS,

    [Description("Trips/Day")]
    TRIPS_PER_DAY,

    [Description(SheetsConfig.HeaderNames.TripsPerHour)]
    TRIPS_PER_HOUR,

    [Description(SheetsConfig.HeaderNames.Type)]
    TYPE,

    [Description(SheetsConfig.HeaderNames.UnitEnd)]
    UNIT_END,

    [Description(SheetsConfig.HeaderNames.VisitFirst)]
    VISIT_FIRST,

    [Description(SheetsConfig.HeaderNames.VisitLast)]
    VISIT_LAST,

    [Description("Visits")]
    VISITS,

    [Description(SheetsConfig.HeaderNames.Week)]
    WEEK,

    [Description(SheetsConfig.HeaderNames.Weekday)]
    WEEKDAY,

    [Description(SheetsConfig.HeaderNames.Year)]
    YEAR,
}