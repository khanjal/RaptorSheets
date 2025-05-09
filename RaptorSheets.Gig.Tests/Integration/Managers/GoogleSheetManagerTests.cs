﻿using Moq;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Gig.Managers;
using RaptorSheets.Gig.Tests.Data.Attributes;
using RaptorSheets.Test.Common.Helpers;
using RaptorSheets.Gig.Tests.Data.Helpers;
using SheetEnum = RaptorSheets.Gig.Enums.SheetEnum;

namespace RaptorSheets.Gig.Tests.Integration.Managers;

public class GoogleSheetManagerTests
{
    private readonly GoogleSheetManager? _googleSheetManager = null;

    private readonly long _currentTime;
    private readonly SheetEnum _sheetEnum;
    private readonly Dictionary<string, string> _credential;

    public GoogleSheetManagerTests()
    {
        var random = new Random();
        _sheetEnum = random.NextEnum<SheetEnum>();
        _currentTime = (long)DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds;

        var spreadsheetId = TestConfigurationHelpers.GetGigSpreadsheet();
        _credential = TestConfigurationHelpers.GetJsonCredential();

        if (GoogleCredentialHelpers.IsCredentialFilled(_credential))
            _googleSheetManager = new GoogleSheetManager(_credential, spreadsheetId);
    }

    [FactCheckUserSecrets]
    public async Task GivenGetSheet_ThenReturnSheetEntity()
    {
        var result = await _googleSheetManager!.GetSheet(_sheetEnum.GetDescription());
        Assert.NotNull(result);
        Assert.Equal(2, result.Messages.Count);
        Assert.Equal(MessageLevelEnum.INFO.GetDescription(), result!.Messages[0].Level);
        Assert.Equal(MessageTypeEnum.GET_SHEETS.GetDescription(), result!.Messages[0].Type);
    }

    [FactCheckUserSecrets]
    public async Task GivenGetSheets_ThenReturnSheetEntity()
    {
        var result = await _googleSheetManager!.GetSheets();
        Assert.NotNull(result);
        Assert.NotNull(result.Messages);
        var messages = result.Messages.Select(x => x.Level == MessageLevelEnum.ERROR.GetDescription());

        Assert.DoesNotContain(true, messages);
    }

    [FactCheckUserSecrets]
    public async Task GivenGetRandomSheet_ThenReturnSheetEntity()
    {
        var result = await _googleSheetManager!.GetSheets(new List<string> { _sheetEnum.GetDescription() });
        Assert.NotNull(result);
        Assert.Equal(2, result!.Messages.Count);
        Assert.Equal(MessageLevelEnum.INFO.GetDescription(), result!.Messages[0].Level);
        Assert.Contains(_sheetEnum.GetDescription(), result!.Messages[0].Message);
        Assert.True(result!.Messages[0].Time >= _currentTime);
    }

    [FactCheckUserSecrets]
    public async Task GivenGetAllSheetsList_ThenReturnSheetEntity()
    {
        var result = await _googleSheetManager!.GetSheets();
        Assert.NotNull(result);
        Assert.Equal(2, result!.Messages.Count);
        Assert.Equal(MessageLevelEnum.INFO.GetDescription(), result!.Messages[0].Level);
        Assert.Contains(_sheetEnum.ToString(), result!.Messages[0].Message);
        Assert.True(result!.Messages[0].Time >= _currentTime);
    }

    [FactCheckUserSecrets]
    public async Task GivenGetSheet_WithInvalidSpreadsheetId_ReturnErrorMessages()
    {
        var googleSheetManager = new GoogleSheetManager(_credential, "invalid");
        var result = await googleSheetManager.GetSheets();
        Assert.NotNull(result);
        Assert.Single(result!.Messages);

        result!.Messages.ForEach(x => Assert.Equal(MessageLevelEnum.ERROR.GetDescription(), x.Level));
    }

    [FactCheckUserSecrets]
    public async Task GivenGetSheet_WithInvalidSpreadsheetIdAndSheet_ReturnSheetErrorMessage()
    {
        var googleSheetManager = new GoogleSheetManager(_credential, "invalid");
        var result = await googleSheetManager.GetSheets(new List<string> { _sheetEnum.GetDescription() });
        Assert.NotNull(result);
        Assert.Single(result!.Messages);
        Assert.Equal(MessageLevelEnum.ERROR.GetDescription(), result!.Messages[0].Level);
        Assert.True(result!.Messages[0].Time >= _currentTime);
    }

    [Fact]
    public async Task GivenAddSheetData_WithValidSheetId_ThenReturnEmpty()
    {
        var googleSheetManager = new Mock<IGoogleSheetManager>();
        googleSheetManager.Setup(x => x.ChangeSheetData(It.IsAny<List<string>>(), It.IsAny<SheetEntity>())).ReturnsAsync(new SheetEntity());
        var result = await googleSheetManager.Object.ChangeSheetData(new List<string>(), new SheetEntity());
        Assert.NotNull(result);
    }

    [FactCheckUserSecrets]
    public async Task GivenAddSheetData_WithData_ThenReturnData()
    {
        var result = await _googleSheetManager!.ChangeSheetData(new List<string> { SheetEnum.TRIPS.GetDescription(), SheetEnum.SHIFTS.GetDescription() }, TestGigHelpers.GenerateShift(ActionTypeEnum.APPEND));
        Assert.NotNull(result);
        Assert.Equal(2, result.Messages.Count);

        foreach (var message in result.Messages)
        {
            Assert.Equal(MessageLevelEnum.INFO.GetDescription(), message.Level);
            Assert.Equal(MessageTypeEnum.SAVE_DATA.GetDescription(), message.Type);
        }
    }

    [Fact]
    public async Task GivenChangeSheetData_WithValidSheetId_ThenReturnEmpty()
    {
        var googleSheetManager = new Mock<IGoogleSheetManager>();
        googleSheetManager.Setup(x => x.ChangeSheetData(It.IsAny<List<string>>(), It.IsAny<SheetEntity>())).ReturnsAsync(new SheetEntity());
        var result = await googleSheetManager.Object.ChangeSheetData(new List<string>(), new SheetEntity());
        Assert.NotNull(result);
    }

    [FactCheckUserSecrets]
    public async Task GivenAppendSheetData_WithData_ThenReturnData()
    {
        var sheetInfo = await _googleSheetManager!.GetSheetProperties([SheetEnum.TRIPS.GetDescription(), SheetEnum.SHIFTS.GetDescription()]);
        var maxShiftId = int.Parse(sheetInfo.FirstOrDefault(x => x.Name == SheetEnum.SHIFTS.GetDescription())!.Attributes!.FirstOrDefault(x => x.Key == PropertyEnum.MAX_ROW_VALUE.GetDescription()).Value);
        var maxTripId = int.Parse(sheetInfo.FirstOrDefault(x => x.Name == SheetEnum.TRIPS.GetDescription())!.Attributes!.FirstOrDefault(x => x.Key == PropertyEnum.MAX_ROW_VALUE.GetDescription()).Value);
        var sheetEntity = TestGigHelpers.GenerateShift(ActionTypeEnum.APPEND, maxShiftId + 1, maxTripId + 1);

        var result = await _googleSheetManager!.ChangeSheetData([SheetEnum.TRIPS.GetDescription(), SheetEnum.SHIFTS.GetDescription()], sheetEntity);
        Assert.NotNull(result);
        Assert.Equal(2, result.Messages.Count);

        foreach (var message in result.Messages)
        {
            Assert.Equal(MessageLevelEnum.INFO.GetDescription(), message.Level);
            Assert.Equal(MessageTypeEnum.SAVE_DATA.GetDescription(), message.Type);
        }
    }

    [FactCheckUserSecrets]
    public async Task GivenDeleteSheetData_WithData_ThenReturnData()
    {
        var data = TestGigHelpers.GenerateShift(ActionTypeEnum.DELETE);
        var result = await _googleSheetManager!.ChangeSheetData(new List<string> { SheetEnum.TRIPS.GetDescription(), SheetEnum.SHIFTS.GetDescription() }, data);
        Assert.NotNull(result);
        Assert.Equal(2, result.Messages.Count);

        foreach (var message in result.Messages)
        {
            Assert.Equal(MessageLevelEnum.INFO.GetDescription(), message.Level);
            Assert.Equal(MessageTypeEnum.SAVE_DATA.GetDescription(), message.Type);
        }
    }

    [FactCheckUserSecrets]
    public async Task GivenUpdateSheetData_WithData_ThenReturnData()
    {
        var result = await _googleSheetManager!.ChangeSheetData(new List<string> { SheetEnum.TRIPS.GetDescription(), SheetEnum.SHIFTS.GetDescription() }, TestGigHelpers.GenerateShift(ActionTypeEnum.UPDATE));
        Assert.NotNull(result);
        Assert.Equal(2, result.Messages.Count);

        foreach (var message in result.Messages)
        {
            Assert.Equal(MessageLevelEnum.INFO.GetDescription(), message.Level);
            Assert.Equal(MessageTypeEnum.SAVE_DATA.GetDescription(), message.Type);
        }
    }

    [FactCheckUserSecrets]
    public async Task GivenCreateSheet_WithValidSheetId_ThenReturnEmpty()
    {
        var googleSheetManager = new Mock<IGoogleSheetManager>();
        googleSheetManager.Setup(x => x.CreateSheets()).ReturnsAsync(new SheetEntity());
        var result = await googleSheetManager.Object.CreateSheets();
        Assert.NotNull(result);
    }

    [FactCheckUserSecrets]
    public async Task GivenCreateSheet_WithValidSheetId_ThenReturnData()
    {
        var result = await _googleSheetManager!.CreateSheets(new List<string> { _sheetEnum.GetDescription() });
        Assert.NotNull(result);
        Assert.Single(result.Messages);
        Assert.Equal(MessageLevelEnum.ERROR.GetDescription(), result.Messages[0].Level);
    }

    [FactCheckUserSecrets]
    public async Task GetSheetProperties_ShouldReturnProperties_WhenSheetsExist()
    {
        // Arrange
        var sheets = new List<string> { SheetEnum.TRIPS.GetDescription(), SheetEnum.SHIFTS.GetDescription() };

        // Act
        var result = await _googleSheetManager!.GetSheetProperties(sheets);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.NotEmpty(result[0].Id);
        Assert.Equal(SheetEnum.TRIPS.GetDescription(), result[0].Name);
        Assert.NotEmpty(result[0].Attributes[PropertyEnum.HEADERS.GetDescription()]); // Look into generating headers from sheet object
        Assert.NotEmpty(result[1].Id);
        Assert.Equal(SheetEnum.SHIFTS.GetDescription(), result[1].Name);
        Assert.NotEmpty(result[1].Attributes[PropertyEnum.HEADERS.GetDescription()]); // Look into generating headers from sheet object
    }

    [FactCheckUserSecrets]
    public async Task GetSheetProperties_ShouldReturnEmptyList_WhenNoSheetsExist()
    {
        // Arrange
        var sheets = new List<string> { "Sheet1", "Sheet2" };

        // Act
        var result = await _googleSheetManager!.GetSheetProperties(sheets);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Empty(result[0].Id);
        Assert.Equal("Sheet1", result[0].Name);
        Assert.Empty(result[0].Attributes[PropertyEnum.HEADERS.GetDescription()]);
        Assert.Empty(result[1].Id);
        Assert.Equal("Sheet2", result[1].Name);
        Assert.Empty(result[1].Attributes[PropertyEnum.HEADERS.GetDescription()]);
    }
}