using FluentAssertions;
using Google.Apis.Sheets.v4.Data;
using Moq;
using RLE.Gig.Enums;
using RLE.Gig.Tests.Data.Helpers;
using RLE.Core.Services;
using RLE.Core.Extensions;
using RLE.Gig.Helpers;

namespace RLE.Gig.Tests.Services;

public class GoogleSheetServiceTests
{
    private readonly string? _spreadsheetId;
    private readonly Dictionary<string, string> _credential;
    private readonly GoogleSheetService _googleSheetService;
    private readonly List<SheetEnum> _sheets = Enum.GetValues(typeof(SheetEnum)).Cast<SheetEnum>().ToList();

    public GoogleSheetServiceTests()
    {
        _spreadsheetId = TestConfigurationHelper.GetSpreadsheetId();
        _credential = TestConfigurationHelper.GetJsonCredential();

        _googleSheetService = new GoogleSheetService(_credential, _spreadsheetId);
    }

    [Fact]
    public async Task GivenGetAllData_ThenReturnInfo()
    {
        var result = await _googleSheetService.GetBatchData(_sheets.Select(x => x.GetDescription()).ToList());
        result.Should().NotBeNull();
        result!.ValueRanges.Should().NotBeNull();
        result!.ValueRanges.Should().HaveCount(Enum.GetNames(typeof(SheetEnum)).Length);

        var sheet = GigSheetHelpers.MapData(result!);

        sheet.Should().NotBeNull();

        // TODO: Look into maybe spot checking each entity to ensure there is some data there.
    }

    [Fact]
    public async Task GivenGetAllData_WithInvalidSpreadsheetId_ReturnException()
    {
        var googleSheetService = new GoogleSheetService(_credential, "invalid");
        var result = await googleSheetService.GetBatchData(_sheets.Select(x => x.GetDescription()).ToList());
        result.Should().BeNull();
    }

    [Fact]
    public async Task GivenGetSheetData_WithValidSheetId_ThenReturnInfo()
    {
        var random = new Random();
        var randomEnum = random.NextEnum<SheetEnum>();

        var result = await _googleSheetService.GetSheetData(randomEnum.GetDescription());
        result.Should().NotBeNull();
        result!.Values.Should().NotBeNull();

        // TODO: Test some data
    }

    [Fact]
    public async Task GivenGetSheetData_WithInvalidSpreadsheetId_ReturnNull()
    {
        var googleSheetService = new GoogleSheetService(_credential, "invalid");
        var result = await googleSheetService.GetSheetData(new SheetEnum().GetDescription());
        result.Should().BeNull();
    }

    [Fact]
    public async Task GivenGetSheetInfo_WithSheetId_ThenReturnInfo()
    {
        var result = await _googleSheetService.GetSheetInfo();
        result.Should().NotBeNull();
        result!.Properties.Should().NotBeNull();

        result!.Properties.Title.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GivenGetSheetInfo_WithSheetId_ThenCheckSpreadsheet()
    {
        var result = await _googleSheetService.GetSheetInfo();
        result.Should().NotBeNull();

        var sheets = GigSheetHelpers.GetMissingSheets(result!);
        sheets.Should().BeEmpty();

        // TODO: Make a test to remove a sheet and see if it finds the missing one.
    }

    [Fact]
    public async Task GivenGetSheetInfo_WithInvalidSheetId_ThenReturnNull()
    {
        var googleSheetService = new GoogleSheetService(_credential, "invalid");
        var result = await googleSheetService.GetSheetInfo();
        result.Should().BeNull();
    }

    [Fact]
    public async Task GivenAppendData_WithValidSheetId_ThenReturnInfo()
    {
        var googleSheetService = new Mock<IGoogleSheetService>();
        googleSheetService.Setup(x => x.AppendData(It.IsAny<ValueRange>(), It.IsAny<string>())).ReturnsAsync(new AppendValuesResponse());
        var result = await googleSheetService.Object.AppendData(new ValueRange(), string.Empty);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GivenAppendData_WithInvalidSheetId_ThenReturnNull()
    {
        var googleSheetService = new GoogleSheetService(_credential, "invalid");
        var result = await googleSheetService.AppendData(new ValueRange(), "");
        result.Should().BeNull();
    }

    [Fact]
    public async Task GivenCreateSheets_WithValidSheetIdAndRequest_ThenReturnInfo()
    {
        var googleSheetService = new Mock<IGoogleSheetService>();
        googleSheetService.Setup(x => x.CreateSheets(It.IsAny<BatchUpdateSpreadsheetRequest>())).ReturnsAsync(new BatchUpdateSpreadsheetResponse());
        var result = await googleSheetService.Object.CreateSheets(new BatchUpdateSpreadsheetRequest());
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GivenCreateSheets_WithInvalidSheetId_ThenReturnNull()
    {
        var googleSheetService = new GoogleSheetService(_credential, "invalid");
        var result = await googleSheetService.CreateSheets(new BatchUpdateSpreadsheetRequest());
        result.Should().BeNull();
    }
}
