using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Enums;
using RaptorSheets.Job.Constants;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace RaptorSheets.Job.Entities;

/// <summary>
/// Represents an interview entry linked to a job application
/// </summary>
[ExcludeFromCodeCoverage]
public class InterviewEntity : SheetRowEntityBase
{
    // Input columns (user-entered data)

    [Column(SheetsConfig.HeaderNames.Date, isInput: true, note: ColumnNotes.InterviewDate, formatType: FormatEnum.DATE)]
    public string Date { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.StartTime, isInput: true, note: ColumnNotes.InterviewStartTime, formatType: FormatEnum.TIME)]
    public string StartTime { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.EndTime, isInput: true, note: ColumnNotes.InterviewEndTime, formatType: FormatEnum.TIME)]
    public string EndTime { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Duration, isInput: true, note: ColumnNotes.InterviewDuration, formatType: FormatEnum.DURATION)]
    public string Duration { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Company, isInput: true, note: ColumnNotes.InterviewCompany, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangeCompany)]
    public string Company { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.JobTitle, isInput: true, note: ColumnNotes.InterviewJobTitle, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangePosition)]
    public string JobTitle { get; set; } = "";

    // Calculated/linked columns
    // Input columns continued

    [Column(SheetsConfig.HeaderNames.InterviewType, isInput: true, note: ColumnNotes.InterviewType, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangeInterviewType)]
    public string InterviewType { get; set; } = "";

    // Calculated column

    [Column(SheetsConfig.HeaderNames.InterviewRound, isInput: false, note: ColumnNotes.InterviewRound, formatType: FormatEnum.NUMBER)]
    public int InterviewRound { get; set; }

    // Input columns continued

    [Column(SheetsConfig.HeaderNames.RecruiterName, isInput: true, note: ColumnNotes.InterviewRecruiterName)]
    public string RecruiterName { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.RecruiterContact, isInput: true, note: ColumnNotes.InterviewRecruiterContact)]
    public string RecruiterContact { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Attendees, isInput: true, note: ColumnNotes.InterviewAttendees)]
    public string Attendees { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Outcome, isInput: true, note: ColumnNotes.InterviewOutcome, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangeInterviewOutcome)]
    public string Outcome { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Notes, isInput: true, note: ColumnNotes.InterviewNotes)]
    public string Notes { get; set; } = "";

    // Duplicate counter for company+jobtitle combos (user-visible '#')
    [Column(SheetsConfig.HeaderNames.Duplicate, isInput: false, note: ColumnNotes.InterviewDuplicate, formatType: FormatEnum.NUMBER)]
    public int? Duplicate { get; set; }

    // Calculated/linked columns - moved to the end so the Key is not directly edited in the main input area
    [Column(SheetsConfig.HeaderNames.Key, isInput: false, note: ColumnNotes.InterviewKey)]
    public string Key { get; set; } = "";
}
