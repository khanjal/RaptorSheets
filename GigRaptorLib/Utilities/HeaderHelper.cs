using GigRaptorLib.Entities;
using GigRaptorLib.Models;
using System.Text.RegularExpressions;

namespace GigRaptorLib.Utilities;

public static class HeaderHelper
{
    public static Dictionary<int, string> ParserHeader(IList<object> sheetHeader)
    {
        var headerValues = new Dictionary<int, string>();

        foreach (var item in sheetHeader.Select((value, index) => new { index, value }))
        {
            headerValues.Add(item.index, item.value.ToString().Trim());
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

        return values[columnId].ToString().Trim().ToUpper() == "TRUE";
    }

    public static string GetDateValue(string columnName, IList<object> values, Dictionary<int, string> headers)
    {
        var columnId = GetHeaderKey(headers, columnName);

        if (columnId > values.Count || columnId < 0 || values.Count == 0 || string.IsNullOrEmpty(values[columnId].ToString()))
        {
            return "";
        }

        return DateTime.Parse(values[columnId].ToString()).ToString("yyyy-MM-dd");
    }

    public static string GetStringValue(string columnName, IList<object> values, Dictionary<int, string> headers)
    {
        var columnId = GetHeaderKey(headers, columnName);

        if (columnId > values.Count || columnId < 0 || values[columnId] == null)
        {
            return "";
        }

        return values[columnId].ToString().Trim();
    }

    public static int GetIntValue(string columnName, IList<object> values, Dictionary<int, string> headers)
    {
        var columnId = GetHeaderKey(headers, columnName);

        if (columnId > values.Count || columnId < 0 || values[columnId] == null)
        {
            return 0;
        }

        var value = values[columnId].ToString().Trim();
        value = Regex.Replace(value, @"[^\d]", ""); // Remove all special symbols.
        if (value == "")
        {
            return 0; // Make empty into 0s.
        }

        int.TryParse(value, out int result);

        return result;
    }

    public static decimal GetDecimalValue(string columnName, IList<object> values, Dictionary<int, string> headers)
    {
        var columnId = GetHeaderKey(headers, columnName);

        if (columnId > values.Count || columnId < 0 || values[columnId] == null)
        {
            return 0;
        }

        var value = values[columnId].ToString().Trim();
        value = Regex.Replace(value, @"[^\d.-]", ""); // Remove all special currency symbols except for .'s and -'s
        if (value == "-" || value == "")
        {
            value = "0";  // Make account -'s into 0s.
        }
        // Console.WriteLine(columnName);
        // Console.WriteLine(value);

        decimal.TryParse(value, out decimal result);

        return result;
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
                messages.Add(new MessageEntity { Message = $"Sheet [{sheetModel.Name}]: Missing [{sheetHeader.Name}]", Type = Enums.MessageEnum.Error });
            }
            else
            {
                if (index < headerArray.Count() && sheetHeader.Name != headerArray[index].Trim())
                {
                    messages.Add(new MessageEntity { Message = $"Sheet [{sheetModel.Name}]: Expected [{sheetHeader.Name}] but found [{headerArray[index].Trim()}]", Type = Enums.MessageEnum.Warning });
                }
            }
            index++;
        }

        return messages;
    }
}