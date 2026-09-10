using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Job.Constants;
using RaptorSheets.Job.Entities;
using RaptorSheets.Job.Helpers;
using RaptorSheets.Job.Managers;
using RaptorSheets.Job.Tests.Integration;

using RaptorSheets.Test.Common.Fixtures;

namespace RaptorSheets.Job.Tests.Integration.Base;

/// <summary>
/// Base class for Job integration tests. Gets its manager from the shared
/// <see cref="JobCleanSlateFixture"/> (null when credentials are absent), which has already
/// deleted/recreated every sheet before this collection's tests run, plus small reusable
/// operations for reading data back.
/// </summary>
public abstract class IntegrationTestBase
{
    protected readonly SheetManager? SheetManager;
    private readonly CleanSlateSheetFixture<SheetEntity, SheetManager> _fixture;
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

        SheetManager = fixture.Manager;
        _fixture = fixture;
    }

    /// <summary>
    /// Each live test states its own precondition rather than trusting whatever the previous
    /// one left behind (#130). The check and its warning reporting live on
    /// CleanSlateSheetFixture.VerifyAndReportPreconditionsAsync, shared by all five domains.
    /// </summary>
    protected Task VerifyPreconditionsAsync()
        => _fixture.VerifyAndReportPreconditionsAsync(JobSheetHelpers.GetSheetNames());

    protected void SkipIfNoCredentials()
    {
        if (SheetManager == null)
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
