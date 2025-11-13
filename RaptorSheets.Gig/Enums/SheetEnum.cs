using System.ComponentModel;
using RaptorSheets.Gig.Constants;

namespace RaptorSheets.Gig.Enums;

/// <summary>
/// Sheet enumeration for type safety and IntelliSense support.
/// Descriptions map to SheetsConfig.SheetNames constants for consistency.
/// Use enums for type safety and constants for switch statements and comparisons.
/// </summary>
public enum SheetEnum
{
    [Description(SheetsConfig.SheetNames.Addresses)]
    ADDRESSES,

    [Description(SheetsConfig.SheetNames.Daily)]
    DAILY,

    [Description(SheetsConfig.SheetNames.Expenses)]
    EXPENSES,

    [Description(SheetsConfig.SheetNames.Monthly)]
    MONTHLY,

    [Description(SheetsConfig.SheetNames.Names)]
    NAMES,

    [Description(SheetsConfig.SheetNames.Places)]
    PLACES,

    [Description(SheetsConfig.SheetNames.Regions)]
    REGIONS,

    [Description(SheetsConfig.SheetNames.Services)]
    SERVICES,

    [Description(SheetsConfig.SheetNames.Setup)]
    SETUP,

    [Description(SheetsConfig.SheetNames.Shifts)]
    SHIFTS,

    [Description(SheetsConfig.SheetNames.Trips)]
    TRIPS,

    [Description(SheetsConfig.SheetNames.Types)]
    TYPES,

    [Description(SheetsConfig.SheetNames.Weekdays)]
    WEEKDAYS,

    [Description(SheetsConfig.SheetNames.Weekly)]
    WEEKLY,

    [Description(SheetsConfig.SheetNames.Yearly)]
    YEARLY
}