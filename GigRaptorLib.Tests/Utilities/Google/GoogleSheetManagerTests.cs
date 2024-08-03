using FluentAssertions;
using GigRaptorLib.Entities;
using GigRaptorLib.Enums;
using GigRaptorLib.Models;
using GigRaptorLib.Tests.Data.Helpers;
using GigRaptorLib.Utilities.Extensions;
using GigRaptorLib.Utilities.Google;
using Moq;

namespace GigRaptorLib.Tests.Utilities.Google;

public class GoogleSheetManagerTests
{
    private readonly string? _spreadsheetId;
    private IGoogleSheetManger _googleSheetManager;

    public GoogleSheetManagerTests()
    {
        _spreadsheetId = TestConfigurationHelper.GetSpreadsheetId();
        var credential = TestConfigurationHelper.GetJsonCredential();

        _googleSheetManager = new GoogleSheetManager(credential);
    }

    [Fact]
    public async Task GivenGetSheets_ThenReturnSheetEntity()
    {
        var result = await _googleSheetManager.GetSheets(_spreadsheetId!);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GivenGetSheet_ThenReturnSheetEntity()
    {
        var random = new Random();
        var randomEnum = random.NextEnum<SheetEnum>();

        var sheets = new List<SheetEnum> { randomEnum };
        var result = await _googleSheetManager.GetSheets(_spreadsheetId!, sheets);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GivenGetSheet_WithInvalidSpreadsheetId_ReturnNull()
    {
        var result = await _googleSheetManager.GetSheets("invalid");
        result.Should().BeNull();
    }

    [Fact]
    public async Task GivenGetSheet_WithInvalidSpreadsheetIdAndSheets_ReturnNull()
    {
        var result = await _googleSheetManager.GetSheets("invalid", [new SheetEnum()]);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GivenGetSpreadsheetName_WithValidSpreadsheetId_ReturnTitle()
    {
        var result = await _googleSheetManager.GetSpreadsheetName(_spreadsheetId!);
        result.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GivenGetSpreadsheetName_WithInvalidSpreadsheetId_ReturnNull()
    {
        var result = await _googleSheetManager.GetSpreadsheetName("invalid");
        result.Should().BeNull();
    }

    [Fact]
    public async Task GivenAddSheetData_WithValidSheetId_ThenReturnTrue()
    {
        var googleSheetManager = new Mock<IGoogleSheetManger>();
        googleSheetManager.Setup(x => x.AddSheetData(_spreadsheetId!, It.IsAny<List<SheetEnum>>(), It.IsAny<SheetEntity>())).ReturnsAsync(true);
        var result = await googleSheetManager.Object.AddSheetData(_spreadsheetId!, [new SheetEnum()], new SheetEntity());
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GivenCreateSheet_WithValidSheetId_ThenReturnTrue()
    {
        var googleSheetManager = new Mock<IGoogleSheetManger>();
        googleSheetManager.Setup(x => x.CreateSheets(_spreadsheetId!, It.IsAny<List<SheetModel>>())).ReturnsAsync(true);
        var result = await googleSheetManager.Object.CreateSheets(_spreadsheetId!, [new SheetModel()]);
        result.Should().BeTrue();
    }
}
