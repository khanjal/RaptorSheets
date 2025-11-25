using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Core.Constants;

[ExcludeFromCodeCoverage]
public static class CellFormatPatterns
{
    // Financial formats
    public const string Accounting = "_(\"$\"* #,##0.00_);_(\"$\"* (\\(#,##0.00\\));_(\"$\"* \"-\"??_);_(@_)";
    public const string Currency = "$#,##0.00";
    public const string CurrencyNoDecimals = "$#,##0";
    public const string Percentage = "0.00%";
    public const string PercentageNoDecimals = "0%";
    
    // Date formats
    public const string Date = "yyyy-MM-dd";
    public const string DateUS = "M/d/yyyy";
    public const string DateEU = "dd/MM/yyyy";
    public const string DateLong = "MMMM d, yyyy";
    public const string DateShort = "M/d/yy";
    public const string DateTime = "M/d/yyyy H:mm:ss";
    public const string DateTimeShort = "M/d/yy H:mm";
    
    // Time formats
    public const string Time = "hh:mm am/pm";
    public const string Time24Hour = "HH:mm";
    public const string TimeWithSeconds = "hh:mm:ss am/pm";
    public const string Time24HourWithSeconds = "HH:mm:ss";
    public const string Duration = "[h]:mm";
    public const string DurationWithSeconds = "[h]:mm:ss";
    
    // Day/Week formats
    public const string Weekday = "ddd";
    public const string WeekdayFull = "dddd";
    public const string Month = "MMM";
    public const string MonthFull = "MMMM";
    
    // Number formats
    public const string Number = "#,##0";
    public const string NumberWithDecimals = "#,##0.00";
    public const string Distance = "#,##0.0";
    public const string Integer = "0";
    public const string Scientific = "0.00E+00";
    
    // Text formats
    public const string Text = "@";
    public const string Phone = "(###) ###-####";
    public const string ZipCode = "00000";
    public const string ZipCodePlus4 = "00000-0000";
    
    // Google Sheets cell format types
    public const string CellFormatText = "TEXT";
    public const string CellFormatNumber = "NUMBER";
    public const string CellFormatCurrency = "CURRENCY";
    public const string CellFormatDateTime = "DATE_TIME";
    public const string CellFormatPercent = "PERCENT";
    
    // Validation patterns
    public const string ValidationEmail = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
    public const string ValidationPhoneNumber = @"^\+?1?\d{9,15}$";
    public const string ValidationUrl = @"^https?://.+$";
}
