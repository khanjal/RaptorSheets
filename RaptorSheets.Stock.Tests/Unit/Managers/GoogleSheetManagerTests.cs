using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Stock.Entities;
using RaptorSheets.Stock.Managers;
using Xunit;
using SheetEnum = RaptorSheets.Stock.Enums.SheetEnum;

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
        Exception? caughtException = Record.Exception(() => new GoogleSheetManager(parameters, "test-spreadsheet"));
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
    public async Task AddSheetData_UnsupportedSheet_AddsErrorMessage()
    {
        var manager = new GoogleSheetManager("token", "spreadsheet");
        var entity = new SheetEntity();
        var result = await manager.AddSheetData([SheetEnum.TICKERS], entity);
        Assert.Contains(result.Messages, m => m.Message.Contains("not supported") || m.Message.Contains("No data to add"));
    }

    [Fact]
    public async Task CreateSheets_WithNullResponse_AddsErrorMessages()
    {
        var manager = new GoogleSheetManager("token", "spreadsheet");
        // This will likely add error messages since the service is not mocked and will return null
        var result = await manager.CreateSheets([SheetEnum.STOCKS]);
        Assert.Contains(result.Messages, m => m.Message.Contains("not created"));
    }
}
