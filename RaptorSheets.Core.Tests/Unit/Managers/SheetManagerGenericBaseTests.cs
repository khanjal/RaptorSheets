using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Logging;
using Moq;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Managers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Core.Registries;
using RaptorSheets.Core.Services;
using Xunit;
using RaptorSheets.Core.Models;

namespace RaptorSheets.Core.Tests.Unit.Managers;

/// <summary>
/// Domain-agnostic coverage for SheetManagerBase&lt;TEntity&gt; - the registry-backed base that
/// Gig/Stock (and future Job/Home) inherit. Uses a minimal in-file entity + registry so the shared
/// read/metadata/layout surface is verified in Core without depending on any domain package.
/// </summary>
public class SheetManagerGenericBaseTests
{
    private sealed class TestEntity : ISheetEntity
    {
        public PropertyEntity Properties { get; set; } = new();
        public List<MessageEntity> Messages { get; set; } = [];
        public Dictionary<string, SheetModel> Structures { get; set; } = [];
        public List<IList<object>> Rows { get; set; } = [];
    }

    private sealed class TestManager : SheetManagerBase<TestEntity>
    {
        public TestManager(IGoogleSheetService service, SheetRegistry<TestEntity> registry, List<string> canonical, ILogger? logger = null)
            : base(service, registry, canonical, logger) { }

        public int CreateMissingCalls { get; private set; }

        protected override Task<TestEntity> CreateMissingSheetsAsync(Dictionary<string, int> missingIndexMap, CancellationToken cancellationToken = default)
        {
            CreateMissingCalls++;
            var entity = new TestEntity();
            foreach (var name in missingIndexMap.Keys)
            {
                entity.Messages.Add(new MessageEntity { Message = $"{name} created", Level = "WARNING" });
            }
            return Task.FromResult(entity);
        }
    }

    private const string SheetName = "TestSheet";

    private static SheetRegistry<TestEntity> BuildRegistry()
    {
        var registry = new SheetRegistry<TestEntity>();
        registry.Register(
            SheetName,
            () => new SheetModel { Name = SheetName },
            (entity, values) => entity.Rows.Add(values.Count > 0 ? values[0] : new List<object>()));
        return registry;
    }

    private static TestManager BuildManager(IGoogleSheetService service)
        => new(service, BuildRegistry(), [SheetName]);

    // A second concrete subclass that DOES override GenerateSheetsRequest, mirroring Gig/Stock -
    // exercises CreateSheets/DeleteSheets, which TestManager above can't (it deliberately leaves the
    // default unimplemented to cover that guard).
    private sealed class TestManagerWithGeneration : SheetManagerBase<TestEntity>
    {
        public TestManagerWithGeneration(IGoogleSheetService service, SheetRegistry<TestEntity> registry, List<string> canonical, ILogger? logger = null)
            : base(service, registry, canonical, logger) { }

        protected override Task<TestEntity> CreateMissingSheetsAsync(Dictionary<string, int> missingIndexMap, CancellationToken cancellationToken = default)
            => Task.FromResult(new TestEntity());

        protected override BatchUpdateSpreadsheetRequest GenerateSheetsRequest(List<string> sheetNames)
        {
            var request = new BatchUpdateSpreadsheetRequest { Requests = [] };
            foreach (var name in sheetNames)
            {
                request.Requests.Add(new Request { AddSheet = new AddSheetRequest { Properties = new SheetProperties { Title = name } } });
            }
            return request;
        }
    }

    private static TestManagerWithGeneration BuildGeneratingManager(IGoogleSheetService service, List<string>? canonicalNames = null)
        => new(service, BuildRegistry(), canonicalNames ?? [SheetName]);

    [Fact]
    public void Constructor_WithNullRegistry_ShouldThrow()
    {
        var service = new Mock<IGoogleSheetService>().Object;
        Assert.Throws<ArgumentNullException>(() => new TestManager(service, null!, [SheetName]));
    }

    [Fact]
    public void Constructor_WithNullCanonicalSheetNames_ShouldThrow()
    {
        var service = new Mock<IGoogleSheetService>().Object;
        Assert.Throws<ArgumentNullException>(() => new TestManager(service, BuildRegistry(), null!));
    }

    [Fact]
    public void GetSheetLayout_ForRegisteredSheet_ReturnsModel()
    {
        var manager = BuildManager(new Mock<IGoogleSheetService>().Object);

        var layout = manager.GetSheetLayout(SheetName);

        Assert.NotNull(layout);
        Assert.Equal(SheetName, layout!.Name);
    }

    [Fact]
    public void GetSheetLayout_ForUnknownSheet_ReturnsNull()
    {
        var manager = BuildManager(new Mock<IGoogleSheetService>().Object);

        Assert.Null(manager.GetSheetLayout("NotARegisteredSheet"));
    }

    [Fact]
    public void GetSheetLayouts_SkipsUnknownSheets()
    {
        var manager = BuildManager(new Mock<IGoogleSheetService>().Object);

        var layouts = manager.GetSheetLayouts([SheetName, "Unknown"]);

        Assert.Single(layouts);
        Assert.Equal(SheetName, layouts[0].Name);
    }

    [Fact]
    public async Task GetAllSheetTabNames_ReturnsTitlesFromService()
    {
        var mockService = new Mock<IGoogleSheetService>();
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<CancellationToken>())).ReturnsAsync(new Spreadsheet
        {
            Sheets = new List<Sheet>
            {
                new() { Properties = new SheetProperties { Title = SheetName } },
                new() { Properties = new SheetProperties { Title = "Another" } }
            }
        });

        var manager = BuildManager(mockService.Object);

        var names = await manager.GetAllSheetTabNames();

        Assert.Equal(new[] { SheetName, "Another" }, names);
    }

    [Fact]
    public async Task GetSheets_HappyPath_MapsDataAndSetsSpreadsheetName()
    {
        var mockService = new Mock<IGoogleSheetService>();
        mockService
            .Setup(s => s.GetBatchData(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BatchGetValuesByDataFilterResponse
            {
                ValueRanges = new List<MatchedValueRange>
                {
                    new()
                    {
                        DataFilters = new List<DataFilter> { new() { A1Range = SheetName } },
                        ValueRange = new ValueRange { Values = new List<IList<object>> { new List<object> { "Header" } } }
                    }
                }
            });
        mockService
            .Setup(s => s.GetBatchDataResult(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(GoogleApiResult<BatchGetValuesByDataFilterResponse>.Ok(new BatchGetValuesByDataFilterResponse
            {
                ValueRanges = new List<MatchedValueRange>
                {
                    new()
                    {
                        DataFilters = new List<DataFilter> { new() { A1Range = SheetName } },
                        ValueRange = new ValueRange { Values = new List<IList<object>> { new List<object> { "Header" } } }
                    }
                }
            }));
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<CancellationToken>())).ReturnsAsync(new Spreadsheet
        {
            Properties = new SpreadsheetProperties { Title = "MyTestBook" },
            Sheets = new List<Sheet> { new() { Properties = new SheetProperties { Title = SheetName } } }
        });

        var manager = BuildManager(mockService.Object);

        var result = await manager.GetSheets([SheetName]);

        Assert.Equal("MyTestBook", result.Properties.Name);
        Assert.Single(result.Rows);
        Assert.Contains(result.Messages, m => m.Message.Contains("Retrieved sheet(s)") && m.Message.Contains(SheetName));
        Assert.Equal(0, manager.CreateMissingCalls);
    }

    [Fact]
    public async Task GetSheets_OnBatchFailure_WithMissingSheet_InvokesCreateMissing()
    {
        var mockService = new Mock<IGoogleSheetService>();
        mockService
            .Setup(s => s.GetBatchData(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BatchGetValuesByDataFilterResponse?)null);
        mockService
            .Setup(s => s.GetBatchDataResult(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(GoogleApiResult<BatchGetValuesByDataFilterResponse>.Failed(new GoogleApiFailure { Reason = GoogleApiFailureReason.Unknown, Message = "test failure" }));
        // Spreadsheet exists but is missing the registered sheet entirely -> self-heal path.
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<CancellationToken>())).ReturnsAsync(new Spreadsheet
        {
            Properties = new SpreadsheetProperties { Title = "MyTestBook" },
            Sheets = new List<Sheet> { new() { Properties = new SheetProperties { Title = "SomethingElse" } } }
        });

        var manager = BuildManager(mockService.Object);

        var result = await manager.GetSheets([SheetName]);

        Assert.Equal(1, manager.CreateMissingCalls);
        Assert.Contains(result.Messages, m => m.Message.Contains("Created missing sheets") && m.Message.Contains(SheetName));
    }

    [Fact]
    public async Task GetSheets_OnQuotaExceeded_ShouldNotAttemptSelfHeal()
    {
        // A rate limit failure tells us nothing about whether the sheets exist. Attempting self-heal
        // anyway would spend another call restating the same failure and risks the exact
        // misdiagnosis this behavior exists to avoid - treating "we couldn't check" as "it's missing".
        var mockService = new Mock<IGoogleSheetService>();
        mockService
            .Setup(s => s.GetBatchDataResult(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(GoogleApiResult<BatchGetValuesByDataFilterResponse>.Failed(
                new GoogleApiFailure { Reason = GoogleApiFailureReason.QuotaExceeded, Message = "rate limited" }));

        var manager = BuildManager(mockService.Object);

        await manager.GetSheets([SheetName]);

        Assert.Equal(0, manager.CreateMissingCalls);
        mockService.Verify(s => s.GetSheetInfo(It.IsAny<CancellationToken>()), Times.Never);
        mockService.Verify(s => s.GetSheetInfo(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetSheets_OnQuotaExceeded_MessageMustReadAsTemporaryNotMissing()
    {
        var mockService = new Mock<IGoogleSheetService>();
        mockService
            .Setup(s => s.GetBatchDataResult(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(GoogleApiResult<BatchGetValuesByDataFilterResponse>.Failed(
                new GoogleApiFailure { Reason = GoogleApiFailureReason.QuotaExceeded, Message = "rate limited" }));

        var manager = BuildManager(mockService.Object);

        var result = await manager.GetSheets([SheetName]);

        var message = Assert.Single(result.Messages);
        Assert.Equal(MessageLevel.ERROR.GetDescription(), message.Level);
        Assert.Contains("temporary", message.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("missing", message.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(GoogleApiFailureReason.Unauthorized, "credentials")]
    [InlineData(GoogleApiFailureReason.Forbidden, "denied")]
    [InlineData(GoogleApiFailureReason.NotFound, "spreadsheet id")]
    public async Task GetSheets_OnFailure_MessageShouldNameTheReason(GoogleApiFailureReason reason, string expectedFragment)
    {
        var mockService = new Mock<IGoogleSheetService>();
        mockService
            .Setup(s => s.GetBatchDataResult(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(GoogleApiResult<BatchGetValuesByDataFilterResponse>.Failed(
                new GoogleApiFailure { Reason = reason, Message = "failure" }));
        // NotFound is one of the reasons that still attempts self-heal; give it metadata to heal
        // against so the test exercises the final message either way.
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<CancellationToken>())).ReturnsAsync(new Spreadsheet
        {
            Properties = new SpreadsheetProperties { Title = "MyTestBook" },
            Sheets = new List<Sheet> { new() { Properties = new SheetProperties { Title = SheetName } } }
        });

        var manager = BuildManager(mockService.Object);

        var result = await manager.GetSheets([SheetName]);

        Assert.Contains(result.Messages, m => m.Message.Contains(expectedFragment, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetSheets_OnNotFound_ShouldStillAttemptSelfHeal()
    {
        // Unlike quota/auth failures, NotFound is consistent with "this sheet might genuinely be
        // missing", so the existing self-heal behavior must be preserved for it.
        var mockService = new Mock<IGoogleSheetService>();
        mockService
            .Setup(s => s.GetBatchDataResult(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(GoogleApiResult<BatchGetValuesByDataFilterResponse>.Failed(
                new GoogleApiFailure { Reason = GoogleApiFailureReason.NotFound, Message = "not found" }));
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<CancellationToken>())).ReturnsAsync(new Spreadsheet
        {
            Properties = new SpreadsheetProperties { Title = "MyTestBook" },
            Sheets = new List<Sheet> { new() { Properties = new SheetProperties { Title = "SomethingElse" } } }
        });

        var manager = BuildManager(mockService.Object);

        await manager.GetSheets([SheetName]);

        Assert.Equal(1, manager.CreateMissingCalls);
    }

    [Fact]
    public async Task CreateSheets_WithoutGenerateSheetsRequestOverride_Throws()
    {
        // TestManager (unlike TestManagerWithGeneration) deliberately doesn't override
        // GenerateSheetsRequest - mirrors a domain that hasn't wired up CreateSheets/DeleteSheets yet.
        var manager = BuildManager(new Mock<IGoogleSheetService>().Object);

        await Assert.ThrowsAsync<NotSupportedException>(() => manager.CreateSheets([SheetName]));
    }

    [Fact]
    public async Task CreateAllSheets_HappyPath_ReturnsCreatedMessages()
    {
        var mockService = new Mock<IGoogleSheetService>();
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Spreadsheet { Sheets = new List<Sheet>() }); // no default "Sheet1" present
        mockService.Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BatchUpdateSpreadsheetResponse
            {
                Replies = new List<Response> { new() { AddSheet = new AddSheetResponse { Properties = new SheetProperties { Title = SheetName } } } }
            });

        var manager = BuildGeneratingManager(mockService.Object);

        var result = await manager.CreateAllSheets();

        Assert.Contains(result.Messages, m => m.Message.Contains(SheetName.ToUpperInvariant(), StringComparison.Ordinal) && m.Message.Contains("created", StringComparison.Ordinal));
    }

    [Fact]
    public async Task CreateSheets_WithNullBatchResponse_ReturnsNotCreatedMessages()
    {
        var mockService = new Mock<IGoogleSheetService>();
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<List<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync((Spreadsheet?)null);
        mockService.Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BatchUpdateSpreadsheetResponse?)null);

        var manager = BuildGeneratingManager(mockService.Object);

        var result = await manager.CreateSheets([SheetName]);

        Assert.Contains(result.Messages, m => m.Message.Contains($"{SheetName} not created"));
    }

    [Fact]
    public async Task CreateSheets_WithDefaultSheetPresent_RelocatesItInTheSameBatch()
    {
        var mockService = new Mock<IGoogleSheetService>();
        var spreadsheetWithDefaultSheet = new Spreadsheet
        {
            Sheets = new List<Sheet> { new() { Properties = new SheetProperties { Title = "Sheet1", SheetId = 0 } } }
        };
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<CancellationToken>())).ReturnsAsync(spreadsheetWithDefaultSheet);
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<List<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(spreadsheetWithDefaultSheet);
        BatchUpdateSpreadsheetRequest? captured = null;
        mockService.Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>(), It.IsAny<CancellationToken>()))
            .Callback<BatchUpdateSpreadsheetRequest, CancellationToken>((r, _) => captured = r)
            .ReturnsAsync(new BatchUpdateSpreadsheetResponse { Replies = new List<Response>() });

        var manager = BuildGeneratingManager(mockService.Object);

        await manager.CreateSheets([SheetName]);

        Assert.NotNull(captured);
        Assert.Contains(captured!.Requests, r => r.UpdateSheetProperties != null);
    }

    [Fact]
    public async Task CreateSheets_WithExistingIndexMap_AppliesProvidedIndices()
    {
        var mockService = new Mock<IGoogleSheetService>();
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<List<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Spreadsheet { Sheets = new List<Sheet>() });
        BatchUpdateSpreadsheetRequest? captured = null;
        mockService.Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>(), It.IsAny<CancellationToken>()))
            .Callback<BatchUpdateSpreadsheetRequest, CancellationToken>((r, _) => captured = r)
            .ReturnsAsync(new BatchUpdateSpreadsheetResponse { Replies = new List<Response>() });

        var manager = BuildGeneratingManager(mockService.Object);

        await manager.CreateSheets([SheetName], new Dictionary<string, int> { [SheetName] = 3 });

        var addRequest = captured!.Requests.Single(r => r.AddSheet != null);
        Assert.Equal(3, addRequest.AddSheet.Properties.Index);
    }

    [Fact]
    public async Task DeleteAllSheets_WithNoExistingSheets_ReturnsInfoMessage()
    {
        var mockService = new Mock<IGoogleSheetService>();
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<List<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Spreadsheet { Sheets = new List<Sheet>() });

        var manager = BuildGeneratingManager(mockService.Object);

        var result = await manager.DeleteAllSheets();

        Assert.Contains(result.Messages, m => m.Message.Contains("No sheets found to delete"));
    }

    [Fact]
    public async Task DeleteSheets_WhenDeletingAllRemainingSheets_CreatesTempSheetFirst()
    {
        var mockService = new Mock<IGoogleSheetService>();
        var spreadsheet = new Spreadsheet
        {
            Sheets = new List<Sheet> { new() { Properties = new SheetProperties { Title = SheetName, SheetId = 111 } } }
        };
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<CancellationToken>())).ReturnsAsync(spreadsheet);
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<List<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(spreadsheet);
        mockService.Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BatchUpdateSpreadsheetResponse { Replies = new List<Response>() });

        var manager = BuildGeneratingManager(mockService.Object);

        var result = await manager.DeleteSheets([SheetName]);

        Assert.Contains(result.Messages, m => m.Message.Contains("Creating 'TempSheet' as safety sheet"));
        Assert.Contains(result.Messages, m => m.Message.Contains("Sheet deletion completed successfully"));
    }

    [Fact]
    public async Task DeleteSheets_WhenOtherSheetsRemain_DoesNotCreateTempSheet()
    {
        var mockService = new Mock<IGoogleSheetService>();
        var spreadsheet = new Spreadsheet
        {
            Sheets = new List<Sheet>
            {
                new() { Properties = new SheetProperties { Title = SheetName, SheetId = 111 } },
                new() { Properties = new SheetProperties { Title = "OtherSheet", SheetId = 222 } }
            }
        };
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<CancellationToken>())).ReturnsAsync(spreadsheet);
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<List<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(spreadsheet);
        mockService.Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BatchUpdateSpreadsheetResponse { Replies = new List<Response>() });

        var manager = BuildGeneratingManager(mockService.Object);

        var result = await manager.DeleteSheets([SheetName]);

        Assert.DoesNotContain(result.Messages, m => m.Message.Contains("safety sheet"));
        Assert.Contains(result.Messages, m => m.Message.Contains("Sheet deletion completed successfully"));
    }

    [Fact]
    public async Task DeleteSheets_WhenTempSheetAlreadyExists_DoesNotAddDuplicate()
    {
        // Regression test (found via live testing - #100): a TempSheet left over from a previous
        // full-delete cycle ("left in place afterward" - see its own doc comment) already satisfies
        // "at least one sheet remains". The old NeedsTempSheet logic unconditionally excluded any
        // sheet named "TempSheet" from the remaining-sheets count, so it asked to add a SECOND
        // "TempSheet" even when one already existed - Google rejects a duplicate tab name, which
        // fails the whole delete batch atomically and leaves the requested sheets undeleted.
        var mockService = new Mock<IGoogleSheetService>();
        var spreadsheet = new Spreadsheet
        {
            Sheets = new List<Sheet>
            {
                new() { Properties = new SheetProperties { Title = SheetName, SheetId = 111 } },
                new() { Properties = new SheetProperties { Title = SheetManagerBase.TempSheetName, SheetId = 222 } }
            }
        };
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<CancellationToken>())).ReturnsAsync(spreadsheet);
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<List<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(spreadsheet);
        BatchUpdateSpreadsheetRequest? captured = null;
        mockService.Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>(), It.IsAny<CancellationToken>()))
            .Callback<BatchUpdateSpreadsheetRequest, CancellationToken>((r, _) => captured = r)
            .ReturnsAsync(new BatchUpdateSpreadsheetResponse { Replies = new List<Response>() });

        var manager = BuildGeneratingManager(mockService.Object);

        var result = await manager.DeleteSheets([SheetName]);

        Assert.DoesNotContain(result.Messages, m => m.Message.Contains("safety sheet"));
        Assert.DoesNotContain(captured!.Requests, r => r.AddSheet != null);
        Assert.Contains(result.Messages, m => m.Message.Contains("Sheet deletion completed successfully"));
    }

    [Fact]
    public async Task DeleteSheets_WithNullBatchResponse_ReturnsErrorMessage()
    {
        var mockService = new Mock<IGoogleSheetService>();
        var spreadsheet = new Spreadsheet
        {
            Sheets = new List<Sheet>
            {
                new() { Properties = new SheetProperties { Title = SheetName, SheetId = 111 } },
                new() { Properties = new SheetProperties { Title = "OtherSheet", SheetId = 222 } }
            }
        };
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<CancellationToken>())).ReturnsAsync(spreadsheet);
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<List<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(spreadsheet);
        mockService.Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BatchUpdateSpreadsheetResponse?)null);

        var manager = BuildGeneratingManager(mockService.Object);

        var result = await manager.DeleteSheets([SheetName]);

        Assert.Contains(result.Messages, m => m.Message.Contains("Sheet deletion failed"));
    }

    [Fact]
    public async Task DeleteSheets_WhenServiceThrows_ReturnsErrorMessage()
    {
        var mockService = new Mock<IGoogleSheetService>();
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("boom"));
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<List<string>>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("boom"));

        var manager = BuildGeneratingManager(mockService.Object);

        var result = await manager.DeleteSheets([SheetName]);

        Assert.Contains(result.Messages, m => m.Message.Contains("Error deleting sheets") && m.Message.Contains("boom"));
    }

    // #102: Google 500s deleting 2+ protected sheets in one batch (reproduced 5/5 live against
    // Stock's Accounts/Tickers). DeleteSheets splits into separate batch calls once 2+ of the
    // sheets being deleted are protected; 0 or 1 protected sheet keeps the single-batch path above
    // completely unchanged (see IsProtectedSheet/ExecuteSplitDeleteAsync).

    private static SheetRegistry<TestEntity> BuildRegistryWithProtection(params (string Name, bool Protect)[] sheets)
    {
        var registry = new SheetRegistry<TestEntity>();
        foreach (var (name, protect) in sheets)
        {
            registry.Register(name, () => new SheetModel { Name = name, ProtectSheet = protect }, (_, _) => { });
        }
        return registry;
    }

    [Fact]
    public async Task DeleteSheets_With2PlusProtectedSheets_SplitsIntoSeparateBatchCalls()
    {
        const string Unprotected = "Stocks";
        const string ProtectedA = "Accounts";
        const string ProtectedB = "Tickers";

        var registry = BuildRegistryWithProtection((Unprotected, false), (ProtectedA, true), (ProtectedB, true));
        var mockService = new Mock<IGoogleSheetService>();
        var spreadsheet = new Spreadsheet
        {
            Sheets = new List<Sheet>
            {
                new() { Properties = new SheetProperties { Title = Unprotected, SheetId = 1 } },
                new() { Properties = new SheetProperties { Title = ProtectedA, SheetId = 2 } },
                new() { Properties = new SheetProperties { Title = ProtectedB, SheetId = 3 } }
            }
        };
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<CancellationToken>())).ReturnsAsync(spreadsheet);
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<List<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(spreadsheet);

        var capturedRequests = new List<BatchUpdateSpreadsheetRequest>();
        mockService.Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>(), It.IsAny<CancellationToken>()))
            .Callback<BatchUpdateSpreadsheetRequest, CancellationToken>((r, _) => capturedRequests.Add(r))
            .ReturnsAsync(new BatchUpdateSpreadsheetResponse { Replies = new List<Response>() });

        var manager = new TestManagerWithGeneration(mockService.Object, registry, [Unprotected, ProtectedA, ProtectedB]);

        var result = await manager.DeleteSheets([Unprotected, ProtectedA, ProtectedB]);

        // 3 separate calls: one grouped call (unprotected sheet + temp-sheet safety net, since all
        // 3 canonical sheets are being deleted), plus one call per protected sheet.
        mockService.Verify(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(3));

        Assert.Contains(capturedRequests[0].Requests, r => r.AddSheet != null && r.AddSheet.Properties.Title == SheetManagerBase.TempSheetName);
        Assert.Contains(capturedRequests[0].Requests, r => r.DeleteSheet != null && r.DeleteSheet.SheetId == 1);

        var laterDeletedIds = capturedRequests.Skip(1)
            .SelectMany(r => r.Requests)
            .Where(r => r.DeleteSheet != null)
            .Select(r => r.DeleteSheet.SheetId)
            .ToList();
        Assert.Equal([2, 3], laterDeletedIds);
        // Each protected sheet gets its own call, containing nothing but that one delete.
        Assert.All(capturedRequests.Skip(1), r => Assert.Single(r.Requests));

        Assert.Equal(3, result.Messages.Count(m => m.Message.Contains("Sheet deletion completed successfully")));
    }

    [Fact]
    public async Task DeleteSheets_With1ProtectedSheet_StaysOnSingleBatchCall()
    {
        // Boundary guard: exactly 1 protected sheet must NOT trigger the split path.
        const string Unprotected = "Stocks";
        const string ProtectedA = "Accounts";

        var registry = BuildRegistryWithProtection((Unprotected, false), (ProtectedA, true));
        var mockService = new Mock<IGoogleSheetService>();
        var spreadsheet = new Spreadsheet
        {
            Sheets = new List<Sheet>
            {
                new() { Properties = new SheetProperties { Title = Unprotected, SheetId = 1 } },
                new() { Properties = new SheetProperties { Title = ProtectedA, SheetId = 2 } }
            }
        };
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<CancellationToken>())).ReturnsAsync(spreadsheet);
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<List<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(spreadsheet);
        mockService.Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BatchUpdateSpreadsheetResponse { Replies = new List<Response>() });

        var manager = new TestManagerWithGeneration(mockService.Object, registry, [Unprotected, ProtectedA]);

        var result = await manager.DeleteSheets([Unprotected, ProtectedA]);

        mockService.Verify(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.Contains(result.Messages, m => m.Message.Contains("Sheet deletion completed successfully"));
    }

    [Fact]
    public async Task DeleteSheets_With2PlusProtectedSheets_WhenOneCallFails_StillAttemptsTheRest()
    {
        // A caller relying on "delete everything" shouldn't be stuck just because one protected
        // sheet's call failed - the others should still be attempted, with failure reported
        // per-call rather than aborting the whole operation. An untouched "Other" sheet is included
        // so no temp-sheet call is needed here, keeping this test focused on the fail/continue
        // behavior rather than the temp-sheet call already covered above.
        const string ProtectedA = "Accounts";
        const string ProtectedB = "Tickers";
        const string Other = "Other";

        var registry = BuildRegistryWithProtection((ProtectedA, true), (ProtectedB, true), (Other, false));
        var mockService = new Mock<IGoogleSheetService>();
        var spreadsheet = new Spreadsheet
        {
            Sheets = new List<Sheet>
            {
                new() { Properties = new SheetProperties { Title = ProtectedA, SheetId = 2 } },
                new() { Properties = new SheetProperties { Title = ProtectedB, SheetId = 3 } },
                new() { Properties = new SheetProperties { Title = Other, SheetId = 4 } }
            }
        };
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<CancellationToken>())).ReturnsAsync(spreadsheet);
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<List<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(spreadsheet);
        mockService.SetupSequence(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BatchUpdateSpreadsheetResponse?)null)
            .ReturnsAsync(new BatchUpdateSpreadsheetResponse { Replies = new List<Response>() });

        var manager = new TestManagerWithGeneration(mockService.Object, registry, [ProtectedA, ProtectedB, Other]);

        var result = await manager.DeleteSheets([ProtectedA, ProtectedB]);

        mockService.Verify(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        Assert.Contains(result.Messages, m => m.Message.Contains("Sheet deletion failed"));
        Assert.Contains(result.Messages, m => m.Message.Contains("Sheet deletion completed successfully"));
    }

    // DetectBrokenColumnsAsync / ReapplyColumnFormulas (#53 gaps 2/3) - detecting and fixing a
    // column that already exists under the same name on both sides but whose live Formula has
    // drifted from canonical. Distinct from missing-column self-heal, which is covered elsewhere.

    private static SheetRegistry<TestEntity> BuildRegistryWithHeaders(string sheetName, List<SheetCellModel> headers)
    {
        var registry = new SheetRegistry<TestEntity>();
        registry.Register(sheetName, () => new SheetModel { Name = sheetName, Headers = headers }, (_, _) => { });
        return registry;
    }

    private static Spreadsheet BuildLiveStructureSpreadsheet(string sheetName, int sheetId, params (string Name, string? Formula)[] liveHeaders)
    {
        return new Spreadsheet
        {
            Sheets = new List<Sheet>
            {
                new()
                {
                    Properties = new SheetProperties { Title = sheetName, SheetId = sheetId },
                    Data = new List<GridData>
                    {
                        new()
                        {
                            RowData = new List<RowData>
                            {
                                new()
                                {
                                    Values = liveHeaders.Select(h => new CellData
                                    {
                                        FormattedValue = h.Name,
                                        UserEnteredValue = string.IsNullOrEmpty(h.Formula) ? null : new ExtendedValue { FormulaValue = h.Formula }
                                    }).ToList()
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    [Fact]
    public async Task DetectBrokenColumnsAsync_WithDriftedFormula_ReturnsCanonicalFixAtLivePosition()
    {
        var headers = new List<SheetCellModel> { new() { Name = "Date" }, new() { Name = "Total", Formula = "=SUM(A:A)" } };
        var registry = BuildRegistryWithHeaders(SheetName, headers);
        var spreadsheet = BuildLiveStructureSpreadsheet(SheetName, 42, ("Date", null), ("Total", "=OLD_BROKEN_FORMULA"));

        var mockService = new Mock<IGoogleSheetService>();
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<List<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(spreadsheet);

        var manager = new TestManagerWithGeneration(mockService.Object, registry, [SheetName]);

        var broken = await manager.DetectBrokenColumnsAsync(SheetName);

        var column = Assert.Single(broken);
        Assert.Equal("Total", column.ColumnName);
        Assert.Equal("=SUM(A:A)", column.Formula); // canonical - ready to reapply, not the drifted live one
        Assert.Equal(42, column.SheetId);
        Assert.Equal(1, column.ColumnIndex);
        Assert.Equal("B", column.ColumnLetter);
    }

    [Fact]
    public async Task DetectBrokenColumnsAsync_WithMatchingFormula_ReturnsEmpty()
    {
        var headers = new List<SheetCellModel> { new() { Name = "Date" }, new() { Name = "Total", Formula = "=SUM(A:A)" } };
        var registry = BuildRegistryWithHeaders(SheetName, headers);
        var spreadsheet = BuildLiveStructureSpreadsheet(SheetName, 42, ("Date", null), ("Total", "=SUM(A:A)"));

        var mockService = new Mock<IGoogleSheetService>();
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<List<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(spreadsheet);

        var manager = new TestManagerWithGeneration(mockService.Object, registry, [SheetName]);

        var broken = await manager.DetectBrokenColumnsAsync(SheetName);

        Assert.Empty(broken);
    }

    [Fact]
    public async Task DetectBrokenColumnsAsync_WithMissingColumn_DoesNotFlagIt()
    {
        // A column absent from the live sheet entirely is missing-column self-heal's job, not this
        // one's - it must not also show up here as "broken".
        var headers = new List<SheetCellModel> { new() { Name = "Date" }, new() { Name = "Total", Formula = "=SUM(A:A)" } };
        var registry = BuildRegistryWithHeaders(SheetName, headers);
        var spreadsheet = BuildLiveStructureSpreadsheet(SheetName, 42, ("Date", null)); // "Total" missing entirely

        var mockService = new Mock<IGoogleSheetService>();
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<List<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(spreadsheet);

        var manager = new TestManagerWithGeneration(mockService.Object, registry, [SheetName]);

        var broken = await manager.DetectBrokenColumnsAsync(SheetName);

        Assert.Empty(broken);
    }

    [Fact]
    public async Task ReapplyColumnFormulas_WithBrokenColumns_CallsBatchUpdateAndReportsSuccess()
    {
        var mockService = new Mock<IGoogleSheetService>();
        mockService.Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BatchUpdateSpreadsheetResponse());

        var manager = BuildGeneratingManager(mockService.Object);
        var broken = new List<ColumnInsertionInfo>
        {
            new() { SheetName = SheetName, SheetId = 1, ColumnIndex = 1, ColumnName = "Total", Formula = "=SUM(A:A)" }
        };

        var result = await manager.ReapplyColumnFormulas(SheetName, broken);

        Assert.Contains(result.Messages, m => m.Message.Contains("Reapplied formula for 1 column"));
        mockService.Verify(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReapplyColumnFormulas_WithNoBrokenColumns_DoesNotCallService()
    {
        var mockService = new Mock<IGoogleSheetService>();
        var manager = BuildGeneratingManager(mockService.Object);

        var result = await manager.ReapplyColumnFormulas(SheetName, []);

        Assert.Contains(result.Messages, m => m.Message.Contains("No broken columns to reapply"));
        mockService.Verify(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // RefreshHeaderFormulasAsync / RefreshDependentSheetsAsync - the automated replacement for
    // manually "reapplying" a sheet to fix a stale cross-sheet formula reference (#REF!/#ERROR!/
    // #N/A caused by a referenced sheet's headers changing after a dependent's formula was
    // written). "Base"/"Dependent" below mirror Stock's real Tickers/Stocks dependency - Dependent's
    // formula references Base via the same quoted 'Name'! pattern ObjectExtensions.GetRange
    // produces, which is what SheetRegistry.GetDependents detects automatically (no manual
    // dependency declaration).

    private const string BaseSheetName = "Base";
    private const string DependentSheetName = "Dependent";

    private static SheetModel DependentSheetModel() => new()
    {
        Name = DependentSheetName,
        Headers = [new SheetCellModel { Name = "Total", Formula = $"=SUM('{BaseSheetName}'!A:A)" }]
    };

    private static SheetRegistry<TestEntity> BuildRegistryWithDependency(List<SheetCellModel>? baseHeaders = null)
    {
        var registry = new SheetRegistry<TestEntity>();
        registry.Register(BaseSheetName, () => new SheetModel { Name = BaseSheetName, Headers = baseHeaders ?? [new SheetCellModel { Name = "Name" }] }, (_, _) => { });
        registry.Register(DependentSheetName, DependentSheetModel, (_, _) => { });
        return registry;
    }

    private static Spreadsheet SpreadsheetWith(params (string Title, int SheetId)[] sheets) => new()
    {
        Properties = new SpreadsheetProperties { Title = "Book" },
        Sheets = sheets.Select(s => new Sheet { Properties = new SheetProperties { Title = s.Title, SheetId = s.SheetId } }).ToList()
    };

    [Fact]
    public async Task RefreshHeaderFormulasAsync_WritesOneBatchRequestCoveringEverySheet()
    {
        var mockService = new Mock<IGoogleSheetService>();
        var spreadsheet = SpreadsheetWith((BaseSheetName, 10), (DependentSheetName, 42));
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<CancellationToken>())).ReturnsAsync(spreadsheet);
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<List<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(spreadsheet);

        var captured = new List<BatchUpdateSpreadsheetRequest>();
        mockService.Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>(), It.IsAny<CancellationToken>()))
            .Callback<BatchUpdateSpreadsheetRequest, CancellationToken>((r, _) => captured.Add(r))
            .ReturnsAsync(new BatchUpdateSpreadsheetResponse { Replies = new List<Response>() });

        var manager = new TestManager(mockService.Object, BuildRegistryWithDependency(), [BaseSheetName, DependentSheetName]);

        await manager.RefreshHeaderFormulasAsync([BaseSheetName, DependentSheetName]);

        var request = Assert.Single(captured);
        Assert.Contains(request.Requests, r => r.UpdateCells?.Range.SheetId == 10);
        Assert.Contains(request.Requests, r => r.UpdateCells?.Range.SheetId == 42);
    }

    [Fact]
    public async Task RefreshHeaderFormulasAsync_SkipsSheetsNotYetCreatedOrUnregistered()
    {
        var mockService = new Mock<IGoogleSheetService>();
        // Only "Base" exists live; "Dependent" hasn't been created yet, "Unknown" isn't registered at all.
        var spreadsheet = SpreadsheetWith((BaseSheetName, 10));
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<CancellationToken>())).ReturnsAsync(spreadsheet);
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<List<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(spreadsheet);

        var captured = new List<BatchUpdateSpreadsheetRequest>();
        mockService.Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>(), It.IsAny<CancellationToken>()))
            .Callback<BatchUpdateSpreadsheetRequest, CancellationToken>((r, _) => captured.Add(r))
            .ReturnsAsync(new BatchUpdateSpreadsheetResponse { Replies = new List<Response>() });

        var manager = new TestManager(mockService.Object, BuildRegistryWithDependency(), [BaseSheetName, DependentSheetName]);

        await manager.RefreshHeaderFormulasAsync([BaseSheetName, DependentSheetName, "Unknown"]);

        var request = Assert.Single(captured);
        var onlyRequest = Assert.Single(request.Requests);
        Assert.Equal(10, onlyRequest.UpdateCells?.Range.SheetId);
    }

    [Fact]
    public async Task RefreshDependentSheetsAsync_WithNoDependents_MakesNoApiCalls()
    {
        var mockService = new Mock<IGoogleSheetService>();
        var manager = BuildManager(mockService.Object); // single-sheet registry, no dependsOn edges

        await manager.RefreshDependentSheetsAsync([SheetName]);

        mockService.Verify(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        mockService.Verify(s => s.GetSheetInfo(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetSheets_SelfHealsMissingSheet_RefreshesAlreadyExistingDependentSheet()
    {
        var mockService = new Mock<IGoogleSheetService>();

        // batchGet fails (Base is missing entirely) -> triggers the self-heal path.
        mockService.Setup(s => s.GetBatchData(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BatchGetValuesByDataFilterResponse?)null);
        mockService
            .Setup(s => s.GetBatchDataResult(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(GoogleApiResult<BatchGetValuesByDataFilterResponse>.Failed(new GoogleApiFailure { Reason = GoogleApiFailureReason.Unknown, Message = "test failure" }));

        // Dependent already exists live; Base doesn't yet - matches the real "Tickers deleted,
        // Stocks still references it" scenario.
        var spreadsheet = SpreadsheetWith((DependentSheetName, 42));
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<CancellationToken>())).ReturnsAsync(spreadsheet);
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<List<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(spreadsheet);

        var captured = new List<BatchUpdateSpreadsheetRequest>();
        mockService.Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>(), It.IsAny<CancellationToken>()))
            .Callback<BatchUpdateSpreadsheetRequest, CancellationToken>((r, _) => captured.Add(r))
            .ReturnsAsync(new BatchUpdateSpreadsheetResponse { Replies = new List<Response>() });

        var manager = new TestManager(mockService.Object, BuildRegistryWithDependency(), [BaseSheetName, DependentSheetName]);

        await manager.GetSheets([BaseSheetName, DependentSheetName]);

        Assert.Contains(captured, r => r.Requests.Any(req => req.UpdateCells?.Range.SheetId == 42));
    }

    [Fact]
    public async Task GetSheets_AutoHealsMissingColumn_RefreshesDependentSheetHeaders()
    {
        var mockService = new Mock<IGoogleSheetService>();

        // Base's live header row is missing "Price" (present in the registered SheetModel below) -
        // simulates a referenced sheet's column layout having drifted/shifted.
        var response = new BatchGetValuesByDataFilterResponse
        {
            ValueRanges =
            [
                new MatchedValueRange
                {
                    DataFilters = [new DataFilter { A1Range = BaseSheetName }],
                    ValueRange = new ValueRange { Values = new List<IList<object>> { new List<object> { "Name" } } }
                },
                new MatchedValueRange
                {
                    DataFilters = [new DataFilter { A1Range = DependentSheetName }],
                    ValueRange = new ValueRange { Values = new List<IList<object>> { new List<object> { "Total" } } }
                }
            ]
        };
        mockService.Setup(s => s.GetBatchData(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);
        mockService
            .Setup(s => s.GetBatchDataResult(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(GoogleApiResult<BatchGetValuesByDataFilterResponse>.Ok(response));

        var spreadsheet = SpreadsheetWith((BaseSheetName, 10), (DependentSheetName, 42));
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<CancellationToken>())).ReturnsAsync(spreadsheet);
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<List<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(spreadsheet);

        var captured = new List<BatchUpdateSpreadsheetRequest>();
        mockService.Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>(), It.IsAny<CancellationToken>()))
            .Callback<BatchUpdateSpreadsheetRequest, CancellationToken>((r, _) => captured.Add(r))
            .ReturnsAsync(new BatchUpdateSpreadsheetResponse { Replies = new List<Response>() });

        var baseHeaders = new List<SheetCellModel> { new() { Name = "Name" }, new() { Name = "Price" } };
        var manager = new TestManager(mockService.Object, BuildRegistryWithDependency(baseHeaders), [BaseSheetName, DependentSheetName]);

        await manager.GetSheets([BaseSheetName, DependentSheetName]);

        // Column insertion (Base, sheetId 10) and the dependent header refresh (Dependent, sheetId
        // 42) must land in the SAME BatchUpdateSpreadsheet call - not two separate API calls.
        var request = Assert.Single(captured);
        Assert.Contains(request.Requests, req => req.InsertDimension?.Range.SheetId == 10);
        Assert.Contains(request.Requests, req => req.UpdateCells?.Range.SheetId == 42);
    }

    [Fact]
    public async Task CreateSheets_WithExistingDependent_RefreshesItInTheSameBatchCall()
    {
        var mockService = new Mock<IGoogleSheetService>();

        // Dependent already exists live (sheetId 42); Base is about to be created by this call.
        var spreadsheet = SpreadsheetWith((DependentSheetName, 42));
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<CancellationToken>())).ReturnsAsync(spreadsheet);
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<List<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(spreadsheet);

        var captured = new List<BatchUpdateSpreadsheetRequest>();
        mockService.Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>(), It.IsAny<CancellationToken>()))
            .Callback<BatchUpdateSpreadsheetRequest, CancellationToken>((r, _) => captured.Add(r))
            .ReturnsAsync(new BatchUpdateSpreadsheetResponse
            {
                Replies = [new Response { AddSheet = new AddSheetResponse { Properties = new SheetProperties { Title = BaseSheetName } } }]
            });

        var manager = new TestManagerWithGeneration(mockService.Object, BuildRegistryWithDependency(), [BaseSheetName, DependentSheetName]);

        await manager.CreateSheets([BaseSheetName]);

        // AddSheet (Base) and the dependent header refresh (Dependent, sheetId 42) must land in the
        // SAME BatchUpdateSpreadsheet call - this is the exact "reapply headers in one call" behavior.
        var request = Assert.Single(captured);
        Assert.Contains(request.Requests, req => req.AddSheet?.Properties.Title == BaseSheetName);
        Assert.Contains(request.Requests, req => req.UpdateCells?.Range.SheetId == 42);
    }

    [Fact]
    public async Task GetLiveSheetsRawValues_MapsEachRangeBackToItsOwnSheetName()
    {
        // Google doesn't guarantee response order matches the request, and each returned range only
        // echoes back the DataFilter that produced it (sheet name + the "!A1:ZZ{maxRows}" suffix this
        // call appends) - this response is deliberately built out of request order to prove the lookup
        // doesn't just assume ValueRanges[i] belongs to sheets[i].
        var response = new BatchGetValuesByDataFilterResponse
        {
            ValueRanges =
            [
                new MatchedValueRange
                {
                    DataFilters = [new DataFilter { A1Range = "Another!A1:ZZ5" }],
                    ValueRange = new ValueRange { Values = new List<IList<object>> { new List<object> { "x" } } }
                },
                new MatchedValueRange
                {
                    DataFilters = [new DataFilter { A1Range = $"{SheetName}!A1:ZZ5" }],
                    ValueRange = new ValueRange { Values = new List<IList<object>> { new List<object> { "Header" }, new List<object> { "Row1" } } }
                }
            ]
        };

        var mockService = new Mock<IGoogleSheetService>();
        mockService.Setup(s => s.GetBatchData(It.IsAny<List<string>>(), "A1:ZZ5", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var manager = BuildManager(mockService.Object);

        var results = await manager.GetLiveSheetsRawValues([SheetName, "Another"], maxRows: 5);

        Assert.Equal(2, results.Count);
        Assert.Equal(new List<string?> { "Header" }, results[SheetName][0]);
        Assert.Equal(new List<string?> { "Row1" }, results[SheetName][1]);
        Assert.Equal(new List<string?> { "x" }, results["Another"][0]);
    }

    [Fact]
    public async Task GetLiveSheetsRawValues_WithEmptySheetList_ReturnsEmptyWithoutCallingService()
    {
        var mockService = new Mock<IGoogleSheetService>();
        var manager = BuildManager(mockService.Object);

        var results = await manager.GetLiveSheetsRawValues([]);

        Assert.Empty(results);
        mockService.Verify(s => s.GetBatchData(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetLiveSheetRawValues_SingleSheet_StillReturnsItsOwnValues()
    {
        // Regression check for the GetLiveSheetsRawValues refactor: the single-sheet overload used to
        // blindly take ValueRanges.FirstOrDefault() (safe only because it never requested more than one
        // sheet); it now goes through the same by-name dictionary lookup the bulk method uses.
        var response = new BatchGetValuesByDataFilterResponse
        {
            ValueRanges =
            [
                new MatchedValueRange
                {
                    DataFilters = [new DataFilter { A1Range = $"{SheetName}!A1:ZZ200" }],
                    ValueRange = new ValueRange { Values = new List<IList<object>> { new List<object> { "Only" } } }
                }
            ]
        };

        var mockService = new Mock<IGoogleSheetService>();
        mockService.Setup(s => s.GetBatchData(It.IsAny<List<string>>(), "A1:ZZ200", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var manager = BuildManager(mockService.Object);

        var values = await manager.GetLiveSheetRawValues(SheetName);

        Assert.Equal(new List<string?> { "Only" }, Assert.Single(values));
    }

    // ReapplyFormatting - the manual, opt-in counterpart to auto-heal for #28. Only
    // FormattingOptionsEntity.ReapplyColumnFormats is implemented today.

    private static SheetRegistry<TestEntity> BuildRegistryWithFormattedHeader()
    {
        var registry = new SheetRegistry<TestEntity>();
        registry.Register(SheetName, () => new SheetModel
        {
            Name = SheetName,
            Headers = [new SheetCellModel { Name = "Amount", Format = Format.ACCOUNTING }, new SheetCellModel { Name = "Label" }]
        }, (_, _) => { });
        return registry;
    }

    [Fact]
    public async Task ReapplyFormatting_DefaultOptions_ReappliesFormatForFormattedHeadersOnly()
    {
        var mockService = new Mock<IGoogleSheetService>();
        var spreadsheet = SpreadsheetWith((SheetName, 10));
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<CancellationToken>())).ReturnsAsync(spreadsheet);
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<List<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(spreadsheet);

        BatchUpdateSpreadsheetRequest? captured = null;
        mockService.Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>(), It.IsAny<CancellationToken>()))
            .Callback<BatchUpdateSpreadsheetRequest, CancellationToken>((r, _) => captured = r)
            .ReturnsAsync(new BatchUpdateSpreadsheetResponse());

        var manager = new TestManager(mockService.Object, BuildRegistryWithFormattedHeader(), [SheetName]);

        var result = await manager.ReapplyFormatting(SheetName);

        Assert.NotNull(captured);
        // Only "Amount" has a Format - "Label" doesn't, so exactly one RepeatCell request, not two.
        var request = Assert.Single(captured!.Requests);
        Assert.Equal(10, request.RepeatCell.Range.SheetId);
        Assert.Equal(0, request.RepeatCell.Range.StartColumnIndex);
        Assert.Contains(result.Messages, m => m.Message.Contains("Reapplied column formats"));
    }

    [Fact]
    public async Task ReapplyFormatting_WithReapplyColumnFormatsDisabled_MakesNoApiCall()
    {
        var mockService = new Mock<IGoogleSheetService>();
        var manager = new TestManager(mockService.Object, BuildRegistryWithFormattedHeader(), [SheetName]);

        var result = await manager.ReapplyFormatting(SheetName, FormattingOptionsEntity.None);

        Assert.Contains(result.Messages, m => m.Message.Contains("nothing to reapply"));
        mockService.Verify(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ReapplyFormatting_ForNonExistentSheet_ReturnsWarningWithoutCallingService()
    {
        var mockService = new Mock<IGoogleSheetService>();
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<CancellationToken>())).ReturnsAsync(new Spreadsheet { Sheets = new List<Sheet>() });

        var manager = new TestManager(mockService.Object, BuildRegistryWithFormattedHeader(), [SheetName]);

        var result = await manager.ReapplyFormatting(SheetName);

        Assert.Contains(result.Messages, m => m.Message.Contains("does not exist"));
        mockService.Verify(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
