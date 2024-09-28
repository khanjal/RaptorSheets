using System.Diagnostics.CodeAnalysis;

namespace RLE.Core.Constants;

[ExcludeFromCodeCoverage]
public static class CellFormatPatterns
{
    public static string Accounting => "_(\"$\"* #,##0.00_);_(\"$\"* \\(#,##0.00\\);_(\"$\"* \"-\"??_);_(@_)";
    public static string Date => "yyyy-mm-dd";
    public static string Distance => "#,##0.0";
    public static string Duration => "[h]:mm";
    public static string Number => "#,##0";
    public static string Time => "hh:mm:ss am/pm";
    public static string Weekday => "ddd";
}
