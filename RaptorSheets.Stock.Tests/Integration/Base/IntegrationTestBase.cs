using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Stock.Entities;
using RaptorSheets.Stock.Managers;
using RaptorSheets.Test.Common.Helpers;
using Xunit;
using SheetName = RaptorSheets.Stock.Enums.SheetName;

namespace RaptorSheets.Stock.Tests.Integration.Base;

/// <summary>
/// Base class for Stock integration tests. Provides a configured manager (or null when credentials
/// are absent) plus small reusable operations for reading data back.
/// </summary>
public abstract class IntegrationTestBase
{
    protected readonly GoogleSheetManager? GoogleSheetManager;
    protected readonly List<string> TestSheets;

    protected IntegrationTestBase()
    {
        TestSheets =
        [
            SheetName.STOCKS.GetDescription(),
            SheetName.ACCOUNTS.GetDescription(),
            SheetName.TICKERS.GetDescription()
        ];

        var spreadsheetId = TestConfigurationHelpers.GetStockSpreadsheet();
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
