using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RaptorSheets.Core.Models;
using RaptorSheets.Core.Services;
using RaptorSheets.Core.Wrappers;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Services;

public class GoogleSheetServiceConcurrencyTests
{
    // Wraps ISheetServiceWrapper.GetSpreadsheet with a delay and tracks the highest number of calls
    // observed running at the same instant, so the concurrency gate's effect is directly measurable.
    private static (Mock<ISheetServiceWrapper> Wrapper, Func<int> MaxObserved) CreateTrackingWrapper()
    {
        var current = 0;
        var max = 0;
        var gate = new object();

        var wrapper = new Mock<ISheetServiceWrapper>();
        wrapper.Setup(w => w.GetSpreadsheet(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                lock (gate)
                {
                    current++;
                    if (current > max) max = current;
                }

                await Task.Delay(50);

                lock (gate) { current--; }

                return new Spreadsheet();
            });

        return (wrapper, () => max);
    }

    [Fact]
    public async Task ExecuteAsync_WithConcurrencyGateOfOne_ShouldSerializeCalls()
    {
        var (wrapper, maxObserved) = CreateTrackingWrapper();
        var service = new GoogleSheetService(wrapper.Object, NullLogger.Instance, new GoogleConcurrencyOptions { MaxConcurrentRequests = 1 });

        await Task.WhenAll(Enumerable.Range(0, 5).Select(_ => service.GetSheetInfo()));

        Assert.Equal(1, maxObserved());
    }

    [Fact]
    public async Task ExecuteAsync_WithoutConcurrencyGate_ShouldRunCallsConcurrently()
    {
        var (wrapper, maxObserved) = CreateTrackingWrapper();
        var service = new GoogleSheetService(wrapper.Object, NullLogger.Instance);

        await Task.WhenAll(Enumerable.Range(0, 5).Select(_ => service.GetSheetInfo()));

        Assert.True(maxObserved() > 1, $"Expected concurrent calls without a gate, observed max {maxObserved()}.");
    }
}
