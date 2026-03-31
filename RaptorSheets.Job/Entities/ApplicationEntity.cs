using RaptorSheets.Core.Attributes;
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

    [Column(SheetsConfig.HeaderNames.Date, isInput: true, formatType: FormatEnum.DATE)]
    public string Date { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Company, isInput: true, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangeCompany)]
    public string Company { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.JobTitle, isInput: true, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangePosition)]
    public string JobTitle { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Posting, isInput: true)]
    public string Posting { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Site, isInput: true, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangeSite)]
    public string Site { get; set; } = "";

    // Calculated columns

    [Column(SheetsConfig.HeaderNames.Key, isInput: false)]
    public string Key { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.InterviewCount, isInput: false, formatType: FormatEnum.NUMBER)]
    public int InterviewCount { get; set; }

    // Input columns continued

    [Column(SheetsConfig.HeaderNames.Decision, isInput: true, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangeDecision)]
    public string Decision { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.DecisionDate, isInput: true, formatType: FormatEnum.DATE)]
    public string DecisionDate { get; set; } = "";

    // Calculated column

    [Column(SheetsConfig.HeaderNames.DaysActive, isInput: false, formatType: FormatEnum.NUMBER)]
    public int? DaysActive { get; set; }

    // Input columns continued

    [Column(SheetsConfig.HeaderNames.Notes, isInput: true)]
    public string Notes { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.PayLow, isInput: true, formatType: FormatEnum.ACCOUNTING)]
    public decimal? PayLow { get; set; }

    [Column(SheetsConfig.HeaderNames.PayHigh, isInput: true, formatType: FormatEnum.ACCOUNTING)]
    public decimal? PayHigh { get; set; }

    // Calculated column

    [Column(SheetsConfig.HeaderNames.PayAvg, isInput: false, formatType: FormatEnum.ACCOUNTING)]
    public decimal? PayAvg { get; set; }

    // Input columns continued

    [Column(SheetsConfig.HeaderNames.Location, isInput: true)]
    public string Location { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Schedule, isInput: true, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangeSchedule)]
    public string Schedule { get; set; } = "";
}
