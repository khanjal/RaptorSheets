using FluentAssertions;
using GigRaptorLib.Enums;
using GigRaptorLib.Utilities.Google;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace GigRaptorLib.Tests.Utilities;

public class GoogleSheetHelperTests
{
    private readonly string? _spreadsheetId;
    private GoogleSheetHelper _googleSheetHelper = new GoogleSheetHelper();

    public GoogleSheetHelperTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
            .Build();

        _spreadsheetId = configuration.GetSection("spreadsheet_id").Value;
    }

    [Fact]
    public async void GivenGetAllDataCall_ThenReturnInfo()
    {
        var result = await _googleSheetHelper.GetAllData(_spreadsheetId);
        result.Should().NotBeNull();
        result.Should().HaveCount(Enum.GetNames(typeof(SheetEnum)).Length);

        // Test all demo data.

        // Look into replacing individual json sheet tests.
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
        var result = await _googleSheetHelper.GetSheetData(_spreadsheetId, sheetEnum);
        result.Should().NotBeNull();

        // Test all demo data.

        // Look into replacing individual json sheet tests.
    }
}
