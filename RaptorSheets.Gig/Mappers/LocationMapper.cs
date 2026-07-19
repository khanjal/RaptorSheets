using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Helpers;

namespace RaptorSheets.Gig.Mappers;

public static class LocationMapper
{
    public static SheetModel GetSheet()
    {
        // Use the centralized SheetsConfig model to avoid configuration drift
        var sheet = SheetsConfig.Locations;

        sheet.Headers.UpdateColumns();

        // Keep one real header, pad the rest up to the original header count
        sheet.Headers.EnsureHeaderPlaceholders(1);

        var tripSheet = SheetsConfig.TripSheet;
        tripSheet.Headers.UpdateColumns();

        var placeRange = tripSheet.GetRange(SheetsConfig.HeaderNames.Place, 2);
        var addressRange = tripSheet.GetRange(SheetsConfig.HeaderNames.AddressStart, 2);

        var sumColumns = new[]
        {
            (SheetsConfig.HeaderNames.Pay, tripSheet.GetRange(SheetsConfig.HeaderNames.Pay, 2)),
            (SheetsConfig.HeaderNames.Tips, tripSheet.GetRange(SheetsConfig.HeaderNames.Tips, 2)),
            (SheetsConfig.HeaderNames.Bonus, tripSheet.GetRange(SheetsConfig.HeaderNames.Bonus, 2)),
            (SheetsConfig.HeaderNames.Total, tripSheet.GetRange(SheetsConfig.HeaderNames.Total, 2)),
            (SheetsConfig.HeaderNames.Distance, tripSheet.GetRange(SheetsConfig.HeaderNames.Distance, 2))
        };

        var query = GoogleFormulaBuilder.BuildQueryGroupTwoColumns(
            SheetsConfig.HeaderNames.Place,
            SheetsConfig.HeaderNames.Address,
            placeRange,
            addressRange,
            SheetsConfig.HeaderNames.Trips,
            sumColumns,
            countColumnIsSecond: true);

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
                sheet.GetLocalRange(SheetsConfig.HeaderNames.Place),
                SheetsConfig.HeaderNames.AmountPerTrip,
                sheet.GetLocalRange(SheetsConfig.HeaderNames.Total),
                sheet.GetLocalRange(SheetsConfig.HeaderNames.Trips));
        }

        var amountPerDistanceHeader = sheet.Headers.FirstOrDefault(h => h.Name == SheetsConfig.HeaderNames.AmountPerDistance);
        if (amountPerDistanceHeader != null)
        {
            amountPerDistanceHeader.HideHeaderName = false;
            amountPerDistanceHeader.Formula = GigFormulaBuilder.BuildArrayFormulaAmountPerDistance(
                sheet.GetLocalRange(SheetsConfig.HeaderNames.Place),
                SheetsConfig.HeaderNames.AmountPerDistance,
                sheet.GetLocalRange(SheetsConfig.HeaderNames.Total),
                sheet.GetLocalRange(SheetsConfig.HeaderNames.Distance));
        }

        return sheet;
    }
}
