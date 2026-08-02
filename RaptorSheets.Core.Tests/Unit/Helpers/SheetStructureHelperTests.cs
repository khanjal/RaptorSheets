using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Helpers;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Helpers;

public class SheetStructureHelperTests
{
    // Mirrors what a real IncludeGridData=true response looks like: header text/note/formula live on
    // row 0, but format/validation are only ever written starting at row 1 (see
    // GoogleRequestHelpers.GenerateRepeatCellRequest's GridRange.StartRowIndex = 1) - never on the
    // header cell itself. formatCells is optional since several tests don't care about format/validation.
    private static Sheet BuildSheet(int sheetId, string title, IList<CellData> headerCells, IList<CellData>? formatCells = null, GridProperties? gridProperties = null, IList<ProtectedRange>? protectedRanges = null, Color? tabColor = null)
    {
        var rowData = new List<RowData> { new() { Values = headerCells } };
        if (formatCells != null)
        {
            rowData.Add(new RowData { Values = formatCells });
        }

        return new Sheet
        {
            Properties = new SheetProperties
            {
                SheetId = sheetId,
                Title = title,
                GridProperties = gridProperties,
                TabColor = tabColor
            },
            ProtectedRanges = protectedRanges,
            Data = [new GridData { RowData = rowData }]
        };
    }

    [Fact]
    public void ParseSheetStructure_ParsesHeaderNamesAndPositions()
    {
        var sheet = BuildSheet(1, "Trips", [
            new CellData { FormattedValue = "Date" },
            new CellData { FormattedValue = "Pay" }
        ]);

        var result = SheetStructureHelper.ParseSheetStructure(sheet);

        Assert.Equal(1, result.Id);
        Assert.Equal("Trips", result.Name);
        Assert.Equal(2, result.Headers.Count);
        Assert.Equal("Date", result.Headers[0].Name);
        Assert.Equal(0, result.Headers[0].Index);
        Assert.Equal("A", result.Headers[0].Column);
        Assert.Equal("Pay", result.Headers[1].Name);
        Assert.Equal(1, result.Headers[1].Index);
        Assert.Equal("B", result.Headers[1].Column);
    }

    [Fact]
    public void ParseSheetStructure_SkipsCellsWithNoFormattedValue_ButPreservesPositionOfLaterHeaders()
    {
        var sheet = BuildSheet(1, "Trips", [
            new CellData { FormattedValue = "Date" },
            new CellData { FormattedValue = null },
            new CellData { FormattedValue = "Pay" }
        ]);

        var result = SheetStructureHelper.ParseSheetStructure(sheet);

        Assert.Equal(2, result.Headers.Count);
        Assert.Equal(0, result.Headers[0].Index);
        Assert.Equal(2, result.Headers[1].Index);
        Assert.Equal("C", result.Headers[1].Column);
    }

    [Fact]
    public void ParseSheetStructure_WithNoGridData_ReturnsEmptyHeaders()
    {
        var sheet = new Sheet { Properties = new SheetProperties { SheetId = 1, Title = "Trips" }, Data = [] };

        var result = SheetStructureHelper.ParseSheetStructure(sheet);

        Assert.Empty(result.Headers);
    }

    [Fact]
    public void ParseSheetStructure_FormatOnHeaderRowItself_IsIgnored()
    {
        // Regression guard: a real sheet never has UserEnteredFormat.NumberFormat on the header cell
        // (only on row 1+) - if a caller only fetched row 0, format/validation must read as absent,
        // not accidentally pick up something that happens to be on the header cell in a test fixture.
        var sheet = BuildSheet(1, "Trips", [
            new CellData
            {
                FormattedValue = "Date",
                UserEnteredFormat = new CellFormat { NumberFormat = new NumberFormat { Type = CellFormatPatterns.CellFormatDate, Pattern = CellFormatPatterns.Date } },
                DataValidation = new DataValidationRule { Condition = new BooleanCondition { Type = "BOOLEAN" } }
            }
        ]);

        var result = SheetStructureHelper.ParseSheetStructure(sheet);

        Assert.Null(result.Headers[0].Format);
        Assert.Null(result.Headers[0].RawFormatType);
        Assert.Equal("", result.Headers[0].Validation);
    }

    [Theory]
    [InlineData(CellFormatPatterns.CellFormatDate, CellFormatPatterns.Date, Format.DATE)]
    [InlineData(CellFormatPatterns.CellFormatDate, CellFormatPatterns.Weekday, Format.WEEKDAY)]
    [InlineData(CellFormatPatterns.CellFormatDate, CellFormatPatterns.Duration, Format.DURATION)]
    [InlineData(CellFormatPatterns.CellFormatDate, CellFormatPatterns.Time, Format.TIME)]
    [InlineData(CellFormatPatterns.CellFormatNumber, CellFormatPatterns.Accounting, Format.ACCOUNTING)]
    [InlineData(CellFormatPatterns.CellFormatNumber, CellFormatPatterns.Currency, Format.CURRENCY)]
    [InlineData(CellFormatPatterns.CellFormatNumber, CellFormatPatterns.Distance, Format.DISTANCE)]
    [InlineData(CellFormatPatterns.CellFormatNumber, CellFormatPatterns.Number, Format.NUMBER)]
    [InlineData(CellFormatPatterns.CellFormatText, null, Format.TEXT)]
    public void ParseSheetStructure_ResolvesFormatFromExactTypeAndPattern_EvenWhenTypeIsSharedAmbiguously(string type, string? pattern, Format expected)
    {
        var sheet = BuildSheet(1, "Trips",
            headerCells: [new CellData { FormattedValue = "Column" }],
            formatCells: [new CellData { UserEnteredFormat = new CellFormat { NumberFormat = new NumberFormat { Type = type, Pattern = pattern } } }]);

        var result = SheetStructureHelper.ParseSheetStructure(sheet);

        Assert.Equal(expected, result.Headers[0].Format);
        Assert.Equal(type, result.Headers[0].RawFormatType);
        Assert.Equal(pattern, result.Headers[0].FormatPattern);
    }

    [Fact]
    public void ParseSheetStructure_WithCustomPattern_LeavesFormatNullButKeepsRawTypeAndPattern()
    {
        var sheet = BuildSheet(1, "Trips",
            headerCells: [new CellData { FormattedValue = "Column" }],
            formatCells: [new CellData { UserEnteredFormat = new CellFormat { NumberFormat = new NumberFormat { Type = CellFormatPatterns.CellFormatNumber, Pattern = "#,##0.0000" } } }]);

        var result = SheetStructureHelper.ParseSheetStructure(sheet);

        Assert.Null(result.Headers[0].Format);
        Assert.Equal(CellFormatPatterns.CellFormatNumber, result.Headers[0].RawFormatType);
        Assert.Equal("#,##0.0000", result.Headers[0].FormatPattern);
    }

    [Fact]
    public void ParseSheetStructure_WithNoNumberFormat_LeavesFormatFieldsNull()
    {
        var sheet = BuildSheet(1, "Trips", [new CellData { FormattedValue = "Column" }]);

        var result = SheetStructureHelper.ParseSheetStructure(sheet);

        Assert.Null(result.Headers[0].Format);
        Assert.Null(result.Headers[0].RawFormatType);
        Assert.Null(result.Headers[0].FormatPattern);
    }

    [Fact]
    public void ParseSheetStructure_WithNoFormatRow_LeavesFormatFieldsNull()
    {
        // A caller that only fetched row 0 (e.g. a header-only range) has no row 1 at all.
        var sheet = new Sheet
        {
            Properties = new SheetProperties { SheetId = 1, Title = "Trips" },
            Data = [new GridData { RowData = [new RowData { Values = [new CellData { FormattedValue = "Column" }] }] }]
        };

        var result = SheetStructureHelper.ParseSheetStructure(sheet);

        Assert.Null(result.Headers[0].Format);
        Assert.Equal("", result.Headers[0].Validation);
    }

    [Fact]
    public void ParseSheetStructure_BooleanValidation_BuildsTypeOnlyDescription()
    {
        var sheet = BuildSheet(1, "Trips",
            headerCells: [new CellData { FormattedValue = "Active" }],
            formatCells: [new CellData { DataValidation = new DataValidationRule { Condition = new BooleanCondition { Type = "BOOLEAN" } } }]);

        var result = SheetStructureHelper.ParseSheetStructure(sheet);

        Assert.Equal("BOOLEAN", result.Headers[0].Validation);
    }

    [Fact]
    public void ParseSheetStructure_OneOfRangeValidation_BuildsTypeAndValuesDescription()
    {
        var sheet = BuildSheet(1, "Trips",
            headerCells: [new CellData { FormattedValue = "Service" }],
            formatCells:
            [
                new CellData
                {
                    DataValidation = new DataValidationRule
                    {
                        Condition = new BooleanCondition
                        {
                            Type = "ONE_OF_RANGE",
                            Values = [new ConditionValue { UserEnteredValue = "=Services!A2:A" }]
                        }
                    }
                }
            ]);

        var result = SheetStructureHelper.ParseSheetStructure(sheet);

        Assert.Equal("ONE_OF_RANGE:=Services!A2:A", result.Headers[0].Validation);
    }

    [Fact]
    public void ParseSheetStructure_WithNoValidation_LeavesValidationEmpty()
    {
        var sheet = BuildSheet(1, "Trips", [new CellData { FormattedValue = "Column" }]);

        var result = SheetStructureHelper.ParseSheetStructure(sheet);

        Assert.Equal("", result.Headers[0].Validation);
    }

    [Fact]
    public void ParseSheetStructure_ReadsNoteAndFormula()
    {
        var sheet = BuildSheet(1, "Trips", [
            new CellData
            {
                FormattedValue = "Total",
                Note = "Sum of all trips",
                UserEnteredValue = new ExtendedValue { FormulaValue = "=SUM(A:A)" }
            }
        ]);

        var result = SheetStructureHelper.ParseSheetStructure(sheet);

        Assert.Equal("Sum of all trips", result.Headers[0].Note);
        Assert.Equal("=SUM(A:A)", result.Headers[0].Formula);
    }

    [Fact]
    public void ParseSheetStructure_WithNoFormula_LeavesFormulaEmpty()
    {
        var sheet = BuildSheet(1, "Trips", [new CellData { FormattedValue = "Date", UserEnteredValue = new ExtendedValue { StringValue = "Date" } }]);

        var result = SheetStructureHelper.ParseSheetStructure(sheet);

        Assert.Equal("", result.Headers[0].Formula);
    }

    [Fact]
    public void ParseSheetStructure_ReadsFreezeRowAndColumnCounts()
    {
        var sheet = BuildSheet(1, "Trips", [new CellData { FormattedValue = "Date" }],
            gridProperties: new GridProperties { FrozenRowCount = 1, FrozenColumnCount = 2 });

        var result = SheetStructureHelper.ParseSheetStructure(sheet);

        Assert.Equal(1, result.FreezeRowCount);
        Assert.Equal(2, result.FreezeColumnCount);
    }

    [Fact]
    public void ParseSheetStructure_WithNoGridProperties_DefaultsFreezeCountsToZero()
    {
        var sheet = BuildSheet(1, "Trips", [new CellData { FormattedValue = "Date" }]);

        var result = SheetStructureHelper.ParseSheetStructure(sheet);

        Assert.Equal(0, result.FreezeRowCount);
        Assert.Equal(0, result.FreezeColumnCount);
    }

    [Fact]
    public void ParseSheetStructure_ResolvesTabColorFromKnownPalette()
    {
        var sheet = BuildSheet(1, "Trips", [new CellData { FormattedValue = "Date" }], tabColor: Colors.Orange);

        var result = SheetStructureHelper.ParseSheetStructure(sheet);

        Assert.Equal(SheetColor.ORANGE, result.TabColor);
    }

    [Fact]
    public void ParseSheetStructure_WithUnrecognizedTabColor_LeavesTabColorAtDefault()
    {
        var sheet = BuildSheet(1, "Trips", [new CellData { FormattedValue = "Date" }], tabColor: new Color { Red = (float?)0.123, Green = (float?)0.456, Blue = (float?)0.789 });

        var result = SheetStructureHelper.ParseSheetStructure(sheet);

        Assert.Equal(default, result.TabColor);
    }

    [Fact]
    public void ParseSheetStructure_ResolvesFontColorFromFirstHeaderCellWithForeground()
    {
        var sheet = BuildSheet(1, "Trips", [
            new CellData { FormattedValue = "Date", UserEnteredFormat = new CellFormat { TextFormat = new TextFormat { ForegroundColor = Colors.White } } },
            new CellData { FormattedValue = "Pay", UserEnteredFormat = new CellFormat { TextFormat = new TextFormat { ForegroundColor = Colors.White } } }
        ]);

        var result = SheetStructureHelper.ParseSheetStructure(sheet);

        Assert.Equal(SheetColor.WHITE, result.FontColor);
    }

    // CellColor is only ever used at write time for alternating-row banding (a per-sheet
    // BandedRange, not a per-cell CellData scalar) - there is no live signal to read it back from,
    // so this documents the gap explicitly rather than leaving it silently untested.
    [Fact]
    public void ParseSheetStructure_NeverPopulatesCellColor()
    {
        var sheet = BuildSheet(1, "Trips", [new CellData { FormattedValue = "Date" }]);

        var result = SheetStructureHelper.ParseSheetStructure(sheet);

        Assert.Equal(default, result.CellColor);
    }

    [Fact]
    public void ParseSheetStructure_WithUnboundedProtectedRange_SetsProtectSheetTrue()
    {
        var sheet = BuildSheet(1, "Trips", [new CellData { FormattedValue = "Date" }],
            protectedRanges: [new ProtectedRange { Range = new GridRange { SheetId = 1 } }]);

        var result = SheetStructureHelper.ParseSheetStructure(sheet);

        Assert.True(result.ProtectSheet);
    }

    [Fact]
    public void ParseSheetStructure_WithBoundedProtectedRange_LeavesProtectSheetFalse()
    {
        var sheet = BuildSheet(1, "Trips", [
            new CellData { FormattedValue = "Date" },
            new CellData { FormattedValue = "Formula" }
        ], protectedRanges: [new ProtectedRange { Range = new GridRange { SheetId = 1, StartRowIndex = 0, EndRowIndex = 1, StartColumnIndex = 1, EndColumnIndex = 2 } }]);

        var result = SheetStructureHelper.ParseSheetStructure(sheet);

        Assert.False(result.ProtectSheet);
    }

    [Fact]
    public void ParseSheetStructure_MarksOnlyHeaderCellsInsideBoundedProtectedRange()
    {
        var sheet = BuildSheet(1, "Trips", [
            new CellData { FormattedValue = "Date" },
            new CellData { FormattedValue = "Formula" }
        ], protectedRanges: [new ProtectedRange { Range = new GridRange { SheetId = 1, StartRowIndex = 0, EndRowIndex = 1, StartColumnIndex = 1, EndColumnIndex = 2 } }]);

        var result = SheetStructureHelper.ParseSheetStructure(sheet);

        Assert.False(result.Headers[0].Protect);
        Assert.True(result.Headers[1].Protect);
    }

    [Fact]
    public void ParseSheetStructures_SkipsSheetsNotInRequestedNames()
    {
        var spreadsheet = new Spreadsheet
        {
            Sheets =
            [
                BuildSheet(1, "Trips", [new CellData { FormattedValue = "Date" }]),
                BuildSheet(2, "Shifts", [new CellData { FormattedValue = "Date" }])
            ]
        };

        var result = SheetStructureHelper.ParseSheetStructures(spreadsheet, ["Trips"]);

        Assert.Single(result);
        Assert.True(result.ContainsKey("Trips"));
    }

    [Fact]
    public void ParseSheetStructures_SkipsRequestedNamesNotInSpreadsheet()
    {
        var spreadsheet = new Spreadsheet { Sheets = [BuildSheet(1, "Trips", [new CellData { FormattedValue = "Date" }])] };

        var result = SheetStructureHelper.ParseSheetStructures(spreadsheet, ["Trips", "Unknown"]);

        Assert.Single(result);
    }

    [Fact]
    public void ParseSheetStructures_WithNullSheets_ReturnsEmpty()
    {
        var spreadsheet = new Spreadsheet { Sheets = null };

        var result = SheetStructureHelper.ParseSheetStructures(spreadsheet, ["Trips"]);

        Assert.Empty(result);
    }

    [Fact]
    public void ParseSheetStructures_LookupIsCaseInsensitive()
    {
        var spreadsheet = new Spreadsheet { Sheets = [BuildSheet(1, "Trips", [new CellData { FormattedValue = "Date" }])] };

        var result = SheetStructureHelper.ParseSheetStructures(spreadsheet, ["TRIPS"]);

        Assert.True(result.ContainsKey("Trips"));
    }
}
