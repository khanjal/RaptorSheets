using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Stock.Entities;
using RaptorSheets.Stock.Helpers;
using RaptorSheets.Stock.Managers;
using RaptorSheets.Stock.Tests.Integration;
using Xunit;
using SheetName = RaptorSheets.Stock.Enums.SheetName;

using RaptorSheets.Test.Common.Fixtures;

namespace RaptorSheets.Stock.Tests.Integration.Base;

/// <summary>
/// Base class for Stock integration tests. Gets its manager from the shared
/// <see cref="StockCleanSlateFixture"/> (null when credentials are absent), which has already
/// deleted/recreated every sheet before this collection's tests run, plus small reusable
/// operations for reading data back.
/// </summary>
public abstract class IntegrationTestBase
{
    protected readonly SheetManager? SheetManager;
    private readonly CleanSlateSheetFixture<SheetEntity, SheetManager> _fixture;
    protected readonly List<string> TestSheets;

    protected IntegrationTestBase(StockCleanSlateFixture fixture)
    {
        TestSheets =
        [
            SheetName.STOCKS.GetDescription(),
            SheetName.ACCOUNTS.GetDescription(),
            SheetName.TICKERS.GetDescription()
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
        => _fixture.VerifyAndReportPreconditionsAsync(StockSheetHelpers.GetSheetNames());

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
