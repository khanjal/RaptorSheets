using System.ComponentModel;
using RaptorSheets.Job.Constants;
using RaptorSheets.Job.Entities;
using RaptorSheets.Job.Managers;
using RaptorSheets.Job.Tests.Data.Attributes;
using RaptorSheets.Job.Tests.Integration.Base;
using RaptorSheets.Test.Common.Fixtures;
using RaptorSheets.Test.Common.Helpers;

namespace RaptorSheets.Job.Tests.Integration;

/// <summary>
/// Integration tests that write to (and read back from) a live Job Google Sheet.
/// Skipped automatically unless credentials and a Job spreadsheet ID are configured in user secrets
/// (add "spreadsheets:job" alongside "spreadsheets:gig"/"spreadsheets:home"). Collection fixture
/// (<see cref="JobCleanSlateFixture"/>) deletes/recreates every sheet before tests run.
/// </summary>
[Collection("JobSheetsIntegration")]
[Category("Integration")]
public class JobSheetsIntegrationTests : IntegrationTestBase
{
    public JobSheetsIntegrationTests(JobCleanSlateFixture fixture) : base(fixture)
    {
    }

    [FactCheckUserSecrets]
    public async Task WriteThenRead_Applications_And_Interviews_RoundTrips()
    {
        SkipIfNoCredentials();

        var data = new SheetEntity();
        data.Sheets.Applications.Add(new ApplicationEntity
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
        data.Sheets.Interviews.Add(new InterviewEntity
        {
            RowId = 2,
            Date = DateTime.Today.AddDays(-2).ToString("yyyy-MM-dd"),
            Company = "TechCorp",
            JobTitle = "Software Engineer",
            InterviewType = "Phone Screen",
            Outcome = "Passed"
        });

        // Act - write
        var writeSheets = new List<string> { SheetsConfig.SheetNames.Applications, SheetsConfig.SheetNames.Interviews };
        var writeResult = await GoogleSheetManager!.ChangeSheetData(writeSheets, data);

        var writeErrors = CriticalErrors(writeResult);
        Assert.True(writeErrors.Count == 0,
            $"Write had critical errors: {string.Join("; ", writeErrors.Select(e => e.Message))}");

        // Let formulas (Key, counts, reference sheets) recalculate
        await Task.Delay(2500);

        // Act - read back
        var readResult = await GoogleSheetManager!.GetSheets(TestSheets);

        var app = readResult.Sheets.Applications.FirstOrDefault(a => a.Company == "TechCorp");
        Assert.NotNull(app);
        // Key is a calculated column (Company-JobTitle-#), so it should be populated after the write
        Assert.False(string.IsNullOrWhiteSpace(app!.Key));

        // Companies is auto-derived from Applications and should now contain TechCorp
        Assert.Contains(readResult.Sheets.Companies, c => c.Company == "TechCorp");
    }

    [FactCheckUserSecrets]
    public async Task SetupDemo_CreatesSheetsAndPopulatesData()
    {
        SkipIfNoCredentials();

        var result = await GoogleSheetManager!.SetupDemo(seed: 42);

        Assert.NotEmpty(result.Sheets.Applications);

        await Task.Delay(2500);

        var readBack = await GoogleSheetManager!.GetSheets([SheetsConfig.SheetNames.Applications]);
        Assert.NotEmpty(readBack.Sheets.Applications);
    }
}

/// <summary>
/// Collection definition for Job Google Sheets integration tests.
/// </summary>
[CollectionDefinition("JobSheetsIntegration")]
public class JobSheetsIntegrationCollection : ICollectionFixture<JobCleanSlateFixture>
{
}

/// <summary>
/// Job's clean-slate integration fixture (see <see cref="CleanSlateSheetFixture{TEntity,TManager}"/>).
/// Deletes and recreates every canonical sheet once, before the collection's tests run. Safe because
/// spreadsheets:job is configured to point at a dedicated blank test spreadsheet, not real data.
/// </summary>
public class JobCleanSlateFixture : CleanSlateSheetFixture<SheetEntity, GoogleSheetManager>
{
    public JobCleanSlateFixture() : base(
        TestConfigurationHelpers.GetJobSpreadsheet(),
        (credential, spreadsheetId) => new GoogleSheetManager(credential, spreadsheetId))
    {
    }
}
