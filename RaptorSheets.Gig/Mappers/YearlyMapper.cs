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
        var keyRange = sheet.GetLocalRange(Header.YEAR.GetDescription());
        var monthlyKeyRange = monthlySheet.GetRange(Header.YEAR.GetDescription());

        // Configure common aggregation patterns from monthly data.
        MapperFormulaHelper.ConfigureCommonAggregationHeaders(sheet, keyRange, monthlySheet, monthlyKeyRange, useShiftTotals: false);
        
        // Configure common ratio calculations.
        MapperFormulaHelper.ConfigureCommonRatioHeaders(sheet, keyRange);

        // Configure specific headers unique to YearlyMapper.
        sheet.Headers.ForEach(header =>
        {
            var headerEnum = header.Name.GetValueFromName<Header>();

            switch (headerEnum)
            {
                case Header.YEAR:
                    // Formula to generate unique yearly values from the Monthly sheet.
                    header.Formula = GoogleFormulaBuilder.BuildArrayLiteralUniqueFiltered(Header.YEAR.GetDescription(), monthlySheet.GetRange(Header.YEAR.GetDescription(), 2));
                    break;
                case Header.DAYS:
                    // Formula to sum days for yearly data.
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaSumIf(keyRange, Header.DAYS.GetDescription(), monthlyKeyRange, monthlySheet.GetRange(Header.DAYS.GetDescription()));
                    header.Format = Format.NUMBER;
                    break;
                case Header.AVERAGE:
                    // Formula to calculate rolling averages for yearly data.
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaRollingAverage(keyRange, Header.AVERAGE.GetDescription(), sheet.GetLocalRange(Header.TOTAL.GetDescription()));
                    header.Format = Format.ACCOUNTING;
                    break;

                // Additional cases for other headers can be added here.
            }
        });

        return sheet;
    }
}

