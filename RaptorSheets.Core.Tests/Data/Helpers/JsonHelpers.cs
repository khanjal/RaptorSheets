using Google.Apis.Sheets.v4.Data;
using Newtonsoft.Json;
using RaptorSheets.Core.Models.Google;

namespace RaptorSheets.Core.Tests.Data.Helpers;

public class JsonHelpers
{

    public static Spreadsheet? LoadDemoSpreadsheet()
    {
        var path = GetDataJsonPath("DemoSheet");
        var json = ReadJson(path);
        var sheetData = JsonConvert.DeserializeObject<Spreadsheet>(json);

        return sheetData;
    }

    public static IList<IList<object>>? LoadJsonData(string filename)
    {
        var path = GetDataJsonPath(filename);
        var json = ReadJson(path);
        var values = DeserializeJson<GoogleResponse>(json)?.Values;

        return values;
    }

    public static IList<IList<object>>? LoadJsonSheetData(string sheet)
    {
        var path = GetDataJsonPath($"Sheets/{sheet}Sheet");
        var json = ReadJson(path);
        var values = DeserializeJson<GoogleResponse>(json)?.Values;

        return values;
    }

    public static string GetDataJsonPath(string filename)
    {
        return $"./Data/Json/{filename}.json";
    }

    public static string ReadJson(string path)
    {
        using StreamReader reader = new(path);
        var json = reader.ReadToEnd();

        return json;
    }

    public static T DeserializeJson<T>(string json)
    {
        var values = JsonConvert.DeserializeObject<T>(json);

        return values ?? default;
    }
}
