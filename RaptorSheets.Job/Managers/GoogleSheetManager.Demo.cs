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
        var companies = new[]
        {
            "TechCorp", "DataSystems Inc", "Cloud Solutions", "AI Innovations", "WebWorks",
            "NextGen Labs", "Pioneer Software", "BrightPath Tech", "Nimbus Analytics", "Quantum Apps",
            "Vertex Systems", "BluePeak Digital", "CoreStack", "Apex Dynamics", "Forge Data"
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
        var targetApplications = Math.Max(220, totalDays * 5); // Hundreds of applications by default

        var applicationId = 2; // start at row 2 (row 1 reserved for headers)
        for (var i = 0; i < targetApplications; i++)
        {
            var company = companies[random.Next(companies.Length)];
            var position = positions[random.Next(positions.Length)];
            var appDate = start.AddDays(random.Next(totalDays));

            var payLow = random.Next(70000, 180000);
            var payHigh = payLow + random.Next(10000, 70000);

            sheetEntity.Applications.Add(new ApplicationEntity
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

        // Generate demo reference data
        sheetEntity.Sites.AddRange(sites.Select((s, i) => new SiteEntity { RowId = i + 1, Site = s }));
        sheetEntity.Decisions.AddRange(decisions.Select((d, i) => new DecisionEntity { RowId = i + 1, Decision = d }));

        var interviewTypes = new[] { "Phone Screen", "Technical Interview", "Behavioral Interview", "On-Site", "Final Round" };
        sheetEntity.InterviewTypes.AddRange(interviewTypes.Select((t, i) => new InterviewTypeEntity { RowId = i + 1, InterviewType = t }));

        var outcomes = new[] { "Passed", "Failed", "Pending", "Moved to Next Round" };
        sheetEntity.InterviewOutcomes.AddRange(outcomes.Select((o, i) => new InterviewOutcomeEntity { RowId = i + 1, Outcome = o }));

        sheetEntity.Schedules.AddRange(schedules.Select((s, i) => new ScheduleEntity { RowId = i + 1, Schedule = s }));

        // Generate interviews:
        // - about half of applications get interviews
        // - about half of interviewed applications get multiple interviews
        var interviewId = 2; // start at row 2
        var interviewCandidates = sheetEntity.Applications
            .OrderBy(_ => random.Next())
            .Take(sheetEntity.Applications.Count / 2)
            .ToList();

        var multiInterviewApps = interviewCandidates
            .OrderBy(_ => random.Next())
            .Take(interviewCandidates.Count / 2)
            .ToHashSet();

        foreach (var app in interviewCandidates)
        {
            var interviewCount = multiInterviewApps.Contains(app)
                ? random.Next(2, 5) // 2-4 interviews
                : 1;

            var baseDate = DateTime.Parse(app.Date);

            for (var round = 0; round < interviewCount; round++)
            {
                var interviewDate = baseDate.AddDays(random.Next(0, 21) + round);
                var startHour = random.Next(8, 17);
                var startMinute = random.Next(0, 2) == 0 ? 0 : 30;
                var durationMinutes = new[] { 30, 45, 60, 90 }[random.Next(4)];

                var startTime = new DateTime(interviewDate.Year, interviewDate.Month, interviewDate.Day, startHour, startMinute, 0);
                var endTime = startTime.AddMinutes(durationMinutes);

                sheetEntity.Interviews.Add(new InterviewEntity
                {
                    RowId = interviewId++,
                    Date = interviewDate.ToString("yyyy-MM-dd"),
                    StartTime = startTime.ToString("hh:mm tt").ToLowerInvariant(),
                    EndTime = endTime.ToString("hh:mm tt").ToLowerInvariant(),
                    Duration = TimeSpan.FromMinutes(durationMinutes).ToString(@"hh\:mm"),
                    Company = app.Company,
                    JobTitle = app.JobTitle,
                    InterviewType = interviewTypes[random.Next(interviewTypes.Length)],
                    RecruiterName = $"Recruiter {random.Next(1, 40)}",
                    RecruiterContact = $"recruiter{random.Next(1, 120)}@example.com",
                    Attendees = $"Interviewer {random.Next(1, 25)}, Interviewer {random.Next(26, 50)}",
                    Outcome = outcomes[random.Next(outcomes.Length)],
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
