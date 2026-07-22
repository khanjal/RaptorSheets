using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Helpers;

namespace RaptorSheets.Gig.Mappers;

/// <summary>
/// Weekly mapper for configuring the Weekly sheet with formulas, validations, and formatting.
/// This mapper focuses on weekly data aggregation and ratio calculations.
/// </summary>
public static class WeeklyMapper
{
    /// <summary>
    /// Retrieves the configured Weekly sheet.
    /// Includes formulas, validations, and formatting specific to the Weekly sheet.
    /// </summary>
    public static SheetModel GetSheet()
    {
        var sheet = SheetsConfig.WeeklySheet;
        sheet.Headers.UpdateColumns();

        var dailySheet = DailyMapper.GetSheet();
        var keyRange = sheet.GetLocalRange(Header.WEEK.GetDescription());
        var dailyKeyRange = dailySheet.GetRange(Header.WEEK.GetDescription());

        // Configure common aggregation patterns to reduce duplication.
        MapperFormulaHelper.ConfigureCommonAggregationHeaders(sheet, keyRange, dailySheet, dailyKeyRange, useShiftTotals: false);
        
        // Configure common ratio calculations.
        MapperFormulaHelper.ConfigureCommonRatioHeaders(sheet, keyRange);

        // Configure specific headers unique to WeeklyMapper.
        sheet.Headers.ForEach(header =>
        {
            var headerEnum = header.Name.GetValueFromName<Header>();

            switch (headerEnum)
            {
                case Header.WEEK:
                    // Formula to generate unique weekly values from the Daily sheet.
                    header.Formula = GoogleFormulaBuilder.BuildArrayLiteralUniqueFiltered(Header.WEEK.GetDescription(), dailySheet.GetRange(Header.WEEK.GetDescription(), 2));
                    break;
                case Header.AVERAGE:
                    // Formula to calculate rolling averages for weekly data.
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaRollingAverage(keyRange, Header.AVERAGE.GetDescription(), sheet.GetLocalRange(Header.TOTAL.GetDescription()));
                    header.Format = Format.ACCOUNTING;
                    break;
                case Header.NUMBER:
                    // Formula to split weekly data by index.
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSplitByIndex(keyRange, Header.NUMBER.GetDescription(), keyRange, "-", 1);
                    break;
                case Header.YEAR:
                    // Formula to extract year from weekly data.
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSplitByIndex(keyRange, Header.YEAR.GetDescription(), keyRange, "-", 2);
                    break;
                case Header.DATE_BEGIN:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaWeekBeginDate(
                        keyRange, 
                        Header.DATE_BEGIN.GetDescription(), 
                        sheet.GetLocalRange(Header.YEAR.GetDescription()),
                        sheet.GetLocalRange(Header.NUMBER.GetDescription()));
                    header.Format = Format.DATE;
                    break;
                case Header.DATE_END:
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaWeekEndDate(
                        keyRange, 
                        Header.DATE_END.GetDescription(), 
                        sheet.GetLocalRange(Header.YEAR.GetDescription()),
                        sheet.GetLocalRange(Header.NUMBER.GetDescription()));
                    header.Format = Format.DATE;
                    break;
            }
        });

        return sheet;
    }
}

