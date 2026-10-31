using Google.Apis.Sheets.v4.Data;
using Moq;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Models;
using RaptorSheets.Core.Services;
using RaptorSheets.Stock.Managers;
using Xunit;
using SheetName = RaptorSheets.Stock.Enums.SheetName;

namespace RaptorSheets.Stock.Tests.Unit.Managers;

/// <summary>
/// PopulateDemoData's wait for GOOGLEFINANCE to settle: refresh once, then poll until clean,
/// refreshing again only when errors are still latched - instead of a fixed 20s of sleeping.
/// </summary>
public class DemoDataSettleTests
{
    private static readonly string Tickers = SheetName.TICKERS.GetDescription();
    private static readonly string Stocks = SheetName.STOCKS.GetDescription();

    private static BatchGetValuesByDataFilterResponse Values(params string[] dataCells) => new()
    {
        ValueRanges = new List<MatchedValueRange>
        {
            new()
            {
                DataFilters = new List<DataFilter> { new() { A1Range = Tickers } },
                ValueRange = new ValueRange
                {
                    Values = new List<IList<object>>
                    {
                        new List<object> { "Ticker", "Current Price" },
                        new List<object>(dataCells),
                    }
                }
            }
        }
    };

    private static GoogleApiResult<BatchGetValuesByDataFilterResponse> Ok(params string[] dataCells)
        => GoogleApiResult<BatchGetValuesByDataFilterResponse>.Ok(Values(dataCells));

    private static readonly GoogleApiResult<BatchGetValuesByDataFilterResponse> Clean = Ok("AAPL", "190.12");
    private static readonly GoogleApiResult<BatchGetValuesByDataFilterResponse> Latched = Ok("AAPL", "#N/A");
    private static readonly GoogleApiResult<BatchGetValuesByDataFilterResponse> Loading = Ok("AAPL", "Loading...");

    // Virtual time: advances only when the settle loop waits or a simulated call takes time.
    private sealed class FakeClock : TimeProvider
    {
        public TimeSpan Now { get; set; }
        public override long TimestampFrequency => TimeSpan.TicksPerSecond;
        public override long GetTimestamp() => Now.Ticks;
    }

    private sealed class Harness
    {
        public FakeClock Clock { get; } = new();

        public Mock<IGoogleSheetService> Service { get; } = new();
        public List<TimeSpan> Delays { get; } = [];
        public SheetManager Manager { get; }

        public Harness(params GoogleApiResult<BatchGetValuesByDataFilterResponse>[] polls)
        {
            var spreadsheet = new Spreadsheet
            {
                Sheets = new List<Sheet>
                {
                    new() { Properties = new SheetProperties { Title = Stocks, SheetId = 1 } },
                    new() { Properties = new SheetProperties { Title = Tickers, SheetId = 2 } },
                }
            };
            Service.Setup(s => s.GetSheetInfo(It.IsAny<CancellationToken>())).ReturnsAsync(spreadsheet);
            Service.Setup(s => s.GetSheetInfo(It.IsAny<List<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(spreadsheet);
            Service.Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BatchUpdateSpreadsheetResponse { Replies = new List<Response>() });

            var sequence = Service.SetupSequence(s => s.GetBatchDataResult(
                It.Is<List<string>>(l => l.Contains(Tickers) && l.Contains(Stocks)), It.IsAny<string>(), It.IsAny<CancellationToken>()));
            foreach (var poll in polls)
            {
                sequence = sequence.ReturnsAsync(poll);
            }

            Manager = new SheetManager(Service.Object)
            {
                Delay = (delay, _) =>
                {
                    // The write is done by the first wait; anything after it belongs to the settle loop.
                    _metadataFetchesByWrite ??= MetadataFetches();
                    Delays.Add(delay);
                    Clock.Now += delay;
                    return Task.CompletedTask;
                },
                Clock = Clock
            };
        }

        private int? _metadataFetchesByWrite;

        public int Polls => Service.Invocations.Count(i => i.Method.Name == nameof(IGoogleSheetService.GetBatchDataResult));

        // Each RefreshHeaderFormulasAsync fetches its own metadata by range list - as does the write
        // itself, which is why only the fetches after it count.
        private int MetadataFetches() => Service.Invocations.Count(i =>
            i.Method.Name == nameof(IGoogleSheetService.GetSheetInfo) && i.Arguments.Count == 2);

        public int Refreshes => MetadataFetches() - (_metadataFetchesByWrite ?? 0);
    }

    private static bool IsWarning(MessageEntity message) => message.Level == MessageLevel.WARNING.GetDescription();

    [Fact]
    public async Task PopulateDemoData_CleanOnFirstPoll_RefreshesOnceAndStops()
    {
        var harness = new Harness(Clean);

        var result = await harness.Manager.PopulateDemoData(seed: 42);

        Assert.Equal(1, harness.Refreshes);
        Assert.Equal(1, harness.Polls);
        Assert.Equal([SheetManager.SettleFirstRefreshDelay, SheetManager.SettlePollInterval], harness.Delays);
        Assert.DoesNotContain(result.Messages, IsWarning);
        Assert.NotEmpty(result.Sheets.Stocks);
    }

    [Fact]
    public async Task PopulateDemoData_StillLatched_RefreshesAgainThenStopsWhenClean()
    {
        var harness = new Harness(Latched, Latched, Clean);

        var result = await harness.Manager.PopulateDemoData(seed: 42);

        Assert.Equal(2, harness.Refreshes);
        Assert.Equal(3, harness.Polls);
        Assert.DoesNotContain(result.Messages, IsWarning);
    }

    [Fact]
    public async Task PopulateDemoData_StillLoading_WaitsWithoutRefreshing()
    {
        var harness = new Harness(Loading, Loading, Loading, Clean);

        await harness.Manager.PopulateDemoData(seed: 42);

        Assert.Equal(1, harness.Refreshes);
        Assert.Equal(4, harness.Polls);
    }

    [Fact]
    public async Task PopulateDemoData_FailedReads_WaitWithoutRefreshing()
    {
        var failed = GoogleApiResult<BatchGetValuesByDataFilterResponse>.Failed(new GoogleApiFailure { Reason = GoogleApiFailureReason.QuotaExceeded, Message = "quota" });
        var harness = new Harness(failed, failed, failed, Clean);

        await harness.Manager.PopulateDemoData(seed: 42);

        Assert.Equal(1, harness.Refreshes);
        Assert.Equal(4, harness.Polls);
    }

    [Fact]
    public async Task PopulateDemoData_NeverSettles_GivesUpWithinTheBoundAndWarns()
    {
        var harness = new Harness(Enumerable.Repeat(Latched, 20).ToArray());

        var result = await harness.Manager.PopulateDemoData(seed: 42);

        var pollsThatFit = (int)((SheetManager.SettleBudget - SheetManager.SettleFirstRefreshDelay) / SheetManager.SettlePollInterval);
        Assert.Equal(pollsThatFit, harness.Polls);
        Assert.True(harness.Delays.Aggregate(TimeSpan.Zero, (a, b) => a + b) <= TimeSpan.FromSeconds(20),
            "the settle wait must stay well inside the ~30s API Gateway ceiling");
        Assert.Contains(result.Messages, m => IsWarning(m) && m.Message.Contains("#N/A"));
    }

    [Fact]
    public async Task PopulateDemoData_SlowCallsWhileThrottled_StopsAtTheTimeBudgetNotThePollCount()
    {
        var harness = new Harness();
        // Retry backoff under throttling makes every read take seconds of its own.
        var slowRead = TimeSpan.FromSeconds(4);
        harness.Service.Setup(s => s.GetBatchDataResult(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback(() => harness.Clock.Now += slowRead)
            .ReturnsAsync(Loading);

        var result = await harness.Manager.PopulateDemoData(seed: 42);

        Assert.Equal(2, harness.Polls); // 3s + (2.5s wait + 4s read) x2 = 16s; a third wait would end past 15s
        Assert.True(harness.Clock.Now <= SheetManager.SettleBudget + slowRead,
            $"settle ran {harness.Clock.Now.TotalSeconds}s - past the budget by more than one in-flight read");
        Assert.Contains(result.Messages, IsWarning);
    }

    [Fact]
    public async Task PopulateDemoData_WhenCallerCancels_StopsWaiting()
    {
        var harness = new Harness(Latched);
        using var cts = new CancellationTokenSource();
        harness.Manager.Delay = async (_, ct) => { await cts.CancelAsync(); ct.ThrowIfCancellationRequested(); };

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => harness.Manager.PopulateDemoData(seed: 42, cts.Token));
        Assert.Equal(0, harness.Polls);
    }

    [Theory]
    [InlineData("190.12", "Settled")]
    [InlineData("", "Settled")]
    [InlineData("#N/A", "Latched")]
    [InlineData("#REF!", "Latched")]
    [InlineData("Loading...", "Pending")]
    public void GetSettleState_ClassifiesDataCells(string cell, string expected)
    {
        // The state enum is internal, so it travels by name - a public test can't take it as a parameter.
        Assert.Equal(expected, SheetManager.GetSettleState(Values("AAPL", cell)).ToString());
    }

    [Fact]
    public void GetSettleState_ErrorBeatsLoading_AndIgnoresTheHeaderRow()
    {
        var response = Values("Loading...", "#N/A");
        response.ValueRanges[0].ValueRange.Values[0] = new List<object> { "#Header", "Loading" };
        Assert.Equal(SheetManager.SettleState.Latched, SheetManager.GetSettleState(response));

        var headerOnlyErrors = Values("AAPL", "1");
        headerOnlyErrors.ValueRanges[0].ValueRange.Values[0] = new List<object> { "#Header", "Loading" };
        Assert.Equal(SheetManager.SettleState.Settled, SheetManager.GetSettleState(headerOnlyErrors));

        Assert.Equal(SheetManager.SettleState.Settled, SheetManager.GetSettleState(null));
    }
}
