using System.ComponentModel;

namespace RaptorSheets.Gig.Enums;

public enum HeaderEnum
{
    [Description("Address")]
    ADDRESS,

    [Description("Start Address")]
    ADDRESS_START,

    [Description("End Address")]
    ADDRESS_END,

    [Description("Curr Amt")]
    AMOUNT_CURRENT,

    [Description("Prev Amt")]
    AMOUNT_PREVIOUS,

    [Description("Amt/Day")]
    AMOUNT_PER_DAY,

    [Description("Amt/Dist")]
    AMOUNT_PER_DISTANCE,

    [Description("Amt/Prev")]
    AMOUNT_PER_PREVIOUS_DAY,

    [Description("Amt/Hour")]
    AMOUNT_PER_TIME,

    [Description("Amt/Trip")]
    AMOUNT_PER_TRIP,

    [Description("Average")]
    AVERAGE,

    [Description("Bonus")]
    BONUS,

    [Description("Cash")]
    CASH,

    [Description("Date")]
    DATE,

    [Description("Begin")]
    DATE_BEGIN,

    [Description("End")]
    DATE_END,

    [Description("Day")]
    DAY,

    [Description("Days")]
    DAYS,

    [Description("D/V")]
    DAYS_PER_VISIT,

    [Description("Since")]
    DAYS_SINCE_VISIT,

    [Description("Dist")]
    DISTANCE,

    [Description("Dropoff")]
    DROPOFF,

    [Description("Duration")]
    DURATION,

    [Description("X")]
    EXCLUDE,

    [Description("Key")]
    KEY,

    [Description("Month")]
    MONTH,

    [Description("Name")]
    NAME,

    [Description("Note")]
    NOTE,

    [Description("#")]
    NUMBER,

    [Description("# Days")]
    NUMBER_OF_DAYS,

    [Description("Odo End")]
    ODOMETER_END,

    [Description("Odo Start")]
    ODOMETER_START,

    [Description("Order #")]
    ORDER_NUMBER,

    [Description("Pay")]
    PAY,

    [Description("Pickup")]
    PICKUP,

    [Description("Place")]
    PLACE,

    [Description("Region")]
    REGION,

    [Description("Service")]
    SERVICE,

    [Description("Active")]
    TIME_ACTIVE,

    [Description("Finish")]
    TIME_END,

    [Description("O")]
    TIME_OMIT,

    [Description("Start")]
    TIME_START,

    [Description("Time")]
    TIME_TOTAL,

    [Description("Tip")]
    TIP,

    [Description("Tips")]
    TIPS,

    [Description("Total")]
    TOTAL,

    [Description("T Bonus")]
    TOTAL_BONUS,

    [Description("T Cash")]
    TOTAL_CASH,

    [Description("T Dist")]
    TOTAL_DISTANCE,

    [Description("G Total")]
    TOTAL_GRAND,

    [Description("T Pay")]
    TOTAL_PAY,

    [Description("T Time")]
    TOTAL_TIME,

    [Description("T Active")]
    TOTAL_TIME_ACTIVE,

    [Description("T Tips")]
    TOTAL_TIPS,

    [Description("T Trips")]
    TOTAL_TRIPS,

    [Description("Trips")]
    TRIPS,

    [Description("Trips/Day")]
    TRIPS_PER_DAY,

    [Description("Trips/Hour")]
    TRIPS_PER_HOUR,

    [Description("Type")]
    TYPE,

    [Description("End Unit")]
    UNIT_END,

    [Description("First Trip")]
    VISIT_FIRST,

    [Description("Last Trip")]
    VISIT_LAST,

    [Description("Visits")]
    VISITS,

    [Description("Week")]
    WEEK,

    [Description("Weekday")]
    WEEKDAY,

    [Description("Year")]
    YEAR,
}