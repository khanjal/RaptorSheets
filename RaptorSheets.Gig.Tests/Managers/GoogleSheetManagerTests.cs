using FluentAssertions;
using Moq;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Gig.Managers;
using RaptorSheets.Gig.Tests.Data.Attributes;
using RaptorSheets.Test.Common.Helpers;

namespace RaptorSheets.Gig.Tests.Managers;

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
        result.Should().NotBeNull();
        result.Messages.Count.Should().Be(1);
        result!.Messages[0].Level.Should().Be(MessageLevelEnum.INFO.GetDescription());
        result!.Messages[0].Type.Should().Be(MessageTypeEnum.GET_SHEETS.GetDescription());
    }

    [FactCheckUserSecrets]
    public async Task GivenGetSheets_ThenReturnSheetEntity()
    {
        var result = await _googleSheetManager!.GetSheets();
        result.Should().NotBeNull();
        result.Messages.Should().NotBeNull();
        var messages = result.Messages.Select(x => x.Level == MessageLevelEnum.ERROR.GetDescription());

        messages.Should().NotContain(true);
    }

    [FactCheckUserSecrets]
    public async Task GivenGetSheetsList_ThenReturnSheetEntity()
    {
        var result = await _googleSheetManager!.GetSheets(new List<SheetEnum> { _sheetEnum });
        result.Should().NotBeNull();
        result!.Messages.Should().HaveCount(1);
        result!.Messages[0].Level.Should().Be(MessageLevelEnum.INFO.GetDescription());
        result!.Messages[0].Message.Should().Contain(_sheetEnum.ToString());
        result!.Messages[0].Time.Should().BeGreaterThanOrEqualTo(_currentTime);
    }

    [FactCheckUserSecrets]
    public async Task GivenGetSheet_WithInvalidSpreadsheetId_ReturnErrorMessages()
    {
        var googleSheetManager = new GoogleSheetManager(_credential, "invalid");
        var result = await googleSheetManager.GetSheets();
        result.Should().NotBeNull();
        result!.Messages.Should().HaveCount(2);

        result!.Messages.ForEach(x => x.Level.Should().Be(MessageLevelEnum.ERROR.GetDescription()));
    }

    [FactCheckUserSecrets]
    public async Task GivenGetSheet_WithInvalidSpreadsheetIdAndSheet_ReturnSheetErrorMessage()
    {
        var googleSheetManager = new GoogleSheetManager(_credential, "invalid");
        var result = await googleSheetManager.GetSheets([_sheetEnum]);
        result.Should().NotBeNull();
        result!.Messages.Should().HaveCount(1);
        result!.Messages[0].Level.Should().Be(MessageLevelEnum.ERROR.GetDescription());
        result!.Messages[0].Time.Should().BeGreaterThanOrEqualTo(_currentTime);
    }

    [FactCheckUserSecrets]
    public async Task GivenGetSpreadsheetName_WithValidSpreadsheetId_ReturnTitle()
    {
        var result = await _googleSheetManager!.GetSpreadsheetName();
        result.Should().NotBeNullOrWhiteSpace();
    }

    [FactCheckUserSecrets]
    public async Task GivenGetSpreadsheetName_WithInvalidSpreadsheetId_ReturnNull()
    {
        var googleSheetManager = new GoogleSheetManager(_credential, "invalid");
        var result = await googleSheetManager.GetSpreadsheetName();
        result.Should().BeNull();
    }

    [Fact]
    public async Task GivenAddSheetData_WithValidSheetId_ThenReturnEmpty()
    {
        var googleSheetManager = new Mock<IGoogleSheetManager>();
        googleSheetManager.Setup(x => x.ChangeSheetData(It.IsAny<List<SheetEnum>>(), It.IsAny<SheetEntity>(), It.IsAny<ActionTypeEnum>())).ReturnsAsync(new SheetEntity());
        var result = await googleSheetManager.Object.ChangeSheetData([new SheetEnum()], new SheetEntity(), ActionTypeEnum.APPEND);
        result.Should().NotBeNull();
    }

    [FactCheckUserSecrets]
    public async Task GivenAddSheetData_WithData_ThenReturnData()
    {
        var result = await _googleSheetManager!.ChangeSheetData([SheetEnum.TRIPS, SheetEnum.SHIFTS], GenerateShift(), ActionTypeEnum.APPEND);
        result.Should().NotBeNull();
        result.Messages.Count.Should().Be(4);

        foreach (var message in result.Messages)
        {
            message.Level.Should().Be(MessageLevelEnum.INFO.GetDescription());
            message.Type.Should().Be(MessageTypeEnum.ADD_DATA.GetDescription());
        }
    }

    [Fact]
    public async Task GivenChangeSheetData_WithValidSheetId_ThenReturnEmpty()
    {
        var googleSheetManager = new Mock<IGoogleSheetManager>();
        googleSheetManager.Setup(x => x.ChangeSheetData(It.IsAny<List<SheetEnum>>(), It.IsAny<SheetEntity>(), It.IsAny<ActionTypeEnum>())).ReturnsAsync(new SheetEntity());
        var result = await googleSheetManager.Object.ChangeSheetData([new SheetEnum()], new SheetEntity(), new ActionTypeEnum());
        result.Should().NotBeNull();
    }

    [FactCheckUserSecrets]
    public async Task GivenAppendSheetData_WithData_ThenReturnData()
    {
        var result = await _googleSheetManager!.ChangeSheetData(new List<SheetEnum> { SheetEnum.TRIPS, SheetEnum.SHIFTS }, GenerateShift(), ActionTypeEnum.APPEND);
        result.Should().NotBeNull();
        result.Messages.Count.Should().Be(4);

        foreach (var message in result.Messages)
        {
            message.Level.Should().Be(MessageLevelEnum.INFO.GetDescription());
            message.Type.Should().Be(MessageTypeEnum.ADD_DATA.GetDescription());
        }
    }

    [FactCheckUserSecrets]
    public async Task GivenDeleteSheetData_WithData_ThenReturnData()
    {
        var result = await _googleSheetManager!.ChangeSheetData(new List<SheetEnum> { SheetEnum.TRIPS, SheetEnum.SHIFTS }, GenerateShift(), ActionTypeEnum.DELETE);
        result.Should().NotBeNull();
        result.Messages.Count.Should().Be(4);

        foreach (var message in result.Messages)
        {
            message.Level.Should().Be(MessageLevelEnum.INFO.GetDescription());
            message.Type.Should().Be(MessageTypeEnum.DELETE_DATA.GetDescription());
        }
    }

    [FactCheckUserSecrets]
    public async Task GivenUpdateSheetData_WithData_ThenReturnData()
    {
        var result = await _googleSheetManager!.ChangeSheetData(new List<SheetEnum> { SheetEnum.TRIPS, SheetEnum.SHIFTS }, GenerateShift(), ActionTypeEnum.UPDATE);
        result.Should().NotBeNull();
        result.Messages.Count.Should().Be(4);

        foreach (var message in result.Messages)
        {
            message.Level.Should().Be(MessageLevelEnum.INFO.GetDescription());
            message.Type.Should().Be(MessageTypeEnum.UPDATE_DATA.GetDescription());
        }
    }

    [FactCheckUserSecrets]
    public async Task GivenCreateSheet_WithValidSheetId_ThenReturnEmpty()
    {
        var googleSheetManager = new Mock<IGoogleSheetManager>();
        googleSheetManager.Setup(x => x.CreateSheets(It.IsAny<List<SheetEnum>>())).ReturnsAsync(new SheetEntity());
        var result = await googleSheetManager.Object.CreateSheets([new SheetEnum()]);
        result.Should().NotBeNull();
    }

    [FactCheckUserSecrets]
    public async Task GivenCreateSheet_WithValidSheetId_ThenReturnData()
    {
        var result = await _googleSheetManager!.CreateSheets(new List<SheetEnum> { _sheetEnum });
        result.Should().NotBeNull();
        result.Messages.Count.Should().Be(1);
        result.Messages[0].Level.Should().Be(MessageLevelEnum.ERROR.GetDescription());
    }

    [FactCheckUserSecrets]
    public async Task GivenCheckSheets_WithNoHeaderCheck_ThenReturnData()
    {
        var result = await _googleSheetManager!.CheckSheets();
        result.Should().NotBeNull();
        result.Count.Should().Be(1);
    }

    [FactCheckUserSecrets]
    public async Task GivenCheckSheets_WithHeaderCheck_ThenReturnData()
    {
        var result = await _googleSheetManager!.CheckSheets(true);
        result.Should().NotBeNull();
        result.Count.Should().Be(2);
    }

    private static SheetEntity GenerateShift()
    {
        // Create shift/trips
        var date = DateTime.Now.ToString("yyyy-MM-dd");
        var random = new Random();
        var number = random.Next();
        var service = $"Test {number}";

        var sheetEntity = new SheetEntity();
        sheetEntity.Shifts.Add(new ShiftEntity { RowId = 2, Date = date, Number = 1, Service = service });

        // Add random amount of trips
        for (int i = 0; i < random.Next(1, 5); i++)
        {
            var tripEntity = GenerateTrip();
            tripEntity.RowId = i+2;
            tripEntity.Date = date;
            tripEntity.Number = 1;
            tripEntity.Service = service;
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
