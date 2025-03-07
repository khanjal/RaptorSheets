﻿using Moq;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Gig.Managers;
using RaptorSheets.Gig.Tests.Data.Attributes;
using RaptorSheets.Test.Common.Helpers;
using RaptorSheets.Core.Tests.Data.Helpers;

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
        Assert.Single(result.Messages);
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
    public async Task GivenGetSheetsList_ThenReturnSheetEntity()
    {
        var result = await _googleSheetManager!.GetSheets(new List<SheetEnum> { _sheetEnum });
        Assert.NotNull(result);
        Assert.Single(result!.Messages);
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
        Assert.Equal(2, result!.Messages.Count);

        result!.Messages.ForEach(x => Assert.Equal(MessageLevelEnum.ERROR.GetDescription(), x.Level));
    }

    [FactCheckUserSecrets]
    public async Task GivenGetSheet_WithInvalidSpreadsheetIdAndSheet_ReturnSheetErrorMessage()
    {
        var googleSheetManager = new GoogleSheetManager(_credential, "invalid");
        var result = await googleSheetManager.GetSheets(new List<SheetEnum> { _sheetEnum });
        Assert.NotNull(result);
        Assert.Single(result!.Messages);
        Assert.Equal(MessageLevelEnum.ERROR.GetDescription(), result!.Messages[0].Level);
        Assert.True(result!.Messages[0].Time >= _currentTime);
    }

    [FactCheckUserSecrets]
    public async Task GivenGetSpreadsheetName_WithValidSpreadsheetId_ReturnTitle()
    {
        var result = await _googleSheetManager!.GetSpreadsheetName();
        Assert.False(string.IsNullOrWhiteSpace(result));
    }

    [FactCheckUserSecrets]
    public async Task GivenGetSpreadsheetName_WithInvalidSpreadsheetId_ReturnNull()
    {
        var googleSheetManager = new GoogleSheetManager(_credential, "invalid");
        var result = await googleSheetManager.GetSpreadsheetName();
        Assert.Null(result);
    }

    [Fact]
    public async Task GivenAddSheetData_WithValidSheetId_ThenReturnEmpty()
    {
        var googleSheetManager = new Mock<IGoogleSheetManager>();
        googleSheetManager.Setup(x => x.ChangeSheetData(It.IsAny<List<SheetEnum>>(), It.IsAny<SheetEntity>())).ReturnsAsync(new SheetEntity());
        var result = await googleSheetManager.Object.ChangeSheetData(new List<SheetEnum>(), new SheetEntity());
        Assert.NotNull(result);
    }

    [FactCheckUserSecrets]
    public async Task GivenAddSheetData_WithData_ThenReturnData()
    {
        var result = await _googleSheetManager!.ChangeSheetData(new List<SheetEnum> { SheetEnum.TRIPS, SheetEnum.SHIFTS }, GenerateShift(ActionTypeEnum.APPEND));
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
        googleSheetManager.Setup(x => x.ChangeSheetData(It.IsAny<List<SheetEnum>>(), It.IsAny<SheetEntity>())).ReturnsAsync(new SheetEntity());
        var result = await googleSheetManager.Object.ChangeSheetData(new List<SheetEnum>(), new SheetEntity());
        Assert.NotNull(result);
    }

    [FactCheckUserSecrets]
    public async Task GivenAppendSheetData_WithData_ThenReturnData()
    {
        var sheetInfo = await _googleSheetManager!.GetSheetProperties(new List<string> { SheetEnum.TRIPS.GetDescription(), SheetEnum.SHIFTS.GetDescription() });
        var maxShiftId = int.Parse(sheetInfo.FirstOrDefault(x => x.Name == SheetEnum.SHIFTS.GetDescription())!.Attributes!.FirstOrDefault(x => x.Key == PropertyEnum.MAX_ROW_VALUE.GetDescription()).Value);
        var maxTripId = int.Parse(sheetInfo.FirstOrDefault(x => x.Name == SheetEnum.TRIPS.GetDescription())!.Attributes!.FirstOrDefault(x => x.Key == PropertyEnum.MAX_ROW_VALUE.GetDescription()).Value);
        var sheetEntity = GenerateShift(ActionTypeEnum.APPEND, maxShiftId + 1, maxTripId + 1);

        var result = await _googleSheetManager!.ChangeSheetData(new List<SheetEnum> { SheetEnum.TRIPS, SheetEnum.SHIFTS }, sheetEntity);
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
        var data = GenerateShift(ActionTypeEnum.DELETE);
        var result = await _googleSheetManager!.ChangeSheetData(new List<SheetEnum> { SheetEnum.TRIPS, SheetEnum.SHIFTS }, data);
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
        var result = await _googleSheetManager!.ChangeSheetData(new List<SheetEnum> { SheetEnum.TRIPS, SheetEnum.SHIFTS }, GenerateShift(ActionTypeEnum.UPDATE));
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
        googleSheetManager.Setup(x => x.CreateSheets(It.IsAny<List<SheetEnum>>())).ReturnsAsync(new SheetEntity());
        var result = await googleSheetManager.Object.CreateSheets(new List<SheetEnum>());
        Assert.NotNull(result);
    }

    [FactCheckUserSecrets]
    public async Task GivenCreateSheet_WithValidSheetId_ThenReturnData()
    {
        var result = await _googleSheetManager!.CreateSheets(new List<SheetEnum> { _sheetEnum });
        Assert.NotNull(result);
        Assert.Single(result.Messages);
        Assert.Equal(MessageLevelEnum.ERROR.GetDescription(), result.Messages[0].Level);
    }

    [FactCheckUserSecrets]
    public async Task GivenCheckSheets_WithNoHeaderCheck_ThenReturnData()
    {
        var result = await _googleSheetManager!.CheckSheets();
        Assert.NotNull(result);
        Assert.Single(result);
    }

    [FactCheckUserSecrets]
    public async Task GivenCheckSheets_WithHeaderCheck_ThenReturnData()
    {
        var result = await _googleSheetManager!.CheckSheets(true);
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
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

    private static SheetEntity GenerateShift(ActionTypeEnum actionType, int shiftStartId = 2, int tripStartId = 2)
    {
        // Select a service type
        var services = JsonHelpers.LoadJsonData<List<string>>("services");

        // Create shift/trips
        var date = DateTime.Now.ToString("yyyy-MM-dd");
        var random = new Random();
        var number = random.Next();
        var service = $"{actionType.GetDescription()} {number}";

        var sheetEntity = new SheetEntity();
        sheetEntity.Shifts.Add(new ShiftEntity { RowId = shiftStartId, Action = actionType.GetDescription(), Date = date, Number = 1, Service = service, Start = DateTime.Now.ToString("T") });

        // Add random amount of trips
        for (int i = tripStartId; i < random.Next(tripStartId + 1, tripStartId + 5); i++)
        {
            var tripEntity = GenerateTrip();
            tripEntity.Action = actionType.GetDescription();
            tripEntity.RowId = i;
            tripEntity.Date = date;
            tripEntity.Number = 1;
            tripEntity.Service = service;
            tripEntity.Pickup = DateTime.Now.ToString("T");
            tripEntity.Dropoff = DateTime.Now.AddMinutes(10).ToString("T");
            tripEntity.Duration = "00:10:00.000";
            sheetEntity.Trips.Add(tripEntity);
        }

        return sheetEntity;
    }

    private static TripEntity GenerateTrip()
    {
        var random = new Random();
        var pay = Math.Round(random.Next(1, 10) + new decimal(random.NextDouble()), 2);
        var distance = Math.Round(random.Next(0, 20) + new decimal(random.NextDouble()), 1);
        var tip = random.Next(1, 5);
        var place = $"Test Place {random.Next(1, 25)}";
        var name = $"Test Name {random.Next(1, 25)}";
        var startAddress = $"Start Address {random.Next(1, 25)}";
        var endAddress = $"End Address {random.Next(1, 25)}";

        return new TripEntity { Type = "Pickup", Place = place, Pay = pay, Tip = tip, Distance = distance, Name = name, StartAddress = startAddress, EndAddress = endAddress };
    }
}