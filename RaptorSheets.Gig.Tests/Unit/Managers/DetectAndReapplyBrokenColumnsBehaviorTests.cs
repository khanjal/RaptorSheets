using Google.Apis.Sheets.v4.Data;
using Moq;
using RaptorSheets.Core.Services;
using RaptorSheets.Gig.Managers;
using Xunit;

namespace RaptorSheets.Gig.Tests.Unit.Managers;

/// <summary>
/// Covers Gig's wiring of #53 gaps 2/3: detecting a column whose live Formula has drifted from
/// canonical, then reapplying it. The underlying logic (SheetManagerBase.DetectBrokenColumnsAsync/
/// ReapplyColumnFormulas, ColumnInsertionHelper.BuildHeaderFixRequests) has its own dedicated tests
/// in RaptorSheets.Core.Tests - this just confirms the two chain together correctly end to end
/// against a real Gig sheet layout.
/// </summary>
public class DetectAndReapplyBrokenColumnsBehaviorTests
{
    [Fact]
    public async Task DetectThenReapply_WithDriftedFormulaColumn_FixesItInOneRoundTrip()
    {
        var mockService = new Mock<IGoogleSheetService>();
        var manager = new SheetManager(mockService.Object);

        // Trips has several ArrayFormula-driven columns (see TripSheet.cs) - use the real canonical
        // model rather than hand-rolled formula text, so this test stays correct as those formulas
        // evolve.
        var canonical = manager.GetSheetLayout("Trips")!;
        var brokenHeader = canonical.Headers.First(h => !string.IsNullOrEmpty(h.Formula));

        // Every other header's live formula must match its canonical one exactly (several Trip
        // columns have formulas) - only brokenHeader should end up flagged.
        static ExtendedValue? LiveFormulaValue(string? formula) => string.IsNullOrEmpty(formula) ? null : new ExtendedValue { FormulaValue = formula };

        var liveHeaders = canonical.Headers
            .Select(h => new CellData
            {
                FormattedValue = h.Name,
                UserEnteredValue = h.Name == brokenHeader.Name ? LiveFormulaValue("=BROKEN") : LiveFormulaValue(h.Formula)
            })
            .ToList();

        var spreadsheet = new Spreadsheet
        {
            Sheets = new List<Sheet>
            {
                new()
                {
                    Properties = new SheetProperties { Title = "Trips", SheetId = 7 },
                    Data = new List<GridData> { new() { RowData = new List<RowData> { new() { Values = liveHeaders } } } }
                }
            }
        };

        mockService.Setup(s => s.GetSheetInfo(It.IsAny<List<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(spreadsheet);
        BatchUpdateSpreadsheetRequest? capturedRequest = null;
        mockService.Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>(), It.IsAny<CancellationToken>()))
            .Callback<BatchUpdateSpreadsheetRequest, CancellationToken>((r, _) => capturedRequest = r)
            .ReturnsAsync(new BatchUpdateSpreadsheetResponse());

        // Act
        var broken = await manager.DetectBrokenColumnsAsync("Trips");
        var result = await manager.ReapplyColumnFormulas("Trips", broken);

        // Assert
        var fixedColumn = Assert.Single(broken);
        Assert.Equal(brokenHeader.Name, fixedColumn.ColumnName);
        Assert.Equal(brokenHeader.Formula, fixedColumn.Formula); // canonical, not the "=BROKEN" live one

        Assert.NotNull(capturedRequest);
        var updateRequest = capturedRequest.Requests.Single();
        Assert.Equal(brokenHeader.Formula, updateRequest.UpdateCells.Rows[0].Values[0].UserEnteredValue.FormulaValue);
        Assert.Contains(result.Messages, m => m.Message.Contains("Reapplied formula for 1 column"));
    }
}
