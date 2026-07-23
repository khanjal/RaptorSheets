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
/// Yearly sheet definition - layout and formulas for the Yearly sheet.
/// Focuses on yearly data aggregation and ratio calculations.
/// </summary>
public static class YearlySheet
{
    internal static SheetModel BaseSheet => new()
    {
        Name = SheetsConfig.SheetNames.Yearly,
        TabColor = SheetColor.LIGHT_GREEN,
        CellColor = SheetColor.LIGHT_GRAY,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<YearlyEntity>()
    };

    /// <summary>
    /// Retrieves the configured Yearly sheet.
    /// Includes formulas, validations, and formatting specific to the Yearly sheet.
    /// </summary>
    public static SheetModel GetSheet()
    {
        var sheet = BaseSheet;
        sheet.Headers.UpdateColumns();

        var monthlySheet = MonthlySheet.GetSheet();
        var keyRange = sheet.GetLocalRange(Header.YEAR.GetDescription());
        var monthlyKeyRange = monthlySheet.GetRange(Header.YEAR.GetDescription());

        // Configure common aggregation patterns from monthly data.
        MapperFormulaHelper.ConfigureCommonAggregationHeaders(sheet, keyRange, monthlySheet, monthlyKeyRange, useShiftTotals: false);

        // Configure common ratio calculations.
        MapperFormulaHelper.ConfigureCommonRatioHeaders(sheet, keyRange);

        // Configure specific headers unique to YearlySheet.
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
