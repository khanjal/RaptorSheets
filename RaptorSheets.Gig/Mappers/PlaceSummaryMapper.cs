using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;

namespace RaptorSheets.Gig.Mappers;

public static class PlaceSummaryMapper
{
    public static SheetModel GetSheet()
    {
        var sheet = new SheetModel
        {
            Name = SheetsConfig.SheetNames.PlaceSummary,
            TabColor = RaptorSheets.Core.Enums.ColorEnum.CYAN,
            CellColor = RaptorSheets.Core.Enums.ColorEnum.LIGHT_CYAN,
            FreezeColumnCount = 1,
            FreezeRowCount = 1,
            ProtectSheet = true,
            Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<PlaceSummaryEntity>()
        };

        sheet.Headers.UpdateColumns();

        var tripSheet = SheetsConfig.TripSheet;
        tripSheet.Headers.UpdateColumns();

        var placeRange = tripSheet.GetRange(SheetsConfig.HeaderNames.Place, 2);
        var startAddressRange = tripSheet.GetRange(SheetsConfig.HeaderNames.AddressStart, 2);

        var query = $"=VSTACK({{"\"Place\"","\"Start Address\"","\"Count\""}},QUERY({{{placeRange},{startAddressRange}}},\"select Col1, Col2, count(Col2) where Col1 is not null and Col2 is not null group by Col1, Col2 order by Col1 asc, count(Col2) desc label Col1 'Place', Col2 'Start Address', count(Col2) 'Count'\",0))";

        if (sheet.Headers.Count > 0)
        {
            sheet.Headers[0].Formula = query;
        }

        return sheet;
    }
}
