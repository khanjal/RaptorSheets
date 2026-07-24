using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Apis.Sheets.v4.Data;
using Moq;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Services;
using RaptorSheets.Stock.Managers;
using Xunit;
using SheetName = RaptorSheets.Stock.Enums.SheetName;
using RaptorSheets.Core.Models;

namespace RaptorSheets.Stock.Tests.Unit.Managers;

/// <summary>
/// Covers GetSheets' orchestration now that it shares GoogleSheetManagerBase.GetSheetsCoreAsync with
/// Gig: unknown-tab detection and spreadsheet-name population happen on every call (not just
/// full-sheet-list requests, as before), and a batchGet failure triggers the same missing-sheet
/// self-heal-and-recreate behavior Gig already had (Stock previously had none - the placeholder
/// comment in GoogleSheetManager.GetSheets() asking for this has since been removed).
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
    public async Task GetSheets_PartialRequest_ShouldStillPopulateSpreadsheetName()
    {
        // Arrange - previously, GetSheets only set Properties.Name when every sheet was requested.
        var mockService = new Mock<IGoogleSheetService>();

        mockService
            .Setup(s => s.GetBatchData(It.IsAny<List<string>>(), It.IsAny<string>()))
            .ReturnsAsync(BuildBatchResponse("Accounts", new List<object> { "Account", "Description" }));
        mockService
            .Setup(s => s.GetBatchDataResult(It.IsAny<List<string>>(), It.IsAny<string>()))
            .ReturnsAsync(GoogleApiResult<BatchGetValuesByDataFilterResponse>.Ok(BuildBatchResponse("Accounts", new List<object> { "Account", "Description" })));

        mockService
            .Setup(s => s.GetSheetInfo())
            .ReturnsAsync(new Spreadsheet
            {
                Properties = new SpreadsheetProperties { Title = "MyStockSheet" },
                Sheets = new List<Sheet> { new() { Properties = new SheetProperties { Title = "Accounts" } } }
            });

        var manager = new GoogleSheetManager(mockService.Object);

        // Act
        var result = await manager.GetSheets(new List<string> { SheetName.ACCOUNTS.GetDescription() });

        // Assert
        Assert.Equal("MyStockSheet", result.Properties.Name);
    }

    [Fact]
    public async Task GetSheets_PartialRequest_WithUnknownTab_ShouldSurfaceWarning()
    {
        // Arrange - unknown-tab detection previously only ran on full-sheet-list requests.
        var mockService = new Mock<IGoogleSheetService>();

        mockService
            .Setup(s => s.GetBatchData(It.IsAny<List<string>>(), It.IsAny<string>()))
            .ReturnsAsync(BuildBatchResponse("Accounts", new List<object> { "Account", "Description" }));
        mockService
            .Setup(s => s.GetBatchDataResult(It.IsAny<List<string>>(), It.IsAny<string>()))
            .ReturnsAsync(GoogleApiResult<BatchGetValuesByDataFilterResponse>.Ok(BuildBatchResponse("Accounts", new List<object> { "Account", "Description" })));

        mockService
            .Setup(s => s.GetSheetInfo())
            .ReturnsAsync(new Spreadsheet
            {
                Properties = new SpreadsheetProperties { Title = "MyStockSheet" },
                Sheets = new List<Sheet>
                {
                    new() { Properties = new SheetProperties { Title = "Accounts" } },
                    new() { Properties = new SheetProperties { Title = "SomeRandomTab" } }
                }
            });

        var manager = new GoogleSheetManager(mockService.Object);

        // Act
        var result = await manager.GetSheets(new List<string> { SheetName.ACCOUNTS.GetDescription() });

        // Assert
        Assert.Contains(result.Messages, m => m.Message.Contains("SomeRandomTab") && m.Message.Contains("does not match any known sheet name"));
    }

    [Fact]
    public async Task GetSheets_OnBatchFailure_WithMissingSheets_ShouldCreateAndReturnRetryMessage()
    {
        // Arrange - a failed batchGet with sheets missing entirely should now self-heal by creating
        // them and returning an info message asking the caller to retry, matching Gig's behavior.
        var mockService = new Mock<IGoogleSheetService>();

        mockService
            .Setup(s => s.GetBatchData(It.IsAny<List<string>>(), It.IsAny<string>()))
            .ReturnsAsync((BatchGetValuesByDataFilterResponse?)null);
        mockService
            .Setup(s => s.GetBatchDataResult(It.IsAny<List<string>>(), It.IsAny<string>()))
            .ReturnsAsync(GoogleApiResult<BatchGetValuesByDataFilterResponse>.Failed(new GoogleApiFailure { Reason = GoogleApiFailureReason.Unknown, Message = "test failure" }));

        mockService
            .Setup(s => s.GetSheetInfo())
            .ReturnsAsync(new Spreadsheet
            {
                Properties = new SpreadsheetProperties { Title = "MyStockSheet" },
                Sheets = new List<Sheet>
                {
                    new() { Properties = new SheetProperties { Title = "Stocks", SheetId = 1 } },
                    new() { Properties = new SheetProperties { Title = "Tickers", SheetId = 2 } }
                }
            });

        mockService
            .Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>()))
            .ReturnsAsync(new BatchUpdateSpreadsheetResponse
            {
                Replies = new List<Response>
                {
                    new() { AddSheet = new AddSheetResponse { Properties = new SheetProperties { Title = "Accounts" } } }
                }
            });

        var manager = new GoogleSheetManager(mockService.Object);

        // Act - Accounts is missing from the spreadsheet (only Stocks/Tickers exist above).
        var result = await manager.GetSheets(new List<string>
        {
            SheetName.ACCOUNTS.GetDescription(),
            SheetName.STOCKS.GetDescription(),
            SheetName.TICKERS.GetDescription()
        });

        // Assert
        Assert.Contains(result.Messages, m => m.Message.Contains("Created missing sheets") && m.Message.Contains("Accounts"));
        mockService.Verify(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>()), Times.Once);
    }
}
