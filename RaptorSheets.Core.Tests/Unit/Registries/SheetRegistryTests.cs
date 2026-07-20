using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Core.Registries;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Registries;

public class SheetRegistryTests
{
    // Minimal ISheetEntity implementation, standing in for a domain's real SheetEntity
    // (RaptorSheets.Gig.Entities.SheetEntity / RaptorSheets.Stock.Entities.SheetEntity), which
    // Core.Tests can't reference directly.
    private class TestSheetEntity : ISheetEntity
    {
        public PropertyEntity Properties { get; set; } = new();
        public List<MessageEntity> Messages { get; set; } = [];
        public List<TestRowEntity> Widgets { get; set; } = [];
    }

    private class TestRowEntity
    {
        public int RowId { get; set; }

        [Column("Name", isInput: true)]
        public string Name { get; set; } = "";
    }

    private static SheetModel TestSheetModel() => new()
    {
        Name = "Widgets",
        Headers = [new SheetCellModel { Name = "Name" }]
    };

    private static BatchGetValuesByDataFilterResponse BuildBatchResponse(string sheetName, IList<object> headerRow, IList<object>? dataRow = null)
    {
        var values = new List<IList<object>> { headerRow };
        if (dataRow != null)
        {
            values.Add(dataRow);
        }

        return new BatchGetValuesByDataFilterResponse
        {
            ValueRanges =
            [
                new MatchedValueRange
                {
                    DataFilters = [new DataFilter { A1Range = sheetName }],
                    ValueRange = new ValueRange { Values = values }
                }
            ]
        };
    }

    [Fact]
    public void Register_ThenIsRegistered_ShouldReturnTrue()
    {
        var registry = new SheetRegistry<TestSheetEntity>();

        registry.Register("Widgets", TestSheetModel, (_, _) => { });

        Assert.True(registry.IsRegistered("Widgets"));
        Assert.False(registry.IsRegistered("Unknown"));
    }

    [Fact]
    public void Register_IsCaseInsensitive()
    {
        var registry = new SheetRegistry<TestSheetEntity>();

        registry.Register("Widgets", TestSheetModel, (_, _) => { });

        Assert.True(registry.IsRegistered("widgets"));
        Assert.True(registry.IsRegistered("WIDGETS"));
    }

    [Fact]
    public void Register_WithNullFactoryOrProcessor_ShouldThrow()
    {
        var registry = new SheetRegistry<TestSheetEntity>();

        Assert.Throws<ArgumentNullException>(() => registry.Register("Widgets", null!, (_, _) => { }));
        Assert.Throws<ArgumentNullException>(() => registry.Register("Widgets", TestSheetModel, null!));
    }

    [Fact]
    public void MapData_BatchResponse_WithNullValueRanges_ShouldReturnNull()
    {
        var registry = new SheetRegistry<TestSheetEntity>();
        var response = new BatchGetValuesByDataFilterResponse { ValueRanges = null };

        var result = registry.MapData(response);

        Assert.Null(result);
    }

    [Fact]
    public void MapData_BatchResponse_WithRegisteredSheet_ShouldInvokeProcessor()
    {
        var registry = new SheetRegistry<TestSheetEntity>();
        registry.Register("Widgets", TestSheetModel, (entity, values) =>
        {
            entity.Widgets = [new TestRowEntity { Name = values[1][0].ToString()! }];
        });

        var response = BuildBatchResponse("Widgets", ["Name"], ["Sprocket"]);

        var result = registry.MapData(response);

        Assert.NotNull(result);
        Assert.Single(result.Widgets);
        Assert.Equal("Sprocket", result.Widgets[0].Name);
    }

    [Fact]
    public void MapData_BatchResponse_WithUnregisteredSheetName_ShouldBeIgnored()
    {
        var registry = new SheetRegistry<TestSheetEntity>();
        registry.Register("Widgets", TestSheetModel, (_, _) => throw new InvalidOperationException("should not be called"));

        var response = BuildBatchResponse("SomeOtherSheet", ["Name"], ["Sprocket"]);

        var result = registry.MapData(response);

        Assert.NotNull(result);
        Assert.Empty(result.Widgets);
    }

    [Fact]
    public void MapData_BatchResponse_WithEmptyValues_ShouldSkipProcessor()
    {
        var registry = new SheetRegistry<TestSheetEntity>();
        var wasCalled = false;
        registry.Register("Widgets", TestSheetModel, (_, _) => wasCalled = true);

        var response = new BatchGetValuesByDataFilterResponse
        {
            ValueRanges =
            [
                new MatchedValueRange
                {
                    DataFilters = [new DataFilter { A1Range = "Widgets" }],
                    ValueRange = new ValueRange { Values = new List<IList<object>>() }
                }
            ]
        };

        registry.MapData(response);

        Assert.False(wasCalled);
    }

    [Fact]
    public void MapData_Spreadsheet_ShouldSetPropertiesNameAndInvokeProcessor()
    {
        var registry = new SheetRegistry<TestSheetEntity>();
        registry.Register("Widgets", TestSheetModel, (entity, values) =>
        {
            entity.Widgets = [new TestRowEntity { Name = values[1][0].ToString()! }];
        });

        var spreadsheet = new Spreadsheet
        {
            Properties = new SpreadsheetProperties { Title = "MySpreadsheet" },
            Sheets =
            [
                new Sheet
                {
                    Properties = new SheetProperties { Title = "Widgets" },
                    Data =
                    [
                        new GridData
                        {
                            RowData =
                            [
                                new RowData { Values = [new CellData { FormattedValue = "Name" }] },
                                new RowData { Values = [new CellData { FormattedValue = "Sprocket" }] }
                            ]
                        }
                    ]
                }
            ]
        };

        var result = registry.MapData(spreadsheet);

        Assert.Equal("MySpreadsheet", result.Properties.Name);
        Assert.Single(result.Widgets);
        Assert.Equal("Sprocket", result.Widgets[0].Name);
    }

    [Fact]
    public void GetMissingSheets_WithSheetNotInSpreadsheet_ShouldReturnItsSheetModel()
    {
        var registry = new SheetRegistry<TestSheetEntity>();
        registry.Register("Widgets", TestSheetModel, (_, _) => { });

        var spreadsheet = new Spreadsheet { Sheets = [] };

        var missing = registry.GetMissingSheets(spreadsheet, ["Widgets"]);

        Assert.Single(missing);
        Assert.Equal("Widgets", missing[0].Name);
    }

    [Fact]
    public void GetMissingSheets_WithSheetAlreadyPresent_ShouldNotReturnIt()
    {
        var registry = new SheetRegistry<TestSheetEntity>();
        registry.Register("Widgets", TestSheetModel, (_, _) => { });

        var spreadsheet = new Spreadsheet
        {
            Sheets = [new Sheet { Properties = new SheetProperties { Title = "widgets" } }]
        };

        var missing = registry.GetMissingSheets(spreadsheet, ["Widgets"]);

        Assert.Empty(missing);
    }

    [Fact]
    public void RegisterGeneric_ShouldCheckHeadersAndMapRowsViaGenericSheetMapper()
    {
        var registry = new SheetRegistry<TestSheetEntity>();

        registry.RegisterGeneric<TestSheetEntity, TestRowEntity>("Widgets", TestSheetModel, (entity, rows) => entity.Widgets = rows);

        var response = BuildBatchResponse("Widgets", ["Name"], ["Sprocket"]);

        var result = registry.MapData(response);

        Assert.NotNull(result);
        Assert.Single(result.Widgets);
        Assert.Equal("Sprocket", result.Widgets[0].Name);
        Assert.Equal(2, result.Widgets[0].RowId);
    }

    [Fact]
    public void RegisterGeneric_WithMismatchedHeader_ShouldAddHeaderCheckMessage()
    {
        var registry = new SheetRegistry<TestSheetEntity>();
        registry.RegisterGeneric<TestSheetEntity, TestRowEntity>("Widgets", TestSheetModel, (entity, rows) => entity.Widgets = rows);

        // "WrongHeader" doesn't match the expected "Name" header from TestSheetModel
        var response = BuildBatchResponse("Widgets", ["WrongHeader"], ["Sprocket"]);

        var result = registry.MapData(response);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Messages);
    }

    [Fact]
    public void CheckUnknownSheets_WithNullSpreadsheet_ShouldReturnErrorMessage()
    {
        var registry = new SheetRegistry<TestSheetEntity>();

        var result = registry.CheckUnknownSheets(null!);

        Assert.Single(result);
        Assert.Contains("Unable to retrieve sheet(s)", result[0].Message);
    }

    [Fact]
    public void CheckUnknownSheets_WithUnregisteredTab_ShouldWarn()
    {
        var registry = new SheetRegistry<TestSheetEntity>();
        registry.Register("Widgets", TestSheetModel, (_, _) => { });

        var spreadsheet = new Spreadsheet
        {
            Sheets =
            [
                new Sheet { Properties = new SheetProperties { Title = "Widgets" } },
                new Sheet { Properties = new SheetProperties { Title = "Gadgets" } }
            ]
        };

        var result = registry.CheckUnknownSheets(spreadsheet);

        Assert.Contains(result, m => m.Message.Contains("Gadgets") && m.Message.Contains("does not match any known sheet name"));
    }

    [Fact]
    public void CheckSheetHeaders_WithNullSpreadsheet_ShouldReturnErrorMessage()
    {
        var registry = new SheetRegistry<TestSheetEntity>();

        var result = registry.CheckSheetHeaders(null!);

        Assert.Single(result);
        Assert.Contains("Unable to retrieve sheet(s)", result[0].Message);
    }

    [Fact]
    public void CheckSheetHeaders_WithMatchingHeader_ShouldReportNoIssues()
    {
        var registry = new SheetRegistry<TestSheetEntity>();
        registry.Register("Widgets", TestSheetModel, (_, _) => { });

        var spreadsheet = new Spreadsheet
        {
            Sheets =
            [
                new Sheet
                {
                    Properties = new SheetProperties { Title = "Widgets" },
                    Data =
                    [
                        new GridData
                        {
                            RowData = [new RowData { Values = [new CellData { FormattedValue = "Name" }] }]
                        }
                    ]
                }
            ]
        };

        var result = registry.CheckSheetHeaders(spreadsheet);

        Assert.Contains(result, m => m.Message.Contains("No sheet header issues found"));
    }

    [Fact]
    public void CheckSheetHeaders_WithMismatchedHeader_ShouldReportIssue()
    {
        var registry = new SheetRegistry<TestSheetEntity>();
        registry.Register("Widgets", TestSheetModel, (_, _) => { });

        var spreadsheet = new Spreadsheet
        {
            Sheets =
            [
                new Sheet
                {
                    Properties = new SheetProperties { Title = "Widgets" },
                    Data =
                    [
                        new GridData
                        {
                            RowData = [new RowData { Values = [new CellData { FormattedValue = "WrongHeader" }] }]
                        }
                    ]
                }
            ]
        };

        var result = registry.CheckSheetHeaders(spreadsheet);

        Assert.Contains(result, m => m.Message.Contains("Found sheet header issue(s)"));
    }

    [Fact]
    public void GetSheetLayout_WithRegisteredSheet_ReturnsModel()
    {
        var registry = new SheetRegistry<TestSheetEntity>();
        registry.Register("Widgets", TestSheetModel, (_, _) => { });

        var layout = registry.GetSheetLayout("Widgets");

        Assert.NotNull(layout);
        Assert.Equal("Widgets", layout.Name);
    }

    [Fact]
    public void GetSheetLayout_WithUnknownSheet_ReturnsNull()
    {
        var registry = new SheetRegistry<TestSheetEntity>();

        Assert.Null(registry.GetSheetLayout("Unknown"));
        Assert.Null(registry.GetSheetLayout(""));
    }

    [Fact]
    public void GetSheetLayouts_WithMixedNames_ReturnsOnlyRegistered()
    {
        var registry = new SheetRegistry<TestSheetEntity>();
        registry.Register("Widgets", TestSheetModel, (_, _) => { });

        var layouts = registry.GetSheetLayouts(["Widgets", "Unknown"]);

        Assert.Single(layouts);
        Assert.Equal("Widgets", layouts[0].Name);
    }
}
