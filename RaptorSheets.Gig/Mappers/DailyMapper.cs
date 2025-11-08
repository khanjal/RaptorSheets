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
/// Daily mapper for Daily sheet configuration and formulas.
/// For data mapping operations, use GenericSheetMapper&lt;DailyEntity&gt; directly.
/// </summary>
public static class DailyMapper
{
    public static SheetModel GetSheet()
    {
        var sheet = SheetsConfig.DailySheet;
        sheet.Headers.UpdateColumns();

        var shiftSheet = ShiftMapper.GetSheet();
        var dateRange = sheet.GetLocalRange(HeaderEnum.DATE.GetDescription());
        var shiftKeyRange = shiftSheet.GetRange(HeaderEnum.DATE.GetDescription());

        // Configure common aggregation patterns from shift data
        MapperFormulaHelper.ConfigureCommonAggregationHeaders(sheet, dateRange, shiftSheet, shiftKeyRange, useShiftTotals: true);
        
        // Configure common ratio calculations
        MapperFormulaHelper.ConfigureCommonRatioHeaders(sheet, dateRange);

        // Configure specific headers unique to DailyMapper
        sheet.Headers.ForEach(header =>
        {
            var headerEnum = header.Name.GetValueFromName<HeaderEnum>();

            switch (headerEnum)
            {
                case HeaderEnum.DATE:
                    header.Formula = GoogleFormulaBuilder.BuildArrayLiteralUniqueFilteredSorted(HeaderEnum.DATE.GetDescription(), shiftSheet.GetRange(HeaderEnum.DATE.GetDescription(), 2));
                    header.Format = FormatEnum.DATE;
                    break;
                case HeaderEnum.WEEKDAY:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaWeekdayText(dateRange, HeaderEnum.WEEKDAY.GetDescription(), dateRange);
                    break;
                case HeaderEnum.DAY:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaWeekday(dateRange, HeaderEnum.DAY.GetDescription(), dateRange);
                    header.Format = FormatEnum.NUMBER;
                    break;
                case HeaderEnum.WEEK:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaWeekNumber(dateRange, HeaderEnum.WEEK.GetDescription(), dateRange);
                    break;
                case HeaderEnum.MONTH:
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaMonthNumber(dateRange, HeaderEnum.MONTH.GetDescription(), dateRange);
                    break;
                case HeaderEnum.YEAR:
                    header.Formula = $"=ARRAYFORMULA(IFS(ROW({dateRange})=1,\"{HeaderEnum.YEAR.GetDescription()}\",ISBLANK({dateRange}), \"\",true,YEAR({dateRange})))";
                    break;
                default:
                    break;
            }
        });

        return sheet;
    }
}

