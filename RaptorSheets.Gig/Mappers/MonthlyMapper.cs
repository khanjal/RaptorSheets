using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Helpers;

namespace RaptorSheets.Gig.Mappers;

/// <summary>
/// Monthly mapper for Monthly sheet configuration and formulas.
/// For data mapping operations, use GenericSheetMapper&lt;MonthlyEntity&gt; directly.
/// </summary>
public static class MonthlyMapper
{
    public static SheetModel GetSheet()
    {
        var sheet = SheetsConfig.MonthlySheet;
        sheet.Headers.UpdateColumns();

        var dailySheet = DailyMapper.GetSheet();
        var keyRange = sheet.GetLocalRange(HeaderEnum.MONTH.GetDescription());
        var dailyKeyRange = dailySheet.GetRange(HeaderEnum.MONTH.GetDescription());

        // Configure common aggregation patterns (eliminates major duplication)
        MapperFormulaHelper.ConfigureCommonAggregationHeaders(sheet, keyRange, dailySheet, dailyKeyRange, useShiftTotals: false);
        
        // Configure common ratio calculations
        MapperFormulaHelper.ConfigureCommonRatioHeaders(sheet, keyRange);

        // Configure specific headers unique to MonthlyMapper
        sheet.Headers.ForEach(header =>
        {
            var headerEnum = header.Name.GetValueFromName<HeaderEnum>();

            switch (headerEnum)
            {
                case HeaderEnum.MONTH:
                    header.Formula = GoogleFormulaBuilder.BuildArrayLiteralUniqueFiltered(HeaderEnum.MONTH.GetDescription(), dailySheet.GetRange(HeaderEnum.MONTH.GetDescription(), 2));
                    break;
                case HeaderEnum.AVERAGE:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaRollingAverage(keyRange, HeaderEnum.AVERAGE.GetDescription(), sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription()));
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                case HeaderEnum.NUMBER:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSplitByIndex(keyRange, HeaderEnum.NUMBER.GetDescription(), keyRange, "-", 1);
                    break;
                case HeaderEnum.YEAR:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSplitByIndex(keyRange, HeaderEnum.YEAR.GetDescription(), keyRange, "-", 2);
                    break;
            }
        });

        return sheet;
    }
}

