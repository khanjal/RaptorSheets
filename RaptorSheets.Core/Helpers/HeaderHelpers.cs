using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Models;
using RaptorSheets.Core.Models.Google;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace RaptorSheets.Core.Helpers;

public static class HeaderHelpers
{
    // Regex patterns with timeout to prevent potential performance issues
    private static readonly Regex NonDigitRegex = new(@"[^\d]", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));
    private static readonly Regex NonDecimalRegex = new(@"[^\d.-]", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));

    // Reverse (name -> index) lookup index, built once per distinct `headers` dictionary instance
    // (i.e. once per sheet, since ParserHeader is only called once per sheet and the same instance
    // is reused for every row) instead of linearly scanned on every single cell read.
    private static readonly ConditionalWeakTable<Dictionary<int, string>, Dictionary<string, int>> HeaderIndexCache = new();

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
        
        if (DateTime.TryParse(dateString, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime result))
        {
            // If the input was in yyyy-MM-dd format, preserve it, otherwise normalize to yyyy-MM-dd
            if (dateString.Contains('-') && dateString.Length == 10)
            {
                return result.ToString(CellFormatPatterns.Date);
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
            return null;
        }

        var value = values[columnId]?.ToString()?.Trim();

        // If the string contains a decimal point, it's not a valid integer
        if (value?.Contains('.') == true)
        {
            return null;
        }

        // Handle negative numbers - preserve the minus sign but remove other non-digit characters
        var isNegative = value?.StartsWith("-") == true;
        value = NonDigitRegex.Replace(value ?? string.Empty, ""); // Remove all non-digit characters with timeout

        if (value == "-" || string.IsNullOrEmpty(value))
        {
            return null;  // Make empty values into nulls.
        }

        if (int.TryParse(value, out int result))
        {
            return isNegative ? -result : result;
        }

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
            return null;
        }

        var value = values[columnId]?.ToString()?.Trim();

        value = NonDecimalRegex.Replace(value ?? string.Empty, ""); // Remove all special currency symbols except for .'s and -'s with timeout

        if (value == "-" || value == "")
        {
            return null;  // Make account -'s into nulls.
        }

        if (decimal.TryParse(value, out decimal result))
        {
            return result;
        }

        return null;
    }

    private static int GetHeaderKey(Dictionary<int, string> header, string value)
    {
        if (header == null || value == null)
        {
            return -1;
        }

        var index = HeaderIndexCache.GetValue(header, BuildHeaderIndex);
        return index.TryGetValue(value.Trim(), out var key) ? key : -1;
    }

    // Builds a name -> column index map from the parsed header row. Header matching stays
    // name-based (not positional), so reordered or swapped columns are still handled correctly -
    // this only changes lookup from an O(n) scan to an O(1) dictionary lookup.
    private static Dictionary<string, int> BuildHeaderIndex(Dictionary<int, string> header)
    {
        var index = new Dictionary<string, int>();

        foreach (var (columnIndex, name) in header)
        {
            var trimmedName = name?.Trim() ?? "";
            if (!string.IsNullOrEmpty(trimmedName) && !index.ContainsKey(trimmedName))
            {
                index[trimmedName] = columnIndex;
            }
        }

        return index;
    }
    
    public static List<MessageEntity> CheckSheetHeaders(IList<object> values, SheetModel sheetModel)
    {
        return CheckSheetHeaders(values, sheetModel, out _);
    }

    /// <summary>
    /// Checks sheet headers against the expected model and additionally reports which expected
    /// columns are missing entirely (not found anywhere in the row), with the index/letter they
    /// should be inserted at according to the model's canonical column order. Callers that don't
    /// need auto-insertion can use the simpler overload above.
    /// </summary>
    /// <param name="values">Actual header values from the sheet</param>
    /// <param name="sheetModel">Expected sheet model with header definitions</param>
    /// <param name="insertionInfo">Output list of columns that need to be inserted (SheetId left at 0 - the caller fills that in from spreadsheet metadata)</param>
    public static List<MessageEntity> CheckSheetHeaders(IList<object> values, SheetModel sheetModel, out List<ColumnInsertionInfo> insertionInfo)
    {
        var messages = new List<MessageEntity>();
        insertionInfo = [];

        // If there are no header values (sheet empty or header row missing), return a single, clear message
        if (values == null || values.Count == 0 || values.All(v => string.IsNullOrWhiteSpace(v?.ToString())))
        {
            messages.Add(MessageHelpers.CreateErrorMessage($"[{sheetModel.Name}]: No header row found or sheet is empty", MessageTypeEnum.CHECK_SHEET));
            return messages;
        }

        var headerArray = new string[values.Count];
        values.CopyTo(headerArray, 0);
        var index = 0;

        foreach (var sheetHeader in sheetModel.Headers)
        {
            var sheetColumn = $"{sheetModel.Name}!{SheetHelpers.GetColumnName(index)}";

            if (!values.Any(x => string.Equals(x?.ToString()?.Trim(), sheetHeader.Name, StringComparison.OrdinalIgnoreCase)))
            {
                // HideHeaderName columns are populated by a spilling QUERY formula placed in an
                // earlier header cell (see SheetHelpers' use of the flag when writing headers) -
                // they never have their own standalone header cell to insert, and physically
                // inserting a column here would land inside the query's contiguous spill range
                // and break it. They're also expected to read back empty whenever the sheet
                // currently has no underlying rows for the query to spill, so this isn't
                // necessarily a real problem - just never a candidate for insertion.
                if (!sheetHeader.HideHeaderName)
                {
                    insertionInfo.Add(new ColumnInsertionInfo
                    {
                        SheetName = sheetModel.Name,
                        ColumnIndex = index,
                        ColumnName = sheetHeader.Name,
                        ColumnLetter = SheetHelpers.GetColumnName(index)
                    });

                    messages.Add(MessageHelpers.CreateErrorMessage($"[{sheetColumn}]: Missing column [{sheetHeader.Name}] - can be inserted", MessageTypeEnum.CHECK_SHEET));
                }
                else
                {
                    messages.Add(MessageHelpers.CreateErrorMessage($"[{sheetColumn}]: Missing column [{sheetHeader.Name}]", MessageTypeEnum.CHECK_SHEET));
                }
            }
            else
            {
                if (index < headerArray.Length && !string.Equals(sheetHeader.Name, headerArray[index]?.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    var actual = headerArray[index]?.Trim() ?? "";
                    messages.Add(MessageHelpers.CreateWarningMessage($"[{sheetColumn}]: Column [{actual}] should be [{sheetHeader.Name}]", MessageTypeEnum.CHECK_SHEET));
                }
            }
            index++;
        }

        // Check for extra columns that aren't in the expected sheet model (case-insensitive)
        var expectedHeaders = sheetModel.Headers.Select(h => h.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
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