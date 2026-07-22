using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Home.Constants;

namespace RaptorSheets.Home.Mappers;

/// <summary>
/// Appliance mapper for the Appliances &amp; Electronics sheet configuration. Configures the
/// calculated Next Filter column (Filter Date + Rpl. Mth). For data mapping operations, use
/// GenericSheetMapper&lt;ApplianceEntity&gt; directly.
/// </summary>
public static class ApplianceMapper
{
    public static SheetModel GetSheet()
    {
        var sheet = SheetsConfig.ApplianceSheet;
        sheet.Headers.UpdateColumns();

        var filterDateRange = sheet.GetLocalRange(SheetsConfig.HeaderNames.FilterDate);
        var replacementMonthsRange = sheet.GetLocalRange(SheetsConfig.HeaderNames.ReplacementMonths);

        var nextFilter = sheet.Headers.FirstOrDefault(h => h.Name == SheetsConfig.HeaderNames.NextFilter);
        if (nextFilter != null)
        {
            // Next Filter = EDATE(Filter Date, Rpl. Mth) - blank when either input is blank
            nextFilter.Formula = GoogleFormulaBuilder.WrapWithArrayFormula(
                filterDateRange,
                SheetsConfig.HeaderNames.NextFilter,
                $"IF(ISBLANK({replacementMonthsRange}), \"\", EDATE({filterDateRange},{replacementMonthsRange}))");
            nextFilter.Format = Format.DATE;
        }

        return sheet;
    }
}
