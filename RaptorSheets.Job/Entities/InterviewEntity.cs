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

    [Column(SheetsConfig.HeaderNames.Date, isInput: true, formatType: FormatEnum.DATE)]
    public string Date { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.StartTime, isInput: true, formatType: FormatEnum.TIME)]
    public string StartTime { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.EndTime, isInput: true, formatType: FormatEnum.TIME)]
    public string EndTime { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Duration, isInput: true, formatType: FormatEnum.DURATION)]
    public string Duration { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Company, isInput: true, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangeCompany)]
    public string Company { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.JobTitle, isInput: true, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangePosition)]
    public string JobTitle { get; set; } = "";

    // Calculated/linked columns
    // Input columns continued

    [Column(SheetsConfig.HeaderNames.InterviewType, isInput: true, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangeInterviewType)]
    public string InterviewType { get; set; } = "";

    // Calculated column

    [Column(SheetsConfig.HeaderNames.InterviewRound, isInput: false, formatType: FormatEnum.NUMBER)]
    public int InterviewRound { get; set; }

    // Input columns continued

    [Column(SheetsConfig.HeaderNames.RecruiterName, isInput: true)]
    public string RecruiterName { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.RecruiterContact, isInput: true)]
    public string RecruiterContact { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Attendees, isInput: true)]
    public string Attendees { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Outcome, isInput: true, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangeInterviewOutcome)]
    public string Outcome { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Notes, isInput: true)]
    public string Notes { get; set; } = "";

    // Duplicate counter for company+jobtitle combos (user-visible '#')
    [Column(SheetsConfig.HeaderNames.Duplicate, isInput: false, formatType: FormatEnum.NUMBER)]
    public int? Duplicate { get; set; }

    // Calculated/linked columns - moved to the end so the Key is not directly edited in the main input area
    [Column(SheetsConfig.HeaderNames.Key, isInput: false)]
    public string Key { get; set; } = "";
}
