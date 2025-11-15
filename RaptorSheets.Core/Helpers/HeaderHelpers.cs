using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Models.Google;
using System.Text.RegularExpressions;

namespace RaptorSheets.Core.Helpers;

public static class HeaderHelpers
{
    // Regex patterns with timeout to prevent potential performance issues
    private static readonly Regex NonDigitRegex = new(@"[^\d]", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));
    private static readonly Regex NonDecimalRegex = new(@"[^\d.-]", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));

    public static IList<object> GetHeadersFromCellData(IList<CellData>? cellData)
    {
        var headers = cellData?.Where(x => x.FormattedValue != null).Select(v => v.FormattedValue).ToList() ?? [];
        return headers.Cast<object>().ToList();
    }

    public static Dictionary<int, string> ParserHeader(IList<object> sheetHeader)
    {
        var headerValues = new Dictionary<int, string>();

        if (sheetHeader == null)
        {
            return headerValues;
        }

        foreach (var item in sheetHeader.Select((value, index) => new { index, value }))
        {
            headerValues.Add(item.index, item.value?.ToString()?.Trim() ?? "");
        }

        return headerValues;
    }

    public static bool GetBoolValue(string columnName, IList<object> values, Dictionary<int, string> headers)
    {
        var columnId = GetHeaderKey(headers, columnName);

        if (columnId > values.Count || columnId < 0 || values[columnId] == null)
        {
            return false;
        }

        return string.Equals(values[columnId]?.ToString()?.Trim(), "TRUE", StringComparison.OrdinalIgnoreCase);
    }

    public static string GetDateValue(string columnName, IList<object> values, Dictionary<int, string> headers)
    {
        var columnId = GetHeaderKey(headers, columnName);

        if (columnId > values.Count || columnId < 0 || values.Count == 0 || string.IsNullOrEmpty(values[columnId].ToString()))
        {
            return "";
        }

        var dateString = values[columnId]!.ToString() ?? "";
        
        if (DateTime.TryParse(dateString, out DateTime result))
        {
            // If the input was in yyyy-MM-dd format, preserve it, otherwise normalize to yyyy-MM-dd
            if (dateString.Contains("-") && dateString.Length == 10)
            {
                return result.ToString("yyyy-MM-dd");
            }
            else
            {
                // For other formats like "2023/10/01", preserve the original format
                return dateString;
            }
        }
        
        // If parsing fails, return the original string as-is
        return dateString;
    }

    public static string GetStringValue(string columnName, IList<object> values, Dictionary<int, string> headers)
    {
        var columnId = GetHeaderKey(headers, columnName);

        if (columnId >= values.Count || columnId < 0 || values[columnId] == null)
        {
            return "";
        }

        return values[columnId]?.ToString()?.Trim() ?? "";
    }

    public static int GetIntValue(string columnName, IList<object> values, Dictionary<int, string> headers)
    {
        return GetIntValueOrNull(columnName, values, headers) ?? 0;
    }
    
    public static int? GetIntValueOrNull(string columnName, IList<object> values, Dictionary<int, string> headers)
    {
        var columnId = GetHeaderKey(headers, columnName);

        if (columnId >= values.Count || columnId < 0 || values[columnId] == null)
        {
            Console.WriteLine($"Column '{columnName}' is out of range or null.");
            return null;
        }

        var value = values[columnId]?.ToString()?.Trim();

        // Log the raw value for debugging purposes
        Console.WriteLine($"Raw value for column '{columnName}': {value}");

        // If the string contains a decimal point, it's not a valid integer
        if (value?.Contains('.') == true)
        {
            Console.WriteLine($"Column '{columnName}' contains decimal point - not a valid integer.");
            return null;
        }

        // Handle negative numbers - preserve the minus sign but remove other non-digit characters
        var isNegative = value?.StartsWith("-") == true;
        value = NonDigitRegex.Replace(value ?? string.Empty, ""); // Remove all non-digit characters with timeout

        // Log the filtered value for debugging purposes
        Console.WriteLine($"Filtered value for column '{columnName}': {value}");

        if (value == "-" || string.IsNullOrEmpty(value))
        {
            Console.WriteLine($"Column '{columnName}' has an empty or invalid value after filtering.");
            return null;  // Make empty values into nulls.
        }

        if (int.TryParse(value, out int result))
        {
            Console.WriteLine($"Parsed value for column '{columnName}': {result}");
            return isNegative ? -result : result;
        }

        Console.WriteLine($"Failed to parse value for column '{columnName}': {value}");
        return null;
    }
    
    public static decimal GetDecimalValue(string columnName, IList<object> values, Dictionary<int, string> headers)
    {
        return GetDecimalValueOrNull(columnName, values, headers) ?? 0;
    }


    public static decimal? GetDecimalValueOrNull(string columnName, IList<object> values, Dictionary<int, string> headers)
    {
        var columnId = GetHeaderKey(headers, columnName);

        if (columnId >= values.Count || columnId < 0 || values[columnId] == null)
        {
            Console.WriteLine($"Column '{columnName}' is out of range or null.");
            return null;
        }

        var value = values[columnId]?.ToString()?.Trim();

        // Log the raw value for debugging purposes
        Console.WriteLine($"Raw value for column '{columnName}': {value}");

        value = NonDecimalRegex.Replace(value ?? string.Empty, ""); // Remove all special currency symbols except for .'s and -'s with timeout

        // Log the filtered value for debugging purposes
        Console.WriteLine($"Filtered value for column '{columnName}': {value}");

        if (value == "-" || value == "")
        {
            Console.WriteLine($"Column '{columnName}' has an empty or invalid value after filtering.");
            return null;  // Make account -'s into nulls.
        }

        if (decimal.TryParse(value, out decimal result))
        {
            Console.WriteLine($"Parsed value for column '{columnName}': {result}");
            return result;
        }

        Console.WriteLine($"Failed to parse value for column '{columnName}': {value}");
        return null;
    }

    private static int GetHeaderKey(Dictionary<int, string> header, string value)
    {
        try
        {
            return header.First(x => x.Value.Trim() == value.Trim()).Key;
        }
        catch (Exception)
        {
            return -1;
        }

    }
    
    public static List<MessageEntity> CheckSheetHeaders(IList<object> values, SheetModel sheetModel)
    {
        var messages = new List<MessageEntity>();
        var headerArray = new string[values.Count];
        values.CopyTo(headerArray, 0);
        var index = 0;

        foreach (var sheetHeader in sheetModel.Headers)
        {
            var sheetColumn = $"{sheetModel.Name}!{SheetHelpers.GetColumnName(index)}";

            if (!values.Any(x => x?.ToString()?.Trim() == sheetHeader.Name))
            {
                messages.Add(MessageHelpers.CreateErrorMessage($"[{sheetColumn}]: Missing column [{sheetHeader.Name}]", MessageTypeEnum.CHECK_SHEET));
            }
            else
            {
                if (index < headerArray.Length && sheetHeader.Name != headerArray[index].Trim())
                {
                    messages.Add(MessageHelpers.CreateWarningMessage($"[{sheetColumn}]: Column [{headerArray[index].Trim()}] should be [{sheetHeader.Name}]", MessageTypeEnum.CHECK_SHEET));
                }
            }
            index++;
        }

        // Check for extra columns that aren't in the expected sheet model
        var expectedHeaders = sheetModel.Headers.Select(h => h.Name).ToHashSet();
        for (int i = 0; i < values.Count; i++)
        {
            var actualHeader = values[i]?.ToString()?.Trim();
            if (!string.IsNullOrEmpty(actualHeader) && !expectedHeaders.Contains(actualHeader))
            {
                var sheetColumn = $"{sheetModel.Name}!{SheetHelpers.GetColumnName(i)}";
                messages.Add(MessageHelpers.CreateWarningMessage($"[{sheetColumn}]: Extra column [{actualHeader}]", MessageTypeEnum.CHECK_SHEET));
            }
        }

        return messages;
    }
}