using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Logging;
using Moq;
using RaptorSheets.Core.Models;
using RaptorSheets.Core.Services;
using RaptorSheets.Core.Wrappers;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Services;

public class GoogleSheetServiceCancellationTests
{
    private static void VerifyNothingLogged(Mock<ILogger> logger)
    {
        logger.Verify(l => l.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
    }

    [Fact]
    public async Task GetSheetInfo_WhenCallerCancels_RethrowsWithoutLoggingAnError()
    {
        using var cts = new CancellationTokenSource();
        var wrapper = new Mock<ISheetServiceWrapper>();
        wrapper.Setup(w => w.GetSpreadsheet(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
            .Returns(async (List<string> _, CancellationToken ct) =>
            {
                await cts.CancelAsync();
                ct.ThrowIfCancellationRequested();
                return new Spreadsheet();
            });
        var logger = new Mock<ILogger>();
        var service = new GoogleSheetService(wrapper.Object, logger.Object);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.GetSheetInfo(cts.Token));

        VerifyNothingLogged(logger);
    }

    [Fact]
    public async Task GetSheetInfoResult_WhenCallerCancels_RethrowsInsteadOfReturningFailure()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var wrapper = new Mock<ISheetServiceWrapper>();
        wrapper.Setup(w => w.GetSpreadsheet(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException(cts.Token));
        var service = new GoogleSheetService(wrapper.Object);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.GetSheetInfoResult(null, cts.Token));
    }

    [Fact]
    public async Task GetSheetInfo_WhenCallerCancelsMidCallWithConcurrencyGate_ReleasesTheSlot()
    {
        using var cts = new CancellationTokenSource();
        var wrapper = new Mock<ISheetServiceWrapper>();
        wrapper.Setup(w => w.GetSpreadsheet(It.IsAny<List<string>>(), cts.Token))
            .Returns(async (List<string> _, CancellationToken ct) =>
            {
                await cts.CancelAsync();
                ct.ThrowIfCancellationRequested();
                return new Spreadsheet();
            });
        wrapper.Setup(w => w.GetSpreadsheet(It.IsAny<List<string>>(), CancellationToken.None))
            .ReturnsAsync(new Spreadsheet());
        var service = new GoogleSheetService(wrapper.Object, null, new GoogleConcurrencyOptions { MaxConcurrentRequests = 1 });

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.GetSheetInfo(cts.Token));

        // With a gate of one, a slot leaked by the cancelled call would make this wait forever.
        var next = service.GetSheetInfo(CancellationToken.None);
        Assert.Same(next, await Task.WhenAny(next, Task.Delay(TimeSpan.FromSeconds(5))));
    }

    [Fact]
    public async Task GetSheetInfo_WhenCallerCancelsWhileWaitingForGate_DoesNotReleaseASlotItNeverHeld()
    {
        var wrapper = new Mock<ISheetServiceWrapper>();
        wrapper.Setup(w => w.GetSpreadsheet(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Spreadsheet());
        var service = new GoogleSheetService(wrapper.Object, null, new GoogleConcurrencyOptions { MaxConcurrentRequests = 1 });

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // The gate is created full, so releasing a slot the cancelled wait never took would throw
        // SemaphoreFullException from the finally - surfacing here instead of the cancellation.
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.GetSheetInfo(cts.Token));
        Assert.NotNull(await service.GetSheetInfo());
    }

    [Fact]
    public async Task GetSheetInfoResult_WhenCancellationWasNotRequestedByCaller_ReturnsFailure()
    {
        // An HTTP timeout surfaces as TaskCanceledException without the caller's token being
        // cancelled - that's a genuine failure and must still be classified, not rethrown.
        var wrapper = new Mock<ISheetServiceWrapper>();
        wrapper.Setup(w => w.GetSpreadsheet(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException("timed out"));
        var service = new GoogleSheetService(wrapper.Object);

        var result = await service.GetSheetInfoResult();

        Assert.False(result.Success);
        Assert.NotNull(result.Failure);
    }
}
