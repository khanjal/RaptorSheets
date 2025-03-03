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

        return values[columnId]?.ToString()?.Trim().ToUpper() == "TRUE";
    }

    public static string GetDateValue(string columnName, IList<object> values, Dictionary<int, string> headers)
    {
        var columnId = GetHeaderKey(headers, columnName);

        if (columnId > values.Count || columnId < 0 || values.Count == 0 || string.IsNullOrEmpty(values[columnId].ToString()))
        {
            return "";
        }

        return DateTime.Parse(values[columnId]!.ToString() ?? "").ToString("yyyy-MM-dd");
    }

    public static string GetStringValue(string columnName, IList<object> values, Dictionary<int, string> headers)
    {
        var columnId = GetHeaderKey(headers, columnName);

        if (columnId > values.Count || columnId < 0 || values[columnId] == null)
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
        value = Regex.Replace(value!, @"[^\d]", ""); // Remove all special symbols.
        if (value == "")
        {
            return 0; // Make empty into 0s.
        }

        int.TryParse(value, out int result);

        return result;
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

    public static List<MessageEntity> CheckSheetHeaders(IList<IList<object>> values, SheetModel sheetModel)
    {
        var messages = new List<MessageEntity>();
        var data = values[0];
        var headerArray = new string[data.Count];
        data.CopyTo(headerArray, 0);
        var index = 0;

        foreach (var sheetHeader in sheetModel.Headers)
        {
            if (!data.Any(x => x?.ToString()?.Trim() == sheetHeader.Name))
            {
                messages.Add(MessageHelpers.CreateErrorMessage($"Sheet [{sheetModel.Name}]: Missing column [{sheetHeader.Name}]", MessageTypeEnum.CHECK_SHEET));
            }
            else
            {
                if (index < headerArray.Count() && sheetHeader.Name != headerArray[index].Trim())
                {
                    messages.Add(MessageHelpers.CreateWarningMessage($"Sheet [{sheetModel.Name}]: Unexpected column [{headerArray[index].Trim()}] should be [{sheetHeader.Name}]", MessageTypeEnum.CHECK_SHEET));
                }
            }
            index++;
        }

        return messages;
    }
}