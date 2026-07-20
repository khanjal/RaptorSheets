using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Logging;
using Moq;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Managers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Core.Registries;
using RaptorSheets.Core.Services;
using Xunit;

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
}
