using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Models.Google;
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
        return GenericSheetMapper<SiteEntity>.GetSheet(SheetsConfig.SiteSheet);
    }

    public static SheetModel GetDecisionSheet()
    {
        return GenericSheetMapper<DecisionEntity>.GetSheet(SheetsConfig.DecisionSheet);
    }

    public static SheetModel GetInterviewTypeSheet()
    {
        return GenericSheetMapper<InterviewTypeEntity>.GetSheet(SheetsConfig.InterviewTypeSheet);
    }

    public static SheetModel GetInterviewOutcomeSheet()
    {
        return GenericSheetMapper<InterviewOutcomeEntity>.GetSheet(SheetsConfig.InterviewOutcomeSheet);
    }

    public static SheetModel GetScheduleSheet()
    {
        return GenericSheetMapper<ScheduleEntity>.GetSheet(SheetsConfig.ScheduleSheet);
    }

    public static SheetModel GetSetupSheet()
    {
        return GenericSheetMapper<SetupEntity>.GetSheet(SheetsConfig.SetupSheet);
    }
}
