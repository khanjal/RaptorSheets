using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Stock.Entities;
using RaptorSheets.Stock.Managers;
using RaptorSheets.Stock.Tests.Integration;
using Xunit;
using SheetName = RaptorSheets.Stock.Enums.SheetName;

namespace RaptorSheets.Stock.Tests.Integration.Base;

/// <summary>
/// Base class for Stock integration tests. Gets its manager from the shared
/// <see cref="StockCleanSlateFixture"/> (null when credentials are absent), which has already
/// deleted/recreated every sheet before this collection's tests run, plus small reusable
/// operations for reading data back.
/// </summary>
public abstract class IntegrationTestBase
{
    protected readonly GoogleSheetManager? GoogleSheetManager;
    protected readonly List<string> TestSheets;

    protected IntegrationTestBase(StockCleanSlateFixture fixture)
    {
        TestSheets =
        [
            SheetName.STOCKS.GetDescription(),
            SheetName.ACCOUNTS.GetDescription(),
            SheetName.TICKERS.GetDescription()
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
