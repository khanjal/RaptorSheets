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
/// Weekly mapper for Weekly sheet configuration and formulas.
/// For data mapping operations, use GenericSheetMapper&lt;WeeklyEntity&gt; directly.
/// </summary>
public static class WeeklyMapper
{
    public static SheetModel GetSheet()
    {
        var sheet = SheetsConfig.WeeklySheet;
        sheet.Headers.UpdateColumns();

        var dailySheet = DailyMapper.GetSheet();
        var keyRange = sheet.GetLocalRange(HeaderEnum.WEEK.GetDescription());
        var dailyKeyRange = dailySheet.GetRange(HeaderEnum.WEEK.GetDescription());

        // Configure common aggregation patterns (eliminates major duplication)
        MapperFormulaHelper.ConfigureCommonAggregationHeaders(sheet, keyRange, dailySheet, dailyKeyRange, useShiftTotals: false);
        
        // Configure common ratio calculations
        MapperFormulaHelper.ConfigureCommonRatioHeaders(sheet, keyRange);

        // Configure specific headers unique to WeeklyMapper
        sheet.Headers.ForEach(header =>
        {
            var headerEnum = header.Name.GetValueFromName<HeaderEnum>();

            switch (headerEnum)
            {
                case HeaderEnum.WEEK:
                    header.Formula = GoogleFormulaBuilder.BuildArrayLiteralUniqueFiltered(HeaderEnum.WEEK.GetDescription(), dailySheet.GetRange(HeaderEnum.WEEK.GetDescription(), 2));
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
                case HeaderEnum.DATE_BEGIN:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaWeekBeginDate(
                        keyRange, 
                        HeaderEnum.DATE_BEGIN.GetDescription(), 
                        sheet.GetLocalRange(HeaderEnum.YEAR.GetDescription()),
                        sheet.GetLocalRange(HeaderEnum.NUMBER.GetDescription()));
                    header.Format = FormatEnum.DATE;
                    break;
                case HeaderEnum.DATE_END:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaWeekEndDate(
                        keyRange, 
                        HeaderEnum.DATE_END.GetDescription(), 
                        sheet.GetLocalRange(HeaderEnum.YEAR.GetDescription()),
                        sheet.GetLocalRange(HeaderEnum.NUMBER.GetDescription()));
                    header.Format = FormatEnum.DATE;
                    break;
            }
        });

        return sheet;
    }
}

