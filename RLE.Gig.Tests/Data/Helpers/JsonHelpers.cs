using Google.Apis.Sheets.v4.Data;
using Newtonsoft.Json;
using RLE.Core.Entities;
using RLE.Core.Models.Google;

namespace RLE.Gig.Tests.Data.Helpers;

internal class JsonHelpers
{
    internal static SheetEntity? LoadSheetJson()
    {
        using StreamReader reader = new($"./Data/Json/ShiftWithTrips.json");
        var json = reader.ReadToEnd();
        var sheetData = JsonConvert.DeserializeObject<SheetEntity>(json);

        return sheetData;
    }

    internal static Spreadsheet? LoadDemoSpreadsheet()
    {
        using StreamReader reader = new($"./Data/Json/DemoSheet.json");
        var json = reader.ReadToEnd();
        var sheetData = JsonConvert.DeserializeObject<Spreadsheet>(json);

        return sheetData;
    }

    internal static IList<IList<object>>? LoadJsonData(string filename)
    {
        var path = $"./Data/Json/{filename}.json";
        var values = ReadJson(path);

        return values;
    }

    internal static IList<IList<object>>? LoadJsonSheetData(string sheet)
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
