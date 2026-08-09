using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Apis.Sheets.v4.Data;
using Moq;
using RaptorSheets.Core.Models;
using RaptorSheets.Core.Services;
using RaptorSheets.Gig.Helpers;
using RaptorSheets.Gig.Managers;
using Xunit;

namespace RaptorSheets.Gig.Tests.Unit.Managers;

/// <summary>
/// Covers the Gig-side wiring of the column auto-insertion feature: CheckSheetHeaders'
/// out-param overload for detecting missing columns, and InsertMissingColumns for physically
/// inserting them. The underlying logic (RaptorSheets.Core.Helpers.ColumnInsertionHelper,
/// RaptorSheets.Core.Registries.SheetRegistry) has its own dedicated tests in RaptorSheets.Core.Tests -
/// these just confirm Gig's manager delegates to it correctly.
/// </summary>
public class InsertMissingColumnsBehaviorTests
{
    [Fact]
    public void CheckSheetHeaders_WithMissingColumn_DetectsInsertionInfo()
    {
        // Arrange - Trips sheet with only "Date" present; the real model has many more columns
        var spreadsheet = new Spreadsheet
        {
            Sheets =
            [
                new()
                {
                    Properties = new SheetProperties { Title = "Trips", SheetId = 42 },
                    Data =
                    [
                        new()
                        {
                            RowData = [new() { Values = [new() { FormattedValue = "Date" }] }]
                        }
                    ]
                }
            ]
        };

        // Act
        var messages = GigSheetHelpers.CheckSheetHeaders(spreadsheet, out var missingColumns);

        // Assert
        Assert.NotEmpty(messages);
        Assert.True(missingColumns.ContainsKey("Trips"));
        Assert.NotEmpty(missingColumns["Trips"]);
        Assert.All(missingColumns["Trips"], c => Assert.Equal(42, c.SheetId));
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
        BatchUpdateSpreadsheetRequest? capturedRequest = null;
        mockService
            .Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>(), It.IsAny<CancellationToken>()))
            .Callback<BatchUpdateSpreadsheetRequest, CancellationToken>((r, _) => capturedRequest = r)
            .ReturnsAsync(new BatchUpdateSpreadsheetResponse());

        var manager = new SheetManager(mockService.Object);
        var missingColumns = new Dictionary<string, List<ColumnInsertionInfo>>
        {
            ["Trips"] = [new ColumnInsertionInfo { SheetName = "Trips", SheetId = 7, ColumnIndex = 3, ColumnName = "Note" }]
        };

        // Act
        var result = await manager.InsertMissingColumns(missingColumns);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Contains(capturedRequest.Requests, r => r.InsertDimension != null && r.InsertDimension.Range.SheetId == 7);
        Assert.Contains(result.Messages, m => m.Message.Contains("Successfully inserted 1 missing column(s)"));
    }

    [Fact]
    public async Task InsertMissingColumns_WithRawValidationName_ResolvesAndAppliesDataValidationRule()
    {
        // #103: a re-inserted dropdown column (e.g. Trip Service) must get its validation rule
        // back - the raw Validation name is resolved via SheetManager's GetDataValidation override.
        var mockService = new Mock<IGoogleSheetService>();
        BatchUpdateSpreadsheetRequest? capturedRequest = null;
        mockService
            .Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>(), It.IsAny<CancellationToken>()))
            .Callback<BatchUpdateSpreadsheetRequest, CancellationToken>((r, _) => capturedRequest = r)
            .ReturnsAsync(new BatchUpdateSpreadsheetResponse());

        var manager = new SheetManager(mockService.Object);
        var missingColumns = new Dictionary<string, List<ColumnInsertionInfo>>
        {
            ["Trips"] = [new ColumnInsertionInfo { SheetName = "Trips", SheetId = 7, ColumnIndex = 3, ColumnName = "Cash", ColumnLetter = "D", Validation = "BOOLEAN" }]
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
