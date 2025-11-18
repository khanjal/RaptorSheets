using Moq;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Stock.Entities;
using RaptorSheets.Stock.Managers;
using RaptorSheets.Stock.Tests.Data.Attributes;
using RaptorSheets.Test.Common.Helpers;
using Xunit;
using SheetEnum = RaptorSheets.Stock.Enums.SheetEnum;

namespace RaptorSheets.Stock.Tests.Integration.Managers;

public class GoogleSheetManagerTests
{
    private readonly GoogleSheetManager? _googleSheetManager;

    private readonly long _currentTime;
    private readonly Enums.SheetEnum _sheetEnum;
    private readonly Dictionary<string, string> _credential;

    public GoogleSheetManagerTests()
    {
        var random = new Random();
        _sheetEnum = random.NextEnum<Enums.SheetEnum>();
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

        var result = await _googleSheetManager.GetSheets(new List<Enums.SheetEnum> { _sheetEnum });
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
        var result = await googleSheetManager.GetSheets(new List<Enums.SheetEnum> { _sheetEnum });
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
        var result = await googleSheetManager.Object.AddSheetData(new List<Enums.SheetEnum>(), new SheetEntity());
        Assert.NotNull(result);
    }

    [FactCheckUserSecrets]
    public async Task GivenCreateSheet_WithValidSheetId_ThenReturnEmpty()
    {
        var googleSheetManager = new Mock<IGoogleSheetManager>();
        googleSheetManager.Setup(x => x.CreateSheets(It.IsAny<List<SheetEnum>>())).ReturnsAsync(new SheetEntity());
        var result = await googleSheetManager.Object.CreateSheets(new List<Enums.SheetEnum>());
        Assert.NotNull(result);
    }

    [FactCheckUserSecrets]
    public async Task GivenCreateSheet_WithValidSheetId_ThenReturnData()
    {
        if (_googleSheetManager == null)
            throw new InvalidOperationException("GoogleSheetManager is not initialized.");

        var result = await _googleSheetManager.CreateSheets(new List<Enums.SheetEnum> { _sheetEnum });
        Assert.NotNull(result);
        Assert.Equal(1, result.Messages?.Count);
        Assert.Equal(MessageLevelEnum.ERROR.GetDescription(), result.Messages?[0].Level);
    }
}