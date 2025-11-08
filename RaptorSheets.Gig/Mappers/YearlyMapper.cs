using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Helpers;

namespace RaptorSheets.Gig.Mappers;

/// <summary>
/// Yearly mapper for Yearly sheet configuration and formulas.
/// For data mapping operations, use GenericSheetMapper&lt;YearlyEntity&gt; directly.
/// </summary>
public static class YearlyMapper
{
    public static SheetModel GetSheet()
    {
        var sheet = SheetsConfig.YearlySheet;
        sheet.Headers.UpdateColumns();

        var monthlySheet = MonthlyMapper.GetSheet();
        var keyRange = sheet.GetLocalRange(HeaderEnum.YEAR.GetDescription());
        var monthlyKeyRange = monthlySheet.GetRange(HeaderEnum.YEAR.GetDescription());

        // Configure common aggregation patterns from monthly data
        MapperFormulaHelper.ConfigureCommonAggregationHeaders(sheet, keyRange, monthlySheet, monthlyKeyRange, useShiftTotals: false);
        
        // Configure common ratio calculations
        MapperFormulaHelper.ConfigureCommonRatioHeaders(sheet, keyRange);

        // Configure specific headers unique to YearlyMapper
        sheet.Headers.ForEach(header =>
        {
            var headerEnum = header.Name.GetValueFromName<HeaderEnum>();

            switch (headerEnum)
            {
                case HeaderEnum.YEAR:
                    header.Formula = GoogleFormulaBuilder.BuildArrayLiteralUniqueFiltered(HeaderEnum.YEAR.GetDescription(), monthlySheet.GetRange(HeaderEnum.YEAR.GetDescription(), 2));
                    break;
                case HeaderEnum.DAYS:
                    // Override common helper: For yearly, we sum days instead of counting
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, HeaderEnum.DAYS.GetDescription(), monthlyKeyRange, monthlySheet.GetRange(HeaderEnum.DAYS.GetDescription()));
                    header.Format = FormatEnum.NUMBER;
                    break;
                case HeaderEnum.AVERAGE:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaRollingAverage(keyRange, HeaderEnum.AVERAGE.GetDescription(), sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription()));
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                default:
                    break;
            }
        });

        return sheet;
    }
}

