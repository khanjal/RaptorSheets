using Google.Apis.Sheets.v4.Data;
using Moq;
using RaptorSheets.Core.Models;
using RaptorSheets.Core.Services;
using RaptorSheets.Stock.Helpers;
using RaptorSheets.Stock.Managers;
using Xunit;

namespace RaptorSheets.Stock.Tests.Unit.Managers;

/// <summary>
/// Covers the Stock-side wiring of the column auto-insertion feature (shared with Gig via
/// RaptorSheets.Core.Helpers.ColumnInsertionHelper / RaptorSheets.Core.Registries.SheetRegistry).
/// </summary>
public class InsertMissingColumnsBehaviorTests
{
    [Fact]
    public void CheckSheetHeaders_WithMissingColumn_DetectsInsertionInfo()
    {
        // Arrange - Stocks sheet is missing one of its expected headers
        var spreadsheet = new Spreadsheet
        {
            Sheets =
            [
                new()
                {
                    Properties = new SheetProperties { Title = "Stocks", SheetId = 3 },
                    Data =
                    [
                        new()
                        {
                            RowData = [new() { Values = [new() { FormattedValue = "Ticker" }] }]
                        }
                    ]
                }
            ]
        };

        // Act
        var messages = StockSheetHelpers.CheckSheetHeaders(spreadsheet, out var missingColumns);

        // Assert
        Assert.NotEmpty(messages);
        Assert.True(missingColumns.ContainsKey("Stocks"));
        Assert.NotEmpty(missingColumns["Stocks"]);
        Assert.All(missingColumns["Stocks"], c => Assert.Equal(3, c.SheetId));
    }

    [Fact]
    public async Task InsertMissingColumns_WithNoMissingColumns_DoesNotCallService()
    {
        // Arrange
        var mockService = new Mock<IGoogleSheetService>();
        var manager = new SheetManager(mockService.Object);

        // Act
        var result = await manager.InsertMissingColumns([]);

        // Assert
        Assert.Contains(result.Messages, m => m.Message.Contains("No missing columns to insert"));
        mockService.Verify(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task InsertMissingColumns_WithMissingColumn_CallsBatchUpdateAndReportsSuccess()
    {
        // Arrange
        var mockService = new Mock<IGoogleSheetService>();
        mockService
            .Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BatchUpdateSpreadsheetResponse());

        var manager = new SheetManager(mockService.Object);
        var missingColumns = new Dictionary<string, List<ColumnInsertionInfo>>
        {
            ["Stocks"] = [new ColumnInsertionInfo { SheetName = "Stocks", SheetId = 3, ColumnIndex = 1, ColumnName = "Name" }]
        };

        // Act
        var result = await manager.InsertMissingColumns(missingColumns);

        // Assert
        Assert.Contains(result.Messages, m => m.Message.Contains("Successfully inserted 1 missing column(s)"));
    }

    [Fact]
    public async Task InsertMissingColumns_WithRawValidationName_ResolvesAndAppliesDataValidationRule()
    {
        // #103: a re-inserted dropdown column's raw Validation name must be resolved via
        // SheetManager's GetDataValidation override and applied as a real DataValidationRule.
        var mockService = new Mock<IGoogleSheetService>();
        BatchUpdateSpreadsheetRequest? capturedRequest = null;
        mockService
            .Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>(), It.IsAny<CancellationToken>()))
            .Callback<BatchUpdateSpreadsheetRequest, CancellationToken>((r, _) => capturedRequest = r)
            .ReturnsAsync(new BatchUpdateSpreadsheetResponse());

        var manager = new SheetManager(mockService.Object);
        var missingColumns = new Dictionary<string, List<ColumnInsertionInfo>>
        {
            ["Stocks"] = [new ColumnInsertionInfo { SheetName = "Stocks", SheetId = 3, ColumnIndex = 1, ColumnName = "Active", Validation = "BOOLEAN" }]
        };

        // Act
        await manager.InsertMissingColumns(missingColumns);

        // Assert
        Assert.NotNull(capturedRequest);
        var validationRequest = capturedRequest.Requests.Single(r => r.RepeatCell != null);
        Assert.Equal("dataValidation", validationRequest.RepeatCell.Fields);
        Assert.Equal("BOOLEAN", validationRequest.RepeatCell.Cell.DataValidation.Condition.Type);
    }
}
