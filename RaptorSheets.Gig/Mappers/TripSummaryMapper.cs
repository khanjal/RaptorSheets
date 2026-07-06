using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;

namespace RaptorSheets.Gig.Mappers;

public static class TripSummaryMapper
{
    public static SheetModel GetSheet()
    {
        var sheet = new SheetModel
        {
            Name = SheetsConfig.SheetNames.TripSummary,
            TabColor = RaptorSheets.Core.Enums.ColorEnum.CYAN,
            CellColor = RaptorSheets.Core.Enums.ColorEnum.LIGHT_CYAN,
            FreezeColumnCount = 1,
            FreezeRowCount = 1,
            ProtectSheet = true,
            Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<TripSummaryEntity>()
        };

        // Ensure header indexes are assigned
        sheet.Headers.UpdateColumns();

        // Build the single QUERY formula that produces Name|Address|Count from Trips
        var tripSheet = SheetsConfig.TripSheet;
        tripSheet.Headers.UpdateColumns();

        var nameRange = tripSheet.GetRange(SheetsConfig.HeaderNames.Name, 2);
        var endAddressRange = tripSheet.GetRange(SheetsConfig.HeaderNames.AddressEnd, 2);

        var query = $"=VSTACK({{"\"Name\"","\"Address\"","\"Count\""}},QUERY({{{nameRange},{endAddressRange}}},\"select Col1, Col2, count(Col1) where Col1 is not null and Col2 is not null group by Col1, Col2 order by Col1 asc, count(Col1) desc label Col1 'Name', Col2 'Address', count(Col1) 'Count'\",0))";

        // Place formula in first header so it will spill across columns
        if (sheet.Headers.Count > 0)
        {
            sheet.Headers[0].Formula = query;
        }

        return sheet;
    }
}
