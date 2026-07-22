using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Helpers;

namespace RaptorSheets.Gig.Sheets;

/// <summary>
/// Delivery sheet definition - a single QUERY-spill summary sheet grouped by Name+Address.
/// </summary>
public static class DeliverySheet
{
    internal static SheetModel BaseSheet => new()
    {
        Name = SheetsConfig.SheetNames.Deliveries,
        TabColor = SheetColor.BLUE,
        CellColor = SheetColor.LIGHT_GRAY,
        FontColor = SheetColor.WHITE,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<DeliveryEntity>()
    };

    public static SheetModel GetSheet()
    {
        var sheet = BaseSheet;

        // Ensure header indexes are assigned
        sheet.Headers.UpdateColumns();

        // Keep one real header, pad the rest up to the original header count
        sheet.Headers.EnsureHeaderPlaceholders(1);

        // Build the single QUERY formula that produces
        // Name|Address|Trips|Pay|Tips|Bonus|Total|Dist|First Trip|Last Trip from Trips
        var tripSheet = TripSheet.GetSheet();

        var nameRange = tripSheet.GetRange(SheetsConfig.HeaderNames.Name, 2);
        var endAddressRange = tripSheet.GetRange(SheetsConfig.HeaderNames.AddressEnd, 2);
        var dateRange = tripSheet.GetRange(SheetsConfig.HeaderNames.Date, 2);

        var aggregateColumns = new (string Header, string Range, string AggregateFunction)[]
        {
            (SheetsConfig.HeaderNames.Pay, tripSheet.GetRange(SheetsConfig.HeaderNames.Pay, 2), "sum"),
            (SheetsConfig.HeaderNames.Tips, tripSheet.GetRange(SheetsConfig.HeaderNames.Tips, 2), "sum"),
            (SheetsConfig.HeaderNames.Bonus, tripSheet.GetRange(SheetsConfig.HeaderNames.Bonus, 2), "sum"),
            (SheetsConfig.HeaderNames.Total, tripSheet.GetRange(SheetsConfig.HeaderNames.Total, 2), "sum"),
            (SheetsConfig.HeaderNames.Distance, tripSheet.GetRange(SheetsConfig.HeaderNames.Distance, 2), "sum"),
            (SheetsConfig.HeaderNames.VisitFirst, dateRange, "min"),
            (SheetsConfig.HeaderNames.VisitLast, dateRange, "max")
        };

        var query = GoogleFormulaBuilder.BuildQueryGroupTwoColumns(
            SheetsConfig.HeaderNames.Name,
            SheetsConfig.HeaderNames.Address,
            nameRange,
            endAddressRange,
            SheetsConfig.HeaderNames.Trips,
            aggregateColumns
        );

        // Place formula in first header so it will spill across columns
        if (sheet.Headers.Count > 0)
        {
            sheet.Headers[0].Formula = query;
        }

        var firstTripHeader = sheet.Headers.FirstOrDefault(h => h.Name == SheetsConfig.HeaderNames.VisitFirst);
        if (firstTripHeader != null)
        {
            firstTripHeader.Format = Format.DATE;
        }

        var lastTripHeader = sheet.Headers.FirstOrDefault(h => h.Name == SheetsConfig.HeaderNames.VisitLast);
        if (lastTripHeader != null)
        {
            lastTripHeader.Format = Format.DATE;
        }

        // Amt/Trip and Amt/Dist are derived from the sheet's own spilled Trips/Total/Distance
        // columns, so they're separate self-referencing ARRAYFORMULAs rather than part of the
        // QUERY spill (a QUERY's spilled array is contiguous - it can't leave gaps for other
        // formulas in the middle and resume after, so they have to come after everything the
        // query itself produces, including First Trip/Last Trip).
        // EnsureHeaderPlaceholders(1) marks every header past index 0 as HideHeaderName, which also
        // suppresses writing the cell's UserEnteredValue entirely (see SheetHelpers.HeadersToRowData) -
        // these headers need that flag cleared or their formulas never make it onto the sheet.
        var nameKeyRange = sheet.GetLocalRange(SheetsConfig.HeaderNames.Name);

        var amountPerTripHeader = sheet.Headers.FirstOrDefault(h => h.Name == SheetsConfig.HeaderNames.AmountPerTrip);
        if (amountPerTripHeader != null)
        {
            amountPerTripHeader.HideHeaderName = false;
            amountPerTripHeader.Formula = GigFormulaBuilder.BuildArrayFormulaAmountPerTrip(
                nameKeyRange,
                SheetsConfig.HeaderNames.AmountPerTrip,
                sheet.GetLocalRange(SheetsConfig.HeaderNames.Total),
                sheet.GetLocalRange(SheetsConfig.HeaderNames.Trips));
        }

        var amountPerDistanceHeader = sheet.Headers.FirstOrDefault(h => h.Name == SheetsConfig.HeaderNames.AmountPerDistance);
        if (amountPerDistanceHeader != null)
        {
            amountPerDistanceHeader.HideHeaderName = false;
            amountPerDistanceHeader.Formula = GigFormulaBuilder.BuildArrayFormulaAmountPerDistance(
                nameKeyRange,
                SheetsConfig.HeaderNames.AmountPerDistance,
                sheet.GetLocalRange(SheetsConfig.HeaderNames.Total),
                sheet.GetLocalRange(SheetsConfig.HeaderNames.Distance));
        }

        return sheet;
    }
}
