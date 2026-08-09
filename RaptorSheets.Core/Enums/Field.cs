using System.ComponentModel;

namespace RaptorSheets.Core.Enums;

public enum Field
{
    /// <summary>
    /// Use this field to update value and format of a cell
    /// </summary>
    [Description("*")]
    USER_ENTERED_VALUE_AND_FORMAT,

    /// <summary>
    /// Use this field to update the value of a cell
    /// </summary>
    [Description("userEnteredValue")]
    USER_ENTERED_VALUE,

    /// <summary>
    /// Use this field to update the format of a cell
    /// </summary>
    [Description("userEnteredFormat")]
    USER_ENTERED_FORMAT,

    /// <summary>
    /// Use this field to update only a cell's number format - narrower than
    /// <see cref="USER_ENTERED_FORMAT"/>, which would also clear any other userEnteredFormat
    /// sub-fields (background color, borders, text format, ...) not present in the request.
    /// </summary>
    [Description("userEnteredFormat.numberFormat")]
    NUMBER_FORMAT,

    /// <summary>
    /// Use this field to update the value and note of a cell together, without touching format or
    /// validation.
    /// </summary>
    [Description("userEnteredValue,note")]
    USER_ENTERED_VALUE_AND_NOTE,

    /// <summary>
    /// Use this field to update only a cell's data validation rule, without touching value, format,
    /// or note.
    /// </summary>
    [Description("dataValidation")]
    DATA_VALIDATION
}
