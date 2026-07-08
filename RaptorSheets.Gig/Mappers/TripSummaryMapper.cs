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
        // Use the centralized SheetsConfig model to avoid configuration drift
        var sheet = SheetsConfig.TripSummary;

        // Ensure header indexes are assigned
        sheet.Headers.UpdateColumns();

        // Keep one real header, pad the rest up to the original header count
        sheet.Headers.EnsureHeaderPlaceholders(1);

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
