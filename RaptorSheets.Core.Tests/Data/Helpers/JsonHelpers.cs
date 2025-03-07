using Google.Apis.Sheets.v4.Data;
using Newtonsoft.Json;
using RaptorSheets.Core.Models.Google;
using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Core.Tests.Data.Helpers;

[ExcludeFromCodeCoverage]
public class JsonHelpers
{
    public static T? LoadJsonData<T>(string filename)
    {
        var path = GetDataJsonPath(filename);
        var json = ReadJson(path);
        var values = DeserializeJson<T>(json);

        return values;
    }

    public static Spreadsheet? LoadDemoSpreadsheet()
    {
        var filename = "DemoSheet";
        var sheetData = LoadJsonData<Spreadsheet>(filename);

        return sheetData;
    }


    public static IList<IList<object>>? LoadJsonSheetData(string sheet)
    {
        var filename = $"Sheets/{sheet}Sheet";
        var json = LoadJsonData<GoogleResponse>(filename);
        var values = json?.Values;

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

    public static T? DeserializeJson<T>(string json)
    {
        var values = JsonConvert.DeserializeObject<T>(json);

        return values;
    }
}
