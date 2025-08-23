using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Models.Google;
using System.Text.RegularExpressions;

namespace RaptorSheets.Core.Helpers;

public static class HeaderHelpers
{
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
        var columnId = GetHeaderKey(headers, columnName);

        if (columnId > values.Count || columnId < 0 || values[columnId] == null)
        {
            return 0;
        }

        var value = values[columnId]?.ToString()?.Trim();
        
        // If the string contains a decimal point, it's not a valid integer
        if (value?.Contains('.') == true)
        {
            return 0;
        }
        
        // Handle negative numbers - preserve the minus sign but remove other non-digit characters
        var isNegative = value?.StartsWith("-") == true;
        value = Regex.Replace(value!, @"[^\d]", ""); // Remove all non-digit characters
        
        if (string.IsNullOrEmpty(value))
        {
            return 0; // Make empty into 0s.
        }

        if (int.TryParse(value, out int result))
        {
            return isNegative ? -result : result;
        }

        return 0;
    }
    public static decimal GetDecimalValue(string columnName, IList<object> values, Dictionary<int, string> headers)
    {
        return GetDecimalValueOrNull(columnName, values, headers) ?? 0;
    }


    public static decimal? GetDecimalValueOrNull(string columnName, IList<object> values, Dictionary<int, string> headers)
    {
        var columnId = GetHeaderKey(headers, columnName);

        // TODO: Look into this closer. had to change to >= values.count. Does this need to be on other ones?
        if (columnId >= values.Count || columnId < 0 || values[columnId] == null)
        {
            return null;
        }

        var value = values[columnId]?.ToString()?.Trim();
        value = Regex.Replace(value!, @"[^\d.-]", ""); // Remove all special currency symbols except for .'s and -'s
        if (value == "-" || value == "")
        {
            return null;  // Make account -'s into nulls.
        }

        if (decimal.TryParse(value, out decimal result))
        {
            return result;
        }
        else
        {
            return null;
        }
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