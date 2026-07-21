using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace RaptorSheets.Job.Entities;

/// <summary>
/// Holds every strongly-typed row collection for the Job domain, nested under
/// <see cref="SheetEntity.Sheets"/> so domain sheet data can never collide with the reserved
/// top-level <c>Properties</c>/<c>Messages</c> members. Sheet order follows
/// SheetsConfig.SheetUtilities.GetAllSheetNames().
/// </summary>
[ExcludeFromCodeCoverage]
public class JobSheets
{
    // Primary input sheets
    [JsonPropertyName("applications")]
    public List<ApplicationEntity> Applications { get; set; } = [];

    [JsonPropertyName("interviews")]
    public List<InterviewEntity> Interviews { get; set; } = [];

    // Optional input sheets
    [JsonPropertyName("companyDetails")]
    public List<CompanyDetailEntity> CompanyDetails { get; set; } = [];

    [JsonPropertyName("positionDetails")]
    public List<PositionDetailEntity> PositionDetails { get; set; } = [];

    // Reference sheets (calculated)
    [JsonPropertyName("companies")]
    public List<CompanyEntity> Companies { get; set; } = [];

    [JsonPropertyName("positions")]
    public List<PositionEntity> Positions { get; set; } = [];

    [JsonPropertyName("sites")]
    public List<SiteEntity> Sites { get; set; } = [];

    [JsonPropertyName("decisions")]
    public List<DecisionEntity> Decisions { get; set; } = [];

    [JsonPropertyName("interviewTypes")]
    public List<InterviewTypeEntity> InterviewTypes { get; set; } = [];

    [JsonPropertyName("interviewOutcomes")]
    public List<InterviewOutcomeEntity> InterviewOutcomes { get; set; } = [];

    [JsonPropertyName("schedules")]
    public List<ScheduleEntity> Schedules { get; set; } = [];

    // Administrative sheet
    [JsonPropertyName("setup")]
    public List<SetupEntity> Setup { get; set; } = [];

    // Analytics sheets - DTO placeholders; sheets/formulas not yet implemented.
    [JsonPropertyName("weekly")]
    public List<WeeklyEntity> Weekly { get; set; } = [];

    [JsonPropertyName("monthly")]
    public List<MonthlyEntity> Monthly { get; set; } = [];

    [JsonPropertyName("summary")]
    public List<SummaryEntity> Summary { get; set; } = [];
}
