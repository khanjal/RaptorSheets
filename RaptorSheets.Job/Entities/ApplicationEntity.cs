using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Job.Constants;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace RaptorSheets.Job.Entities;

/// <summary>
/// Represents a job application entry
/// </summary>
[ExcludeFromCodeCoverage]
public class ApplicationEntity : SheetRowEntityBase
{
    // Input columns (user-entered data)

    [Column(SheetsConfig.HeaderNames.Date, isInput: true, note: ColumnNotes.ApplicationDate, formatType: Format.DATE)]
    public string Date { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Company, isInput: true, note: ColumnNotes.ApplicationCompany, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangeCompany)]
    public string Company { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.JobTitle, isInput: true, note: ColumnNotes.ApplicationJobTitle, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangePosition)]
    public string JobTitle { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Posting, isInput: true, note: ColumnNotes.ApplicationPosting)]
    public string Posting { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Site, isInput: true, note: ColumnNotes.ApplicationSite, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangeSite)]
    public string Site { get; set; } = "";

    // Calculated columns

    [Column(SheetsConfig.HeaderNames.InterviewCount, isInput: false, note: ColumnNotes.ApplicationInterviewCount, formatType: Format.NUMBER)]
    public int InterviewCount { get; set; }

    // Input columns continued

    [Column(SheetsConfig.HeaderNames.Decision, isInput: true, note: ColumnNotes.ApplicationDecision, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangeDecision)]
    public string Decision { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.DecisionDate, isInput: true, note: ColumnNotes.ApplicationDecisionDate, formatType: Format.DATE)]
    public string DecisionDate { get; set; } = "";

    // Calculated column

    [Column(SheetsConfig.HeaderNames.DaysActive, isInput: false, note: ColumnNotes.ApplicationDaysActive, formatType: Format.NUMBER)]
    public int? DaysActive { get; set; }

    // Input columns continued

    [Column(SheetsConfig.HeaderNames.Notes, isInput: true, note: ColumnNotes.ApplicationNotes)]
    public string Notes { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.PayLow, isInput: true, note: ColumnNotes.ApplicationPayLow, formatType: Format.ACCOUNTING)]
    public decimal? PayLow { get; set; }

    [Column(SheetsConfig.HeaderNames.PayHigh, isInput: true, note: ColumnNotes.ApplicationPayHigh, formatType: Format.ACCOUNTING)]
    public decimal? PayHigh { get; set; }

    // Calculated column

    [Column(SheetsConfig.HeaderNames.PayAvg, isInput: false, note: ColumnNotes.ApplicationPayAvg, formatType: Format.ACCOUNTING)]
    public decimal? PayAvg { get; set; }

    // Input columns continued

    [Column(SheetsConfig.HeaderNames.Location, isInput: true, note: ColumnNotes.ApplicationLocation)]
    public string Location { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Schedule, isInput: true, note: ColumnNotes.ApplicationSchedule, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangeSchedule)]
    public string Schedule { get; set; } = "";

    // Duplicate counter for company+jobtitle combos (user-visible '#')
    [Column(SheetsConfig.HeaderNames.Duplicate, isInput: false, note: ColumnNotes.ApplicationDuplicate, formatType: Format.NUMBER)]
    public int? Duplicate { get; set; }

    // Calculated/linked columns - moved to the end so the Key column is not visible in primary input area
    [Column(SheetsConfig.HeaderNames.Key, isInput: false, note: ColumnNotes.ApplicationKey)]
    public string Key { get; set; } = "";
}
