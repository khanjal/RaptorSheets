using RaptorSheets.Job.Managers;
using RaptorSheets.Test.Common.Helpers;

namespace RaptorSheets.Job.Tests.Integration.Base;

/// <summary>
/// Base class for integration tests with modular, reusable operations
/// </summary>
public abstract class IntegrationTestBase
{
    internal readonly GoogleSheetManager? GoogleSheetManager;
    protected readonly List<string> TestSheets;

    protected IntegrationTestBase()
    {
        // Start with core sheets for testing
        TestSheets = [
            SheetsConfig.SheetNames.Applications,
            SheetsConfig.SheetNames.Interviews,
            SheetsConfig.SheetNames.Companies,
            SheetsConfig.SheetNames.Positions,
            SheetsConfig.SheetNames.Sites,
            SheetsConfig.SheetNames.Decisions,
            SheetsConfig.SheetNames.InterviewTypes,
            SheetsConfig.SheetNames.InterviewOutcomes,
            SheetsConfig.SheetNames.Schedules
        ];

        var spreadsheetId = TestConfigurationHelpers.GetJobSheet();
        var credential = TestConfigurationHelpers.GetJsonCredential();

        if (GoogleCredentialHelpers.IsCredentialFilled(credential))
            GoogleSheetManager = new GoogleSheetManager(credential, spreadsheetId);
    }

    #region Skip Helpers

    protected void SkipIfNoCredentials()
    {
        if (GoogleSheetManager == null)
        {
            Assert.Fail("Google Sheets credentials not available. Configure user secrets to run integration tests.");
        }
    }

    public bool HasCredentials() => GoogleSheetManager != null;

    #endregion

    #region Test Data Generation

    /// <summary>
    /// Generates demo data using the production GenerateDemoData method.
    /// This ensures consistency between demo data and test data.
    /// </summary>
    /// <param name="startDate">Start date for demo data</param>
    /// <param name="endDate">End date for demo data</param>
    /// <returns>SheetEntity with realistic demo data</returns>
    protected SheetEntity CreateDemoData(DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.Today.AddDays(-30);
        var end = endDate ?? DateTime.Today;

        return GoogleSheetManager!.GenerateDemoData(start, end);
    }

    /// <summary>
    /// Creates a simple test dataset with minimal records
    /// </summary>
    protected static SheetEntity CreateSimpleTestData()
    {
        var testData = new SheetEntity();

        // Add a few applications
        testData.Applications.Add(new ApplicationEntity
        {
            RowId = 2,
            Date = DateTime.Today.AddDays(-5).ToString("yyyy-MM-dd"),
            Company = "TechCorp",
            JobTitle = "Software Engineer",
            Posting = "https://example.com/job/123",
            Site = "LinkedIn",
            Decision = "Pending",
            PayLow = 100000,
            PayHigh = 150000,
            Location = "Remote",
            Schedule = "Full-time"
        });

        testData.Applications.Add(new ApplicationEntity
        {
            RowId = 3,
            Date = DateTime.Today.AddDays(-3).ToString("yyyy-MM-dd"),
            Company = "DataSystems Inc",
            JobTitle = "Senior Developer",
            Posting = "https://example.com/job/456",
            Site = "Indeed",
            Decision = "Interview Scheduled",
            PayLow = 120000,
            PayHigh = 170000,
            Location = "New York, NY",
            Schedule = "Full-time"
        });

        // Add reference data
        testData.Sites.AddRange(new[]
        {
            new SiteEntity { RowId = 2, Site = "LinkedIn" },
            new SiteEntity { RowId = 3, Site = "Indeed" },
            new SiteEntity { RowId = 4, Site = "Glassdoor" }
        });

        testData.Decisions.AddRange(new[]
        {
            new DecisionEntity { RowId = 2, Decision = "Pending" },
            new DecisionEntity { RowId = 3, Decision = "Accepted" },
            new DecisionEntity { RowId = 4, Decision = "Rejected" },
            new DecisionEntity { RowId = 5, Decision = "Interview Scheduled" }
        });

        testData.InterviewTypes.AddRange(new[]
        {
            new InterviewTypeEntity { RowId = 2, InterviewType = "Phone Screen" },
            new InterviewTypeEntity { RowId = 3, InterviewType = "Technical Interview" },
            new InterviewTypeEntity { RowId = 4, InterviewType = "Behavioral Interview" }
        });

        testData.InterviewOutcomes.AddRange(new[]
        {
            new InterviewOutcomeEntity { RowId = 2, Outcome = "Passed" },
            new InterviewOutcomeEntity { RowId = 3, Outcome = "Failed" },
            new InterviewOutcomeEntity { RowId = 4, Outcome = "Pending" }
        });

        testData.Schedules.AddRange(new[]
        {
            new ScheduleEntity { RowId = 2, Schedule = "Full-time" },
            new ScheduleEntity { RowId = 3, Schedule = "Part-time" },
            new ScheduleEntity { RowId = 4, Schedule = "Contract" }
        });

        return testData;
    }

    #endregion
}
