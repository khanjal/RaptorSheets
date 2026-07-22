using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Job.Constants;
using RaptorSheets.Job.Entities;

namespace RaptorSheets.Job.Mappers;

/// <summary>
/// Mapper for validation reference sheets (Sites, Decisions, InterviewTypes, InterviewOutcomes, Schedules).
/// These are simple single-column sheets used for dropdown validation.
/// </summary>
public static class ValidationMapper
{
    public static SheetModel GetSiteSheet()
    {
        return GenericSheetMapper<SiteEntity>.GetSheet(SheetsConfig.SiteSheet, ConfigureSiteFormulas);
    }

    public static SheetModel GetDecisionSheet()
    {
        return GenericSheetMapper<DecisionEntity>.GetSheet(SheetsConfig.DecisionSheet, ConfigureDecisionFormulas);
    }

    public static SheetModel GetInterviewTypeSheet()
    {
        return GenericSheetMapper<InterviewTypeEntity>.GetSheet(SheetsConfig.InterviewTypeSheet, ConfigureInterviewTypeFormulas);
    }

    public static SheetModel GetInterviewOutcomeSheet()
    {
        return GenericSheetMapper<InterviewOutcomeEntity>.GetSheet(SheetsConfig.InterviewOutcomeSheet, ConfigureInterviewOutcomeFormulas);
    }

    public static SheetModel GetScheduleSheet()
    {
        return GenericSheetMapper<ScheduleEntity>.GetSheet(SheetsConfig.ScheduleSheet, ConfigureScheduleFormulas);
    }

    public static SheetModel GetSetupSheet()
    {
        return GenericSheetMapper<SetupEntity>.GetSheet(SheetsConfig.SetupSheet);
    }

    private static void ConfigureSiteFormulas(SheetModel sheet)
    {
        var appSheet = ApplicationMapper.GetSheet();
        var intSheet = InterviewMapper.GetSheet();

        var valueRange = sheet.GetLocalRange(SheetsConfig.HeaderNames.Site);
        var sourceRange = appSheet.GetRange(SheetsConfig.HeaderNames.Site, 2);
        var appKeyRange = appSheet.GetRange(SheetsConfig.HeaderNames.Key, 2);
        var intKeyRange = intSheet.GetRange(SheetsConfig.HeaderNames.Key, 2);

        ApplyReferenceCountFormulas(
            sheet,
            SheetsConfig.HeaderNames.Site,
            valueRange,
            sourceRange,
            sourceRange,
            BuildInterviewCountFromApplicationDimensionFormula(valueRange, sourceRange, appKeyRange, intKeyRange));
    }

    private static void ConfigureDecisionFormulas(SheetModel sheet)
    {
        var appSheet = ApplicationMapper.GetSheet();
        var intSheet = InterviewMapper.GetSheet();

        var valueRange = sheet.GetLocalRange(SheetsConfig.HeaderNames.Decision);
        var sourceRange = appSheet.GetRange(SheetsConfig.HeaderNames.Decision, 2);
        var appKeyRange = appSheet.GetRange(SheetsConfig.HeaderNames.Key, 2);
        var intKeyRange = intSheet.GetRange(SheetsConfig.HeaderNames.Key, 2);

        ApplyReferenceCountFormulas(
            sheet,
            SheetsConfig.HeaderNames.Decision,
            valueRange,
            sourceRange,
            sourceRange,
            BuildInterviewCountFromApplicationDimensionFormula(valueRange, sourceRange, appKeyRange, intKeyRange));
    }

    private static void ConfigureInterviewTypeFormulas(SheetModel sheet)
    {
        var intSheet = InterviewMapper.GetSheet();

        var valueRange = sheet.GetLocalRange(SheetsConfig.HeaderNames.InterviewType);
        var sourceRange = intSheet.GetRange(SheetsConfig.HeaderNames.InterviewType, 2);
        var intKeyRange = intSheet.GetRange(SheetsConfig.HeaderNames.Key, 2);

        ApplyReferenceCountFormulas(
            sheet,
            SheetsConfig.HeaderNames.InterviewType,
            valueRange,
            sourceRange,
            BuildApplicationCountFromInterviewDimensionFormula(valueRange, sourceRange, intKeyRange),
            sourceRange);
    }

    private static void ConfigureInterviewOutcomeFormulas(SheetModel sheet)
    {
        var intSheet = InterviewMapper.GetSheet();

        var valueRange = sheet.GetLocalRange(SheetsConfig.HeaderNames.Outcome);
        var sourceRange = intSheet.GetRange(SheetsConfig.HeaderNames.Outcome, 2);
        var intKeyRange = intSheet.GetRange(SheetsConfig.HeaderNames.Key, 2);

        ApplyReferenceCountFormulas(
            sheet,
            SheetsConfig.HeaderNames.Outcome,
            valueRange,
            sourceRange,
            BuildApplicationCountFromInterviewDimensionFormula(valueRange, sourceRange, intKeyRange),
            sourceRange);
    }

    private static void ConfigureScheduleFormulas(SheetModel sheet)
    {
        var appSheet = ApplicationMapper.GetSheet();
        var intSheet = InterviewMapper.GetSheet();

        var valueRange = sheet.GetLocalRange(SheetsConfig.HeaderNames.Schedule);
        var sourceRange = appSheet.GetRange(SheetsConfig.HeaderNames.Schedule, 2);
        var appKeyRange = appSheet.GetRange(SheetsConfig.HeaderNames.Key, 2);
        var intKeyRange = intSheet.GetRange(SheetsConfig.HeaderNames.Key, 2);

        ApplyReferenceCountFormulas(
            sheet,
            SheetsConfig.HeaderNames.Schedule,
            valueRange,
            sourceRange,
            sourceRange,
            BuildInterviewCountFromApplicationDimensionFormula(valueRange, sourceRange, appKeyRange, intKeyRange));
    }

    private static void ApplyReferenceCountFormulas(
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

    private static string BuildInterviewCountFromApplicationDimensionFormula(
        string keyRange,
        string appDimensionRange,
        string appKeyRange,
        string intKeyRange)
    {
        return $"=ARRAYFORMULA(IFS(ROW({keyRange})=1,\"{SheetsConfig.HeaderNames.InterviewCount}\",ISBLANK({keyRange}), \"\", true, IFERROR(COUNTA(FILTER({intKeyRange}, ISNUMBER(MATCH({intKeyRange}, FILTER({appKeyRange}, {appDimensionRange}={keyRange}), 0)))),0)))";
    }

    private static string BuildApplicationCountFromInterviewDimensionFormula(
        string keyRange,
        string intDimensionRange,
        string intKeyRange)
    {
        return $"=ARRAYFORMULA(IFS(ROW({keyRange})=1,\"{SheetsConfig.HeaderNames.ApplicationCount}\",ISBLANK({keyRange}), \"\", true, IFERROR(COUNTA(UNIQUE(FILTER({intKeyRange}, {intDimensionRange}={keyRange}))),0)))";
    }
}
