using FluentAssertions;
using GigRaptorLib.Entities;
using GigRaptorLib.Enums;
using GigRaptorLib.Tests.Data.Helpers;
using GigRaptorLib.Utilities.Extensions;
using GigRaptorLib.Utilities.Google;
using Moq;

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
        result!.Messages[0].Type.Should().Be(MessageEnum.Info.UpperName());
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

        result!.Messages.ForEach(x => x.Type.Should().Be(MessageEnum.Error.UpperName()));
    }

    [Fact]
    public async Task GivenGetSheet_WithInvalidSpreadsheetIdAndSheet_ReturnSheetErrorMessage()
    {
        var googleSheetManager = new GoogleSheetManager(_credential, "invalid");
        var result = await googleSheetManager.GetSheets([ _sheetEnum ]);
        result.Should().NotBeNull();
        result!.Messages.Should().HaveCount(1);
        result!.Messages[0].Type.Should().Be(MessageEnum.Error.UpperName());
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
    public async Task GivenAddSheetData_WithValidSheetId_ThenReturnTrue()
    {
        var googleSheetManager = new Mock<IGoogleSheetManager>();
        googleSheetManager.Setup(x => x.AddSheetData(It.IsAny<List<SheetEnum>>(), It.IsAny<SheetEntity>())).ReturnsAsync(true);
        var result = await googleSheetManager.Object.AddSheetData([new SheetEnum()], new SheetEntity());
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GivenCreateSheet_WithValidSheetId_ThenReturnTrue()
    {
        var googleSheetManager = new Mock<IGoogleSheetManager>();
        googleSheetManager.Setup(x => x.CreateSheets(It.IsAny<List<SheetEnum>>())).ReturnsAsync(It.IsAny<SheetEntity>());
        var result = await googleSheetManager.Object.CreateSheets([new SheetEnum()]);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GivenCreateSheet_WithValidSheetId_ThenReturnData()
    {
        var result = await _googleSheetManager.CreateSheets([_sheetEnum]);
        result.Should().NotBeNull();
        result.Messages.Count.Should().Be(1);
        result.Messages[0].Type.Should().Be(MessageEnum.Error.UpperName());
    }
}
