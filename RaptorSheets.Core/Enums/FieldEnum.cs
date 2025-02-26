using System.ComponentModel;

namespace RaptorSheets.Core.Enums;

public enum FieldEnum
{
    /// <summary>
    /// Use this field to update value and format of a cell
    /// </summary>
    [Description("*")]
    ALL,

    /// <summary>
    /// Use this field to update the value of a cell
    /// </summary>
    [Description("userEnteredValue")]
    USER_ENTERED_VALUE,

    /// <summary>
    /// Use this field to update the format of a cell
    /// </summary>
    [Description("userEnteredFormat")]
    USER_ENTERED_FORMAT
}
