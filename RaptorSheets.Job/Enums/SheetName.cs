using System.ComponentModel;
using RaptorSheets.Job.Constants;

namespace RaptorSheets.Job.Enums;

/// <summary>
/// Sheet enumeration for type safety and IntelliSense support.
/// Descriptions map to SheetsConfig.SheetNames constants.
/// </summary>
public enum SheetName
{
    [Description(SheetsConfig.SheetNames.Applications)]
    APPLICATIONS,

    [Description(SheetsConfig.SheetNames.Interviews)]
    INTERVIEWS,

    [Description(SheetsConfig.SheetNames.CompanyDetails)]
    COMPANY_DETAILS,

    [Description(SheetsConfig.SheetNames.PositionDetails)]
    POSITION_DETAILS,

    [Description(SheetsConfig.SheetNames.Companies)]
    COMPANIES,

    [Description(SheetsConfig.SheetNames.Positions)]
    POSITIONS,

    [Description(SheetsConfig.SheetNames.Sites)]
    SITES,

    [Description(SheetsConfig.SheetNames.Decisions)]
    DECISIONS,

    [Description(SheetsConfig.SheetNames.InterviewTypes)]
    INTERVIEW_TYPES,

    [Description(SheetsConfig.SheetNames.InterviewOutcomes)]
    INTERVIEW_OUTCOMES,

    [Description(SheetsConfig.SheetNames.Schedules)]
    SCHEDULES,

    [Description(SheetsConfig.SheetNames.Setup)]
    SETUP
}
