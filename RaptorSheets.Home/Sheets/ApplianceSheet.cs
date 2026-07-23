using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Home.Constants;
using RaptorSheets.Home.Entities;

namespace RaptorSheets.Home.Sheets;

/// <summary>
/// Appliance sheet definition for the Appliances &amp; Electronics sheet. Configures the
/// calculated Next Filter column (Filter Date + Rpl. Mth). For data mapping operations, use
/// GenericSheetMapper&lt;ApplianceEntity&gt; directly.
/// </summary>
public static class ApplianceSheet
{
    internal static SheetModel BaseSheet => new()
    {
        Name = SheetsConfig.SheetNames.Appliances,
        TabColor = SheetColor.BLUE,
        CellColor = SheetColor.LIGHT_GRAY,
        FontColor = SheetColor.WHITE, // BLUE is a dark TabColor - see SheetColor for the dark/light list
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<ApplianceEntity>()
    };

    public static SheetModel GetSheet()
    {
        var sheet = BaseSheet;
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
