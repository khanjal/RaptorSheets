using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Helpers;

public class SheetPropertyHelperTests
{
    [Fact]
    public void BuildCombinedRanges_ReturnsHeaderAndRowRangePerSheet()
    {
        var result = SheetPropertyHelper.BuildCombinedRanges(["Trips", "Shifts"]);

        Assert.Equal(4, result.Count);
        Assert.Contains($"Trips!{GoogleConfig.HeaderRange}", result);
        Assert.Contains($"Trips!{GoogleConfig.RowRange}", result);
        Assert.Contains($"Shifts!{GoogleConfig.HeaderRange}", result);
        Assert.Contains($"Shifts!{GoogleConfig.RowRange}", result);
    }

    [Fact]
    public void BuildCombinedRanges_WithEmptyList_ReturnsEmpty()
    {
        var result = SheetPropertyHelper.BuildCombinedRanges([]);

        Assert.Empty(result);
    }

    [Fact]
    public void ProcessSheetData_WithSheetNotFound_ReturnsDefaultProperty()
    {
        var spreadsheet = new Spreadsheet { Sheets = [] };

        var result = SheetPropertyHelper.ProcessSheetData("Trips", spreadsheet);

        Assert.Equal("Trips", result.Name);
        Assert.Equal("", result.Id);
        Assert.Equal("1000", result.Attributes[PropertyEnum.MAX_ROW.GetDescription()]);
        Assert.Equal("1", result.Attributes[PropertyEnum.MAX_ROW_VALUE.GetDescription()]);
        Assert.Equal("", result.Attributes[PropertyEnum.HEADERS.GetDescription()]);
    }

    [Fact]
    public void ProcessSheetData_WithNullSpreadsheet_ReturnsDefaultProperty()
    {
        var result = SheetPropertyHelper.ProcessSheetData("Trips", null);

        Assert.Equal("Trips", result.Name);
        Assert.Equal("", result.Id);
    }

    [Fact]
    public void ProcessSheetData_WithFoundSheet_PopulatesFromRealData()
    {
        var sheet = new Sheet
        {
            Properties = new SheetProperties { Title = "Trips", SheetId = 42, GridProperties = new GridProperties { RowCount = 500 } },
            Data =
            [
                new GridData
                {
                    RowData =
                    [
                        new RowData { Values = [new CellData { FormattedValue = "Date" }, new CellData { FormattedValue = "Pay" }] },
                        new RowData { Values = [new CellData { FormattedValue = "2024-01-01" }] }
                    ]
                }
            ]
        };
        var spreadsheet = new Spreadsheet { Sheets = [sheet] };

        var result = SheetPropertyHelper.ProcessSheetData("Trips", spreadsheet);

        Assert.Equal("42", result.Id);
        Assert.Equal("500", result.Attributes[PropertyEnum.MAX_ROW.GetDescription()]);
        Assert.Equal("Date,Pay", result.Attributes[PropertyEnum.HEADERS.GetDescription()]);
    }

    [Fact]
    public void PopulateSheetBasicInfo_SetsIdAndMaxRow()
    {
        var property = new PropertyEntity();
        var sheetData = new Sheet
        {
            Properties = new SheetProperties { SheetId = 7, GridProperties = new GridProperties { RowCount = 250 } }
        };

        SheetPropertyHelper.PopulateSheetBasicInfo(property, sheetData);

        Assert.Equal("7", property.Id);
        Assert.Equal("250", property.Attributes[PropertyEnum.MAX_ROW.GetDescription()]);
    }

    [Fact]
    public void PopulateSheetBasicInfo_WithNoGridProperties_DefaultsMaxRowTo1000()
    {
        var property = new PropertyEntity();
        var sheetData = new Sheet { Properties = new SheetProperties { SheetId = 7 } };

        SheetPropertyHelper.PopulateSheetBasicInfo(property, sheetData);

        Assert.Equal("1000", property.Attributes[PropertyEnum.MAX_ROW.GetDescription()]);
    }

    [Fact]
    public void ParseSheetDataRanges_WithNoData_LeavesPropertyUnchanged()
    {
        var property = new PropertyEntity { Name = "Trips" };
        var sheetData = new Sheet { Data = [] };

        SheetPropertyHelper.ParseSheetDataRanges(property, sheetData);

        Assert.Empty(property.Attributes);
    }

    [Fact]
    public void ParseSheetDataRanges_SkipsRangesWithNoRowData()
    {
        var property = new PropertyEntity { Name = "Trips" };
        var sheetData = new Sheet { Data = [new GridData { RowData = [] }, new GridData { RowData = null }] };

        SheetPropertyHelper.ParseSheetDataRanges(property, sheetData);

        Assert.Empty(property.Attributes);
    }

    [Fact]
    public void ParseSheetDataRanges_ProcessesEachValidRange()
    {
        var property = new PropertyEntity { Name = "Trips" };
        var sheetData = new Sheet
        {
            Data =
            [
                new GridData { RowData = [new RowData { Values = [new CellData { FormattedValue = "A" }, new CellData { FormattedValue = "B" }] }] }
            ]
        };

        SheetPropertyHelper.ParseSheetDataRanges(property, sheetData);

        Assert.Equal("A,B", property.Attributes[PropertyEnum.HEADERS.GetDescription()]);
    }

    [Theory]
    [InlineData(1, 1, RangeType.Unknown)]
    [InlineData(1, 2, RangeType.HeadersOnly)]
    [InlineData(2, 1, RangeType.ColumnDataOnly)]
    [InlineData(2, 2, RangeType.FullRange)]
    public void DetermineRangeType_ReturnsExpectedType(int rowCount, int columnCount, RangeType expected)
    {
        var rowData = new List<RowData>();
        for (var i = 0; i < rowCount; i++)
        {
            var values = new List<CellData>();
            for (var j = 0; j < columnCount; j++) values.Add(new CellData());
            rowData.Add(new RowData { Values = values });
        }
        var dataRange = new GridData { RowData = rowData };

        var result = SheetPropertyHelper.DetermineRangeType(dataRange);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void ProcessDataRange_HeadersOnly_ExtractsHeaders()
    {
        var property = new PropertyEntity();
        var dataRange = new GridData { RowData = [new RowData { Values = [new CellData { FormattedValue = "A" }, new CellData { FormattedValue = "B" }] }] };

        SheetPropertyHelper.ProcessDataRange(property, dataRange, RangeType.HeadersOnly);

        Assert.Equal("A,B", property.Attributes[PropertyEnum.HEADERS.GetDescription()]);
    }

    [Fact]
    public void ProcessDataRange_ColumnDataOnly_SetsMaxRowValue()
    {
        var property = new PropertyEntity();
        var dataRange = new GridData
        {
            RowData =
            [
                new RowData { Values = [new CellData { FormattedValue = "Header" }] },
                new RowData { Values = [new CellData { FormattedValue = "Row1" }] },
                new RowData { Values = [new CellData { FormattedValue = "" }] }
            ]
        };

        SheetPropertyHelper.ProcessDataRange(property, dataRange, RangeType.ColumnDataOnly);

        Assert.Equal("2", property.Attributes[PropertyEnum.MAX_ROW_VALUE.GetDescription()]);
    }

    [Fact]
    public void ProcessDataRange_FullRange_ExtractsHeadersAndMaxRowValue()
    {
        var property = new PropertyEntity();
        var dataRange = new GridData
        {
            RowData =
            [
                new RowData { Values = [new CellData { FormattedValue = "A" }, new CellData { FormattedValue = "B" }] },
                new RowData { Values = [new CellData { FormattedValue = "1" }, new CellData { FormattedValue = "2" }] }
            ]
        };

        SheetPropertyHelper.ProcessDataRange(property, dataRange, RangeType.FullRange);

        Assert.Equal("A,B", property.Attributes[PropertyEnum.HEADERS.GetDescription()]);
        Assert.Equal("2", property.Attributes[PropertyEnum.MAX_ROW_VALUE.GetDescription()]);
    }

    [Fact]
    public void ProcessDataRange_Unknown_DoesNothing()
    {
        var property = new PropertyEntity();
        var dataRange = new GridData { RowData = [new RowData { Values = [new CellData()] }] };

        SheetPropertyHelper.ProcessDataRange(property, dataRange, RangeType.Unknown);

        Assert.Empty(property.Attributes);
    }

    [Fact]
    public void ProcessHeadersRange_WithNullValues_ReturnsEarly()
    {
        var property = new PropertyEntity();
        var dataRange = new GridData { RowData = [new RowData { Values = null }] };

        SheetPropertyHelper.ProcessHeadersRange(property, dataRange);

        Assert.Empty(property.Attributes);
    }

    [Fact]
    public void ProcessHeadersRange_FiltersNullFormattedValues()
    {
        var property = new PropertyEntity();
        var dataRange = new GridData
        {
            RowData =
            [
                new RowData
                {
                    Values =
                    [
                        new CellData { FormattedValue = "A" },
                        new CellData { FormattedValue = null },
                        new CellData { FormattedValue = "C" }
                    ]
                }
            ]
        };

        SheetPropertyHelper.ProcessHeadersRange(property, dataRange);

        Assert.Equal("A,C", property.Attributes[PropertyEnum.HEADERS.GetDescription()]);
    }

    [Fact]
    public void ProcessFullRange_WithSingleColumnFirstRow_SkipsHeaderExtraction()
    {
        var property = new PropertyEntity();
        var dataRange = new GridData
        {
            RowData =
            [
                new RowData { Values = [new CellData { FormattedValue = "OnlyOne" }] },
                new RowData { Values = [new CellData { FormattedValue = "Data" }] }
            ]
        };

        SheetPropertyHelper.ProcessFullRange(property, dataRange);

        Assert.False(property.Attributes.ContainsKey(PropertyEnum.HEADERS.GetDescription()));
        Assert.Equal("2", property.Attributes[PropertyEnum.MAX_ROW_VALUE.GetDescription()]);
    }

    [Fact]
    public void FindLastRowWithData_ReturnsLastNonEmptyRowIndexPlusOne()
    {
        var dataRange = new GridData
        {
            RowData =
            [
                new RowData { Values = [new CellData { FormattedValue = "Header" }] },
                new RowData { Values = [new CellData { FormattedValue = "Row1" }] },
                new RowData { Values = [new CellData { FormattedValue = "Row2" }] },
                new RowData { Values = [new CellData { FormattedValue = "" }] }
            ]
        };

        var result = SheetPropertyHelper.FindLastRowWithData(dataRange);

        Assert.Equal(3, result);
    }

    [Fact]
    public void FindLastRowWithData_WithNoDataRows_ReturnsOne()
    {
        var dataRange = new GridData { RowData = [new RowData { Values = [new CellData { FormattedValue = "Header" }] }] };

        var result = SheetPropertyHelper.FindLastRowWithData(dataRange);

        Assert.Equal(1, result);
    }

    [Fact]
    public void FindLastRowWithData_WithEmptyValues_SkipsAndReturnsOne()
    {
        var dataRange = new GridData
        {
            RowData =
            [
                new RowData { Values = [new CellData { FormattedValue = "Header" }] },
                new RowData { Values = [] }
            ]
        };

        var result = SheetPropertyHelper.FindLastRowWithData(dataRange);

        Assert.Equal(1, result);
    }
}
