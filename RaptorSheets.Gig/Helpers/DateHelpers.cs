using RaptorSheets.Core.Constants;
using System.Globalization;

namespace RaptorSheets.Gig.Helpers
{
    /// <summary>
    /// Helper class for date parsing and formatting.
    /// </summary>
    public static class DateHelpers
    {
        private const string DateFormat = CellFormatPatterns.Date;

        /// <summary>
        /// Formats a DateTime object to a string using the standard date format.
        /// </summary>
        /// <param name="date">The DateTime object to format.</param>
        /// <returns>A formatted date string.</returns>
        public static string FormatDate(DateTime date)
        {
            return date.ToString(DateFormat);
        }

        /// <summary>
        /// Parses a date string into a DateTime object using the standard date format.
        /// </summary>
        /// <param name="date">The date string to parse.</param>
        /// <returns>A DateTime object.</returns>
        public static DateTime ParseDate(string date)
        {
            return DateTime.ParseExact(date, DateFormat, CultureInfo.InvariantCulture);
        }
    }
}