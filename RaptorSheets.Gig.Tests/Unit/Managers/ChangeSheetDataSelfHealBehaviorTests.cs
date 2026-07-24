using Google.Apis.Sheets.v4.Data;
using Moq;
using RaptorSheets.Core.Services;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Managers;
using Xunit;

namespace RaptorSheets.Gig.Tests.Unit.Managers;

/// <summary>
/// Covers ChangeSheetData's failure path: when the batch update comes back null, the manager
/// checks whether the spreadsheet is missing sheets entirely and attempts to (re)create them
/// (HandleMissingSheets) before reporting the save as failed. This never surfaces with the
/// "real fake-token manager" pattern used elsewhere in this test project, because a fake token
/// also fails the GetSheetInfo call that would otherwise reveal missing sheets - it needs a
/// mocked IGoogleSheetService where GetSheetInfo succeeds but BatchUpdateSpreadsheet doesn't.
/// </summary>
public class ChangeSheetDataSelfHealBehaviorTests
{
    [Fact]
    public async Task ChangeSheetData_WhenBatchUpdateFailsAndSpreadsheetIsMissingSheets_AttemptsToRecreateThem()
    {
        // Arrange
        var mockService = new Mock<IGoogleSheetService>();

        // No sheets present at all, so every canonical Gig sheet is "missing".
        mockService.Setup(s => s.GetSheetInfo(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Spreadsheet { Sheets = new List<Sheet>() });

        mockService.Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BatchUpdateSpreadsheetResponse?)null);

        var manager = new GoogleSheetManager(mockService.Object);
        var sheets = new List<string> { "Expenses" };
        var sheetEntity = new SheetEntity
        {
            Sheets = { Expenses = { new ExpenseEntity { Name = "Test", Amount = 100 } } }
        };

        // Act
        var result = await manager.ChangeSheetData(sheets, sheetEntity);

        // Assert
        Assert.Contains(result.Messages, m => m.Message.Contains("Unable to save data"));
        // HandleMissingSheets ran and attempted creation - evidenced by messages about the
        // missing/created sheets, not just the generic save failure.
        Assert.True(result.Messages.Count > 1);
    }
}
