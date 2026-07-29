using Moq;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Stock.Entities;
using RaptorSheets.Stock.Managers;
using RaptorSheets.Stock.Tests.Data.Attributes;
using RaptorSheets.Test.Common.Helpers;
using Xunit;

namespace RaptorSheets.Stock.Tests.Integration.Managers;

[Collection("StockSheetsIntegration")]
public class SheetManagerTests
{
    private readonly SheetManager? _SheetManager;

    private readonly long _currentTime;
    private readonly Enums.SheetName _sheetEnum;
    private readonly Dictionary<string, string> _credential;

    public SheetManagerTests()
    {
        var random = new Random();
        _sheetEnum = random.NextEnum<Enums.SheetName>();
        _currentTime = (long)DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds;

        var spreadsheetId = TestConfigurationHelpers.GetStockSpreadsheet();
        _credential = TestConfigurationHelpers.GetJsonCredential();

        if (GoogleCredentialHelpers.IsCredentialFilled(_credential))
            _SheetManager = new SheetManager(_credential, spreadsheetId);
    }

    [FactCheckUserSecrets]
    public async Task GivenGetSheets_ThenReturnSheetEntity()
    {
        if (_SheetManager == null)
            throw new InvalidOperationException("SheetManager is not initialized.");

        var result = await _SheetManager.GetAllSheets();
        Assert.NotNull(result);
    }

    [FactCheckUserSecrets]
    public async Task GivenGetSheet_ThenReturnSheetEntity()
    {
        if (_SheetManager == null)
            throw new InvalidOperationException("SheetManager is not initialized.");

        var result = await _SheetManager.GetSheets(new List<string> { _sheetEnum.GetDescription() });
        Assert.NotNull(result);
        // Shared orchestration (SheetManagerBase<TEntity>.GetSheets) preserves per-sheet
        // header-validation messages from MapData and appends unknown-tab detection, so the
        // "Retrieved sheet(s)" INFO is no longer guaranteed to be first. Assert the order-independent
        // invariant: an INFO message naming the requested sheet is present.
        Assert.NotEmpty(result!.Messages);
        // The shared orchestration lists provider sheet names (descriptions) in the "Retrieved
        // sheet(s)" INFO, e.g. "Accounts" rather than the enum identifier "ACCOUNTS".
        var retrievedMessage = result!.Messages.FirstOrDefault(m =>
            m.Level == MessageLevel.INFO.GetDescription() && m.Message.Contains(_sheetEnum.GetDescription()));
        Assert.NotNull(retrievedMessage);
        Assert.True(retrievedMessage!.Time >= _currentTime);
    }

    [FactCheckUserSecrets]
    public async Task GivenGetSheet_WithInvalidSpreadsheetId_ReturnErrorMessages()
    {
        var SheetManager = new SheetManager(_credential, "invalid");
        var result = await SheetManager.GetAllSheets();
        Assert.NotNull(result);
        // Shared orchestration returns the "Unable to retrieve sheet(s)" ERROR once the batch fetch
        // and metadata self-heal both fail; assert every message is an ERROR rather than an exact count.
        Assert.NotEmpty(result!.Messages);
        result!.Messages.ForEach(x => Assert.Equal(MessageLevel.ERROR.GetDescription(), x.Level));
    }

    [FactCheckUserSecrets]
    public async Task GivenGetSheet_WithInvalidSpreadsheetIdAndSheet_ReturnSheetErrorMessage()
    {
        var SheetManager = new SheetManager(_credential, "invalid");
        var result = await SheetManager.GetSheets(new List<string> { _sheetEnum.GetDescription() });
        Assert.NotNull(result);
        Assert.Equal(1, result!.Messages?.Count);
        Assert.Equal(MessageLevel.ERROR.GetDescription(), result!.Messages?[0].Level);
        Assert.True(result!.Messages?[0].Time >= _currentTime);
    }

    [FactCheckUserSecrets]
    public async Task GivenChangeSheetData_WithValidSheetId_ThenReturnEmpty()
    {
        var SheetManager = new Mock<ISheetManager>();
        SheetManager.Setup(x => x.ChangeSheetData(It.IsAny<List<string>>(), It.IsAny<SheetEntity>(), It.IsAny<CancellationToken>())).ReturnsAsync(new SheetEntity());
        var result = await SheetManager.Object.ChangeSheetData(new List<string>(), new SheetEntity());
        Assert.NotNull(result);
    }

    [FactCheckUserSecrets]
    public async Task GivenCreateSheet_WithValidSheetId_ThenReturnEmpty()
    {
        var SheetManager = new Mock<ISheetManager>();
        SheetManager.Setup(x => x.CreateSheets(It.IsAny<List<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new SheetEntity());
        var result = await SheetManager.Object.CreateSheets(new List<string>());
        Assert.NotNull(result);
    }

    [FactCheckUserSecrets]
    public async Task GivenCreateSheet_WithValidSheetId_ThenReturnData()
    {
        if (_SheetManager == null)
            throw new InvalidOperationException("SheetManager is not initialized.");

        var result = await _SheetManager.CreateSheets(new List<string> { _sheetEnum.GetDescription() });
        Assert.NotNull(result);
        Assert.Equal(1, result.Messages?.Count);
        Assert.Equal(MessageLevel.ERROR.GetDescription(), result.Messages?[0].Level);
    }
}