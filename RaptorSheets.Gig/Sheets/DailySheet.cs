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
/// Daily sheet definition - layout and formulas for the Daily sheet.
/// For data mapping operations, use GenericSheetMapper&lt;DailyEntity&gt; directly.
/// </summary>
public static class DailySheet
{
    internal static SheetModel BaseSheet => new()
    {
        Name = SheetsConfig.SheetNames.Daily,
        TabColor = SheetColor.LIGHT_GREEN,
        CellColor = SheetColor.LIGHT_GRAY,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<DailyEntity>()
    };

    public static SheetModel GetSheet()
    {
        var sheet = BaseSheet;
        sheet.Headers.UpdateColumns();

        var shiftSheet = ShiftSheet.GetSheet();
        var dateRange = sheet.GetLocalRange(Header.DATE.GetDescription());
        var shiftKeyRange = shiftSheet.GetRange(Header.DATE.GetDescription());

        // Configure common aggregation patterns from shift data
        MapperFormulaHelper.ConfigureCommonAggregationHeaders(sheet, dateRange, shiftSheet, shiftKeyRange, useShiftTotals: true);

        // Configure common ratio calculations
        MapperFormulaHelper.ConfigureCommonRatioHeaders(sheet, dateRange);

        // Configure specific headers unique to DailySheet
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
