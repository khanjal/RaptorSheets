using System.ComponentModel;

namespace RaptorSheets.Gig.Enums;

public enum SheetEnum
{

    // Core
    [Description("Trips")]
    TRIPS,

    [Description("Shifts")]
    SHIFTS,

    [Description("Expenses")]
    EXPENSES,

    // Auxillary
    [Description("Addresses")]
    ADDRESSES,

    [Description("Names")]
    NAMES,

    [Description("Places")]
    PLACES,

    [Description("Regions")]
    REGIONS,

    [Description("Services")]
    SERVICES,

    [Description("Types")]
    TYPES,

    // Period stats
    [Description("Daily")]
    DAILY,

    [Description("Weekdays")]
    WEEKDAYS,

    [Description("Weekly")]
    WEEKLY,

    [Description("Monthly")]
    MONTHLY,

    [Description("Yearly")]
    YEARLY,
}