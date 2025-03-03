namespace RaptorSheets.Core.Extensions;

public static class StringExtensions
{
    public static double? ToSerialDate(this string stringDate)
    {
        if (DateTime.TryParse(stringDate, out var date))
        {
            // Convert date to a serial number representing the total number of days since January 1, 1900
            var serialDate = (date - new DateTime(1899, 12, 30)).TotalDays;

            return serialDate;
        }

        return null;
    }

    public static double? ToSerialDuration(this string stringDuration)
    {
        try
        {
            // Split off milliseconds
            var durationParts = stringDuration.Split('.');

            // Split duration into hours, minutes, and seconds
            var timeParts = durationParts[0].Split(':');

            // Convert time parts to a time span
            var timeSpan = new TimeSpan(
                int.Parse(timeParts[0]),
                int.Parse(timeParts[1]),
                int.Parse(timeParts[2])
            );

            return timeSpan.TotalDays;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static double? ToSerialTime(this string stringTime)
    {
        if (DateTime.TryParse(stringTime, out var dateTime))
        {
            // Convert time to a serial number representing the total number of days
            return dateTime.TimeOfDay.TotalDays;
        }

        return null;
    }
}