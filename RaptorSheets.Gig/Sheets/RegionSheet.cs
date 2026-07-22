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
/// Region sheet definition - layout and formulas for the Regions sheet.
/// For data mapping operations, use GenericSheetMapper&lt;RegionEntity&gt; directly.
/// </summary>
public static class RegionSheet
{
    internal static SheetModel BaseSheet => new()
    {
        Name = SheetsConfig.SheetNames.Regions,
        TabColor = SheetColor.CYAN,
        CellColor = SheetColor.LIGHT_CYAN,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<RegionEntity>()
    };

    public static SheetModel GetSheet()
    {
        var sheet = BaseSheet;
        sheet.Headers.UpdateColumns();

        var shiftSheet = ShiftSheet.GetSheet();
        var keyRange = sheet.GetLocalRange(Header.REGION.GetDescription());
        var shiftKeyRange = shiftSheet.GetRange(Header.REGION.GetDescription());

        // Configure common aggregation patterns (eliminates ~80% of duplication)
        MapperFormulaHelper.ConfigureCommonAggregationHeaders(sheet, keyRange, shiftSheet, shiftKeyRange, useShiftTotals: true);

        // Configure common ratio calculations
        MapperFormulaHelper.ConfigureCommonRatioHeaders(sheet, keyRange);

        // Configure specific headers unique to RegionSheet
        sheet.Headers.ForEach(header =>
        {
            var headerEnum = header!.Name.ToString()!.Trim().GetValueFromName<Header>();

            switch (headerEnum)
            {
                case Header.REGION:
                    header.Formula = GoogleFormulaBuilder.BuildArrayLiteralUniqueCombinedFiltered(
                        Header.REGION.GetDescription(),
                        TripSheet.GetSheet().GetRange(Header.REGION.GetDescription(), 2),
                        ShiftSheet.GetSheet().GetRange(Header.REGION.GetDescription(), 2));
                    break;
                case Header.VISIT_FIRST:
                    header.Formula = GigFormulaBuilder.Common.BuildVisitDateLookup(keyRange, Header.VISIT_FIRST.GetDescription(),
                        SheetName.SHIFTS.GetDescription(),
                        shiftSheet.GetColumn(Header.DATE.GetDescription()),
                        shiftSheet.GetColumn(Header.REGION.GetDescription()), true);
                    header.Note = ColumnNotes.DateFormat;
                    header.Format = Format.DATE;
                    break;
                case Header.VISIT_LAST:
                    header.Formula = GigFormulaBuilder.Common.BuildVisitDateLookup(keyRange, Header.VISIT_LAST.GetDescription(),
                        SheetName.SHIFTS.GetDescription(),
                        shiftSheet.GetColumn(Header.DATE.GetDescription()),
                        shiftSheet.GetColumn(Header.REGION.GetDescription()), false);
                    header.Note = ColumnNotes.DateFormat;
                    header.Format = Format.DATE;
                    break;
            }
        });

        return sheet;
    }
}
