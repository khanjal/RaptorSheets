using RaptorSheets.Core.Tests.Data.Helpers;
using RaptorSheets.Gig.Entities;

namespace RaptorSheets.Gig.Tests.Data.Helpers;

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
