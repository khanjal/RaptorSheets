using Google.Apis.Sheets.v4.Data;
using Newtonsoft.Json;
using RLE.Core.Models.Google;

namespace RLE.Core.Tests.Data.Helpers;

public class JsonHelpers
{

    public static Spreadsheet? LoadDemoSpreadsheet()
    {
        using StreamReader reader = new($"./Data/Json/DemoSheet.json");
        var json = reader.ReadToEnd();
        var sheetData = JsonConvert.DeserializeObject<Spreadsheet>(json);

        return sheetData;
    }

    public static IList<IList<object>>? LoadJsonData(string filename)
    {
        var path = $"./Data/Json/{filename}.json";
        var values = ReadJson(path);

        return values;
    }

    public static IList<IList<object>>? LoadJsonSheetData(string sheet)
    {
        var path = $"./Data/Json/Sheets/{sheet}Sheet.json";
        var values = ReadJson(path);

        return values;
    }

    private static IList<IList<object>>? ReadJson(string path)
    {
        using StreamReader reader = new(path);
        var json = reader.ReadToEnd();
        var values = JsonConvert.DeserializeObject<GoogleResponse>(json)?.Values;

        return values;
    }
}
