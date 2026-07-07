using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Extensions;
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

        // For this summary the QUERY will provide the full header row (labels),
        // so only keep the first header cell for placing the spilling formula
        if (sheet.Headers.Count > 1)
        {
            var firstHeader = sheet.Headers[0];
            sheet.Headers.Clear();
            sheet.Headers.Add(firstHeader);
            sheet.Headers.UpdateColumns();
        }

        // Build the single QUERY formula that produces Name|Address|Count from Trips
        var tripSheet = SheetsConfig.TripSheet;
        tripSheet.Headers.UpdateColumns();

        var nameRange = tripSheet.GetRange(SheetsConfig.HeaderNames.Name, 2);
        var endAddressRange = tripSheet.GetRange(SheetsConfig.HeaderNames.AddressEnd, 2);

        var query = GoogleFormulaBuilder.BuildQueryGroupTwoColumns(
            SheetsConfig.HeaderNames.Name,
            SheetsConfig.HeaderNames.Address,
            nameRange,
            endAddressRange,
            SheetsConfig.HeaderNames.Count
        );

        // Place formula in first header so it will spill across columns
        if (sheet.Headers.Count > 0)
        {
            sheet.Headers[0].Formula = query;
        }

        return sheet;
    }
}
