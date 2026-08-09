using Google.Apis.Sheets.v4.Data;
using Moq;
using RaptorSheets.Core.Models;
using RaptorSheets.Core.Services;
using RaptorSheets.Home.Managers;
using Xunit;

namespace RaptorSheets.Home.Tests.Unit.Managers;

/// <summary>
/// Covers Home's wiring of the self-heal column-validation restoration (GitHub issue #103). The
/// shared insertion logic (RaptorSheets.Core.Helpers.ColumnInsertionHelper) has its own dedicated
/// tests in RaptorSheets.Core.Tests - this just confirms Home's SheetManager.GetDataValidation
/// override actually resolves and wires through.
/// </summary>
public class InsertMissingColumnsBehaviorTests
{
    [Fact]
    public async Task InsertMissingColumns_WithRawValidationName_ResolvesAndAppliesDataValidationRule()
    {
        var mockService = new Mock<IGoogleSheetService>();
        BatchUpdateSpreadsheetRequest? capturedRequest = null;
        mockService
            .Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>(), It.IsAny<CancellationToken>()))
            .Callback<BatchUpdateSpreadsheetRequest, CancellationToken>((r, _) => capturedRequest = r)
            .ReturnsAsync(new BatchUpdateSpreadsheetResponse());

        var manager = new SheetManager(mockService.Object);
        var missingColumns = new Dictionary<string, List<ColumnInsertionInfo>>
        {
            ["Appliances"] = [new ColumnInsertionInfo { SheetName = "Appliances", SheetId = 3, ColumnIndex = 1, ColumnName = "Active", ColumnLetter = "B", Validation = "BOOLEAN" }]
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
