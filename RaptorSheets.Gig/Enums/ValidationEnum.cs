using System.ComponentModel;
using RaptorSheets.Gig.Constants;

namespace RaptorSheets.Gig.Enums;

/// <summary>
/// Validation types for Google Sheets data validation.
/// Description attributes map to the constant values in SheetsConfig.ValidationNames.
/// </summary>
public enum ValidationEnum
{
    DEFAULT = 0,

    [Description(SheetsConfig.ValidationNames.Boolean)]
    BOOLEAN,

    [Description(SheetsConfig.ValidationNames.RangeAddress)]
    RANGE_ADDRESS,

    [Description(SheetsConfig.ValidationNames.RangeName)]
    RANGE_NAME,

    [Description(SheetsConfig.ValidationNames.RangePlace)]
    RANGE_PLACE,

    [Description(SheetsConfig.ValidationNames.RangeRegion)]
    RANGE_REGION,

    [Description(SheetsConfig.ValidationNames.RangeService)]
    RANGE_SERVICE,

    [Description(SheetsConfig.ValidationNames.RangeSelf)]
    RANGE_SELF,

    [Description(SheetsConfig.ValidationNames.RangeType)]
    RANGE_TYPE
}