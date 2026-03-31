using RaptorSheets.Job.Entities;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Job.Mappers;
using Google.Apis.Sheets.v4.Data;

namespace RaptorSheets.Job.Managers;

/// <summary>
/// Demo data generation for the Job domain.
/// Creates sample application and interview data for testing and demonstration purposes.
/// </summary>
public partial class GoogleSheetManager
{
    public async Task<SheetEntity> SetupDemo(DateTime? startDate = null, DateTime? endDate = null, int? seed = null)
    {
        // Create all sheets first
        await CreateAllSheets();

        // Then populate with demo data
        return await PopulateDemoData(startDate, endDate, seed);
    }



    public SheetEntity GenerateDemoData(DateTime? startDate = null, DateTime? endDate = null, int? seed = null)
    {
        // Use default date range if not specified (last 30 days)
        var start = startDate ?? DateTime.Today.AddDays(-30);
        var end = endDate ?? DateTime.Today;
        var random = seed.HasValue ? new Random(seed.Value) : new Random();

        var sheetEntity = new SheetEntity();

        // Generate demo applications
        var companies = new[] { "TechCorp", "DataSystems Inc", "Cloud Solutions", "AI Innovations", "WebWorks" };
        var positions = new[] { "Software Engineer", "Senior Developer", "Data Scientist", "Product Manager", "UX Designer" };
        var sites = new[] { "LinkedIn", "Indeed", "Glassdoor", "Company Website" };
        var decisions = new[] { "Pending", "Accepted", "Rejected", "Interview Scheduled" };

        var applicationId = 2; // start at row 2 (row 1 reserved for headers)
        for (var date = start; date <= end; date = date.AddDays(random.Next(2, 7)))
        {
            var company = companies[random.Next(companies.Length)];
            var position = positions[random.Next(positions.Length)];

            sheetEntity.Applications.Add(new ApplicationEntity
            {
                RowId = applicationId++,
                Date = date.ToString("yyyy-MM-dd"),
                Company = company,
                JobTitle = position,
                Posting = $"https://example.com/job/{random.Next(1000, 9999)}",
                Site = sites[random.Next(sites.Length)],
                Decision = decisions[random.Next(decisions.Length)],
                PayLow = random.Next(70000, 120000),
                PayHigh = random.Next(120000, 180000),
                Location = random.Next(2) == 0 ? "Remote" : "New York, NY",
                Schedule = random.Next(3) == 0 ? "Contract" : "Full-time"
            });
        }

        // Add a few explicit duplicate company/job combos to exercise duplicate counting
        if (sheetEntity.Applications.Count >= 2)
        {
            // Duplicate first application twice
            var first = sheetEntity.Applications[0];
            sheetEntity.Applications.Add(new ApplicationEntity
            {
                RowId = applicationId++,
                Date = start.AddDays(1).ToString("yyyy-MM-dd"),
                Company = first.Company,
                JobTitle = first.JobTitle,
                Posting = first.Posting,
                Site = first.Site,
                Decision = first.Decision,
                PayLow = first.PayLow,
                PayHigh = first.PayHigh,
                Location = first.Location,
                Schedule = first.Schedule
            });

            sheetEntity.Applications.Add(new ApplicationEntity
            {
                RowId = applicationId++,
                Date = start.AddDays(2).ToString("yyyy-MM-dd"),
                Company = first.Company,
                JobTitle = first.JobTitle,
                Posting = first.Posting,
                Site = first.Site,
                Decision = first.Decision,
                PayLow = first.PayLow,
                PayHigh = first.PayHigh,
                Location = first.Location,
                Schedule = first.Schedule
            });
        }

        // Generate demo reference data
        sheetEntity.Sites.AddRange(sites.Select((s, i) => new SiteEntity { RowId = i + 1, Site = s }));
        sheetEntity.Decisions.AddRange(decisions.Select((d, i) => new DecisionEntity { RowId = i + 1, Decision = d }));

        var interviewTypes = new[] { "Phone Screen", "Technical Interview", "Behavioral Interview", "On-Site", "Final Round" };
        sheetEntity.InterviewTypes.AddRange(interviewTypes.Select((t, i) => new InterviewTypeEntity { RowId = i + 1, InterviewType = t }));

        var outcomes = new[] { "Passed", "Failed", "Pending", "Moved to Next Round" };
        sheetEntity.InterviewOutcomes.AddRange(outcomes.Select((o, i) => new InterviewOutcomeEntity { RowId = i + 1, Outcome = o }));

        var schedules = new[] { "Full-time", "Part-time", "Contract", "Temporary" };
        sheetEntity.Schedules.AddRange(schedules.Select((s, i) => new ScheduleEntity { RowId = i + 1, Schedule = s }));

        // Generate a few interview records linked to some applications (if available)
        var interviewId = 2; // start at row 2
        if (sheetEntity.Applications.Count > 0)
        {
            // Use first two applications as sources for interviews
            var sourceApps = sheetEntity.Applications.Take(2).ToList();
            foreach (var app in sourceApps)
            {
                sheetEntity.Interviews.Add(new InterviewEntity
                {
                    RowId = interviewId++,
                    Date = DateTime.Parse(app.Date).ToString("yyyy-MM-dd"),
                    StartTime = "09:00",
                    EndTime = "10:00",
                    Duration = "01:00",
                    Company = app.Company,
                    JobTitle = app.JobTitle,
                    InterviewType = "Phone Screen",
                    RecruiterName = "Recruiter",
                    RecruiterContact = "recruiter@example.com",
                    Attendees = "Interviewer 1, Interviewer 2",
                    Outcome = "Pending",
                    Notes = "Auto-generated interview"
                });
            }
        }

        return sheetEntity;
    }

    public async Task<SheetEntity> PopulateDemoData(DateTime? startDate = null, DateTime? endDate = null, int? seed = null)
    {
        var demoData = GenerateDemoData(startDate, endDate, seed);

        // Write Applications
        var appSheet = ApplicationMapper.GetSheet();
        var appHeaders = appSheet.Headers.Select(h => (object)h.Name).ToList();
        var appRows = GenericSheetMapper<ApplicationEntity>.MapToRangeData(demoData.Applications, appHeaders);

        var appRowValues = new Dictionary<int, IList<IList<object?>>>
        {
            [2] = appRows
        };

        var appUpdate = GoogleRequestHelpers.GenerateUpdateValueRequest(appSheet.Name, appRowValues);
        await _googleSheetService.BatchUpdateData(appUpdate);

        // Write Interviews if any
        var intSheet = InterviewMapper.GetSheet();
        var intHeaders = intSheet.Headers.Select(h => (object)h.Name).ToList();
        var intRows = GenericSheetMapper<InterviewEntity>.MapToRangeData(demoData.Interviews, intHeaders);

        if (intRows.Count > 0)
        {
            var intRowValues = new Dictionary<int, IList<IList<object?>>>
            {
                [2] = intRows
            };

            var intUpdate = GoogleRequestHelpers.GenerateUpdateValueRequest(intSheet.Name, intRowValues);
            await _googleSheetService.BatchUpdateData(intUpdate);
        }

        return demoData;
    }
}
