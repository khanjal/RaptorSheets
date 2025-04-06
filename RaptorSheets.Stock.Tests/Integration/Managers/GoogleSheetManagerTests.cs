using Moq;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Stock.Entities;
using RaptorSheets.Stock.Enums;
using RaptorSheets.Stock.Managers;
using RaptorSheets.Stock.Tests.Data.Attributes;
using RaptorSheets.Test.Common.Helpers;
using Xunit;

namespace RaptorSheets.Stock.Tests.Integration.Managers;

public class GoogleSheetManagerTests
{
    private readonly GoogleSheetManager? _googleSheetManager;

    private readonly long _currentTime;
    private readonly SheetEnum _sheetEnum;
    private readonly Dictionary<string, string> _credential;

    public GoogleSheetManagerTests()
    {
        var random = new Random();
        _sheetEnum = random.NextEnum<SheetEnum>();
        _currentTime = (long)DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds;

        var spreadsheetId = TestConfigurationHelpers.GetStockSpreadsheet();
        _credential = TestConfigurationHelpers.GetJsonCredential();

        if (GoogleCredentialHelpers.IsCredentialFilled(_credential))
            _googleSheetManager = new GoogleSheetManager(_credential, spreadsheetId);
    }

    [FactCheckUserSecrets]
    public async Task GivenGetSheets_ThenReturnSheetEntity()
    {
        if (_googleSheetManager == null)
            throw new InvalidOperationException("GoogleSheetManager is not initialized.");

        var result = await _googleSheetManager.GetSheets();
        Assert.NotNull(result);
    }

    [FactCheckUserSecrets]
    public async Task GivenGetSheet_ThenReturnSheetEntity()
    {
        if (_googleSheetManager == null)
            throw new InvalidOperationException("GoogleSheetManager is not initialized.");

        var result = await _googleSheetManager.GetSheets(new List<SheetEnum> { _sheetEnum });
        Assert.NotNull(result);
        Assert.Equal(1, result!.Messages?.Count);
        Assert.Equal(MessageLevelEnum.INFO.GetDescription(), result!.Messages?[0].Level);
        Assert.Contains(_sheetEnum.ToString(), result!.Messages?[0].Message);
        Assert.True(result!.Messages?[0].Time >= _currentTime);
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
        Assert.Equal(1, result!.Messages?.Count);
        Assert.Equal(MessageLevelEnum.ERROR.GetDescription(), result!.Messages?[0].Level);
        Assert.True(result!.Messages?[0].Time >= _currentTime);
    }

    [FactCheckUserSecrets]
    public async Task GivenAddSheetData_WithValidSheetId_ThenReturnEmpty()
    {
        var googleSheetManager = new Mock<IGoogleSheetManager>();
        googleSheetManager.Setup(x => x.AddSheetData(It.IsAny<List<SheetEnum>>(), It.IsAny<SheetEntity>())).ReturnsAsync(new SheetEntity());
        var result = await googleSheetManager.Object.AddSheetData(new List<SheetEnum>(), new SheetEntity());
        Assert.NotNull(result);
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
        if (_googleSheetManager == null)
            throw new InvalidOperationException("GoogleSheetManager is not initialized.");

        var result = await _googleSheetManager.CreateSheets(new List<SheetEnum> { _sheetEnum });
        Assert.NotNull(result);
        Assert.Equal(1, result.Messages?.Count);
        Assert.Equal(MessageLevelEnum.ERROR.GetDescription(), result.Messages?[0].Level);
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