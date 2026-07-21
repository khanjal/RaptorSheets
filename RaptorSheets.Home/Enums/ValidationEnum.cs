using System.ComponentModel;
using RaptorSheets.Home.Constants;

namespace RaptorSheets.Home.Enums;

/// <summary>
/// Validation types for Google Sheets data validation.
/// Description attributes map to the constant values in SheetsConfig.ValidationNames.
/// </summary>
public enum ValidationEnum
{
    DEFAULT = 0,

    [Description(SheetsConfig.ValidationNames.Boolean)]
    BOOLEAN,

    [Description(SheetsConfig.ValidationNames.RangeRoom)]
    RANGE_ROOM,

    [Description(SheetsConfig.ValidationNames.RangeContact)]
    RANGE_CONTACT,

    [Description(SheetsConfig.ValidationNames.RangeSelf)]
    RANGE_SELF
}
