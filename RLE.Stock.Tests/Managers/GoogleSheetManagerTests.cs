using FluentAssertions;
using Moq;
using RLE.Core.Enums;
using RLE.Core.Extensions;
using Xunit;
using RLE.Stock.Entities;
using RLE.Stock.Enums;
using RLE.Stock.Managers;
using RLE.Test.Helpers;

namespace RLE.Stock.Tests.Managers;

public class GoogleSheetManagerTests
{
    private readonly string? _spreadsheetId;
    private readonly IGoogleSheetManager _googleSheetManager;

    private readonly long _currentTime;
    private readonly SheetEnum _sheetEnum;
    private readonly Dictionary<string, string> _credential;

    public GoogleSheetManagerTests()
    {
        var random = new Random();
        _sheetEnum = random.NextEnum<SheetEnum>();
        _currentTime = (long)DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds;

        _spreadsheetId = TestConfigurationHelper.GetStockSpreadsheet();
        _credential = TestConfigurationHelper.GetJsonCredential();

        _googleSheetManager = new GoogleSheetManager(_credential, _spreadsheetId);
    }

    [Fact]
    public async Task GivenGetSheets_ThenReturnSheetEntity()
    {
        var result = await _googleSheetManager.GetSheets();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GivenGetSheet_ThenReturnSheetEntity()
    {
        var result = await _googleSheetManager.GetSheets([_sheetEnum]);
        result.Should().NotBeNull();
        result!.Messages.Should().HaveCount(1);
        result!.Messages[0].Level.Should().Be(MessageLevelEnum.Info.UpperName());
        result!.Messages[0].Message.Should().Contain(_sheetEnum.ToString());
        result!.Messages[0].Time.Should().BeGreaterThanOrEqualTo(_currentTime);
    }

    [Fact]
    public async Task GivenGetSheet_WithInvalidSpreadsheetId_ReturnErrorMessages()
    {
        var googleSheetManager = new GoogleSheetManager(_credential, "invalid");
        var result = await googleSheetManager.GetSheets();
        result.Should().NotBeNull();
        result!.Messages.Should().HaveCount(2);

        result!.Messages.ForEach(x => x.Level.Should().Be(MessageLevelEnum.Error.UpperName()));
    }

    [Fact]
    public async Task GivenGetSheet_WithInvalidSpreadsheetIdAndSheet_ReturnSheetErrorMessage()
    {
        var googleSheetManager = new GoogleSheetManager(_credential, "invalid");
        var result = await googleSheetManager.GetSheets([_sheetEnum]);
        result.Should().NotBeNull();
        result!.Messages.Should().HaveCount(1);
        result!.Messages[0].Level.Should().Be(MessageLevelEnum.Error.UpperName());
        result!.Messages[0].Time.Should().BeGreaterThanOrEqualTo(_currentTime);
    }

    [Fact]
    public async Task GivenGetSpreadsheetName_WithValidSpreadsheetId_ReturnTitle()
    {
        var result = await _googleSheetManager.GetSpreadsheetName();
        result.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
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
        googleSheetManager.Setup(x => x.AddSheetData(It.IsAny<List<SheetEnum>>(), It.IsAny<SheetEntity>())).ReturnsAsync(new SheetEntity());
        var result = await googleSheetManager.Object.AddSheetData([new SheetEnum()], new SheetEntity());
        result.Should().NotBeNull();
    }

    //[Fact]
    //public async Task GivenAddSheetData_WithData_ThenReturnData()
    //{
    //    var result = await _googleSheetManager.AddSheetData([SheetEnum.TRIPS, SheetEnum.SHIFTS], GenerateShift());
    //    result.Should().NotBeNull();
    //    result.Messages.Count.Should().Be(4);

    //    foreach (var message in result.Messages)
    //    {
    //        message.Level.Should().Be(MessageLevelEnum.Info.UpperName());
    //        message.Type.Should().Be(MessageTypeEnum.AddData.GetDescription());
    //    }
    //}

    [Fact]
    public async Task GivenCreateSheet_WithValidSheetId_ThenReturnEmpty()
    {
        var googleSheetManager = new Mock<IGoogleSheetManager>();
        googleSheetManager.Setup(x => x.CreateSheets(It.IsAny<List<SheetEnum>>())).ReturnsAsync(new SheetEntity());
        var result = await googleSheetManager.Object.CreateSheets([new SheetEnum()]);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GivenCreateSheet_WithValidSheetId_ThenReturnData()
    {
        var result = await _googleSheetManager.CreateSheets([_sheetEnum]);
        result.Should().NotBeNull();
        result.Messages.Count.Should().Be(1);
        result.Messages[0].Level.Should().Be(MessageLevelEnum.Error.UpperName());
    }

    [Fact]
    public async Task GivenCheckSheets_WithNoHeaderCheck_ThenReturnData()
    {
        var result = await _googleSheetManager.CheckSheets();
        result.Should().NotBeNull();
        result.Count.Should().Be(1);
    }

    [Fact]
    public async Task GivenCheckSheets_WithHeaderCheck_ThenReturnData()
    {
        var result = await _googleSheetManager.CheckSheets(true);
        result.Should().NotBeNull();
        result.Count.Should().Be(2);
    }

    //private static SheetEntity GenerateShift()
    //{
    //    // Create shift/trips
    //    var date = DateTime.Now.ToString("yyyy-MM-dd");
    //    var random = new Random();
    //    var number = random.Next();
    //    var service = $"Test {number}";

    //    var sheetEntity = new SheetEntity();
    //    sheetEntity.Shifts.Add(new ShiftEntity { Date = date, Number = 1, Service = service });

    //    // Add random amount of trips
    //    for (int i = 0; i < random.Next(1, 5); i++)
    //    {
    //        var tripEntity = GenerateTrip();
    //        tripEntity.Date = date;
    //        tripEntity.Number = 1;
    //        tripEntity.Service = service;
    //        sheetEntity.Trips.Add(tripEntity);
    //    }

    //    return sheetEntity;
    //}

    //private static TripEntity GenerateTrip()
    //{
    //    var random = new Random();
    //    var pay = Math.Round(random.Next(1, 10) + new decimal(random.NextDouble()), 2);
    //    var distance = Math.Round(random.Next(0, 20) + new decimal(random.NextDouble()), 1);
    //    var tip = random.Next(1, 5);
    //    var place = $"Test Place {random.Next(1, 25)}";
    //    var name = $"Test Name {random.Next(1, 25)}";
    //    var startAddress = $"Start Address {random.Next(1, 25)}";
    //    var endAddress = $"End Address {random.Next(1, 25)}";

    //    return new TripEntity { Type = "Pickup", Place = place, Pay = pay, Tip = tip, Distance = distance, Name = name, StartAddress = startAddress, EndAddress = endAddress };
    //}
}
