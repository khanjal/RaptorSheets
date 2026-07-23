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
/// Service sheet definition - layout and formulas for the Services sheet.
/// For data mapping operations, use GenericSheetMapper&lt;ServiceEntity&gt; directly.
/// </summary>
public static class ServiceSheet
{
    internal static SheetModel BaseSheet => new()
    {
        Name = SheetsConfig.SheetNames.Services,
        TabColor = SheetColor.CYAN,
        CellColor = SheetColor.LIGHT_CYAN,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<ServiceEntity>()
    };

    public static SheetModel GetSheet()
    {
        var sheet = BaseSheet;
        sheet.Headers.UpdateColumns();

        var shiftSheet = ShiftSheet.GetSheet();
        var keyRange = sheet.GetLocalRange(Header.SERVICE.GetDescription());
        var shiftKeyRange = shiftSheet.GetRange(Header.SERVICE.GetDescription());

        // Configure common aggregation patterns
        MapperFormulaHelper.ConfigureCommonAggregationHeaders(sheet, keyRange, shiftSheet, shiftKeyRange, useShiftTotals: true);

        // Configure common ratio calculations
        MapperFormulaHelper.ConfigureCommonRatioHeaders(sheet, keyRange);

        // Configure specific headers unique to ServiceSheet
        sheet.Headers.ForEach(header =>
        {
            var headerEnum = header!.Name.ToString()!.Trim().GetValueFromName<Header>();

            switch (headerEnum)
            {
                case Header.SERVICE:
                    // Combine services from both trips and shifts
                    header.Formula = GoogleFormulaBuilder.BuildArrayLiteralUniqueCombined(
                        Header.SERVICE.GetDescription(),
                        TripSheet.GetSheet().GetRange(Header.SERVICE.GetDescription(), 2),
                        ShiftSheet.GetSheet().GetRange(Header.SERVICE.GetDescription(), 2));
                    break;
                case Header.VISIT_FIRST:
                    header.Formula = GigFormulaBuilder.Common.BuildVisitDateLookup(keyRange, Header.VISIT_FIRST.GetDescription(),
                        SheetName.SHIFTS.GetDescription(),
                        shiftSheet.GetColumn(Header.DATE.GetDescription()),
                        shiftSheet.GetColumn(Header.SERVICE.GetDescription()), true);
                    header.Note = ColumnNotes.DateFormat;
                    header.Format = Format.DATE;
                    break;
                case Header.VISIT_LAST:
                    header.Formula = GigFormulaBuilder.Common.BuildVisitDateLookup(keyRange, Header.VISIT_LAST.GetDescription(),
                        SheetName.SHIFTS.GetDescription(),
                        shiftSheet.GetColumn(Header.DATE.GetDescription()),
                        shiftSheet.GetColumn(Header.SERVICE.GetDescription()), false);
                    header.Note = ColumnNotes.DateFormat;
                    header.Format = Format.DATE;
                    break;
            }
        });

        return sheet;
    }
}
