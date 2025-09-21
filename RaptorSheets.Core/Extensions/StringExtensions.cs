using System.Globalization;

namespace RaptorSheets.Core.Extensions;

public static class StringExtensions
{
    // Excel serial date epoch: 1899-12-30 (Excel's day 1 is 1900-01-01, but Excel incorrectly treats 1900 as a leap year)
    private static readonly DateTime ExcelSerialDateEpoch = new DateTime(1899, 12, 30, 0, 0, 0, DateTimeKind.Unspecified);

    public static double? ToSerialDate(this string stringDate)
    {
        if (DateTime.TryParse(stringDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            // Convert date to a serial number representing the total number of days since the Excel serial date epoch
            var serialDate = (date - ExcelSerialDateEpoch).TotalDays;
            return serialDate;
        }

        return null;
    }

    public static double? ToSerialDuration(this string stringDuration)
    {
        if (string.IsNullOrWhiteSpace(stringDuration))
        {
            return null;
        }

        try
        {
            bool isNegative = stringDuration.StartsWith('-');
            string normalizedDuration = isNegative ? stringDuration.Substring(1) : stringDuration;

            // Split off milliseconds
            var durationParts = normalizedDuration.Split('.');

            // Split duration into hours, minutes, and seconds
            var timeParts = durationParts[0].Split(':');

            // Validate time format - must have exactly 3 parts (hours:minutes:seconds)
            if (timeParts.Length != 3)
            {
                return null;
            }

            // Parse time parts with validation
            if (!int.TryParse(timeParts[0], out int hours) ||
                !int.TryParse(timeParts[1], out int minutes) ||
                !int.TryParse(timeParts[2], out int seconds))
            {
                return null;
            }

            // Create timespan
            var timeSpan = new TimeSpan(hours, minutes, seconds);

            // Calculate result and apply negative sign if needed
            double result = timeSpan.TotalDays;
            return isNegative ? -result : result;
        }
        catch
        {
            return null;
        }
    }

    public static double? ToSerialTime(this string stringTime)
    {
        if (DateTime.TryParse(stringTime, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
        {
            // Convert time to a serial number representing the total number of days
            return dateTime.TimeOfDay.TotalDays;
        }

        return null;
    }
}