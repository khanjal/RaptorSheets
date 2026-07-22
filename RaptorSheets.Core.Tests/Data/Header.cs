using System.ComponentModel;

namespace RaptorSheets.Core.Tests.Data;

public enum Header
{
    [Description("First Column")]
    FIRST_COLUMN,

    [Description("Second Column")]
    SECOND_COLUMN,

    THIRD_COLUMN
}
