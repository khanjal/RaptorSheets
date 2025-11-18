using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Core.Constants;

[ExcludeFromCodeCoverage]
public static class CellFormatPatterns
{
    public const string Accounting = "_(\"$\"* #,##0.00_);_(\"$\"* (\\(#,##0.00\\));_(\"$\"* \"-\"??_);_(@_)";
    public const string Currency = "$#,##0.00";
    public const string Date = "yyyy-MM-dd";
    public const string Distance = "#,##0.0";
    public const string Duration = "[h]:mm";
    public const string Number = "#,##0";
    public const string Time = "hh:mm am/pm";
    public const string Weekday = "ddd";
}
