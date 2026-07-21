using System.ComponentModel;
using RaptorSheets.Home.Constants;

namespace RaptorSheets.Home.Enums;

/// <summary>
/// Sheet enumeration for type safety and IntelliSense support.
/// Descriptions map to SheetsConfig.SheetNames constants.
/// </summary>
public enum SheetEnum
{
    [Description(SheetsConfig.SheetNames.Appliances)]
    APPLIANCES,

    [Description(SheetsConfig.SheetNames.Projects)]
    PROJECTS,

    [Description(SheetsConfig.SheetNames.Maintenance)]
    MAINTENANCE,

    [Description(SheetsConfig.SheetNames.Doors)]
    DOORS,

    [Description(SheetsConfig.SheetNames.Paints)]
    PAINTS,

    [Description(SheetsConfig.SheetNames.Power)]
    POWER,

    [Description(SheetsConfig.SheetNames.Rooms)]
    ROOMS,

    [Description(SheetsConfig.SheetNames.Contacts)]
    CONTACTS,

    [Description(SheetsConfig.SheetNames.Stats)]
    STATS
}
