using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Home.Constants;
using RaptorSheets.Home.Entities;
using RaptorSheets.Home.Managers;
using RaptorSheets.Test.Common.Helpers;

namespace RaptorSheets.Home.Tests.Integration.Base;

/// <summary>
/// Base class for Home integration tests. Provides a configured manager (or null when credentials
/// are absent) plus small reusable operations for ensuring sheets exist and reading data back.
/// </summary>
public abstract class IntegrationTestBase
{
    protected readonly GoogleSheetManager? GoogleSheetManager;
    protected readonly List<string> TestSheets;

    protected IntegrationTestBase()
    {
        TestSheets =
        [
            SheetsConfig.SheetNames.Rooms,
            SheetsConfig.SheetNames.Contacts,
            SheetsConfig.SheetNames.Appliances
        ];

        var spreadsheetId = TestConfigurationHelpers.GetHomeSpreadsheet();
        var credential = TestConfigurationHelpers.GetJsonCredential();

        if (GoogleCredentialHelpers.IsCredentialFilled(credential))
            GoogleSheetManager = new GoogleSheetManager(credential, spreadsheetId);
    }

    protected void SkipIfNoCredentials()
    {
        if (GoogleSheetManager == null)
        {
            Assert.Fail("Google Sheets credentials not available. Configure user secrets to run integration tests.");
        }
    }

    /// <summary>
    /// Ensures the given sheets exist, creating any that are missing. Returns false if creation errored.
    /// </summary>
    protected async Task<bool> EnsureSheetsExist(List<string> sheets)
    {
        var properties = await GoogleSheetManager!.GetSheetProperties(sheets);
        var missingSheets = sheets.Where(sheet =>
            !properties.Any(prop => prop.Name.Equals(sheet, StringComparison.OrdinalIgnoreCase) &&
                                    !string.IsNullOrEmpty(prop.Id))
        ).ToList();

        if (missingSheets.Count == 0) return true;

        var result = await GoogleSheetManager.CreateSheets(missingSheets);
        var hasErrors = result.Messages.Any(m => m.Level == MessageLevel.ERROR.GetDescription());

        if (!hasErrors)
        {
            await Task.Delay(2000);
        }

        return !hasErrors;
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
