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
    Task<SheetEntity> ChangeSheetData(List<string> sheets, SheetEntity sheetEntity);
    Task<SheetEntity> CreateAllSheets();
    Task<SheetEntity> CreateSheets(List<string> sheets);
    Task<SheetEntity> DeleteAllSheets();
    Task<SheetEntity> DeleteSheets(List<string> sheets);
    Task<SheetEntity> GetSheet(string sheet);
    Task<SheetEntity> GetAllSheets();
    Task<SheetEntity> GetSheets(List<string> sheets);

    // Metadata & Properties
    Task<List<PropertyEntity>> GetAllSheetProperties();
    Task<List<PropertyEntity>> GetSheetProperties(List<string> sheets);
    Task<List<string>> GetAllSheetTabNames();
    Task<Spreadsheet?> GetSpreadsheetInfo(List<string>? ranges = null);
    Task<BatchGetValuesByDataFilterResponse?> GetBatchData(List<string> sheets);
    SheetModel? GetSheetLayout(string sheet);
    List<SheetModel> GetSheetLayouts(List<string> sheets);

    // Header Management
    Task<SheetEntity> InsertMissingColumns(Dictionary<string, List<ColumnInsertionInfo>> missingColumns);

    // Demo Data Generation
    Task<SheetEntity> SetupDemo(DateTime? startDate = null, DateTime? endDate = null, int? seed = null);
    Task<SheetEntity> PopulateDemoData(DateTime? startDate = null, DateTime? endDate = null, int? seed = null);
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

    protected override Task<SheetEntity> CreateMissingSheetsAsync(Dictionary<string, int> missingIndexMap)
    {
        return CreateSheets(missingIndexMap);
    }

    protected override BatchUpdateSpreadsheetRequest GenerateSheetsRequest(List<string> sheetNames)
    {
        return GenerateSheetsHelpers.Generate(sheetNames);
    }

    #endregion

    #region Create Operations

    public async Task<SheetEntity> CreateSheets(List<string> sheets)
    {
        return await CreateSheets(sheets, null);
    }

    public async Task<SheetEntity> CreateSheets(Dictionary<string, int> sheetsWithIndices)
    {
        if (sheetsWithIndices == null || sheetsWithIndices.Count == 0)
        {
            return await CreateSheets(new List<string>());
        }

        var sheets = SheetOrderingHelper.OrderSheetTitlesByIndex(sheetsWithIndices);

        return await CreateSheets(sheets, sheetsWithIndices);
    }

    #endregion

    #region Read Operations

    public async Task<SheetEntity> GetSheet(string sheet)
    {
        var sheetExists = GenerateSheetsHelpers.GetSheetNames()
            .Any(name => string.Equals(name, sheet, StringComparison.OrdinalIgnoreCase));

        if (!sheetExists)
        {
            return new SheetEntity { Messages = [MessageHelpers.CreateErrorMessage($"Sheet {sheet.ToUpperInvariant()} does not exist", MessageType.GET_SHEETS)] };
        }

        return await GetSheets([sheet]);
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

    public async Task<SheetEntity> ChangeSheetData(List<string> sheets, SheetEntity sheetEntity)
    {
        var (sheetsWithData, resolveMessages) = GoogleRequestHelpers.ResolveSheetsWithData(sheets, sheetEntity, _sheetAccessors);
        sheetEntity.Messages.AddRange(resolveMessages);

        if (sheetsWithData.Count == 0)
        {
            sheetEntity.Messages.Add(MessageHelpers.CreateWarningMessage("No data to change", MessageType.GENERAL));
            return sheetEntity;
        }

        var sheetInfo = await GetSheetProperties(sheets);
        var (requests, buildMessages) = GoogleRequestHelpers.BuildChangeRequests(sheetsWithData, sheetEntity, _sheetAccessors, sheetInfo);
        sheetEntity.Messages.AddRange(buildMessages);

        var batchUpdateSpreadsheetRequest = new BatchUpdateSpreadsheetRequest { Requests = requests };
        var batchUpdateSpreadsheetResponse = await _googleSheetService.BatchUpdateSpreadsheet(batchUpdateSpreadsheetRequest);

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
    public async Task<SheetEntity> SetupDemo(DateTime? startDate = null, DateTime? endDate = null, int? seed = null)
    {
        await CreateAllSheets();
        await Task.Delay(1500); // let freshly-created sheets become writable
        return await PopulateDemoData(startDate, endDate, seed);
    }

    /// <summary>
    /// Writes generated demo data (Applications and any Interviews) into the spreadsheet.
    /// Reference sheets are auto-populated by their formulas, so only the input sheets are written.
    /// </summary>
    public async Task<SheetEntity> PopulateDemoData(DateTime? startDate = null, DateTime? endDate = null, int? seed = null)
    {
        var demoData = GenerateDemoData(startDate, endDate, seed);

        var sheetsToWrite = new List<string> { SheetsConfig.SheetNames.Applications };
        if (demoData.Sheets.Interviews.Count > 0)
        {
            sheetsToWrite.Add(SheetsConfig.SheetNames.Interviews);
        }

        await ChangeSheetData(sheetsToWrite, demoData);
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
        var random = seed.HasValue ? new Random(seed.Value) : Random.Shared;

        var sheetEntity = new SheetEntity();

        var companies = new[]
        {
            "TechCorp", "DataSystems Inc", "Cloud Solutions", "AI Innovations", "WebWorks",
            "NextGen Labs", "Pioneer Software", "BrightPath Tech", "Nimbus Analytics", "Quantum Apps",
            "Vertex Systems", "BluePeak Digital", "CoreStack", "Apex Dynamics", "Forge Data",
            "Summit Software", "Sterling Systems", "Catalyst Labs", "Horizon Tech", "Meridian Solutions",
            "Orbit Analytics", "Silverline Digital", "Maple Street Co", "Paramount Logic", "Bluewater Tech",
            "Greenfield Apps", "UrbanGrid", "Solstice Software", "Ivory Tower Labs", "Praxis Systems", "NorthStar AI"
        };

        var positions = new[]
        {
            "Software Engineer", "Senior Developer", "Data Scientist", "Product Manager", "UX Designer",
            "QA Engineer", "DevOps Engineer", "Backend Engineer", "Frontend Engineer", "Engineering Manager",
            "Solutions Architect", "Security Engineer", "Data Engineer", "Business Analyst", "Mobile Developer"
        };

        var sites = new[] { "LinkedIn", "Indeed", "Glassdoor", "Company Website", "Wellfound", "ZipRecruiter" };
        var decisions = new[] { "Pending", "Accepted", "Rejected", "Interview Scheduled", "On Hold" };
        var schedules = new[] { "Full-time", "Part-time", "Contract", "Temporary", "Hybrid" };

        var totalDays = Math.Max(1, (end.Date - start.Date).Days + 1);
        var originalTargetApplications = Math.Max(220, totalDays * 5);

        var maxPerCompany = 3;
        var maxPossibleApplications = companies.Length * maxPerCompany;
        var targetApplications = Math.Min(originalTargetApplications, maxPossibleApplications);

        var applicationId = 2; // start at row 2 (row 1 reserved for headers)
        var companyCounts = companies.ToDictionary(c => c, _ => 0);

        for (var i = 0; i < targetApplications; i++)
        {
            var availableCompanies = companies.Where(c => companyCounts[c] < maxPerCompany).ToList();
            if (availableCompanies.Count == 0)
            {
                break;
            }

            var company = availableCompanies[random.Next(availableCompanies.Count)];
            companyCounts[company]++;

            var position = positions[random.Next(positions.Length)];
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
                Site = sites[random.Next(sites.Length)],
                Decision = decisions[random.Next(decisions.Length)],
                PayLow = payLow,
                PayHigh = payHigh,
                Location = random.Next(3) switch
                {
                    0 => "Remote",
                    1 => "Hybrid",
                    _ => "New York, NY"
                },
                Schedule = schedules[random.Next(schedules.Length)]
            });
        }

        // Reference lists (informational - written by their sheet formulas, not by PopulateDemoData)
        sheetEntity.Sheets.Sites.AddRange(sites.Select((s, i) => new SiteEntity { RowId = i + 1, Site = s }));
        sheetEntity.Sheets.Decisions.AddRange(decisions.Select((d, i) => new DecisionEntity { RowId = i + 1, Decision = d }));

        var interviewTypes = new[] { "Phone Screen", "Technical Interview", "Behavioral Interview", "On-Site", "Final Round" };
        sheetEntity.Sheets.InterviewTypes.AddRange(interviewTypes.Select((t, i) => new InterviewTypeEntity { RowId = i + 1, InterviewType = t }));

        var outcomes = new[] { "Passed", "Failed", "Pending", "Moved to Next Round" };
        sheetEntity.Sheets.InterviewOutcomes.AddRange(outcomes.Select((o, i) => new InterviewOutcomeEntity { RowId = i + 1, Outcome = o }));

        sheetEntity.Sheets.Schedules.AddRange(schedules.Select((s, i) => new ScheduleEntity { RowId = i + 1, Schedule = s }));

        // Interviews: about half of applications get interviews; popular combos get multiple rounds
        var interviewId = 2;
        var recruiters = new[]
        {
            "Alice Johnson", "Michael Brown", "Olivia Davis", "Daniel Wilson", "Emma Thompson",
            "James Anderson", "Sophia Martinez", "Liam Garcia", "Isabella Robinson", "Noah Clark",
            "Ava Rodriguez", "Ethan Lewis", "Mia Walker", "Lucas Hall", "Amelia Young",
            "Benjamin Allen", "Charlotte King", "Henry Wright", "Evelyn Scott", "Owen Green"
        };

        var interviewers = new[]
        {
            "Karen Miller", "Robert Moore", "Patricia Taylor", "John Jackson", "Linda White",
            "Barbara Harris", "Elizabeth Martin", "Thomas Thompson", "Christopher Garcia", "Matthew Martinez",
            "Anthony Robinson", "Mark Clark", "Steven Rodriguez", "Paul Lewis", "Andrew Lee",
            "Rachel Walker", "Rebecca Hall", "Julia Allen", "Victoria Young", "Kevin Hernandez"
        };

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

        foreach (var app in interviewCandidates)
        {
            var isMultiCombo = multiInterviewCombos.Contains((app.Company, app.JobTitle));

            int interviewCount = isMultiCombo
                ? random.Next(2, 6)
                : (random.NextDouble() < 0.5 ? 1 : (random.NextDouble() < 0.2 ? 2 : 1));

            var baseDate = DateTime.Parse(app.Date);

            for (var round = 0; round < interviewCount; round++)
            {
                var interviewDate = baseDate.AddDays(random.Next(0, 21) + round);
                var startHour = random.Next(8, 17);
                var startMinute = random.Next(0, 2) == 0 ? 0 : 30;
                var durationMinutes = new[] { 30, 45, 60, 90 }[random.Next(4)];

                var startTime = new DateTime(interviewDate.Year, interviewDate.Month, interviewDate.Day, startHour, startMinute, 0);
                var endTime = startTime.AddMinutes(durationMinutes);

                var recruiter = recruiters[random.Next(recruiters.Length)];
                var recruiterContact = recruiter.ToLower().Replace(' ', '.') + "@example.com";

                var numInterviewers = random.Next(1, 4);
                var attendees = new List<string>();
                for (var ai = 0; ai < numInterviewers; ai++)
                {
                    attendees.Add(interviewers[random.Next(interviewers.Length)]);
                }

                string interviewType;
                if (isMultiCombo)
                {
                    var progression = new[] { "Phone Screen", "Technical Interview", "Behavioral Interview", "On-Site", "Final Round" };
                    interviewType = progression[Math.Min(round, progression.Length - 1)];
                }
                else
                {
                    interviewType = interviewTypes[random.Next(interviewTypes.Length)];
                }

                sheetEntity.Sheets.Interviews.Add(new InterviewEntity
                {
                    RowId = interviewId++,
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
                    Outcome = outcomes[random.Next(outcomes.Length)],
                    Notes = "Auto-generated interview"
                });
            }
        }

        return sheetEntity;
    }

    #endregion
}
