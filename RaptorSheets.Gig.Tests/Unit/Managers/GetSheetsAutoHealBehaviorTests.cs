using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Sheets.v4.Data;
using Moq;
using RaptorSheets.Core.Models;
using RaptorSheets.Core.Services;
using RaptorSheets.Gig.Managers;
using Xunit;

namespace RaptorSheets.Gig.Tests.Unit.Managers;

/// <summary>
/// Covers the <c>autoHeal</c> opt-out added to GetSheets/GetAllSheets/GetSheet (see #113): with
/// write-capable credentials, automatic missing-column self-heal was inserting columns into the live
/// spreadsheet as an unintended side effect of what looks like a read-only call, with no way to
/// disable it. Defaults to true everywhere so every existing caller - source and binary - keeps its
/// current behavior; these tests pin both that default and the opt-out itself, on both the
/// values-only and includeStructure=true paths.
/// </summary>
public class GetSheetsAutoHealBehaviorTests
{
    private static BatchGetValuesByDataFilterResponse BuildBatchResponse(string sheetName, IList<object> headerRow)
    {
        return new BatchGetValuesByDataFilterResponse
        {
            ValueRanges = new List<MatchedValueRange>
            {
                new()
                {
                    DataFilters = new List<DataFilter> { new() { A1Range = sheetName } },
                    ValueRange = new ValueRange { Values = new List<IList<object>> { headerRow } }
                }
            }
        };
    }

    private static Spreadsheet BuildGridDataSpreadsheet(string sheetName, int sheetId, IList<string> headerNames)
    {
        var headerCells = headerNames.Select(h => new CellData { FormattedValue = h }).ToList();

        return new Spreadsheet
        {
            Properties = new SpreadsheetProperties { Title = "MySpreadsheet" },
            Sheets = new List<Sheet>
            {
                new()
                {
                    Properties = new SheetProperties { SheetId = sheetId, Title = sheetName },
                    Data = new List<GridData> { new() { RowData = new List<RowData> { new() { Values = headerCells } } } }
                }
            }
        };
    }

    private static Mock<IGoogleSheetService> BuildValuesOnlyMockWithMissingColumns()
    {
        var mockService = new Mock<IGoogleSheetService>();

        // Real Shifts headers have far more than this one column, so this always trips auto-heal.
        var response = BuildBatchResponse("Shifts", new List<object> { "Date" });

        mockService
            .Setup(s => s.GetBatchDataResult(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(GoogleApiResult<BatchGetValuesByDataFilterResponse>.Ok(response));
        mockService
            .Setup(s => s.GetSheetInfo(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Spreadsheet
            {
                Properties = new SpreadsheetProperties { Title = "MySpreadsheet" },
                Sheets = new List<Sheet>
                {
                    new() { Properties = new SheetProperties { Title = "Shifts", SheetId = 5 } }
                }
            });
        mockService
            .Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BatchUpdateSpreadsheetResponse());

        return mockService;
    }

    [Fact]
    public async Task GetSheets_ValuesOnlyPath_DefaultAutoHeal_DetectsMissingColumns_AndAttemptsInsertion()
    {
        var mockService = BuildValuesOnlyMockWithMissingColumns();
        var manager = new SheetManager(mockService.Object);

        await manager.GetSheets(new List<string> { "Shifts" });

        mockService.Verify(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetSheets_ValuesOnlyPath_WithAutoHealFalse_NeverCallsBatchUpdate()
    {
        var mockService = BuildValuesOnlyMockWithMissingColumns();
        var manager = new SheetManager(mockService.Object);

        // includeStructure has to be passed explicitly to reach the autoHeal parameter - see #113's
        // suggested API shape (GetSheets(sheets, includeStructure, autoHeal: false, ...)).
        var result = await manager.GetSheets(new List<string> { "Shifts" }, includeStructure: false, autoHeal: false);

        mockService.Verify(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        // The read itself must still succeed - autoHeal:false is a read-only guarantee, not a failure.
        Assert.Contains(result.Messages, m => m.Message.Contains("Retrieved sheet(s)"));
    }

    [Fact]
    public async Task GetSheets_StructurePath_WithAutoHealFalse_NeverCallsBatchUpdate()
    {
        var mockService = new Mock<IGoogleSheetService>();

        mockService
            .Setup(s => s.GetSheetInfoResult(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(GoogleApiResult<Spreadsheet>.Ok(BuildGridDataSpreadsheet("Shifts", 5, new List<string> { "Date", "Number", "Service" })));
        mockService
            .Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BatchUpdateSpreadsheetResponse());

        var manager = new SheetManager(mockService.Object);

        // Real Shifts headers have far more than these 3 columns - the corresponding autoHeal:true
        // case is GetSheetsStructureBehaviorTests.GetSheets_WithIncludeStructureTrue_DetectsMissingColumns_AndAttemptsInsertion.
        await manager.GetSheets(new List<string> { "Shifts" }, includeStructure: true, autoHeal: false);

        mockService.Verify(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAllSheets_WithAutoHealFalse_PropagatesToEveryCanonicalSheet_NeverCallsBatchUpdate()
    {
        var mockService = BuildValuesOnlyMockWithMissingColumns();
        var manager = new SheetManager(mockService.Object);

        await manager.GetAllSheets(includeStructure: false, autoHeal: false);

        mockService.Verify(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetSheet_WithAutoHealFalse_NeverCallsBatchUpdate()
    {
        var mockService = BuildValuesOnlyMockWithMissingColumns();
        var manager = new SheetManager(mockService.Object);

        await manager.GetSheet("Shifts", includeStructure: false, autoHeal: false);

        mockService.Verify(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
