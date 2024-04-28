using FluentAssertions;
using GigRaptorLib.Enums;
using GigRaptorLib.Models;
using GigRaptorLib.Tests.Data.Helpers;
using GigRaptorLib.Utilities.Google;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Moq;
using static Google.Apis.Sheets.v4.SpreadsheetsResource;

namespace GigRaptorLib.Tests.Utilities;

public class GoogleSheetHelperTests
{
    private readonly string? _spreadsheetId;
    private GoogleSheetService _googleSheetService;

    public GoogleSheetHelperTests()
    {
        _spreadsheetId = TestConfigurationHelper.GetSpreadsheetId();
        var credential = TestConfigurationHelper.GetJsonCredential();

        _googleSheetService = new GoogleSheetService(credential);
    }

    [Fact]
    public async void GivenGetAllData_ThenReturnInfo()
    {
        var result = await _googleSheetService.GetBatchData(_spreadsheetId!);
        result.Should().NotBeNull();
        result!.ValueRanges.Should().NotBeNull();
        result!.ValueRanges.Should().HaveCount(Enum.GetNames(typeof(SheetEnum)).Length);

        // Test all demo data.

        // Look into replacing individual json sheet tests.
    }

    [Fact]
    public async void GivenGetAllData_WithInvalidSpreadsheetId_ReturnException()
    {
        var result = await _googleSheetService.GetBatchData("invalid");
        result.Should().BeNull();
    }

    [Theory]
    [InlineData(SheetEnum.ADDRESSES)]
    [InlineData(SheetEnum.DAILY)]
    [InlineData(SheetEnum.MONTHLY)]
    [InlineData(SheetEnum.NAMES)]
    [InlineData(SheetEnum.PLACES)]
    [InlineData(SheetEnum.REGIONS)]
    [InlineData(SheetEnum.SERVICES)]
    [InlineData(SheetEnum.SHIFTS)]
    [InlineData(SheetEnum.TRIPS)]
    [InlineData(SheetEnum.TYPES)]
    [InlineData(SheetEnum.WEEKDAYS)]
    [InlineData(SheetEnum.WEEKLY)]
    [InlineData(SheetEnum.YEARLY)]
    public async void GivenGetSheetData_WithValidSheetId_ThenReturnInfo(SheetEnum sheetEnum)
    {
        var result = await _googleSheetService.GetSheetData(_spreadsheetId!, sheetEnum);
        result.Should().NotBeNull();
        result!.Values.Should().NotBeNull();

        // TODO: Test some data
        // Test all demo data.
    }

    [Fact]
    public async void GivenGetSheetData_WithInvalidSpreadsheetId_ReturnNull()
    {
        var result = await _googleSheetService.GetSheetData("invalid", new SheetEnum());
        result.Should().BeNull();
    }

    [Fact]
    public async void GivenGetSheetInfo_WithSheetId_ThenReturnInfo()
    {
        var result = await _googleSheetService.GetSheetInfo(_spreadsheetId!);
        result.Should().NotBeNull();
        result!.Properties.Should().NotBeNull();

        result!.Properties.Title.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async void GivenGetSheetInfo_WithInvalidSheetId_ThenReturnNull()
    {
        var result = await _googleSheetService.GetSheetInfo("invalid");
        result.Should().BeNull();
    }


    [Fact]
    public async void GivenAppendData_WithValidSheetId_ThenReturnInfo()
    {
        var googleSheetService = new Mock<IGoogleSheetService>();
        googleSheetService.Setup(x => x.AppendData(_spreadsheetId!, It.IsAny<ValueRange>(), It.IsAny<string>())).ReturnsAsync(new AppendValuesResponse());
        var result = await googleSheetService.Object.AppendData(_spreadsheetId!, new ValueRange(), string.Empty);
        result.Should().NotBeNull();
    }

    [Fact]
    public async void GivenAppendData_WithInvalidSheetId_ThenReturnNull()
    {
        var result = await _googleSheetService.AppendData("invalid", new ValueRange(), "");
        result.Should().BeNull();
    }

    [Fact]
    public async void GivenCreateSheets_WithValidSheetId_ThenReturnInfo()
    {
        var googleSheetService = new Mock<IGoogleSheetService>();
        googleSheetService.Setup(x => x.CreateSheets(_spreadsheetId!, It.IsAny<List<SheetModel>>())).ReturnsAsync(new BatchUpdateSpreadsheetResponse());
        var result = await googleSheetService.Object.CreateSheets(_spreadsheetId!, []);
        result.Should().NotBeNull();
    }

    [Fact]
    public async void GivenCreateSheets_WithInvalidSheetId_ThenReturnNull()
    {
        var result = await _googleSheetService.CreateSheets("invalid", []);
        result.Should().BeNull();
    }
}
