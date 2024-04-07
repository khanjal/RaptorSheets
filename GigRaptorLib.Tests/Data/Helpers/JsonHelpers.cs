using GigRaptorLib.Models;
using Newtonsoft.Json;

namespace GigRaptorLib.Tests.Data.Helpers;

internal class JsonHelpers
{
    internal static IList<IList<object>>? LoadJson(string sheet)
    {
        using StreamReader reader = new($"./Data/Json/Sheets/{sheet}Sheet.json");
        var json = reader.ReadToEnd();
        var values = JsonConvert.DeserializeObject<GoogleResponse>(json)?.Values;

        return values;
    }
}
