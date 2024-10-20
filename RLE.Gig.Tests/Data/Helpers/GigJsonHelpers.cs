using RLE.Core.Tests.Data.Helpers;
using RLE.Gig.Entities;

namespace RLE.Gig.Tests.Data.Helpers;

internal class GigJsonHelpers
{
    internal static SheetEntity? LoadSheetJson()
    {
        var path = JsonHelpers.GetDataJsonPath("ShiftWithTrips");
        var json = JsonHelpers.ReadJson(path);
        var sheetData = JsonHelpers.DeserializeJson<SheetEntity>(json);

        return sheetData;
    }
}
