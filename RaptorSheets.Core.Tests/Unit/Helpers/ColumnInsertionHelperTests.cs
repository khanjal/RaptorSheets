using Google.Apis.Sheets.v4.Data;
using Moq;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models;
using RaptorSheets.Core.Services;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Helpers;

public class ColumnInsertionHelperTests
{
    // Minimal ISheetEntity implementation - Core.Tests can't reference a real domain SheetEntity.
    private class TestSheetEntity : ISheetEntity
    {
        public PropertyEntity Properties { get; set; } = new();
        public List<MessageEntity> Messages { get; set; } = [];
    }

    [Fact]
    public void BuildInsertRequests_WithSingleMissingColumn_BuildsInsertAndUpdateRequests()
    {
        var missingColumns = new Dictionary<string, List<ColumnInsertionInfo>>
        {
            ["Widgets"] = [new ColumnInsertionInfo { SheetName = "Widgets", SheetId = 5, ColumnIndex = 2, ColumnName = "Price", ColumnLetter = "C" }]
        };

        var requests = ColumnInsertionHelper.BuildInsertRequests(missingColumns);

        Assert.Equal(2, requests.Count);
        var insertRequest = requests[0];
        Assert.NotNull(insertRequest.InsertDimension);
        Assert.Equal(5, insertRequest.InsertDimension.Range.SheetId);
        Assert.Equal(2, insertRequest.InsertDimension.Range.StartIndex);
        Assert.Equal(3, insertRequest.InsertDimension.Range.EndIndex);

        var updateRequest = requests[1];
        Assert.NotNull(updateRequest.UpdateCells);
        Assert.Equal(2, updateRequest.UpdateCells.Range.StartColumnIndex);
        Assert.Equal("Price", updateRequest.UpdateCells.Rows[0].Values[0].UserEnteredValue.StringValue);
    }

    [Fact]
    public void BuildInsertRequests_WithMultipleMissingColumnsInOneSheet_InsertsRightToLeft()
    {
        // Inserting right-to-left (highest index first) so earlier insertions in the batch
        // don't shift the index of columns still waiting to be inserted.
        var missingColumns = new Dictionary<string, List<ColumnInsertionInfo>>
        {
            ["Widgets"] =
            [
                new ColumnInsertionInfo { SheetName = "Widgets", SheetId = 1, ColumnIndex = 0, ColumnName = "First" },
                new ColumnInsertionInfo { SheetName = "Widgets", SheetId = 1, ColumnIndex = 3, ColumnName = "Fourth" },
                new ColumnInsertionInfo { SheetName = "Widgets", SheetId = 1, ColumnIndex = 1, ColumnName = "Second" }
            ]
        };

        var requests = ColumnInsertionHelper.BuildInsertRequests(missingColumns);

        // Each column produces 2 requests (insert + update); check the insert requests' order
        var insertIndices = requests
            .Where(r => r.InsertDimension != null)
            .Select(r => r.InsertDimension.Range.StartIndex)
            .ToList();

        Assert.Equal([3, 1, 0], insertIndices);
    }

    [Fact]
    public void BuildInsertRequests_WithNoMissingColumns_ReturnsEmptyList()
    {
        var requests = ColumnInsertionHelper.BuildInsertRequests([]);

        Assert.Empty(requests);
    }

    [Fact]
    public async Task InsertMissingColumnsAsync_WithNoMissingColumns_ReturnsInfoMessageWithoutCallingService()
    {
        var mockService = new Mock<IGoogleSheetService>();

        var result = await ColumnInsertionHelper.InsertMissingColumnsAsync<TestSheetEntity>(mockService.Object, []);

        Assert.Contains(result.Messages, m => m.Message.Contains("No missing columns to insert"));
        mockService.Verify(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>()), Times.Never);
    }

    [Fact]
    public async Task InsertMissingColumnsAsync_OnSuccess_ReturnsSuccessMessageAndCallsService()
    {
        var mockService = new Mock<IGoogleSheetService>();
        mockService
            .Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>()))
            .ReturnsAsync(new BatchUpdateSpreadsheetResponse());

        var missingColumns = new Dictionary<string, List<ColumnInsertionInfo>>
        {
            ["Widgets"] = [new ColumnInsertionInfo { SheetName = "Widgets", SheetId = 1, ColumnIndex = 0, ColumnName = "Price" }]
        };

        var result = await ColumnInsertionHelper.InsertMissingColumnsAsync<TestSheetEntity>(mockService.Object, missingColumns);

        Assert.Contains(result.Messages, m => m.Message.Contains("Inserting column 'Price'"));
        Assert.Contains(result.Messages, m => m.Message.Contains("Successfully inserted 1 missing column(s)"));
        mockService.Verify(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>()), Times.Once);
    }

    [Fact]
    public async Task InsertMissingColumnsAsync_WhenServiceReturnsNull_ReturnsErrorMessage()
    {
        var mockService = new Mock<IGoogleSheetService>();
        mockService
            .Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>()))
            .ReturnsAsync((BatchUpdateSpreadsheetResponse?)null);

        var missingColumns = new Dictionary<string, List<ColumnInsertionInfo>>
        {
            ["Widgets"] = [new ColumnInsertionInfo { SheetName = "Widgets", SheetId = 1, ColumnIndex = 0, ColumnName = "Price" }]
        };

        var result = await ColumnInsertionHelper.InsertMissingColumnsAsync<TestSheetEntity>(mockService.Object, missingColumns);

        Assert.Contains(result.Messages, m => m.Message.Contains("Failed to insert missing columns"));
    }
}
