using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Job.Constants;
using RaptorSheets.Job.Entities;
using RaptorSheets.Job.Helpers;

namespace RaptorSheets.Job.Sheets;

/// <summary>
/// Decisions sheet definition - validation reference sheet for decision dropdown values,
/// calculated from Applications and Interviews.
/// </summary>
public static class DecisionSheet
{
    internal static SheetModel BaseSheet => new()
    {
        Name = SheetsConfig.SheetNames.Decisions,
        CellColor = SheetColor.LIGHT_GRAY,
        TabColor = SheetColor.LIGHT_GRAY,
        FreezeColumnCount = 0,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<DecisionEntity>()
    };

    public static SheetModel GetSheet()
    {
        return GenericSheetMapper<DecisionEntity>.GetSheet(BaseSheet, ConfigureDecisionFormulas);
    }

    private static void ConfigureDecisionFormulas(SheetModel sheet)
    {
        var appSheet = ApplicationSheet.GetSheet();
        var intSheet = InterviewSheet.GetSheet();

        var valueRange = sheet.GetLocalRange(SheetsConfig.HeaderNames.Decision);
        var sourceRange = appSheet.GetRange(SheetsConfig.HeaderNames.Decision, 2);
        var appKeyRange = appSheet.GetRange(SheetsConfig.HeaderNames.Key, 2);
        var intKeyRange = intSheet.GetRange(SheetsConfig.HeaderNames.Key, 2);

        ReferenceSheetFormulaHelper.ApplyReferenceCountFormulas(
            sheet,
            SheetsConfig.HeaderNames.Decision,
            valueRange,
            sourceRange,
            sourceRange,
            ReferenceSheetFormulaHelper.BuildInterviewCountFromApplicationDimensionFormula(valueRange, sourceRange, appKeyRange, intKeyRange));
    }
}
