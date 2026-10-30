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
                cts.Cancel();
                ct.ThrowIfCancellationRequested();
                await Task.CompletedTask;
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
        cts.Cancel();
        var wrapper = new Mock<ISheetServiceWrapper>();
        wrapper.Setup(w => w.GetSpreadsheet(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException(cts.Token));
        var service = new GoogleSheetService(wrapper.Object);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.GetSheetInfoResult(null, cts.Token));
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
