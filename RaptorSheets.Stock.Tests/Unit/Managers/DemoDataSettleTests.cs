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
    private static readonly List<string> Polled = [Tickers, Stocks];

    private static MatchedValueRange Sheet(string name, IList<object> header, params IList<object>[] dataRows) => new()
    {
        DataFilters = new List<DataFilter> { new() { A1Range = name } },
        ValueRange = new ValueRange { Values = new List<IList<object>> { header }.Concat(dataRows).ToList() }
    };

    // Both polled sheets, Stocks clean, Tickers' one data row holding the given cells.
    private static BatchGetValuesByDataFilterResponse Values(params string[] tickerCells) => new()
    {
        ValueRanges = new List<MatchedValueRange>
        {
            Sheet(Tickers, new List<object> { "Ticker", "Current Price" }, new List<object>(tickerCells)),
            Sheet(Stocks, new List<object> { "Ticker", "Shares" }, new List<object> { "AAPL", "3" }),
        }
    };

    private static GoogleApiResult<BatchGetValuesByDataFilterResponse> Ok(params string[] tickerCells)
        => GoogleApiResult<BatchGetValuesByDataFilterResponse>.Ok(Values(tickerCells));

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
        private readonly Queue<GoogleApiResult<BatchGetValuesByDataFilterResponse>> _polls;
        private int? _writesBeforeSettle;

        public Mock<IGoogleSheetService> Service { get; } = new();
        public FakeClock Clock { get; } = new();
        public List<TimeSpan> Delays { get; } = [];
        public TimeSpan ReadLatency { get; set; }
        public TimeSpan WriteLatency { get; set; }
        public SheetManager Manager { get; }

        // Each poll returns the next result given; the last one repeats once they run out.
        public Harness(params GoogleApiResult<BatchGetValuesByDataFilterResponse>[] polls)
        {
            _polls = new Queue<GoogleApiResult<BatchGetValuesByDataFilterResponse>>(polls);
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
                .Callback(() => Clock.Now += WriteLatency)
                .ReturnsAsync(new BatchUpdateSpreadsheetResponse { Replies = new List<Response>() });
            Service.Setup(s => s.GetBatchDataResult(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback(() => Clock.Now += ReadLatency)
                .ReturnsAsync(() => _polls.Count > 1 ? _polls.Dequeue() : _polls.Peek());

            Manager = new SheetManager(Service.Object)
            {
                Delay = (delay, _) =>
                {
                    // The write is done by the first wait; anything after it belongs to the settle loop.
                    _writesBeforeSettle ??= Writes();
                    Delays.Add(delay);
                    Clock.Now += delay;
                    return Task.CompletedTask;
                },
                Clock = Clock
            };
        }

        private int Writes() => Service.Invocations.Count(i => i.Method.Name == nameof(IGoogleSheetService.BatchUpdateSpreadsheet));

        public int Polls => Service.Invocations.Count(i => i.Method.Name == nameof(IGoogleSheetService.GetBatchDataResult));

        // Every write after the demo data itself is a header-formula refresh.
        public int Refreshes => Writes() - (_writesBeforeSettle ?? Writes());
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
    public async Task PopulateDemoData_StillLatched_RefreshesAgainTwoPollsLaterThenStopsWhenClean()
    {
        var harness = new Harness(Latched, Latched, Latched, Latched, Clean);

        var result = await harness.Manager.PopulateDemoData(seed: 42);

        // First refresh, then one after each pair of latched polls - not one per poll.
        Assert.Equal(3, harness.Refreshes);
        Assert.Equal(5, harness.Polls);
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
    public async Task PopulateDemoData_NeverSettles_GivesUpWithinTheBudgetAndWarns()
    {
        var harness = new Harness(Latched);

        var result = await harness.Manager.PopulateDemoData(seed: 42);

        // 3s + 2.5s x6 = 18s; a seventh wait would end past the 18s budget.
        Assert.Equal(6, harness.Polls);
        // Refreshes at 3s, after polls 2 and 4 - none after poll 6, since no poll could follow it.
        Assert.Equal(3, harness.Refreshes);
        Assert.True(harness.Clock.Now <= SheetManager.SettleBudget);
        Assert.Contains(result.Messages, m => IsWarning(m) && m.Message.Contains("formula error"));
    }

    [Fact]
    public async Task PopulateDemoData_SlowCallsWhileThrottled_StopsAtTheTimeBudgetNotThePollCount()
    {
        // Retry backoff under throttling makes every read take seconds of its own.
        var harness = new Harness(Loading) { ReadLatency = TimeSpan.FromSeconds(4) };

        var result = await harness.Manager.PopulateDemoData(seed: 42);

        Assert.Equal(2, harness.Polls); // 3s + (2.5s wait + 4s read) x2 = 16s; a third wait would end past 18s
        Assert.True(harness.Clock.Now <= SheetManager.SettleBudget + harness.ReadLatency,
            $"settle ran {harness.Clock.Now.TotalSeconds}s - past the budget by more than the one in-flight read");
        Assert.Contains(result.Messages, m => IsWarning(m) && m.Message.Contains("loading"));
    }

    [Fact]
    public async Task PopulateDemoData_SlowWrite_CountsAgainstTheBudget()
    {
        // The budget runs from the top of PopulateDemoData, so time the write spent is time the
        // settle loop no longer has.
        var harness = new Harness(Latched) { WriteLatency = TimeSpan.FromSeconds(5) };

        await harness.Manager.PopulateDemoData(seed: 42);

        // write 5s, wait 3s, refresh 5s = 13s; two 2.5s polls fit, a third would end past 18s.
        Assert.Equal(2, harness.Polls);
    }

    [Fact]
    public async Task PopulateDemoData_WriteTooSlowToLeaveRoom_SkipsTheSettleAndWarns()
    {
        var harness = new Harness(Clean) { WriteLatency = TimeSpan.FromSeconds(13) };

        var result = await harness.Manager.PopulateDemoData(seed: 42);

        Assert.Empty(harness.Delays);
        Assert.Equal(0, harness.Polls);
        Assert.Contains(result.Messages, IsWarning);
    }

    [Fact]
    public async Task PopulateDemoData_FetchesSheetIdsWithoutGridData()
    {
        var harness = new Harness(Clean);

        await harness.Manager.PopulateDemoData(seed: 42);

        // The ranges overload makes Google return every cell of both sheets; ids need none of that.
        // (The write itself only ever asks about Stocks.)
        harness.Service.Verify(s => s.GetSheetInfo(It.Is<List<string>>(l => l.Contains(Tickers)), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task PopulateDemoData_WhenTheWriteFails_DoesNotWaitForAnything()
    {
        var harness = new Harness(Clean);
        harness.Service.Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BatchUpdateSpreadsheetResponse?)null);

        var result = await harness.Manager.PopulateDemoData(seed: 42);

        Assert.Contains(result.Messages, m => m.Level == MessageLevel.ERROR.GetDescription());
        Assert.Empty(harness.Delays);
        Assert.Equal(0, harness.Polls);
    }

    [Fact]
    public async Task PopulateDemoData_WhenCallerCancelsMidPolling_StopsWithoutAnotherRefresh()
    {
        var harness = new Harness(Latched);
        using var cts = new CancellationTokenSource();
        var waits = 0;
        harness.Manager.Delay = async (_, ct) =>
        {
            // Let the first refresh and two latched polls through, then cancel during the next wait.
            if (++waits == 4)
            {
                await cts.CancelAsync();
            }
            ct.ThrowIfCancellationRequested();
        };

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => harness.Manager.PopulateDemoData(seed: 42, cts.Token));

        Assert.Equal(2, harness.Polls);
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
        Assert.Equal(expected, SheetManager.GetSettleState(Values("AAPL", cell), Polled).ToString());
    }

    [Fact]
    public void GetSettleState_ErrorBeatsLoading()
    {
        Assert.Equal(SheetManager.SettleState.Latched, SheetManager.GetSettleState(Values("Loading...", "#N/A"), Polled));
    }

    [Fact]
    public void GetSettleState_AnErrorInTheHeaderRow_IsLatched()
    {
        // A whole-column ARRAYFORMULA that fails outright shows its error where its title would be,
        // leaving the data rows beneath it blank rather than erroring.
        var response = Values("AAPL", "");
        response.ValueRanges[0].ValueRange.Values[0] = new List<object> { "Ticker", "#N/A" };

        Assert.Equal(SheetManager.SettleState.Latched, SheetManager.GetSettleState(response, Polled));
    }

    [Fact]
    public void GetSettleState_ASheetWithNoDataRowsYet_IsPending()
    {
        var response = new BatchGetValuesByDataFilterResponse
        {
            ValueRanges = new List<MatchedValueRange>
            {
                Sheet(Tickers, new List<object> { "Ticker", "Current Price" }),
                Sheet(Stocks, new List<object> { "Ticker", "Shares" }, new List<object> { "AAPL", "3" }),
            }
        };

        Assert.Equal(SheetManager.SettleState.Pending, SheetManager.GetSettleState(response, Polled));
    }

    [Fact]
    public void GetSettleState_AMissingSheetOrNoResponse_IsPending()
    {
        var response = Values("AAPL", "190.12");
        response.ValueRanges.RemoveAt(1);

        Assert.Equal(SheetManager.SettleState.Pending, SheetManager.GetSettleState(response, Polled));
        Assert.Equal(SheetManager.SettleState.Pending, SheetManager.GetSettleState(null, Polled));
    }
}
