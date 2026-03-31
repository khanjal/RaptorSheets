using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Enums;
using RaptorSheets.Job.Constants;
using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Job.Entities;

/// <summary>
/// Company reference data (calculated from Applications)
/// </summary>
[ExcludeFromCodeCoverage]
public class CompanyEntity : SheetRowEntityBase
{
    [Column(SheetsConfig.HeaderNames.Company, isInput: false)]
    public string Company { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.ApplicationCount, isInput: false, formatType: FormatEnum.NUMBER)]
    public int ApplicationCount { get; set; }

    [Column(SheetsConfig.HeaderNames.InterviewCount, isInput: false, formatType: FormatEnum.NUMBER)]
    public int InterviewCount { get; set; }
}

/// <summary>
/// Position reference data (calculated from Applications)
/// </summary>
[ExcludeFromCodeCoverage]
public class PositionEntity : SheetRowEntityBase
{
    [Column(SheetsConfig.HeaderNames.Position, isInput: false)]
    public string Position { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.ApplicationCount, isInput: false, formatType: FormatEnum.NUMBER)]
    public int ApplicationCount { get; set; }

    [Column(SheetsConfig.HeaderNames.InterviewCount, isInput: false, formatType: FormatEnum.NUMBER)]
    public int InterviewCount { get; set; }
}

/// <summary>
/// Optional company details (user input)
/// </summary>
[ExcludeFromCodeCoverage]
public class CompanyDetailEntity : SheetRowEntityBase
{
    [Column(SheetsConfig.HeaderNames.Company, isInput: true)]
    public string Company { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Industry, isInput: true)]
    public string Industry { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Location, isInput: true)]
    public string Location { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Website, isInput: true)]
    public string Website { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Notes, isInput: true)]
    public string Notes { get; set; } = "";
}

/// <summary>
/// Optional position details (user input)
/// </summary>
[ExcludeFromCodeCoverage]
public class PositionDetailEntity : SheetRowEntityBase
{
    [Column(SheetsConfig.HeaderNames.Position, isInput: true)]
    public string Position { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Category, isInput: true)]
    public string Category { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Seniority, isInput: true)]
    public string Seniority { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Notes, isInput: true)]
    public string Notes { get; set; } = "";
}

/// <summary>
/// Site/Job board reference
/// </summary>
[ExcludeFromCodeCoverage]
public class SiteEntity : SheetRowEntityBase
{
    [Column(SheetsConfig.HeaderNames.Site, isInput: true)]
    public string Site { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.ApplicationCount, isInput: false, formatType: FormatEnum.NUMBER)]
    public int ApplicationCount { get; set; }

    [Column(SheetsConfig.HeaderNames.InterviewCount, isInput: false, formatType: FormatEnum.NUMBER)]
    public int InterviewCount { get; set; }
}

/// <summary>
/// Decision reference
/// </summary>
[ExcludeFromCodeCoverage]
public class DecisionEntity : SheetRowEntityBase
{
    [Column(SheetsConfig.HeaderNames.Decision, isInput: true)]
    public string Decision { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.ApplicationCount, isInput: false, formatType: FormatEnum.NUMBER)]
    public int ApplicationCount { get; set; }

    [Column(SheetsConfig.HeaderNames.InterviewCount, isInput: false, formatType: FormatEnum.NUMBER)]
    public int InterviewCount { get; set; }
}

/// <summary>
/// Interview type reference
/// </summary>
[ExcludeFromCodeCoverage]
public class InterviewTypeEntity : SheetRowEntityBase
{
    [Column(SheetsConfig.HeaderNames.InterviewType, isInput: true)]
    public string InterviewType { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.ApplicationCount, isInput: false, formatType: FormatEnum.NUMBER)]
    public int ApplicationCount { get; set; }

    [Column(SheetsConfig.HeaderNames.InterviewCount, isInput: false, formatType: FormatEnum.NUMBER)]
    public int InterviewCount { get; set; }
}

/// <summary>
/// Interview outcome reference
/// </summary>
[ExcludeFromCodeCoverage]
public class InterviewOutcomeEntity : SheetRowEntityBase
{
    [Column(SheetsConfig.HeaderNames.Outcome, isInput: true)]
    public string Outcome { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.ApplicationCount, isInput: false, formatType: FormatEnum.NUMBER)]
    public int ApplicationCount { get; set; }

    [Column(SheetsConfig.HeaderNames.InterviewCount, isInput: false, formatType: FormatEnum.NUMBER)]
    public int InterviewCount { get; set; }
}

/// <summary>
/// Schedule reference
/// </summary>
[ExcludeFromCodeCoverage]
public class ScheduleEntity : SheetRowEntityBase
{
    [Column(SheetsConfig.HeaderNames.Schedule, isInput: true)]
    public string Schedule { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.ApplicationCount, isInput: false, formatType: FormatEnum.NUMBER)]
    public int ApplicationCount { get; set; }

    [Column(SheetsConfig.HeaderNames.InterviewCount, isInput: false, formatType: FormatEnum.NUMBER)]
    public int InterviewCount { get; set; }
}

/// <summary>
/// Setup/configuration entity
/// </summary>
[ExcludeFromCodeCoverage]
public class SetupEntity : SheetRowEntityBase
{
    [Column("Setting", isInput: true)]
    public string Setting { get; set; } = "";

    [Column("Value", isInput: true)]
    public string Value { get; set; } = "";
}
