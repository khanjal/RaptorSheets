using System.ComponentModel;

namespace RaptorSheets.Core.Constants;

public enum GoogleFinanceAttributesEnum
{
    [Description("high")]
    HIGH,

    [Description("low")]
    LOW,

    [Description("name")]
    NAME,

    [Description("pe")]
    PE_RATIO,

    [Description("price")]
    PRICE,

    [Description("high52")]
    WEEK_HIGH_52,

    [Description("low52")]
    WEEK_LOW_52,
}
