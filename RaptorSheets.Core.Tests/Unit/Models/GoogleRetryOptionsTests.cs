using RaptorSheets.Core.Models;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Models;

public class GoogleRetryOptionsTests
{
    [Fact]
    public void Default_ShouldEnableRetriesWithinTheUnderlyingClientLimits()
    {
        var options = GoogleRetryOptions.Default;

        Assert.True(options.Enabled);
        Assert.InRange(options.MaxRetries, 1, GoogleRetryOptions.MaximumRetryCount);
        Assert.InRange(options.BackOffDelta, TimeSpan.FromTicks(1), GoogleRetryOptions.MaximumBackOffDelta);
    }

    [Fact]
    public void Default_TotalWaitShouldStayWellInsideACommonThirtySecondRequestBudget()
    {
        var options = GoogleRetryOptions.Default;

        // Worst case is every retry capped at MaxDelay; even that has to leave room for the
        // requests themselves inside a 30s gateway timeout.
        var worstCaseWait = options.MaxDelay * options.MaxRetries;

        Assert.True(worstCaseWait < TimeSpan.FromSeconds(30),
            $"Worst-case retry wait of {worstCaseWait.TotalSeconds}s would consume a 30s request budget.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(GoogleRetryOptions.MaximumRetryCount)]
    public void MaxRetries_ShouldAcceptValuesInRange(int retries)
    {
        var options = new GoogleRetryOptions { MaxRetries = retries };

        Assert.Equal(retries, options.MaxRetries);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(GoogleRetryOptions.MaximumRetryCount + 1)]
    public void MaxRetries_ShouldRejectValuesOutOfRange(int retries)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new GoogleRetryOptions { MaxRetries = retries });
    }

    [Fact]
    public void BackOffDelta_ShouldAcceptTheUnderlyingClientMaximum()
    {
        var options = new GoogleRetryOptions { BackOffDelta = GoogleRetryOptions.MaximumBackOffDelta };

        Assert.Equal(GoogleRetryOptions.MaximumBackOffDelta, options.BackOffDelta);
    }

    [Fact]
    public void BackOffDelta_ShouldRejectValuesAboveTheUnderlyingClientMaximum()
    {
        // The Google client rejects a delta above one second outright, so catching it here gives a
        // clearer error than letting the client throw from inside service construction.
        var tooLarge = GoogleRetryOptions.MaximumBackOffDelta + TimeSpan.FromMilliseconds(1);

        Assert.Throws<ArgumentOutOfRangeException>(() => new GoogleRetryOptions { BackOffDelta = tooLarge });
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void BackOffDelta_ShouldRejectNonPositiveValues(int seconds)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new GoogleRetryOptions { BackOffDelta = TimeSpan.FromSeconds(seconds) });
    }

    [Fact]
    public void Disabled_ShouldBeExpressibleForCallersDoingTheirOwnOrchestration()
    {
        var options = new GoogleRetryOptions { Enabled = false };

        Assert.False(options.Enabled);
    }
}
