using RaptorSheets.Core.Entities;
using System.Text.Json.Serialization;
using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Job.Entities;

/// <summary>
/// Main data container entity for holding all sheet data from a Google Sheets workbook.
/// This is a data transfer object that aggregates data from all sheets.
/// Sheet order is determined by the explicit array in SheetsConfig.SheetUtilities.
/// </summary>
[ExcludeFromCodeCoverage]
public class SheetEntity
{
    [JsonPropertyName("properties")]
    public PropertyEntity Properties { get; set; } = new();

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

    // Analytics sheets
    [JsonPropertyName("weekly")]
    public List<WeeklyEntity> Weekly { get; set; } = [];

    [JsonPropertyName("monthly")]
    public List<MonthlyEntity> Monthly { get; set; } = [];

    [JsonPropertyName("summary")]
    public List<SummaryEntity> Summary { get; set; } = [];

    // Administrative sheet
    [JsonPropertyName("setup")]
    public List<SetupEntity> Setup { get; set; } = [];

    // Messages for operation feedback
    [JsonPropertyName("messages")]
    public List<MessageEntity> Messages { get; set; } = [];
}
