using System.Globalization;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Logging;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Managers;
using RaptorSheets.Core.Models;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Job.Constants;
using RaptorSheets.Job.Entities;
using RaptorSheets.Job.Helpers;

namespace RaptorSheets.Job.Managers;

/// <summary>
/// Main interface for Google Sheet operations in the Job domain.
/// </summary>
public interface IGoogleSheetManager
{
    // CRUD Operations
    Task<SheetEntity> ChangeSheetData(List<string> sheets, SheetEntity sheetEntity, CancellationToken cancellationToken = default);
    Task<SheetEntity> CreateAllSheets(CancellationToken cancellationToken = default);
    Task<SheetEntity> CreateSheets(List<string> sheets, CancellationToken cancellationToken = default);
    Task<SheetEntity> DeleteAllSheets(CancellationToken cancellationToken = default);
    Task<SheetEntity> DeleteSheets(List<string> sheets, CancellationToken cancellationToken = default);
    Task<SheetEntity> GetSheet(string sheet, CancellationToken cancellationToken = default);
    Task<SheetEntity> GetAllSheets(CancellationToken cancellationToken = default);
    Task<SheetEntity> GetSheets(List<string> sheets, CancellationToken cancellationToken = default);

    // Metadata & Properties
    Task<List<PropertyEntity>> GetAllSheetProperties(CancellationToken cancellationToken = default);
    Task<List<PropertyEntity>> GetSheetProperties(List<string> sheets, CancellationToken cancellationToken = default);
    Task<List<string>> GetAllSheetTabNames(CancellationToken cancellationToken = default);
    Task<Spreadsheet?> GetSpreadsheetInfo(List<string>? ranges = null, CancellationToken cancellationToken = default);
    Task<BatchGetValuesByDataFilterResponse?> GetBatchData(List<string> sheets, CancellationToken cancellationToken = default);
    SheetModel? GetSheetLayout(string sheet);
    List<SheetModel> GetSheetLayouts(List<string> sheets);

    // Header Management
    Task<SheetEntity> InsertMissingColumns(Dictionary<string, List<ColumnInsertionInfo>> missingColumns, CancellationToken cancellationToken = default);

    // Demo Data Generation
    Task<SheetEntity> SetupDemo(DateTime? startDate = null, DateTime? endDate = null, int? seed = null, CancellationToken cancellationToken = default);
    Task<SheetEntity> PopulateDemoData(DateTime? startDate = null, DateTime? endDate = null, int? seed = null, CancellationToken cancellationToken = default);
    SheetEntity GenerateDemoData(DateTime? startDate = null, DateTime? endDate = null, int? seed = null);
}

/// <summary>
/// Main Google Sheet Manager for the Job domain.
///
/// Domain-agnostic read/metadata/layout/heal orchestration is inherited from
/// <see cref="GoogleSheetManagerBase{TEntity}"/>. This class adds only the Job-specific pieces:
/// constructors, the CreateMissingSheetsAsync self-heal hook, the GenerateSheetsRequest override,
/// the domain write operations, and demo-data generation.
/// </summary>
public class GoogleSheetManager : GoogleSheetManagerBase<SheetEntity>, IGoogleSheetManager
{
    #region Construction

    public GoogleSheetManager(RaptorSheets.Core.Services.IGoogleSheetService googleSheetService, ILogger? logger = null)
        : base(googleSheetService, JobSheetHelpers.Registry, GenerateSheetsHelpers.GetSheetNames(), logger)
    {
    }

    public GoogleSheetManager(string accessToken, string spreadsheetId, ILogger? logger = null)
        : base(accessToken, spreadsheetId, JobSheetHelpers.Registry, GenerateSheetsHelpers.GetSheetNames(), logger)
    {
    }

    public GoogleSheetManager(Dictionary<string, string> parameters, string spreadsheetId, ILogger? logger = null)
        : base(parameters, spreadsheetId, JobSheetHelpers.Registry, GenerateSheetsHelpers.GetSheetNames(), logger)
    {
    }

    protected override Task<SheetEntity> CreateMissingSheetsAsync(Dictionary<string, int> missingIndexMap, CancellationToken cancellationToken = default)
    {
        return CreateSheets(missingIndexMap, cancellationToken);
    }

    protected override BatchUpdateSpreadsheetRequest GenerateSheetsRequest(List<string> sheetNames)
    {
        return GenerateSheetsHelpers.Generate(sheetNames);
    }

    #endregion

    #region Create Operations

    public async Task<SheetEntity> CreateSheets(List<string> sheets, CancellationToken cancellationToken = default)
    {
        return await CreateSheets(sheets, null, cancellationToken);
    }

    public async Task<SheetEntity> CreateSheets(Dictionary<string, int> sheetsWithIndices, CancellationToken cancellationToken = default)
    {
        if (sheetsWithIndices == null || sheetsWithIndices.Count == 0)
        {
            return await CreateSheets(new List<string>(), cancellationToken);
        }

        var sheets = SheetOrderingHelper.OrderSheetTitlesByIndex(sheetsWithIndices);

        return await CreateSheets(sheets, sheetsWithIndices, cancellationToken);
    }

    #endregion

    #region Read Operations

    public async Task<SheetEntity> GetSheet(string sheet, CancellationToken cancellationToken = default)
    {
        var sheetExists = GenerateSheetsHelpers.GetSheetNames()
            .Any(name => string.Equals(name, sheet, StringComparison.OrdinalIgnoreCase));

        if (!sheetExists)
        {
            return new SheetEntity { Messages = [MessageHelpers.CreateErrorMessage($"Sheet {sheet.ToUpperInvariant()} does not exist", MessageType.GET_SHEETS)] };
        }

        return await GetSheets([sheet], cancellationToken);
    }

    #endregion

    #region Update Operations

    private static readonly Dictionary<string, GoogleRequestHelpers.SheetChangeAccessor<SheetEntity>> _sheetAccessors =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [SheetsConfig.SheetNames.Applications] = new(
                entity => entity.Sheets.Applications.Count,
                entity => entity.Sheets.Applications,
                (data, properties) => JobRequestHelpers.ChangeApplicationSheetData(data as List<ApplicationEntity> ?? [], properties)),
            [SheetsConfig.SheetNames.Interviews] = new(
                entity => entity.Sheets.Interviews.Count,
                entity => entity.Sheets.Interviews,
                (data, properties) => JobRequestHelpers.ChangeInterviewSheetData(data as List<InterviewEntity> ?? [], properties)),
            [SheetsConfig.SheetNames.CompanyDetails] = new(
                entity => entity.Sheets.CompanyDetails.Count,
                entity => entity.Sheets.CompanyDetails,
                (data, properties) => JobRequestHelpers.ChangeCompanyDetailSheetData(data as List<CompanyDetailEntity> ?? [], properties)),
            [SheetsConfig.SheetNames.PositionDetails] = new(
                entity => entity.Sheets.PositionDetails.Count,
                entity => entity.Sheets.PositionDetails,
                (data, properties) => JobRequestHelpers.ChangePositionDetailSheetData(data as List<PositionDetailEntity> ?? [], properties)),
            [SheetsConfig.SheetNames.Setup] = new(
                entity => entity.Sheets.Setup.Count,
                entity => entity.Sheets.Setup,
                (data, properties) => JobRequestHelpers.ChangeSetupSheetData(data as List<SetupEntity> ?? [], properties))
        };

    public async Task<SheetEntity> ChangeSheetData(List<string> sheets, SheetEntity sheetEntity, CancellationToken cancellationToken = default)
    {
        var (sheetsWithData, resolveMessages) = GoogleRequestHelpers.ResolveSheetsWithData(sheets, sheetEntity, _sheetAccessors);
        sheetEntity.Messages.AddRange(resolveMessages);

        if (sheetsWithData.Count == 0)
        {
            sheetEntity.Messages.Add(MessageHelpers.CreateWarningMessage("No data to change", MessageType.GENERAL));
            return sheetEntity;
        }

        var sheetInfo = await GetSheetProperties(sheets, cancellationToken);
        var (requests, buildMessages) = GoogleRequestHelpers.BuildChangeRequests(sheetsWithData, sheetEntity, _sheetAccessors, sheetInfo);
        sheetEntity.Messages.AddRange(buildMessages);

        var batchUpdateSpreadsheetRequest = new BatchUpdateSpreadsheetRequest { Requests = requests };
        var batchUpdateSpreadsheetResponse = await _googleSheetService.BatchUpdateSpreadsheet(batchUpdateSpreadsheetRequest, cancellationToken);

        if (batchUpdateSpreadsheetResponse == null)
        {
            sheetEntity.Messages.Add(MessageHelpers.CreateErrorMessage($"Unable to save data", MessageType.SAVE_DATA));
        }

        return sheetEntity;
    }

    #endregion

    #region Header Validation

    public static List<MessageEntity> CheckUnknownSheets(Spreadsheet sheetInfoResponse)
    {
        return JobSheetHelpers.CheckUnknownSheets(sheetInfoResponse);
    }

    public static List<MessageEntity> CheckSheetHeaders(Spreadsheet sheetInfoResponse)
    {
        return JobSheetHelpers.CheckSheetHeaders(sheetInfoResponse);
    }

    public static List<MessageEntity> CheckSheetHeaders(Spreadsheet sheetInfoResponse, out Dictionary<string, List<ColumnInsertionInfo>> missingColumns)
    {
        return JobSheetHelpers.CheckSheetHeaders(sheetInfoResponse, out missingColumns);
    }

    #endregion

    #region Demo Data Generation

    /// <summary>
    /// Creates all sheets and then fills Applications/Interviews with realistic demo data.
    /// </summary>
    public async Task<SheetEntity> SetupDemo(DateTime? startDate = null, DateTime? endDate = null, int? seed = null, CancellationToken cancellationToken = default)
    {
        await CreateAllSheets(cancellationToken);
        await Task.Delay(1500, cancellationToken); // let freshly-created sheets become writable
        return await PopulateDemoData(startDate, endDate, seed, cancellationToken);
    }

    /// <summary>
    /// Writes generated demo data (Applications and any Interviews) into the spreadsheet.
    /// Reference sheets are auto-populated by their formulas, so only the input sheets are written.
    /// </summary>
    public async Task<SheetEntity> PopulateDemoData(DateTime? startDate = null, DateTime? endDate = null, int? seed = null, CancellationToken cancellationToken = default)
    {
        var demoData = GenerateDemoData(startDate, endDate, seed);

        var sheetsToWrite = new List<string> { SheetsConfig.SheetNames.Applications };
        if (demoData.Sheets.Interviews.Count > 0)
        {
            sheetsToWrite.Add(SheetsConfig.SheetNames.Interviews);
        }

        await ChangeSheetData(sheetsToWrite, demoData, cancellationToken);
        return demoData;
    }

    /// <summary>
    /// Generates realistic demo Applications and Interviews (plus reference lists) without writing
    /// them to any spreadsheet. Applications/Interviews use RowIds starting at 2 so a subsequent
    /// write lands them below the header row.
    /// </summary>
    public SheetEntity GenerateDemoData(DateTime? startDate = null, DateTime? endDate = null, int? seed = null)
    {
        var start = startDate ?? DateTime.Today.AddDays(-30);
        var end = endDate ?? DateTime.Today;
        // SonarQube S2245: Using Random is safe here - this generates demo/sample data, not security-sensitive values
#pragma warning disable S2245
        var random = seed.HasValue ? new Random(seed.Value) : Random.Shared;
#pragma warning restore S2245

        var sheetEntity = new SheetEntity();
        var refData = BuildDemoReferenceData();

        GenerateApplications(sheetEntity, random, refData, start, end);

        // Reference lists (informational - written by their sheet formulas, not by PopulateDemoData)
        sheetEntity.Sheets.Sites.AddRange(refData.Sites.Select((s, i) => new SiteEntity { RowId = i + 1, Site = s }));
        sheetEntity.Sheets.Decisions.AddRange(refData.Decisions.Select((d, i) => new DecisionEntity { RowId = i + 1, Decision = d }));
        sheetEntity.Sheets.InterviewTypes.AddRange(refData.InterviewTypes.Select((t, i) => new InterviewTypeEntity { RowId = i + 1, InterviewType = t }));
        sheetEntity.Sheets.InterviewOutcomes.AddRange(refData.Outcomes.Select((o, i) => new InterviewOutcomeEntity { RowId = i + 1, Outcome = o }));
        sheetEntity.Sheets.Schedules.AddRange(refData.Schedules.Select((s, i) => new ScheduleEntity { RowId = i + 1, Schedule = s }));

        GenerateInterviews(sheetEntity, random, refData);

        return sheetEntity;
    }

    private static DemoReferenceData BuildDemoReferenceData()
    {
        return new DemoReferenceData(
            Companies:
            [
                "TechCorp", "DataSystems Inc", "Cloud Solutions", "AI Innovations", "WebWorks",
                "NextGen Labs", "Pioneer Software", "BrightPath Tech", "Nimbus Analytics", "Quantum Apps",
                "Vertex Systems", "BluePeak Digital", "CoreStack", "Apex Dynamics", "Forge Data",
                "Summit Software", "Sterling Systems", "Catalyst Labs", "Horizon Tech", "Meridian Solutions",
                "Orbit Analytics", "Silverline Digital", "Maple Street Co", "Paramount Logic", "Bluewater Tech",
                "Greenfield Apps", "UrbanGrid", "Solstice Software", "Ivory Tower Labs", "Praxis Systems", "NorthStar AI"
            ],
            Positions:
            [
                "Software Engineer", "Senior Developer", "Data Scientist", "Product Manager", "UX Designer",
                "QA Engineer", "DevOps Engineer", "Backend Engineer", "Frontend Engineer", "Engineering Manager",
                "Solutions Architect", "Security Engineer", "Data Engineer", "Business Analyst", "Mobile Developer"
            ],
            Sites: ["LinkedIn", "Indeed", "Glassdoor", "Company Website", "Wellfound", "ZipRecruiter"],
            Decisions: ["Pending", "Accepted", "Rejected", "Interview Scheduled", "On Hold"],
            Schedules: ["Full-time", "Part-time", "Contract", "Temporary", "Hybrid"],
            Recruiters:
            [
                "Alice Johnson", "Michael Brown", "Olivia Davis", "Daniel Wilson", "Emma Thompson",
                "James Anderson", "Sophia Martinez", "Liam Garcia", "Isabella Robinson", "Noah Clark",
                "Ava Rodriguez", "Ethan Lewis", "Mia Walker", "Lucas Hall", "Amelia Young",
                "Benjamin Allen", "Charlotte King", "Henry Wright", "Evelyn Scott", "Owen Green"
            ],
            Interviewers:
            [
                "Karen Miller", "Robert Moore", "Patricia Taylor", "John Jackson", "Linda White",
                "Barbara Harris", "Elizabeth Martin", "Thomas Thompson", "Christopher Garcia", "Matthew Martinez",
                "Anthony Robinson", "Mark Clark", "Steven Rodriguez", "Paul Lewis", "Andrew Lee",
                "Rachel Walker", "Rebecca Hall", "Julia Allen", "Victoria Young", "Kevin Hernandez"
            ],
            InterviewTypes: ["Phone Screen", "Technical Interview", "Behavioral Interview", "On-Site", "Final Round"],
            Outcomes: ["Passed", "Failed", "Pending", "Moved to Next Round"]
        );
    }

    private static void GenerateApplications(SheetEntity sheetEntity, Random random, DemoReferenceData refData, DateTime start, DateTime end)
    {
        var totalDays = Math.Max(1, (end.Date - start.Date).Days + 1);
        var originalTargetApplications = Math.Max(220, totalDays * 5);

        var maxPerCompany = 3;
        var maxPossibleApplications = refData.Companies.Length * maxPerCompany;
        var targetApplications = Math.Min(originalTargetApplications, maxPossibleApplications);

        var applicationId = 2; // start at row 2 (row 1 reserved for headers)
        var companyCounts = refData.Companies.ToDictionary(c => c, _ => 0);

        for (var i = 0; i < targetApplications; i++)
        {
            var availableCompanies = refData.Companies.Where(c => companyCounts[c] < maxPerCompany).ToList();
            if (availableCompanies.Count == 0)
            {
                break;
            }

            var company = availableCompanies[random.Next(availableCompanies.Count)];
            companyCounts[company]++;

            var position = refData.Positions[random.Next(refData.Positions.Length)];
            var appDate = start.AddDays(random.Next(totalDays));

            var payLow = random.Next(70000, 180000);
            var payHigh = payLow + random.Next(10000, 70000);

            sheetEntity.Sheets.Applications.Add(new ApplicationEntity
            {
                RowId = applicationId++,
                Date = appDate.ToString("yyyy-MM-dd"),
                Company = company,
                JobTitle = position,
                Posting = $"https://example.com/job/{random.Next(100000, 999999)}",
                Site = refData.Sites[random.Next(refData.Sites.Length)],
                Decision = refData.Decisions[random.Next(refData.Decisions.Length)],
                PayLow = payLow,
                PayHigh = payHigh,
                Location = random.Next(3) switch
                {
                    0 => "Remote",
                    1 => "Hybrid",
                    _ => "New York, NY"
                },
                Schedule = refData.Schedules[random.Next(refData.Schedules.Length)]
            });
        }
    }

    // Interviews: about half of applications get interviews; popular combos get multiple rounds
    private static void GenerateInterviews(SheetEntity sheetEntity, Random random, DemoReferenceData refData)
    {
        var combos = sheetEntity.Sheets.Applications.Select(a => (a.Company, a.JobTitle)).Distinct().ToList();
        var multiInterviewCombos = new HashSet<(string Company, string JobTitle)>();
        var comboCount = Math.Max(8, combos.Count / 10);
        var shuffledCombos = combos.OrderBy(_ => random.Next()).ToList();
        for (var c = 0; c < Math.Min(comboCount, shuffledCombos.Count); c++)
        {
            multiInterviewCombos.Add(shuffledCombos[c]);
        }

        var interviewCandidates = sheetEntity.Sheets.Applications
            .OrderBy(_ => random.Next())
            .Take(sheetEntity.Sheets.Applications.Count / 2)
            .ToList();

        var interviewId = 2;
        foreach (var app in interviewCandidates)
        {
            var isMultiCombo = multiInterviewCombos.Contains((app.Company, app.JobTitle));
            var interviewCount = DetermineInterviewCount(random, isMultiCombo);
            var baseDate = DateTime.Parse(app.Date, CultureInfo.InvariantCulture);

            for (var round = 0; round < interviewCount; round++)
            {
                var interview = BuildInterview(app, round, isMultiCombo, baseDate, random, refData);
                interview.RowId = interviewId++;
                sheetEntity.Sheets.Interviews.Add(interview);
            }
        }
    }

    private static int DetermineInterviewCount(Random random, bool isMultiCombo)
    {
        if (isMultiCombo)
        {
            return random.Next(2, 6);
        }

        if (random.NextDouble() < 0.5)
        {
            return 1;
        }

        return random.NextDouble() < 0.2 ? 2 : 1;
    }

    private static InterviewEntity BuildInterview(ApplicationEntity app, int round, bool isMultiCombo, DateTime baseDate, Random random, DemoReferenceData refData)
    {
        var interviewDate = baseDate.AddDays(random.Next(0, 21) + round);
        var startHour = random.Next(8, 17);
        var startMinute = random.Next(0, 2) == 0 ? 0 : 30;
        var durationMinutes = new[] { 30, 45, 60, 90 }[random.Next(4)];

        var startTime = new DateTime(interviewDate.Year, interviewDate.Month, interviewDate.Day, startHour, startMinute, 0, DateTimeKind.Unspecified);
        var endTime = startTime.AddMinutes(durationMinutes);

        var recruiter = refData.Recruiters[random.Next(refData.Recruiters.Length)];
        var recruiterContact = recruiter.ToLower().Replace(' ', '.') + "@example.com";

        var numInterviewers = random.Next(1, 4);
        var attendees = new List<string>();
        for (var ai = 0; ai < numInterviewers; ai++)
        {
            attendees.Add(refData.Interviewers[random.Next(refData.Interviewers.Length)]);
        }

        string interviewType;
        if (isMultiCombo)
        {
            var progression = new[] { "Phone Screen", "Technical Interview", "Behavioral Interview", "On-Site", "Final Round" };
            interviewType = progression[Math.Min(round, progression.Length - 1)];
        }
        else
        {
            interviewType = refData.InterviewTypes[random.Next(refData.InterviewTypes.Length)];
        }

        return new InterviewEntity
        {
            Date = interviewDate.ToString("yyyy-MM-dd"),
            StartTime = startTime.ToString("hh:mm tt").ToLowerInvariant(),
            EndTime = endTime.ToString("hh:mm tt").ToLowerInvariant(),
            Duration = TimeSpan.FromMinutes(durationMinutes).ToString(@"hh\:mm"),
            Company = app.Company,
            JobTitle = app.JobTitle,
            InterviewType = interviewType,
            RecruiterName = recruiter,
            RecruiterContact = recruiterContact,
            Attendees = string.Join(", ", attendees),
            Outcome = refData.Outcomes[random.Next(refData.Outcomes.Length)],
            Notes = "Auto-generated interview"
        };
    }

    private readonly record struct DemoReferenceData(
        string[] Companies,
        string[] Positions,
        string[] Sites,
        string[] Decisions,
        string[] Schedules,
        string[] Recruiters,
        string[] Interviewers,
        string[] InterviewTypes,
        string[] Outcomes);

    #endregion
}
