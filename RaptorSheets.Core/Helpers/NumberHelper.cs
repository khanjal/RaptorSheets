using System.Text.RegularExpressions;

namespace RaptorSheets.Core.Helpers;

/// <summary>
/// Helper class for cleaning and processing numeric strings.
/// </summary>
public static class NumberHelper
{
    /// <summary>
    /// Cleans a numeric string by removing all non-numeric characters except for '.' and '-'.
    /// </summary>
    /// <param name="value">The input string to clean.</param>
    /// <returns>The cleaned numeric string.</returns>
    public static string CleanNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return Regex.Replace(value, "[^\\d.\\-]", "").Trim();
    }
}