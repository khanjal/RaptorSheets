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
        // Set via GenericSheetMapper<T>'s cached reflection (typeof(T).GetProperty("RowId")),
        // not directly in this file - invisible to static analysis (S1144/S3459 false positive).
#pragma warning disable S1144, S3459
        public int RowId { get; set; }
#pragma warning restore S1144, S3459

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

    // CheckSheetHeaders(spreadsheet, out missingColumns) - the overload that also detects columns
    // missing entirely, for use with RaptorSheets.Core.Helpers.ColumnInsertionHelper.

    private static SheetModel TestSheetModelWithTwoColumns() => new()
    {
        Name = "Widgets",
        Headers = [new SheetCellModel { Name = "Name" }, new SheetCellModel { Name = "Price" }]
    };

    [Fact]
    public void CheckSheetHeaders_OutParam_WithMissingColumn_PopulatesSheetIdFromSpreadsheet()
    {
        var registry = new SheetRegistry<TestSheetEntity>();
        registry.Register("Widgets", TestSheetModelWithTwoColumns, (_, _) => { });

        var spreadsheet = new Spreadsheet
        {
            Sheets =
            [
                new Sheet
                {
                    Properties = new SheetProperties { Title = "Widgets", SheetId = 99 },
                    Data =
                    [
                        // Only "Name" present - "Price" is missing entirely
                        new GridData { RowData = [new RowData { Values = [new CellData { FormattedValue = "Name" }] }] }
                    ]
                }
            ]
        };

        var messages = registry.CheckSheetHeaders(spreadsheet, out var missingColumns);

        Assert.True(missingColumns.ContainsKey("Widgets"));
        var missing = Assert.Single(missingColumns["Widgets"]);
        Assert.Equal("Price", missing.ColumnName);
        Assert.Equal(1, missing.ColumnIndex);
        Assert.Equal(99, missing.SheetId);
        Assert.Contains(messages, m => m.Message.Contains("Found sheet header issue(s)"));
    }

    [Fact]
    public void CheckSheetHeaders_OutParam_WithNoMissingColumns_ReturnsEmptyDictionary()
    {
        var registry = new SheetRegistry<TestSheetEntity>();
        registry.Register("Widgets", TestSheetModel, (_, _) => { });

        var spreadsheet = new Spreadsheet
        {
            Sheets =
            [
                new Sheet
                {
                    Properties = new SheetProperties { Title = "Widgets", SheetId = 1 },
                    Data = [new GridData { RowData = [new RowData { Values = [new CellData { FormattedValue = "Name" }] }] }]
                }
            ]
        };

        registry.CheckSheetHeaders(spreadsheet, out var missingColumns);

        Assert.Empty(missingColumns);
    }

    [Fact]
    public void CheckSheetHeaders_OutParam_WithNullSpreadsheet_ReturnsEmptyDictionary()
    {
        var registry = new SheetRegistry<TestSheetEntity>();

        var messages = registry.CheckSheetHeaders(null!, out var missingColumns);

        Assert.Empty(missingColumns);
        Assert.Single(messages);
    }

    // GetDependents - backs RefreshDependentSheetsAsync's "sheet B's headers changed, rewrite every
    // sheet whose formulas cross-reference it" behavior (RaptorSheets.Core.Managers.
    // GoogleSheetManagerBase{TEntity}). There's no manual dependency declaration - the graph is
    // derived by building each registered sheet and scanning its headers' Formula text for the
    // changed sheet's cross-sheet range pattern ('{name}'!, per ObjectExtensions.GetRange), so these
    // tests build fixture SheetModels whose Formula literally contains that pattern.

    private static SheetModel SheetModelReferencing(string ownName, params string[] referencedSheetNames) => new()
    {
        Name = ownName,
        Headers = [.. referencedSheetNames.Select(referenced => new SheetCellModel { Name = referenced, Formula = $"=SUM('{referenced}'!A:A)" })]
    };

    [Fact]
    public void GetDependents_WithNoMatchingFormulas_ReturnsEmpty()
    {
        var registry = new SheetRegistry<TestSheetEntity>();
        registry.Register("Tickers", TestSheetModel, (_, _) => { });

        var dependents = registry.GetDependents(["Tickers"]);

        Assert.Empty(dependents);
    }

    [Fact]
    public void GetDependents_WithDirectDependency_ReturnsDependent()
    {
        var registry = new SheetRegistry<TestSheetEntity>();
        registry.Register("Tickers", TestSheetModel, (_, _) => { });
        registry.Register("Stocks", () => SheetModelReferencing("Stocks", "Tickers"), (_, _) => { });

        var dependents = registry.GetDependents(["Tickers"]);

        Assert.Equal(["Stocks"], dependents);
    }

    [Fact]
    public void GetDependents_IsCaseInsensitive()
    {
        var registry = new SheetRegistry<TestSheetEntity>();
        registry.Register("Stocks", () => SheetModelReferencing("Stocks", "Tickers"), (_, _) => { });

        var dependents = registry.GetDependents(["tickers"]);

        Assert.Equal(["Stocks"], dependents);
    }

    [Fact]
    public void GetDependents_WithFormulaMissingThePattern_IsNotADependent()
    {
        // "Tickers" appears in the formula text, but not as a real cross-sheet reference ('Tickers'!)
        // - e.g. a coincidental substring - so it must not be detected as a dependency.
        var registry = new SheetRegistry<TestSheetEntity>();
        registry.Register("Tickers", TestSheetModel, (_, _) => { });
        registry.Register("Notes", () => new SheetModel
        {
            Name = "Notes",
            Headers = [new SheetCellModel { Name = "Comment", Formula = "=\"See Tickers for details\"" }]
        }, (_, _) => { });

        var dependents = registry.GetDependents(["Tickers"]);

        Assert.Empty(dependents);
    }

    [Fact]
    public void GetDependents_WithTransitiveChain_ReturnsFullClosure()
    {
        // Mirrors Gig's Trip -> Shift -> Daily -> Weekday chain: a changed Trip should also
        // surface Daily and Weekday, not just the sheet that directly reads Trip.
        var registry = new SheetRegistry<TestSheetEntity>();
        registry.Register("Trip", TestSheetModel, (_, _) => { });
        registry.Register("Shift", () => SheetModelReferencing("Shift", "Trip"), (_, _) => { });
        registry.Register("Daily", () => SheetModelReferencing("Daily", "Shift"), (_, _) => { });
        registry.Register("Weekday", () => SheetModelReferencing("Weekday", "Daily"), (_, _) => { });

        var dependents = registry.GetDependents(["Trip"]);

        Assert.Equal(["Shift", "Daily", "Weekday"], dependents);
    }

    [Fact]
    public void GetDependents_WithDiamondDependency_DoesNotDuplicateEntries()
    {
        // Mirrors Gig's Region/Service, which both depend on Trip and Shift directly - Shift
        // itself also depends on Trip, so Trip's closure would reach Shift twice without dedup.
        var registry = new SheetRegistry<TestSheetEntity>();
        registry.Register("Trip", TestSheetModel, (_, _) => { });
        registry.Register("Shift", () => SheetModelReferencing("Shift", "Trip"), (_, _) => { });
        registry.Register("Region", () => SheetModelReferencing("Region", "Trip", "Shift"), (_, _) => { });

        var dependents = registry.GetDependents(["Trip"]);

        Assert.Equal(["Shift", "Region"], dependents);
    }

    [Fact]
    public void GetDependents_WithCycle_DoesNotInfiniteLoop()
    {
        var registry = new SheetRegistry<TestSheetEntity>();
        registry.Register("A", () => SheetModelReferencing("A", "B"), (_, _) => { });
        registry.Register("B", () => SheetModelReferencing("B", "A"), (_, _) => { });

        var dependents = registry.GetDependents(["A"]);

        Assert.Equal(["B"], dependents);
    }

    [Fact]
    public void GetDependents_WithMultipleChangedSheets_MergesResults()
    {
        var registry = new SheetRegistry<TestSheetEntity>();
        registry.Register("Applications", TestSheetModel, (_, _) => { });
        registry.Register("Interviews", TestSheetModel, (_, _) => { });
        registry.Register("Companies", () => SheetModelReferencing("Companies", "Applications", "Interviews"), (_, _) => { });

        var dependents = registry.GetDependents(["Applications", "Interviews"]);

        Assert.Equal(["Companies"], dependents);
    }

    [Fact]
    public void GetDependents_ReflectsCurrentFormulaOnEveryCall()
    {
        // The graph is derived fresh each call (never cached), so a mapper's formula changing
        // between calls - the exact scenario this whole feature protects against - is picked up
        // immediately without any re-registration step.
        var registry = new SheetRegistry<TestSheetEntity>();
        var stocksReferencesTickers = false;
        registry.Register("Tickers", TestSheetModel, (_, _) => { });
        registry.Register("Stocks", () => stocksReferencesTickers
            ? SheetModelReferencing("Stocks", "Tickers")
            : TestSheetModel(), (_, _) => { });

        Assert.Empty(registry.GetDependents(["Tickers"]));

        stocksReferencesTickers = true;

        Assert.Equal(["Stocks"], registry.GetDependents(["Tickers"]));
    }

    [Fact]
    public void RegisterGeneric_IsReflectedInGetDependents()
    {
        var registry = new SheetRegistry<TestSheetEntity>();
        registry.RegisterGeneric<TestSheetEntity, TestRowEntity>("Tickers", TestSheetModel, (e, rows) => e.Widgets = rows);
        registry.RegisterGeneric<TestSheetEntity, TestRowEntity>("Stocks", () => SheetModelReferencing("Stocks", "Tickers"), (e, rows) => e.Widgets = rows);

        var dependents = registry.GetDependents(["Tickers"]);

        Assert.Equal(["Stocks"], dependents);
    }
}
