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
/// Monthly sheet definition - layout and formulas for the Monthly sheet.
/// For data mapping operations, use GenericSheetMapper&lt;MonthlyEntity&gt; directly.
/// </summary>
public static class MonthlySheet
{
    internal static SheetModel BaseSheet => new()
    {
        Name = SheetsConfig.SheetNames.Monthly,
        TabColor = SheetColor.LIGHT_GREEN,
        CellColor = SheetColor.LIGHT_GRAY,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<MonthlyEntity>()
    };

    public static SheetModel GetSheet()
    {
        var sheet = BaseSheet;
        sheet.Headers.UpdateColumns();

        var dailySheet = DailySheet.GetSheet();
        var keyRange = sheet.GetLocalRange(Header.MONTH.GetDescription());
        var dailyKeyRange = dailySheet.GetRange(Header.MONTH.GetDescription());

        // Configure common aggregation patterns (eliminates major duplication)
        MapperFormulaHelper.ConfigureCommonAggregationHeaders(sheet, keyRange, dailySheet, dailyKeyRange, useShiftTotals: false);

        // Configure common ratio calculations
        MapperFormulaHelper.ConfigureCommonRatioHeaders(sheet, keyRange);

        // Configure specific headers unique to MonthlySheet
        sheet.Headers.ForEach(header =>
        {
            var headerEnum = header.Name.GetValueFromName<Header>();

            switch (headerEnum)
            {
                case Header.MONTH:
                    header.Formula = GoogleFormulaBuilder.BuildArrayLiteralUniqueFiltered(Header.MONTH.GetDescription(), dailySheet.GetRange(Header.MONTH.GetDescription(), 2));
                    break;
                case Header.AVERAGE:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaRollingAverage(keyRange, Header.AVERAGE.GetDescription(), sheet.GetLocalRange(Header.TOTAL.GetDescription()));
                    header.Format = Format.ACCOUNTING;
                    break;
                case Header.NUMBER:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSplitByIndex(keyRange, Header.NUMBER.GetDescription(), keyRange, "-", 1);
                    break;
                case Header.YEAR:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSplitByIndex(keyRange, Header.YEAR.GetDescription(), keyRange, "-", 2);
                    break;
            }
        });

        return sheet;
    }
}
