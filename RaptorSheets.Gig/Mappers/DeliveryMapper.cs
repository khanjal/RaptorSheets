using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Helpers;

namespace RaptorSheets.Gig.Mappers;

public static class DeliveryMapper
{
    public static SheetModel GetSheet()
    {
        // Use the centralized SheetsConfig model to avoid configuration drift
        var sheet = SheetsConfig.Deliveries;

        // Ensure header indexes are assigned
        sheet.Headers.UpdateColumns();

        // Keep one real header, pad the rest up to the original header count
        sheet.Headers.EnsureHeaderPlaceholders(1);

        // Build the single QUERY formula that produces Name|Address|Trips|Pay|Tips|Bonus|Total|Dist from Trips
        var tripSheet = SheetsConfig.TripSheet;
        tripSheet.Headers.UpdateColumns();

        var nameRange = tripSheet.GetRange(SheetsConfig.HeaderNames.Name, 2);
        var endAddressRange = tripSheet.GetRange(SheetsConfig.HeaderNames.AddressEnd, 2);

        var sumColumns = new[]
        {
            (SheetsConfig.HeaderNames.Pay, tripSheet.GetRange(SheetsConfig.HeaderNames.Pay, 2)),
            (SheetsConfig.HeaderNames.Tips, tripSheet.GetRange(SheetsConfig.HeaderNames.Tips, 2)),
            (SheetsConfig.HeaderNames.Bonus, tripSheet.GetRange(SheetsConfig.HeaderNames.Bonus, 2)),
            (SheetsConfig.HeaderNames.Total, tripSheet.GetRange(SheetsConfig.HeaderNames.Total, 2)),
            (SheetsConfig.HeaderNames.Distance, tripSheet.GetRange(SheetsConfig.HeaderNames.Distance, 2))
        };

        var query = GoogleFormulaBuilder.BuildQueryGroupTwoColumns(
            SheetsConfig.HeaderNames.Name,
            SheetsConfig.HeaderNames.Address,
            nameRange,
            endAddressRange,
            SheetsConfig.HeaderNames.Trips,
            sumColumns
        );

        // Place formula in first header so it will spill across columns
        if (sheet.Headers.Count > 0)
        {
            sheet.Headers[0].Formula = query;
        }

        // Amt/Trip and Amt/Dist are derived from the sheet's own spilled Trips/Total/Distance columns,
        // so they're separate self-referencing ARRAYFORMULAs rather than part of the QUERY spill.
        // EnsureHeaderPlaceholders(1) marks every header past index 0 as HideHeaderName, which also
        // suppresses writing the cell's UserEnteredValue entirely (see SheetHelpers.HeadersToRowData) -
        // these two headers need that flag cleared or their formulas never make it onto the sheet.
        var amountPerTripHeader = sheet.Headers.FirstOrDefault(h => h.Name == SheetsConfig.HeaderNames.AmountPerTrip);
        if (amountPerTripHeader != null)
        {
            amountPerTripHeader.HideHeaderName = false;
            amountPerTripHeader.Formula = GigFormulaBuilder.BuildArrayFormulaAmountPerTrip(
                sheet.GetLocalRange(SheetsConfig.HeaderNames.Name),
                SheetsConfig.HeaderNames.AmountPerTrip,
                sheet.GetLocalRange(SheetsConfig.HeaderNames.Total),
                sheet.GetLocalRange(SheetsConfig.HeaderNames.Trips));
        }

        var amountPerDistanceHeader = sheet.Headers.FirstOrDefault(h => h.Name == SheetsConfig.HeaderNames.AmountPerDistance);
        if (amountPerDistanceHeader != null)
        {
            amountPerDistanceHeader.HideHeaderName = false;
            amountPerDistanceHeader.Formula = GigFormulaBuilder.BuildArrayFormulaAmountPerDistance(
                sheet.GetLocalRange(SheetsConfig.HeaderNames.Name),
                SheetsConfig.HeaderNames.AmountPerDistance,
                sheet.GetLocalRange(SheetsConfig.HeaderNames.Total),
                sheet.GetLocalRange(SheetsConfig.HeaderNames.Distance));
        }

        return sheet;
    }
}
