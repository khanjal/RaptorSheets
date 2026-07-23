using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Home.Constants;
using RaptorSheets.Home.Entities;
using RaptorSheets.Home.Managers;
using RaptorSheets.Home.Tests.Integration;

namespace RaptorSheets.Home.Tests.Integration.Base;

/// <summary>
/// Base class for Home integration tests. Gets its manager from the shared
/// <see cref="HomeCleanSlateFixture"/> (null when credentials are absent), which has already
/// deleted/recreated every sheet before this collection's tests run, plus small reusable
/// operations for reading data back.
/// </summary>
public abstract class IntegrationTestBase
{
    protected readonly GoogleSheetManager? GoogleSheetManager;
    protected readonly List<string> TestSheets;

    protected IntegrationTestBase(HomeCleanSlateFixture fixture)
    {
        TestSheets =
        [
            SheetsConfig.SheetNames.Rooms,
            SheetsConfig.SheetNames.Contacts,
            SheetsConfig.SheetNames.Appliances
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

    /// <summary>
    /// Errors that indicate a real failure (excludes benign, expected messages).
    /// </summary>
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
