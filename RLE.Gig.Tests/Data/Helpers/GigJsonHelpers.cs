using Newtonsoft.Json;
using RLE.Gig.Entities;

namespace RLE.Gig.Tests.Data.Helpers;

internal class GigJsonHelpers
{
    internal static SheetEntity? LoadSheetJson()
    {
        using StreamReader reader = new($"./Data/Json/ShiftWithTrips.json");
        var json = reader.ReadToEnd();
        var sheetData = JsonConvert.DeserializeObject<SheetEntity>(json);

        return sheetData;
    }
}
