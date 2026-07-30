using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Sheets.v4.Data;
using Moq;
using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Models;
using RaptorSheets.Core.Services;
using RaptorSheets.Gig.Managers;
using Xunit;

namespace RaptorSheets.Gig.Tests.Unit.Managers;

/// <summary>
/// Covers the includeStructure opt-in on GetSheets: when true, GetSheets should switch from the
/// values-only batchGet path to a single full grid-data Spreadsheet fetch (GetSheetInfoResult) that
/// serves both row data and Structures, rather than issuing a second call on top of the values-only
/// path. See <see cref="GetSheetsBehaviorTests"/> for the includeStructure=false regression guard.
/// </summary>
public class GetSheetsStructureBehaviorTests
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

    [Fact]
    public async Task GetSheets_WithIncludeStructureFalse_DelegatesToOriginalPath_NeverCallsGetSheetInfoResult()
    {
        var mockService = new Mock<IGoogleSheetService>();

        mockService
            .Setup(s => s.GetBatchDataResult(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(GoogleApiResult<BatchGetValuesByDataFilterResponse>.Ok(BuildBatchResponse("Shifts", new List<object> { "Date", "Number", "Service" })));
        mockService
            .Setup(s => s.GetSheetInfo(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Spreadsheet
            {
                Properties = new SpreadsheetProperties { Title = "MySpreadsheet" },
                Sheets = new List<Sheet> { new() { Properties = new SheetProperties { Title = "Shifts" } } }
            });

        var manager = new SheetManager(mockService.Object);

        var result = await manager.GetSheets(new List<string> { "Shifts" }, includeStructure: false);

        Assert.NotNull(result);
        Assert.Empty(result.Structures);
        mockService.Verify(s => s.GetSheetInfoResult(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()), Times.Never);
        mockService.Verify(s => s.GetBatchDataResult(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetSheets_WithIncludeStructureTrue_RequestsFullRange_AndNeverCallsBatchData()
    {
        var mockService = new Mock<IGoogleSheetService>();
        List<string>? capturedRanges = null;

        mockService
            .Setup(s => s.GetSheetInfoResult(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
            .Callback<List<string>?, CancellationToken>((ranges, _) => capturedRanges = ranges)
            .ReturnsAsync(GoogleApiResult<Spreadsheet>.Ok(BuildGridDataSpreadsheet("Shifts", 5, new List<string> { "Date", "Number", "Service" })));

        var manager = new SheetManager(mockService.Object);

        await manager.GetSheets(new List<string> { "Shifts" }, includeStructure: true);

        Assert.Equal(new List<string> { $"Shifts!{GoogleConfig.Range}" }, capturedRanges);
        mockService.Verify(s => s.GetBatchData(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        mockService.Verify(s => s.GetBatchDataResult(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetSheets_WithIncludeStructureTrue_PopulatesStructures()
    {
        var mockService = new Mock<IGoogleSheetService>();

        mockService
            .Setup(s => s.GetSheetInfoResult(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(GoogleApiResult<Spreadsheet>.Ok(BuildGridDataSpreadsheet("Shifts", 5, new List<string> { "Date", "Number", "Service" })));

        var manager = new SheetManager(mockService.Object);

        var result = await manager.GetSheets(new List<string> { "Shifts" }, includeStructure: true);

        Assert.True(result.Structures.ContainsKey("Shifts"));
        Assert.Equal(3, result.Structures["Shifts"].Headers.Count);
        Assert.Equal("Date", result.Structures["Shifts"].Headers[0].Name);
    }

    [Fact]
    public async Task GetSheets_WithIncludeStructureTrue_StillDetectsUnknownTabs()
    {
        var mockService = new Mock<IGoogleSheetService>();
        var spreadsheet = BuildGridDataSpreadsheet("Shifts", 5, new List<string> { "Date", "Number", "Service" });
        spreadsheet.Sheets.Add(new Sheet
        {
            Properties = new SheetProperties { Title = "SomeRandomTab" },
            Data = new List<GridData> { new() { RowData = new List<RowData>() } }
        });

        mockService
            .Setup(s => s.GetSheetInfoResult(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(GoogleApiResult<Spreadsheet>.Ok(spreadsheet));

        var manager = new SheetManager(mockService.Object);

        var result = await manager.GetSheets(new List<string> { "Shifts" }, includeStructure: true);

        Assert.Contains(result.Messages, m => m.Message.Contains("SomeRandomTab"));
    }

    [Fact]
    public async Task GetSheets_WithIncludeStructureTrue_PopulatesSpreadsheetTitleAndRowData()
    {
        var mockService = new Mock<IGoogleSheetService>();

        mockService
            .Setup(s => s.GetSheetInfoResult(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(GoogleApiResult<Spreadsheet>.Ok(BuildGridDataSpreadsheet("Shifts", 5, new List<string> { "Date", "Number", "Service" })));

        var manager = new SheetManager(mockService.Object);

        var result = await manager.GetSheets(new List<string> { "Shifts" }, includeStructure: true);

        // Confirms _registry.MapData(Spreadsheet) still runs (row mapping unaffected by structure parsing).
        Assert.Equal("MySpreadsheet", result.Properties.Name);
    }

    [Fact]
    public async Task GetSheets_WithIncludeStructureTrue_OnFailure_ReturnsErrorMessageWithoutThrowing()
    {
        var mockService = new Mock<IGoogleSheetService>();

        mockService
            .Setup(s => s.GetSheetInfoResult(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(GoogleApiResult<Spreadsheet>.Failed(new GoogleApiFailure { Reason = GoogleApiFailureReason.Forbidden, Message = "denied" }));

        var manager = new SheetManager(mockService.Object);

        var result = await manager.GetSheets(new List<string> { "Shifts" }, includeStructure: true);

        Assert.Contains(result.Messages, m => m.Message.Contains("denied"));
        Assert.Empty(result.Structures);
    }

    [Fact]
    public async Task GetSheets_WithIncludeStructureTrue_DetectsMissingColumns_AndAttemptsInsertion()
    {
        var mockService = new Mock<IGoogleSheetService>();

        mockService
            .Setup(s => s.GetSheetInfoResult(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(GoogleApiResult<Spreadsheet>.Ok(BuildGridDataSpreadsheet("Shifts", 5, new List<string> { "Date", "Number", "Service" })));
        mockService
            .Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BatchUpdateSpreadsheetResponse());

        var manager = new SheetManager(mockService.Object);

        // Real Shifts headers have far more than these 3 columns, so this should trip auto-heal.
        await manager.GetSheets(new List<string> { "Shifts" }, includeStructure: true);

        mockService.Verify(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
