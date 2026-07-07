using RaptorSheets.Core.Helpers;
using System.Collections.Generic;
using RaptorSheets.Core.Extensions;
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

        // Keep one real header, pad the rest up to the original header count
        sheet.Headers.EnsureHeaderPlaceholders(1);

        var tripSheet = SheetsConfig.TripSheet;
        tripSheet.Headers.UpdateColumns();

        var placeRange = tripSheet.GetRange(SheetsConfig.HeaderNames.Place, 2);
        var addressRange = tripSheet.GetRange(SheetsConfig.HeaderNames.AddressStart, 2);

        var query = GoogleFormulaBuilder.BuildQueryGroupTwoColumns(
            SheetsConfig.HeaderNames.Place,
            SheetsConfig.HeaderNames.Address,
            placeRange,
            addressRange,
            SheetsConfig.HeaderNames.Count,
            countColumnIsSecond: true);

        if (sheet.Headers.Count > 0)
        {
            sheet.Headers[0].Formula = query;
        }

        return sheet;
    }
}
