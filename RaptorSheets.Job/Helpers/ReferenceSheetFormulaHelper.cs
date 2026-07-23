using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Job.Constants;

namespace RaptorSheets.Job.Helpers;

/// <summary>
/// Common helper for configuring formula patterns shared across Job's validation reference sheets
/// (Sites, Decisions, Interview Types, Interview Outcomes, Schedules). Eliminates duplication by
/// centralizing the unique-value + application/interview-count formula logic.
/// </summary>
public static class ReferenceSheetFormulaHelper
{
    public static void ApplyReferenceCountFormulas(
        SheetModel sheet,
        string valueHeader,
        string valueRange,
        string uniqueSourceRange,
        string applicationCountLookupOrFormula,
        string interviewCountLookupOrFormula)
    {
        sheet.Headers.ForEach(header =>
        {
            var name = header!.Name.ToString()!.Trim();
            if (name == valueHeader)
            {
                header.Formula = GoogleFormulaBuilder.BuildArrayLiteralUniqueFilteredSorted(valueHeader, uniqueSourceRange);
            }
            else if (name == SheetsConfig.HeaderNames.ApplicationCount)
            {
                header.Formula = applicationCountLookupOrFormula.StartsWith('=')
                    ? applicationCountLookupOrFormula
                    : GoogleFormulaBuilder.BuildArrayFormulaCountIf(valueRange, SheetsConfig.HeaderNames.ApplicationCount, applicationCountLookupOrFormula);
            }
            else if (name == SheetsConfig.HeaderNames.InterviewCount)
            {
                header.Formula = interviewCountLookupOrFormula.StartsWith('=')
                    ? interviewCountLookupOrFormula
                    : GoogleFormulaBuilder.BuildArrayFormulaCountIf(valueRange, SheetsConfig.HeaderNames.InterviewCount, interviewCountLookupOrFormula);
            }
        });
    }

    public static string BuildInterviewCountFromApplicationDimensionFormula(
        string keyRange,
        string appDimensionRange,
        string appKeyRange,
        string intKeyRange)
    {
        return $"=ARRAYFORMULA(IFS(ROW({keyRange})=1,\"{SheetsConfig.HeaderNames.InterviewCount}\",ISBLANK({keyRange}), \"\", true, IFERROR(COUNTA(FILTER({intKeyRange}, ISNUMBER(MATCH({intKeyRange}, FILTER({appKeyRange}, {appDimensionRange}={keyRange}), 0)))),0)))";
    }

    public static string BuildApplicationCountFromInterviewDimensionFormula(
        string keyRange,
        string intDimensionRange,
        string intKeyRange)
    {
        return $"=ARRAYFORMULA(IFS(ROW({keyRange})=1,\"{SheetsConfig.HeaderNames.ApplicationCount}\",ISBLANK({keyRange}), \"\", true, IFERROR(COUNTA(UNIQUE(FILTER({intKeyRange}, {intDimensionRange}={keyRange}))),0)))";
    }
}
