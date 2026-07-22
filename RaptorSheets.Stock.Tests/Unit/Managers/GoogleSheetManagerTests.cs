using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Stock.Entities;
using RaptorSheets.Stock.Managers;
using Xunit;
using SheetName = RaptorSheets.Stock.Enums.SheetName;

namespace RaptorSheets.Stock.Tests.Unit.Managers;

public class GoogleSheetManagerTests
{
    [Fact]
    public void Constructor_WithAccessToken_ShouldInitialize()
    {
        var manager = new GoogleSheetManager("token", "spreadsheet");
        Assert.NotNull(manager);
    }

    [Fact]
    public void Constructor_WithParameters_ShouldInitialize()
    {
        // Arrange - provide all required parameters for service account authentication
        var parameters = new Dictionary<string, string>
        {
            ["type"] = "service_account",
            ["privateKeyId"] = "test-key-id",
            ["privateKey"] = "invalid-key",
            ["clientEmail"] = "test@example.com",
            ["clientId"] = "123456"
        };

        // Act & Assert - With the new fallback behavior for invalid private keys,
        // construction should not throw and the manager should initialize.
        var caughtException = Record.Exception(() => new GoogleSheetManager(parameters, "test-spreadsheet"));
        Assert.Null(caughtException);
        var manager = new GoogleSheetManager(parameters, "test-spreadsheet");
        Assert.NotNull(manager);
    }

    [Fact]
    public void CheckSheetHeaders_WithNullSpreadsheet_ReturnsError()
    {
        var result = GoogleSheetManager.CheckSheetHeaders(default!);
        Assert.Single(result);
        Assert.Contains("Unable to retrieve sheet(s)", result[0].Message);
    }

    [Fact]
    public void CheckSheetHeaders_WithEmptySheets_ReturnsInfo()
    {
        var spreadsheet = new Spreadsheet { Sheets = [] };
        var result = GoogleSheetManager.CheckSheetHeaders(spreadsheet);
        Assert.Single(result);
        Assert.Contains("No sheet header issues found", result[0].Message);
    }

    [Fact]
    public void CheckSheetHeaders_WithStockSheet_HandlesGracefully()
    {
        var spreadsheet = new Spreadsheet
        {
            Sheets =
            [
                new()
                {
                    Properties = new SheetProperties { Title = "Stocks" },
                    Data =
                    [
                        new()
                        {
                            RowData =
                            [
                                new() { Values = [new() { FormattedValue = "Header1" }] }
                            ]
                        }
                    ]
                }
            ]
        };
        var result = GoogleSheetManager.CheckSheetHeaders(spreadsheet);
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void CheckSheetHeaders_WithMismatchedAccountsHeader_ShouldReportIssue()
    {
        // Accounts (and Tickers) header validation used to be silently skipped (dead/commented-out
        // switch cases) - this confirms the shared registry-backed implementation actually checks them.
        var spreadsheet = new Spreadsheet
        {
            Sheets =
            [
                new()
                {
                    Properties = new SheetProperties { Title = "Accounts" },
                    Data =
                    [
                        new()
                        {
                            RowData =
                            [
                                new() { Values = [new() { FormattedValue = "NotARealHeader" }] }
                            ]
                        }
                    ]
                }
            ]
        };

        var result = GoogleSheetManager.CheckSheetHeaders(spreadsheet);

        Assert.Contains(result, m => m.Message.Contains("Found sheet header issue(s)"));
    }

    [Fact]
    public void CheckUnknownSheets_WithUnknownTab_ShouldReturnWarning()
    {
        var spreadsheet = new Spreadsheet
        {
            Sheets =
            [
                new() { Properties = new SheetProperties { Title = "Stocks" } },
                new() { Properties = new SheetProperties { Title = "SomeRandomTab" } }
            ]
        };

        var result = GoogleSheetManager.CheckUnknownSheets(spreadsheet);

        Assert.Contains(result, m => m.Message.Contains("SomeRandomTab") && m.Message.Contains("does not match any known sheet name"));
    }

    [Fact]
    public void CheckUnknownSheets_WithOnlyKnownSheets_ShouldReturnNoWarnings()
    {
        var spreadsheet = new Spreadsheet
        {
            Sheets =
            [
                new() { Properties = new SheetProperties { Title = "Accounts" } },
                new() { Properties = new SheetProperties { Title = "Stocks" } },
                new() { Properties = new SheetProperties { Title = "Tickers" } }
            ]
        };

        var result = GoogleSheetManager.CheckUnknownSheets(spreadsheet);

        Assert.Empty(result);
    }

    [Fact]
    public void GetSheetLayout_WithValidSheet_ReturnsModel()
    {
        var manager = new GoogleSheetManager("token", "spreadsheet");
        var model = manager.GetSheetLayout("Stocks");
        Assert.NotNull(model);
    }

    [Fact]
    public void GetSheetLayout_WithInvalidSheet_ReturnsNull()
    {
        var manager = new GoogleSheetManager("token", "spreadsheet");
        var model = manager.GetSheetLayout("InvalidSheet");
        Assert.Null(model);
    }

    [Fact]
    public void GetSheetLayouts_MixedSheets_ReturnsOnlyValid()
    {
        var manager = new GoogleSheetManager("token", "spreadsheet");
        var layouts = manager.GetSheetLayouts(["Stocks", "InvalidSheet", "Accounts"]);
        Assert.Contains(layouts, l => l != null);
        Assert.DoesNotContain(layouts, l => l == null);
    }

    [Fact]
    public async Task GetSheet_WithInvalidSheet_ReturnsError()
    {
        var manager = new GoogleSheetManager("token", "spreadsheet");
        var result = await manager.GetSheet("InvalidSheet");
        Assert.Single(result.Messages);
        Assert.Contains("does not exist", result.Messages[0].Message);
    }

    [Fact]
    public async Task ChangeSheetData_WithEmptyEntity_ReturnsWarningMessage()
    {
        // Tickers has no accessor entry at all (fully formula-driven, nothing to change); Stocks
        // has an accessor but an empty SheetEntity carries no Stocks rows to change either way.
        var manager = new GoogleSheetManager("token", "spreadsheet");
        var entity = new SheetEntity();
        var result = await manager.ChangeSheetData([SheetName.TICKERS.GetDescription(), SheetName.STOCKS.GetDescription()], entity);
        Assert.Contains(result.Messages, m => m.Message.Contains("No data to change"));
    }

    [Fact]
    public async Task CreateSheets_WithNullResponse_AddsErrorMessages()
    {
        var manager = new GoogleSheetManager("token", "spreadsheet");
        // This will likely add error messages since the service is not mocked and will return null
        var result = await manager.CreateSheets([SheetName.STOCKS.GetDescription()]);
        Assert.Contains(result.Messages, m => m.Message.Contains("not created"));
    }

    [Fact]
    public async Task ChangeSheetData_WithStockShares_ShouldProcessRequest()
    {
        // Ticker/Account/Shares are the genuinely user-insertable columns on the Stocks sheet -
        // everything else (including Name) is a header-row ARRAYFORMULA. This exercises the
        // accessor -> Core's ChangeSheetData<T>/CreateUpdateCellRequests<T> -> StockMapper.MapToRowData
        // path end to end.
        var manager = new GoogleSheetManager("token", "spreadsheet");
        var sheetEntity = new SheetEntity
        {
            Sheets = { Stocks = { new StockEntity { RowId = 2, Shares = 10 } } }
        };

        var result = await manager.ChangeSheetData([SheetName.STOCKS.GetDescription()], sheetEntity);

        Assert.NotNull(result);
        // No mocked service, so this will fail to actually save, but should get past request-building
        // and attempt the save (rather than short-circuiting on "No data to change").
        Assert.DoesNotContain(result.Messages, m => m.Message.Contains("No data to change"));
        Assert.Contains(result.Messages, m => m.Message.Contains("Unable to save data"));
    }

    [Fact]
    public async Task DeleteSheets_WithEmptyList_ShouldReturnInfoMessage()
    {
        var manager = new GoogleSheetManager("token", "spreadsheet");
        var result = await manager.DeleteSheets([]);
        Assert.Single(result.Messages);
        Assert.Contains("No sheets found to delete", result.Messages[0].Message);
    }

    [Fact]
    public async Task DeleteSheets_WithSheetNames_ShouldAttemptDeletion()
    {
        var manager = new GoogleSheetManager("token", "spreadsheet");
        var result = await manager.DeleteSheets([SheetName.STOCKS.GetDescription()]);
        Assert.NotNull(result);
        Assert.NotEmpty(result.Messages);
    }
}
