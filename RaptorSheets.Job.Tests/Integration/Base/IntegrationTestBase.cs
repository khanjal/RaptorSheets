using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Job.Constants;
using RaptorSheets.Job.Entities;
using RaptorSheets.Job.Managers;
using RaptorSheets.Job.Tests.Integration;

namespace RaptorSheets.Job.Tests.Integration.Base;

/// <summary>
/// Base class for Job integration tests. Gets its manager from the shared
/// <see cref="JobCleanSlateFixture"/> (null when credentials are absent), which has already
/// deleted/recreated every sheet before this collection's tests run, plus small reusable
/// operations for reading data back.
/// </summary>
public abstract class IntegrationTestBase
{
    protected readonly GoogleSheetManager? GoogleSheetManager;
    protected readonly List<string> TestSheets;

    protected IntegrationTestBase(JobCleanSlateFixture fixture)
    {
        TestSheets =
        [
            SheetsConfig.SheetNames.Applications,
            SheetsConfig.SheetNames.Interviews,
            SheetsConfig.SheetNames.Companies,
            SheetsConfig.SheetNames.Positions,
            SheetsConfig.SheetNames.Sites
        ];

        GoogleSheetManager = fixture.Manager;
    }

    protected void SkipIfNoCredentials()
    {
        if (GoogleSheetManager == null)
        {
            Assert.Fail("Google Sheets credentials not available. Configure user secrets to run integration tests.");
        }
    }

    protected static List<MessageEntity> CriticalErrors(SheetEntity result) =>
        result.Messages
            .Where(m => m.Level == MessageLevel.ERROR.GetDescription() && !IsExpectedError(m.Message))
            .ToList();

    private static bool IsExpectedError(string message) =>
        message.Contains("not supported") ||
        message.Contains("already exists") ||
        message.Contains("header issue") ||
        message.Contains("No data to change");
}
