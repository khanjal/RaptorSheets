using FluentAssertions;
using GigRaptorLib.Enums;
using GigRaptorLib.Tests.Data.Helpers;
using GigRaptorLib.Utilities.Google;

namespace GigRaptorLib.Tests.Utilities;

public class GoogleSheetHelperTests
{
    private readonly string? _spreadsheetId;
    private GoogleSheetHelper _googleSheetHelper = new GoogleSheetHelper();

    public GoogleSheetHelperTests()
    {
        var configuration = TestConfigurationHelper.GetConfiguration();

        _spreadsheetId = configuration.GetSection("spreadsheet_id").Value;
    }

    [Fact]
    public async void GivenGetAllDataCall_ThenReturnInfo()
    {
        var result = await _googleSheetHelper.GetBatchData(_spreadsheetId!);
        result.Should().NotBeNull();
        result!.ValueRanges.Should().NotBeNull();
        result!.ValueRanges.Should().HaveCount(Enum.GetNames(typeof(SheetEnum)).Length);

        // Test all demo data.

        // Look into replacing individual json sheet tests.
    }

    [Fact]
    public async void GivenGetAllDataCall_WithInvalidSpreadsheetId_ReturnException()
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
    public async void GivenGetSheetDataCall_ThenReturnInfo(SheetEnum sheetEnum)
    {
        var result = await _googleSheetHelper.GetSheetData(_spreadsheetId!, sheetEnum);
        result.Should().NotBeNull();
        result.Values.Should().NotBeNull();

        // Test all demo data.

        // Look into replacing individual json sheet tests.
    }

    [Fact]
    public async void GivenGetSheetDataCall_WithInvalidSpreadsheetId_ReturnException()
    {
        var result = await _googleSheetHelper.GetSheetData("invalid", new SheetEnum());
        result.Should().BeNull();
    }

    [Fact]
    public async void GivenGetSheetProperties_ThenReturnInfo()
    {
        var result = await _googleSheetHelper.GetSheetInfo(_spreadsheetId!);
        result.Should().NotBeNull();
        result!.Properties.Should().NotBeNull();

        result!.Properties.Title.Should().Be("Demo Raptor Gig Sheet");
    }
}
