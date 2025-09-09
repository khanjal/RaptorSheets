using System.ComponentModel;
using RaptorSheets.Gig.Constants;

namespace RaptorSheets.Gig.Enums;

/// <summary>
/// Compatibility wrapper around SheetsConfig.SheetNames constants.
/// This allows existing code to continue working while we migrate to the new constants approach.
/// </summary>
[Obsolete("Use SheetsConfig.SheetNames constants directly instead of SheetEnum.FIELD.GetDescription()")]
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