using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Factories;
using RaptorSheets.Job.Entities;

namespace RaptorSheets.Sample.Web.Services;

/// <summary>Connects to the Job spreadsheet on first use - see GigSheetOperations for the shared
/// reasoning.</summary>
public class JobSheetOperations(
    ISheetManagerFactory<RaptorSheets.Job.Managers.ISheetManager> factory,
    IConfiguration configuration,
    ReferenceSheetCache cache)
    : SheetOperationsBase<RaptorSheets.Job.Managers.ISheetManager, SheetEntity, JobSheets>(factory, configuration, cache)
{
    public override string DomainName => "job";
    public override string DomainLabel => "Job Applications";
    public override Type SheetsType => typeof(JobSheets);
    public override Type SheetNamesType => typeof(RaptorSheets.Job.Constants.SheetsConfig.SheetNames);

    // Weekly/Monthly/Summary are DTO placeholders with no backing sheet/formula yet (see
    // JobSheets.cs) - SheetsConfig.SheetNames has no matching constants for them either, so
    // SheetMetadata would fall back to these exact property names for real Job sheets too if not
    // excluded here.
    public override IReadOnlySet<string> ExcludedSheetNames { get; } = new HashSet<string> { "Weekly", "Monthly", "Summary" };

    public override IReadOnlyDictionary<string, string> ValidationSheetMap { get; } = new Dictionary<string, string>
    {
        [RaptorSheets.Job.Constants.SheetsConfig.ValidationNames.RangeCompany] = RaptorSheets.Job.Constants.SheetsConfig.SheetNames.Companies,
        [RaptorSheets.Job.Constants.SheetsConfig.ValidationNames.RangePosition] = RaptorSheets.Job.Constants.SheetsConfig.SheetNames.Positions,
        [RaptorSheets.Job.Constants.SheetsConfig.ValidationNames.RangeSite] = RaptorSheets.Job.Constants.SheetsConfig.SheetNames.Sites,
        [RaptorSheets.Job.Constants.SheetsConfig.ValidationNames.RangeDecision] = RaptorSheets.Job.Constants.SheetsConfig.SheetNames.Decisions,
        [RaptorSheets.Job.Constants.SheetsConfig.ValidationNames.RangeSchedule] = RaptorSheets.Job.Constants.SheetsConfig.SheetNames.Schedules,
        [RaptorSheets.Job.Constants.SheetsConfig.ValidationNames.RangeInterviewType] = RaptorSheets.Job.Constants.SheetsConfig.SheetNames.InterviewTypes,
        [RaptorSheets.Job.Constants.SheetsConfig.ValidationNames.RangeInterviewOutcome] = RaptorSheets.Job.Constants.SheetsConfig.SheetNames.InterviewOutcomes,
    };

    // PopulateDemoData directly, not SetupDemo - sheet creation is CreateAllSheetsAsync's job
    // (see IConnectedSheet.InsertDemoDataAsync).
    protected override async Task<List<MessageEntity>> InsertDemoDataAsync(RaptorSheets.Job.Managers.ISheetManager manager)
    {
        var result = await manager.PopulateDemoData();
        return result.Messages;
    }
}
