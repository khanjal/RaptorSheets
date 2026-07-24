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
/// Domain-agnostic coverage for GoogleSheetManagerBase&lt;TEntity&gt; - the registry-backed base that
/// Gig/Stock (and future Job/Home) inherit. Uses a minimal in-file entity + registry so the shared
/// read/metadata/layout surface is verified in Core without depending on any domain package.
/// </summary>
public class GoogleSheetManagerGenericBaseTests
{
    private sealed class TestEntity : ISheetEntity
    {
        public PropertyEntity Properties { get; set; } = new();
        public List<MessageEntity> Messages { get; set; } = [];
        public List<IList<object>> Rows { get; set; } = [];
    }

    private sealed class TestManager : GoogleSheetManagerBase<TestEntity>
    {
        public TestManager(IGoogleSheetService service, SheetRegistry<TestEntity> registry, List<string> canonical, ILogger? logger = null)
            : base(service, registry, canonical, logger) { }

        public int CreateMissingCalls { get; private set; }

        protected override Task<TestEntity> CreateMissingSheetsAsync(Dictionary<string, int> missingIndexMap)
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
    private sealed class TestManagerWithGeneration : GoogleSheetManagerBase<TestEntity>
    {
        public TestManagerWithGeneration(IGoogleSheetService service, SheetRegistry<TestEntity> registry, List<string> canonical, ILogger? logger = null)
            : base(service, registry, canonical, logger) { }

        protected override Task<TestEntity> CreateMissingSheetsAsync(Dictionary<string, int> missingIndexMap)
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
        mockService.Setup(s => s.GetSheetInfo()).ReturnsAsync(new Spreadsheet
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
            .Setup(s => s.GetBatchData(It.IsAny<List<string>>(), It.IsAny<string>()))
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
            .Setup(s => s.GetBatchDataResult(It.IsAny<List<string>>(), It.IsAny<string>()))
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
        mockService.Setup(s => s.GetSheetInfo()).ReturnsAsync(new Spreadsheet
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
            .Setup(s => s.GetBatchData(It.IsAny<List<string>>(), It.IsAny<string>()))
            .ReturnsAsync((BatchGetValuesByDataFilterResponse?)null);
        mockService
            .Setup(s => s.GetBatchDataResult(It.IsAny<List<string>>(), It.IsAny<string>()))
            .ReturnsAsync(GoogleApiResult<BatchGetValuesByDataFilterResponse>.Failed(new GoogleApiFailure { Reason = GoogleApiFailureReason.Unknown, Message = "test failure" }));
        // Spreadsheet exists but is missing the registered sheet entirely -> self-heal path.
        mockService.Setup(s => s.GetSheetInfo()).ReturnsAsync(new Spreadsheet
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
            .Setup(s => s.GetBatchDataResult(It.IsAny<List<string>>(), It.IsAny<string>()))
            .ReturnsAsync(GoogleApiResult<BatchGetValuesByDataFilterResponse>.Failed(
                new GoogleApiFailure { Reason = GoogleApiFailureReason.QuotaExceeded, Message = "rate limited" }));

        var manager = BuildManager(mockService.Object);

        await manager.GetSheets([SheetName]);

        Assert.Equal(0, manager.CreateMissingCalls);
        mockService.Verify(s => s.GetSheetInfo(), Times.Never);
        mockService.Verify(s => s.GetSheetInfo(It.IsAny<List<string>>()), Times.Never);
    }

    [Fact]
    public async Task GetSheets_OnQuotaExceeded_MessageMustReadAsTemporaryNotMissing()
    {
        var mockService = new Mock<IGoogleSheetService>();
        mockService
            .Setup(s => s.GetBatchDataResult(It.IsAny<List<string>>(), It.IsAny<string>()))
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
            .Setup(s => s.GetBatchDataResult(It.IsAny<List<string>>(), It.IsAny<string>()))
            .ReturnsAsync(GoogleApiResult<BatchGetValuesByDataFilterResponse>.Failed(
                new GoogleApiFailure { Reason = reason, Message = "failure" }));
        // NotFound is one of the reasons that still attempts self-heal; give it metadata to heal
        // against so the test exercises the final message either way.
        mockService.Setup(s => s.GetSheetInfo()).ReturnsAsync(new Spreadsheet
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
            .Setup(s => s.GetBatchDataResult(It.IsAny<List<string>>(), It.IsAny<string>()))
            .ReturnsAsync(GoogleApiResult<BatchGetValuesByDataFilterResponse>.Failed(
                new GoogleApiFailure { Reason = GoogleApiFailureReason.NotFound, Message = "not found" }));
        mockService.Setup(s => s.GetSheetInfo()).ReturnsAsync(new Spreadsheet
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
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<List<string>>()))
            .ReturnsAsync(new Spreadsheet { Sheets = new List<Sheet>() }); // no default "Sheet1" present
        mockService.Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>()))
            .ReturnsAsync(new BatchUpdateSpreadsheetResponse
            {
                Replies = new List<Response> { new() { AddSheet = new AddSheetResponse { Properties = new SheetProperties { Title = SheetName } } } }
            });

        var manager = BuildGeneratingManager(mockService.Object);

        var result = await manager.CreateAllSheets();

        Assert.Contains(result.Messages, m => m.Message.Contains(SheetName.ToUpperInvariant()) && m.Message.Contains("created"));
    }

    [Fact]
    public async Task CreateSheets_WithNullBatchResponse_ReturnsNotCreatedMessages()
    {
        var mockService = new Mock<IGoogleSheetService>();
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<List<string>>())).ReturnsAsync((Spreadsheet?)null);
        mockService.Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>()))
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
        mockService.Setup(s => s.GetSheetInfo()).ReturnsAsync(spreadsheetWithDefaultSheet);
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<List<string>>())).ReturnsAsync(spreadsheetWithDefaultSheet);
        BatchUpdateSpreadsheetRequest? captured = null;
        mockService.Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>()))
            .Callback<BatchUpdateSpreadsheetRequest>(r => captured = r)
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
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<List<string>>())).ReturnsAsync(new Spreadsheet { Sheets = new List<Sheet>() });
        BatchUpdateSpreadsheetRequest? captured = null;
        mockService.Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>()))
            .Callback<BatchUpdateSpreadsheetRequest>(r => captured = r)
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
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<List<string>>())).ReturnsAsync(new Spreadsheet { Sheets = new List<Sheet>() });

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
        mockService.Setup(s => s.GetSheetInfo()).ReturnsAsync(spreadsheet);
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<List<string>>())).ReturnsAsync(spreadsheet);
        mockService.Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>()))
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
        mockService.Setup(s => s.GetSheetInfo()).ReturnsAsync(spreadsheet);
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<List<string>>())).ReturnsAsync(spreadsheet);
        mockService.Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>()))
            .ReturnsAsync(new BatchUpdateSpreadsheetResponse { Replies = new List<Response>() });

        var manager = BuildGeneratingManager(mockService.Object);

        var result = await manager.DeleteSheets([SheetName]);

        Assert.DoesNotContain(result.Messages, m => m.Message.Contains("safety sheet"));
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
        mockService.Setup(s => s.GetSheetInfo()).ReturnsAsync(spreadsheet);
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<List<string>>())).ReturnsAsync(spreadsheet);
        mockService.Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>()))
            .ReturnsAsync((BatchUpdateSpreadsheetResponse?)null);

        var manager = BuildGeneratingManager(mockService.Object);

        var result = await manager.DeleteSheets([SheetName]);

        Assert.Contains(result.Messages, m => m.Message.Contains("Sheet deletion failed"));
    }

    [Fact]
    public async Task DeleteSheets_WhenServiceThrows_ReturnsErrorMessage()
    {
        var mockService = new Mock<IGoogleSheetService>();
        mockService.Setup(s => s.GetSheetInfo()).ThrowsAsync(new InvalidOperationException("boom"));
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<List<string>>())).ThrowsAsync(new InvalidOperationException("boom"));

        var manager = BuildGeneratingManager(mockService.Object);

        var result = await manager.DeleteSheets([SheetName]);

        Assert.Contains(result.Messages, m => m.Message.Contains("Error deleting sheets") && m.Message.Contains("boom"));
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
        mockService.Setup(s => s.GetSheetInfo()).ReturnsAsync(spreadsheet);
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<List<string>>())).ReturnsAsync(spreadsheet);

        var captured = new List<BatchUpdateSpreadsheetRequest>();
        mockService.Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>()))
            .Callback<BatchUpdateSpreadsheetRequest>(r => captured.Add(r))
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
        mockService.Setup(s => s.GetSheetInfo()).ReturnsAsync(spreadsheet);
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<List<string>>())).ReturnsAsync(spreadsheet);

        var captured = new List<BatchUpdateSpreadsheetRequest>();
        mockService.Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>()))
            .Callback<BatchUpdateSpreadsheetRequest>(r => captured.Add(r))
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

        mockService.Verify(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>()), Times.Never);
        mockService.Verify(s => s.GetSheetInfo(), Times.Never);
    }

    [Fact]
    public async Task GetSheets_SelfHealsMissingSheet_RefreshesAlreadyExistingDependentSheet()
    {
        var mockService = new Mock<IGoogleSheetService>();

        // batchGet fails (Base is missing entirely) -> triggers the self-heal path.
        mockService.Setup(s => s.GetBatchData(It.IsAny<List<string>>(), It.IsAny<string>()))
            .ReturnsAsync((BatchGetValuesByDataFilterResponse?)null);
        mockService
            .Setup(s => s.GetBatchDataResult(It.IsAny<List<string>>(), It.IsAny<string>()))
            .ReturnsAsync(GoogleApiResult<BatchGetValuesByDataFilterResponse>.Failed(new GoogleApiFailure { Reason = GoogleApiFailureReason.Unknown, Message = "test failure" }));

        // Dependent already exists live; Base doesn't yet - matches the real "Tickers deleted,
        // Stocks still references it" scenario.
        var spreadsheet = SpreadsheetWith((DependentSheetName, 42));
        mockService.Setup(s => s.GetSheetInfo()).ReturnsAsync(spreadsheet);
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<List<string>>())).ReturnsAsync(spreadsheet);

        var captured = new List<BatchUpdateSpreadsheetRequest>();
        mockService.Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>()))
            .Callback<BatchUpdateSpreadsheetRequest>(r => captured.Add(r))
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
        mockService.Setup(s => s.GetBatchData(It.IsAny<List<string>>(), It.IsAny<string>())).ReturnsAsync(response);
        mockService
            .Setup(s => s.GetBatchDataResult(It.IsAny<List<string>>(), It.IsAny<string>()))
            .ReturnsAsync(GoogleApiResult<BatchGetValuesByDataFilterResponse>.Ok(response));

        var spreadsheet = SpreadsheetWith((BaseSheetName, 10), (DependentSheetName, 42));
        mockService.Setup(s => s.GetSheetInfo()).ReturnsAsync(spreadsheet);
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<List<string>>())).ReturnsAsync(spreadsheet);

        var captured = new List<BatchUpdateSpreadsheetRequest>();
        mockService.Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>()))
            .Callback<BatchUpdateSpreadsheetRequest>(r => captured.Add(r))
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
        mockService.Setup(s => s.GetSheetInfo()).ReturnsAsync(spreadsheet);
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<List<string>>())).ReturnsAsync(spreadsheet);

        var captured = new List<BatchUpdateSpreadsheetRequest>();
        mockService.Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>()))
            .Callback<BatchUpdateSpreadsheetRequest>(r => captured.Add(r))
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
}
