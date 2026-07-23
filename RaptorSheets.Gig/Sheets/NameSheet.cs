using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Helpers;

namespace RaptorSheets.Gig.Sheets;

/// <summary>
/// Name sheet definition - layout and formulas for the Names sheet.
/// For data mapping operations, use GenericSheetMapper&lt;NameEntity&gt; directly.
/// </summary>
public static class NameSheet
{
    internal static SheetModel BaseSheet => new()
    {
        Name = SheetsConfig.SheetNames.Names,
        TabColor = SheetColor.CYAN,
        CellColor = SheetColor.LIGHT_CYAN,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<NameEntity>()
    };

    public static SheetModel GetSheet()
    {
        var sheet = BaseSheet;
        sheet.Headers.UpdateColumns();

        var tripSheet = TripSheet.GetSheet();
        var keyRange = sheet.GetLocalRange(Header.NAME.GetDescription());
        var tripKeyRange = tripSheet.GetRange(Header.NAME.GetDescription());

        // Configure common aggregation patterns (for trip-based data)
        MapperFormulaHelper.ConfigureCommonAggregationHeaders(
            sheet,
            keyRange,
            tripSheet,
            tripKeyRange,
            countTrips: true);  // Count individual trip occurrences

        // Configure common ratio calculations
        MapperFormulaHelper.ConfigureCommonRatioHeaders(sheet, keyRange);

        // Configure specific headers unique to NameSheet
        sheet.Headers.ForEach(header =>
        {
            var headerEnum = header!.Name.ToString()!.Trim().GetValueFromName<Header>();

            switch (headerEnum)
            {
                case Header.NAME:
                    MapperFormulaHelper.ConfigureUniqueValueHeader(header, tripSheet.GetRange(Header.NAME.GetDescription(), 2));
                    break;
                case Header.VISIT_FIRST:
                    header.Formula = GigFormulaBuilder.Common.BuildVisitDateLookup(keyRange, Header.VISIT_FIRST.GetDescription(),
                        SheetName.TRIPS.GetDescription(),
                        tripSheet.GetColumn(Header.DATE.GetDescription()),
                        tripSheet.GetColumn(Header.NAME.GetDescription()), true);
                    header.Format = Format.DATE;
                    break;
                case Header.VISIT_LAST:
                    header.Formula = GigFormulaBuilder.Common.BuildVisitDateLookup(keyRange, Header.VISIT_LAST.GetDescription(),
                        SheetName.TRIPS.GetDescription(),
                        tripSheet.GetColumn(Header.DATE.GetDescription()),
                        tripSheet.GetColumn(Header.NAME.GetDescription()), false);
                    header.Format = Format.DATE;
                    break;
            }
        });

        return sheet;
    }
}
