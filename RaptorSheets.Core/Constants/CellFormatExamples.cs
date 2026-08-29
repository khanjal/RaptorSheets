using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Core.Constants;

/// <summary>
/// A rendered example for each format pattern, shown in a column's note beside the pattern itself.
///
/// The pattern alone tells a user very little - Accounting's is
/// <c>_("$"* #,##0.00_);_("$"* (\(#,##0.00\));_("$"* "-"??_);_(@_)</c>, which says nothing about
/// what will actually appear in the cell. The example does.
///
/// Deliberately a lookup rather than a formatter. These are Google Sheets number patterns, not .NET
/// ones: "am/pm", "[h]:mm" and the "_(" alignment escapes have no .NET equivalent, so rendering
/// them faithfully would mean writing a Sheets format engine to produce a hint. A pattern with no
/// entry here simply gets no example, which is the correct failure mode.
/// </summary>
[ExcludeFromCodeCoverage]
public static class CellFormatExamples
{
    private static readonly Dictionary<string, string> Examples = new()
    {
        // Currency - the accounting pattern is the whole reason this exists.
        [CellFormatPatterns.Accounting] = "$ 1,234.56",
        [CellFormatPatterns.Currency] = "$1,234.56",
        [CellFormatPatterns.CurrencyNoDecimals] = "$1,235",
        [CellFormatPatterns.Percentage] = "12.34%",
        [CellFormatPatterns.PercentageNoDecimals] = "12%",

        // Dates - the same day rendered every way, so the difference between them is obvious.
        [CellFormatPatterns.Date] = "2026-03-09",
        [CellFormatPatterns.DateUS] = "3/9/2026",
        [CellFormatPatterns.DateEU] = "09/03/2026",
        [CellFormatPatterns.DateLong] = "March 9, 2026",
        [CellFormatPatterns.DateShort] = "3/9/26",
        [CellFormatPatterns.DateTime] = "3/9/2026 14:30:00",
        [CellFormatPatterns.DateTimeShort] = "3/9/26 14:30",

        // Times. Duration uses [h] so it accumulates past 24 hours rather than wrapping - worth
        // making visible, since that is the difference between a clock time and an elapsed one.
        [CellFormatPatterns.Time] = "02:30 pm",
        [CellFormatPatterns.Time24Hour] = "14:30",
        [CellFormatPatterns.TimeWithSeconds] = "02:30:15 pm",
        [CellFormatPatterns.Time24HourWithSeconds] = "14:30:15",
        [CellFormatPatterns.Duration] = "26:30",
        [CellFormatPatterns.DurationWithSeconds] = "26:30:15",

        [CellFormatPatterns.Weekday] = "Mon",
        [CellFormatPatterns.WeekdayFull] = "Monday",
        [CellFormatPatterns.Month] = "Mar",
        [CellFormatPatterns.MonthFull] = "March",

        [CellFormatPatterns.Number] = "1,235",
        [CellFormatPatterns.NumberWithDecimals] = "1,234.56",
        [CellFormatPatterns.Distance] = "1,234.5",
        [CellFormatPatterns.Integer] = "1235",
        [CellFormatPatterns.Scientific] = "1.23E+03",

        [CellFormatPatterns.Phone] = "(555) 123-4567",
        [CellFormatPatterns.ZipCode] = "01234",
        [CellFormatPatterns.ZipCodePlus4] = "01234-5678",
    };

    /// <summary>
    /// The example for a pattern, or null when there is none. Callers append it only when present,
    /// so an unrecognised pattern degrades to the bare pattern rather than to a wrong example.
    /// </summary>
    public static string? For(string? pattern)
    {
        if (string.IsNullOrEmpty(pattern))
        {
            return null;
        }

        return Examples.TryGetValue(pattern, out var example) ? example : null;
    }
}
