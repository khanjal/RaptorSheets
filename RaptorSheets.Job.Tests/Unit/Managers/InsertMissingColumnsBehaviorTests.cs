using Google.Apis.Sheets.v4.Data;
using Moq;
using RaptorSheets.Core.Models;
using RaptorSheets.Core.Services;
using RaptorSheets.Job.Managers;
using Xunit;

namespace RaptorSheets.Job.Tests.Unit.Managers;

/// <summary>
/// Covers Job's wiring of the self-heal column-validation restoration (GitHub issue #103). Job
/// stores the actual A1 range directly as the raw validation value (no enum), so this also
/// confirms SheetManager.GetDataValidation passes it straight through to JobSheetHelpers rather
/// than trying to parse it as an enum name. The shared insertion logic
/// (RaptorSheets.Core.Helpers.ColumnInsertionHelper) has its own dedicated tests in
/// RaptorSheets.Core.Tests.
/// </summary>
public class InsertMissingColumnsBehaviorTests
{
    [Fact]
    public async Task InsertMissingColumns_WithRawValidationRange_ResolvesAndAppliesDataValidationRule()
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
            ["Applications"] = [new ColumnInsertionInfo { SheetName = "Applications", SheetId = 4, ColumnIndex = 1, ColumnName = "Company", Validation = "Companies!$A$2:$A" }]
        };

        // Act
        await manager.InsertMissingColumns(missingColumns);

        // Assert
        Assert.NotNull(capturedRequest);
        var validationRequest = capturedRequest.Requests.Single(r => r.RepeatCell != null);
        Assert.Equal("dataValidation", validationRequest.RepeatCell.Fields);
        Assert.Equal("ONE_OF_RANGE", validationRequest.RepeatCell.Cell.DataValidation.Condition.Type);
    }
}
