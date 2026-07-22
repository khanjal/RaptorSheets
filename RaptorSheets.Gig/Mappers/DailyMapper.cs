using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
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
        var dateRange = sheet.GetLocalRange(Header.DATE.GetDescription());
        var shiftKeyRange = shiftSheet.GetRange(Header.DATE.GetDescription());

        // Configure common aggregation patterns from shift data
        MapperFormulaHelper.ConfigureCommonAggregationHeaders(sheet, dateRange, shiftSheet, shiftKeyRange, useShiftTotals: true);
        
        // Configure common ratio calculations
        MapperFormulaHelper.ConfigureCommonRatioHeaders(sheet, dateRange);

        // Configure specific headers unique to DailyMapper
        sheet.Headers.ForEach(header =>
        {
            var headerEnum = header.Name.GetValueFromName<Header>();

            switch (headerEnum)
            {
                case Header.DATE:
                    header.Formula = GoogleFormulaBuilder.BuildArrayLiteralUniqueFilteredSorted(Header.DATE.GetDescription(), shiftSheet.GetRange(Header.DATE.GetDescription(), 2));
                    header.Format = Format.DATE;
                    break;
                case Header.WEEKDAY:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaWeekdayText(dateRange, Header.WEEKDAY.GetDescription(), dateRange);
                    break;
                case Header.DAY:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaWeekday(dateRange, Header.DAY.GetDescription(), dateRange);
                    header.Format = Format.NUMBER;
                    break;
                case Header.WEEK:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaWeekNumber(dateRange, Header.WEEK.GetDescription(), dateRange);
                    break;
                case Header.MONTH:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaMonthNumber(dateRange, Header.MONTH.GetDescription(), dateRange);
                    break;
                case Header.YEAR:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaYear(dateRange, Header.YEAR.GetDescription(), dateRange);
                    break;
                default:
                    break;
            }
        });

        return sheet;
    }
}

