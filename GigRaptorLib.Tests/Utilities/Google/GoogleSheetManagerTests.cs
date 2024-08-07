using FluentAssertions;
using GigRaptorLib.Entities;
using GigRaptorLib.Enums;
using GigRaptorLib.Models;
using GigRaptorLib.Tests.Data.Helpers;
using GigRaptorLib.Utilities.Extensions;
using GigRaptorLib.Utilities.Google;
using Moq;
using System;

namespace GigRaptorLib.Tests.Utilities.Google;

public class GoogleSheetManagerTests
{
    private readonly string? _spreadsheetId;
    private IGoogleSheetManger _googleSheetManager;

    private long _currentTime;
    private SheetEnum _sheetEnum;

    public GoogleSheetManagerTests()
    {
        var random = new Random();
        _sheetEnum = random.NextEnum<SheetEnum>();
        _currentTime = (long)DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds;

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
        var result = await _googleSheetManager.GetSheets(_spreadsheetId!, [ _sheetEnum ]);
        result.Should().NotBeNull();
        result!.Messages.Should().HaveCount(1);
        result!.Messages[0].Type.Should().Be(MessageEnum.Info.DisplayName());
        result!.Messages[0].Message.Should().Contain(_sheetEnum.ToString());
        result!.Messages[0].Time.Should().BeGreaterThanOrEqualTo(_currentTime);
    }

    [Fact]
    public async Task GivenGetSheet_WithInvalidSpreadsheetId_ReturnErrorMessages()
    {
        var result = await _googleSheetManager.GetSheets("invalid");
        result.Should().NotBeNull();
        result!.Messages.Should().HaveCount(2);

        result!.Messages.ForEach(x => x.Type.Should().Be(MessageEnum.Error.DisplayName()));
    }

    [Fact]
    public async Task GivenGetSheet_WithInvalidSpreadsheetIdAndSheet_ReturnSheetErrorMessage()
    {
        var result = await _googleSheetManager.GetSheets("invalid", [ _sheetEnum ]);
        result.Should().NotBeNull();
        result!.Messages.Should().HaveCount(1);
        result!.Messages[0].Type.Should().Be(MessageEnum.Error.DisplayName());
        result!.Messages[0].Time.Should().BeGreaterThanOrEqualTo(_currentTime);
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
