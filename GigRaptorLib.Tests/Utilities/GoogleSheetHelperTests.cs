using FluentAssertions;
using GigRaptorLib.Enums;
using GigRaptorLib.Tests.Data.Helpers;
using GigRaptorLib.Utilities.Google;
using Google.Apis.Sheets.v4.Data;

namespace GigRaptorLib.Tests.Utilities;

public class GoogleSheetHelperTests
{
    private readonly string? _spreadsheetId;
    private GoogleSheetHelper _googleSheetHelper;

    public GoogleSheetHelperTests()
    {
        _spreadsheetId = TestConfigurationHelper.GetSpreadsheetId();
        var credential = TestConfigurationHelper.GetJsonCredential();

        _googleSheetHelper = new GoogleSheetHelper(credential);
    }

    [Fact]
    public async void GivenGetAllData_ThenReturnInfo()
    {
        var result = await _googleSheetHelper.GetBatchData(_spreadsheetId!);
        result.Should().NotBeNull();
        result!.ValueRanges.Should().NotBeNull();
        result!.ValueRanges.Should().HaveCount(Enum.GetNames(typeof(SheetEnum)).Length);

        // Test all demo data.

        // Look into replacing individual json sheet tests.
    }

    [Fact]
    public async void GivenGetAllData_WithInvalidSpreadsheetId_ReturnException()
    {
        var result = await _googleSheetHelper.GetBatchData("invalid");
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
        var result = await _googleSheetHelper.GetSheetData(_spreadsheetId!, sheetEnum);
        result.Should().NotBeNull();
        result!.Values.Should().NotBeNull();

        // TODO: Test some data
        // Test all demo data.
    }

    [Fact]
    public async void GivenGetSheetData_WithInvalidSpreadsheetId_ReturnNull()
    {
        var result = await _googleSheetHelper.GetSheetData("invalid", new SheetEnum());
        result.Should().BeNull();
    }

    [Fact]
    public async void GivenGetSheetInfo_WithSheetId_ThenReturnInfo()
    {
        var result = await _googleSheetHelper.GetSheetInfo(_spreadsheetId!);
        result.Should().NotBeNull();
        result!.Properties.Should().NotBeNull();

        result!.Properties.Title.Should().Be("Demo Raptor Gig Sheet");
    }

    [Fact]
    public async void GivenGetSheetInfo_WithInvalidSheetId_ThenReturnNull()
    {
        var result = await _googleSheetHelper.GetSheetInfo("invalid");
        result.Should().BeNull();
    }


    [Fact]
    public async void GivenAppendData_WithValidSheetId_ThenReturnInfo()
    {
        // TODO: Mock this out
        var result = await _googleSheetHelper.AppendData(_spreadsheetId!, new ValueRange(), "");
        result.Should().BeNull();
    }

    [Fact]
    public async void GivenAppendData_WithInvalidSheetId_ThenReturnNull()
    {
        var result = await _googleSheetHelper.AppendData("invalid", new ValueRange(), "");
        result.Should().BeNull();
    }

    [Fact]
    public async void GivenCreateSheets_WithValidSheetId_ThenReturnInfo()
    {
        // TODO: Mock this out
        var result = await _googleSheetHelper.CreateSheets(_spreadsheetId!, []);
        result.Should().BeNull();
    }

    [Fact]
    public async void GivenCreateSheets_WithInvalidSheetId_ThenReturnNull()
    {
        var result = await _googleSheetHelper.CreateSheets("invalid", []);
        result.Should().BeNull();
    }
}
