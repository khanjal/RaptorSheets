using FluentAssertions;
using GigRaptorLib.Entities;
using GigRaptorLib.Enums;
using GigRaptorLib.Tests.Data.Helpers;
using GigRaptorLib.Utilities.Extensions;
using GigRaptorLib.Utilities.Google;
using Moq;
using System;

namespace GigRaptorLib.Tests.Utilities.Google;

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

        _spreadsheetId = TestConfigurationHelper.GetSpreadsheetId();
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
        var result = await _googleSheetManager.GetSheets([ _sheetEnum ]);
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
        var result = await googleSheetManager.GetSheets([ _sheetEnum ]);
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

    [Fact]
    public async Task GivenAddSheetData_WithData_ThenReturnData()
    {
        // Create shift/trips
        var date = DateTime.Now.ToString("yyyy-MM-dd");
        var random = new Random();
        var number = random.Next();
        var service = $"Test {number}";

        var sheetEntity = new SheetEntity();
        sheetEntity.Shifts.Add(new ShiftEntity { Date = date, Number = 1, Service = service });

        // Loop randomly
        for (int i = 0; i < random.Next(1,5); i++)
        {
            sheetEntity.Trips.Add(new TripEntity { Date = date, Number = 1, Service = service, Type = "Pickup", Pay = Math.Round(random.Next(1, 10) + new decimal(random.NextDouble()),2), Tip = random.Next(1, 5), Distance = Math.Round(random.Next(1, 10) + new decimal(random.NextDouble()),2), Name = "Test Name", StartAddress = "Start Address", EndAddress = "End Address" });
        }

        var result = await _googleSheetManager.AddSheetData([SheetEnum.TRIPS, SheetEnum.SHIFTS], sheetEntity);
        result.Should().NotBeNull();
        //result.Messages.Count.Should().Be(1);
        //result.Messages[0].Type.Should().Be(MessageEnum.Error.UpperName());
    }

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
}
