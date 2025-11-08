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
        var keyRange = sheet.GetLocalRange(HeaderEnum.WEEK.GetDescription());
        var dailyKeyRange = dailySheet.GetRange(HeaderEnum.WEEK.GetDescription());

        // Configure common aggregation patterns to reduce duplication.
        MapperFormulaHelper.ConfigureCommonAggregationHeaders(sheet, keyRange, dailySheet, dailyKeyRange, useShiftTotals: false);
        
        // Configure common ratio calculations.
        MapperFormulaHelper.ConfigureCommonRatioHeaders(sheet, keyRange);

        // Configure specific headers unique to WeeklyMapper.
        sheet.Headers.ForEach(header =>
        {
            var headerEnum = header.Name.GetValueFromName<HeaderEnum>();

            switch (headerEnum)
            {
                case HeaderEnum.WEEK:
                    // Formula to generate unique weekly values from the Daily sheet.
                    header.Formula = GoogleFormulaBuilder.BuildArrayLiteralUniqueFiltered(HeaderEnum.WEEK.GetDescription(), dailySheet.GetRange(HeaderEnum.WEEK.GetDescription(), 2));
                    break;
                case HeaderEnum.AVERAGE:
                    // Formula to calculate rolling averages for weekly data.
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaRollingAverage(keyRange, HeaderEnum.AVERAGE.GetDescription(), sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription()));
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                case HeaderEnum.NUMBER:
                    // Formula to split weekly data by index.
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSplitByIndex(keyRange, HeaderEnum.NUMBER.GetDescription(), keyRange, "-", 1);
                    break;
                case HeaderEnum.YEAR:
                    // Formula to extract year from weekly data.
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSplitByIndex(keyRange, HeaderEnum.YEAR.GetDescription(), keyRange, "-", 2);
                    break;

                // Additional cases for other headers can be added here.
            }
        });

        return sheet;
    }
}

