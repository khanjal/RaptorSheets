using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Apis.Sheets.v4.Data;
using Moq;
using RaptorSheets.Core.Services;
using RaptorSheets.Gig.Managers;
using Xunit;
using RaptorSheets.Core.Models;

namespace RaptorSheets.Gig.Tests.Unit.Managers;

/// <summary>
/// Covers GetSheets' success path after removing the redundant, expensive
/// GetSheetInfo(ranges)/IncludeGridData=true call: known-sheet header validation already
/// happens via GigSheetHelpers.MapData from the batchGet response, so GetSheets should now only
/// ever call the cheap, no-ranges GetSheetInfo() overload (used solely for unknown-tab detection
/// and the spreadsheet title).
/// </summary>
public class GetSheetsBehaviorTests
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

    [Fact]
    public async Task GetSheets_OnSuccess_ShouldNotCallExpensiveGetSheetInfoWithRanges()
    {
        // Arrange
        var mockService = new Mock<IGoogleSheetService>();

        mockService
            .Setup(s => s.GetBatchData(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildBatchResponse("Shifts", new List<object> { "Date", "Number", "Service" }));
        mockService
            .Setup(s => s.GetBatchDataResult(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(GoogleApiResult<BatchGetValuesByDataFilterResponse>.Ok(BuildBatchResponse("Shifts", new List<object> { "Date", "Number", "Service" })));

        mockService
            .Setup(s => s.GetSheetInfo(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Spreadsheet
            {
                Properties = new SpreadsheetProperties { Title = "MySpreadsheet" },
                Sheets = new List<Sheet>
                {
                    new() { Properties = new SheetProperties { Title = "Shifts" } }
                }
            });

        var manager = new GoogleSheetManager(mockService.Object);

        // Act
        var result = await manager.GetSheets(new List<string> { "Shifts" });

        // Assert
        Assert.NotNull(result);
        mockService.Verify(s => s.GetSheetInfo(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()), Times.Never);
        mockService.Verify(s => s.GetSheetInfo(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetSheets_WithUnknownSheetTab_ShouldSurfaceWarningWithoutGridDataCall()
    {
        // Arrange
        var mockService = new Mock<IGoogleSheetService>();

        mockService
            .Setup(s => s.GetBatchData(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildBatchResponse("Shifts", new List<object> { "Date", "Number", "Service" }));
        mockService
            .Setup(s => s.GetBatchDataResult(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(GoogleApiResult<BatchGetValuesByDataFilterResponse>.Ok(BuildBatchResponse("Shifts", new List<object> { "Date", "Number", "Service" })));

        mockService
            .Setup(s => s.GetSheetInfo(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Spreadsheet
            {
                Properties = new SpreadsheetProperties { Title = "MySpreadsheet" },
                Sheets = new List<Sheet>
                {
                    new() { Properties = new SheetProperties { Title = "Shifts" } },
                    new() { Properties = new SheetProperties { Title = "SomeRandomTab" } }
                }
            });

        var manager = new GoogleSheetManager(mockService.Object);

        // Act
        var result = await manager.GetSheets(new List<string> { "Shifts" });

        // Assert - unknown tab detection still works even though it now comes from the cheap,
        // no-grid-data GetSheetInfo() call rather than the removed IncludeGridData=true one.
        Assert.Contains(result.Messages, m => m.Message.Contains("SomeRandomTab"));
        mockService.Verify(s => s.GetSheetInfo(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetSheets_OnSuccess_ShouldPopulateSpreadsheetTitleFromCheapCall()
    {
        // Arrange
        var mockService = new Mock<IGoogleSheetService>();

        mockService
            .Setup(s => s.GetBatchData(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildBatchResponse("Shifts", new List<object> { "Date", "Number", "Service" }));
        mockService
            .Setup(s => s.GetBatchDataResult(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(GoogleApiResult<BatchGetValuesByDataFilterResponse>.Ok(BuildBatchResponse("Shifts", new List<object> { "Date", "Number", "Service" })));

        mockService
            .Setup(s => s.GetSheetInfo(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Spreadsheet
            {
                Properties = new SpreadsheetProperties { Title = "MySpreadsheet" },
                Sheets = new List<Sheet>
                {
                    new() { Properties = new SheetProperties { Title = "Shifts" } }
                }
            });

        var manager = new GoogleSheetManager(mockService.Object);

        // Act
        var result = await manager.GetSheets(new List<string> { "Shifts" });

        // Assert - spreadsheet-level metadata (title) is present on the Spreadsheet regardless of
        // IncludeGridData, so the cheap no-ranges call is sufficient to populate it.
        Assert.Equal("MySpreadsheet", result.Properties.Name);
    }
}
