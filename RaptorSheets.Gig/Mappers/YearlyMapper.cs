using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Helpers;

namespace RaptorSheets.Gig.Mappers;

/// <summary>
/// Yearly mapper for configuring the Yearly sheet with formulas, validations, and formatting.
/// This mapper focuses on yearly data aggregation and ratio calculations.
/// </summary>
public static class YearlyMapper
{
    /// <summary>
    /// Retrieves the configured Yearly sheet.
    /// Includes formulas, validations, and formatting specific to the Yearly sheet.
    /// </summary>
    public static SheetModel GetSheet()
    {
        var sheet = SheetsConfig.YearlySheet;
        sheet.Headers.UpdateColumns();

        var monthlySheet = MonthlyMapper.GetSheet();
        var keyRange = sheet.GetLocalRange(HeaderEnum.YEAR.GetDescription());
        var monthlyKeyRange = monthlySheet.GetRange(HeaderEnum.YEAR.GetDescription());

        // Configure common aggregation patterns from monthly data.
        MapperFormulaHelper.ConfigureCommonAggregationHeaders(sheet, keyRange, monthlySheet, monthlyKeyRange, useShiftTotals: false);
        
        // Configure common ratio calculations.
        MapperFormulaHelper.ConfigureCommonRatioHeaders(sheet, keyRange);

        // Configure specific headers unique to YearlyMapper.
        sheet.Headers.ForEach(header =>
        {
            var headerEnum = header.Name.GetValueFromName<HeaderEnum>();

            switch (headerEnum)
            {
                case HeaderEnum.YEAR:
                    // Formula to generate unique yearly values from the Monthly sheet.
                    header.Formula = GoogleFormulaBuilder.BuildArrayLiteralUniqueFiltered(HeaderEnum.YEAR.GetDescription(), monthlySheet.GetRange(HeaderEnum.YEAR.GetDescription(), 2));
                    break;
                case HeaderEnum.DAYS:
                    // Formula to sum days for yearly data.
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, HeaderEnum.DAYS.GetDescription(), monthlyKeyRange, monthlySheet.GetRange(HeaderEnum.DAYS.GetDescription()));
                    header.Format = FormatEnum.NUMBER;
                    break;
                case HeaderEnum.AVERAGE:
                    // Formula to calculate rolling averages for yearly data.
                    header.Formula = GigFormulaBuilder.BuildArrayFormulaRollingAverage(keyRange, HeaderEnum.AVERAGE.GetDescription(), sheet.GetLocalRange(HeaderEnum.TOTAL.GetDescription()));
                    header.Format = FormatEnum.ACCOUNTING;
                    break;

                // Additional cases for other headers can be added here.
            }
        });

        return sheet;
    }
}

