using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Job.Constants;
using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Job.Entities;

/// <summary>
/// Weekly analytics summary
/// </summary>
[ExcludeFromCodeCoverage]
public class WeeklyEntity : SheetRowEntityBase
{
    [Column(SheetsConfig.HeaderNames.Week, isInput: false, formatType: Format.DATE)]
    public string Week { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.ApplicationCount, isInput: false, formatType: Format.NUMBER)]
    public int Applications { get; set; }

    [Column(SheetsConfig.HeaderNames.Interviews, isInput: false, formatType: Format.NUMBER)]
    public int Interviews { get; set; }

    [Column(SheetsConfig.HeaderNames.InterviewRate, isInput: false, formatType: Format.PERCENT)]
    public decimal InterviewRate { get; set; }

    [Column(SheetsConfig.HeaderNames.Decision, isInput: false, formatType: Format.NUMBER)]
    public int Decisions { get; set; }

    [Column(SheetsConfig.HeaderNames.ResponseRate, isInput: false, formatType: Format.PERCENT)]
    public decimal ResponseRate { get; set; }
}

/// <summary>
/// Monthly analytics summary
/// </summary>
[ExcludeFromCodeCoverage]
public class MonthlyEntity : SheetRowEntityBase
{
    [Column(SheetsConfig.HeaderNames.Month, isInput: false)]
    public string Month { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.ApplicationCount, isInput: false, formatType: Format.NUMBER)]
    public int Applications { get; set; }

    [Column(SheetsConfig.HeaderNames.Interviews, isInput: false, formatType: Format.NUMBER)]
    public int Interviews { get; set; }

    [Column(SheetsConfig.HeaderNames.InterviewRate, isInput: false, formatType: Format.PERCENT)]
    public decimal InterviewRate { get; set; }

    [Column(SheetsConfig.HeaderNames.Decision, isInput: false, formatType: Format.NUMBER)]
    public int Decisions { get; set; }

    [Column(SheetsConfig.HeaderNames.ResponseRate, isInput: false, formatType: Format.PERCENT)]
    public decimal ResponseRate { get; set; }

    [Column(SheetsConfig.HeaderNames.Accepted, isInput: false, formatType: Format.NUMBER)]
    public int Accepted { get; set; }

    [Column(SheetsConfig.HeaderNames.Rejected, isInput: false, formatType: Format.NUMBER)]
    public int Rejected { get; set; }
}

/// <summary>
/// Overall summary statistics
/// </summary>
[ExcludeFromCodeCoverage]
public class SummaryEntity : SheetRowEntityBase
{
    [Column("Metric", isInput: false)]
    public string Metric { get; set; } = "";

    [Column("Value", isInput: false)]
    public string Value { get; set; } = "";
}
