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
    /// The clean slate runs once per collection, so a test that fails before restoring what it
    /// removed hands the wreckage to everything after it (#130). Checking here means a test states
    /// its own precondition instead of trusting the previous one, and damage is named where it is
    /// found rather than wherever it eventually causes a failure.
    ///
    /// Mirrors Gig's and Core's adoption of this - Stock was left opt-in when
    /// CleanSlateSheetFixture first gained the capability.
    /// </summary>
    protected async Task VerifyPreconditionsAsync()
    {
        if (SheetManager == null)
        {
            return;
        }

        var (repaired, drift) = await _fixture.VerifyPreconditionsAsync(StockSheetHelpers.GetSheetNames());

        if (repaired.Count > 0)
        {
            // Console, not Debug.WriteLine: Debug.WriteLine is [Conditional("DEBUG")] and CI builds
            // Release, so every diagnostic written that way is absent from the one run anybody reads
            // after the fact.
            Console.WriteLine(
                $"WARNING: repaired {repaired.Count} sheet(s) missing before this test ran: {string.Join(", ", repaired)}. " +
                "An earlier test removed them without restoring them - see #130.");
        }

        if (drift.Count > 0)
        {
            Console.WriteLine(
                $"WARNING: {drift.Count} sheet(s) have drifted columns before this test ran: {string.Join(" | ", drift)}. " +
                "An earlier test changed them without restoring them - see #130.");
        }
    }

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
