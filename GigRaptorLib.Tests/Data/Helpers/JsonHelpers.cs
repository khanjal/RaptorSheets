using GigRaptorLib.Models;
using Newtonsoft.Json;

namespace GigRaptorLib.Tests.Data.Helpers;

internal class JsonHelpers
{
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
