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
/// Interview Outcomes sheet definition - validation reference sheet for interview outcome dropdown
/// values, calculated from Interviews.
/// </summary>
public static class InterviewOutcomeSheet
{
    internal static SheetModel BaseSheet => new()
    {
        Name = SheetsConfig.SheetNames.InterviewOutcomes,
        CellColor = SheetColor.LIGHT_GRAY,
        TabColor = SheetColor.LIGHT_GRAY,
        FreezeColumnCount = 0,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<InterviewOutcomeEntity>()
    };

    public static SheetModel GetSheet()
    {
        return GenericSheetMapper<InterviewOutcomeEntity>.GetSheet(BaseSheet, ConfigureInterviewOutcomeFormulas);
    }

    private static void ConfigureInterviewOutcomeFormulas(SheetModel sheet)
    {
        var intSheet = InterviewSheet.GetSheet();

        var valueRange = sheet.GetLocalRange(SheetsConfig.HeaderNames.Outcome);
        var sourceRange = intSheet.GetRange(SheetsConfig.HeaderNames.Outcome, 2);
        var intKeyRange = intSheet.GetRange(SheetsConfig.HeaderNames.Key, 2);

        ReferenceSheetFormulaHelper.ApplyReferenceCountFormulas(
            sheet,
            SheetsConfig.HeaderNames.Outcome,
            valueRange,
            sourceRange,
            ReferenceSheetFormulaHelper.BuildApplicationCountFromInterviewDimensionFormula(valueRange, sourceRange, intKeyRange),
            sourceRange);
    }
}
