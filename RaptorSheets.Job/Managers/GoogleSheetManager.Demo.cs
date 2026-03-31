using RaptorSheets.Job.Entities;

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

    public async Task<SheetEntity> PopulateDemoData(DateTime? startDate = null, DateTime? endDate = null, int? seed = null)
    {
        var demoData = GenerateDemoData(startDate, endDate, seed);

        // TODO: Implement actual writing of demo data to sheets
        // For now, return the generated data
        return demoData;
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

        var applicationId = 1;
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

        // Generate demo reference data
        sheetEntity.Sites.AddRange(sites.Select((s, i) => new SiteEntity { RowId = i + 1, Site = s }));
        sheetEntity.Decisions.AddRange(decisions.Select((d, i) => new DecisionEntity { RowId = i + 1, Decision = d }));

        var interviewTypes = new[] { "Phone Screen", "Technical Interview", "Behavioral Interview", "On-Site", "Final Round" };
        sheetEntity.InterviewTypes.AddRange(interviewTypes.Select((t, i) => new InterviewTypeEntity { RowId = i + 1, InterviewType = t }));

        var outcomes = new[] { "Passed", "Failed", "Pending", "Moved to Next Round" };
        sheetEntity.InterviewOutcomes.AddRange(outcomes.Select((o, i) => new InterviewOutcomeEntity { RowId = i + 1, Outcome = o }));

        var schedules = new[] { "Full-time", "Part-time", "Contract", "Temporary" };
        sheetEntity.Schedules.AddRange(schedules.Select((s, i) => new ScheduleEntity { RowId = i + 1, Schedule = s }));

        return sheetEntity;
    }
}
