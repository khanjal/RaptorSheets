using FluentAssertions;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Moq;
using RLE.Core.Services;
using Xunit;

namespace RLE.Gig.Tests.Services;

public class GoogleSheetServiceTests
{
    IGoogleSheetService _googleSheetService;
    Mock<IGoogleSheetService> _mockGoogleSheetService;
    Mock<SheetsService> _mockSheetService;

    public GoogleSheetServiceTests()
    {
        _googleSheetService = new GoogleSheetService("", "");
        _mockGoogleSheetService = new Mock<IGoogleSheetService>();
        _mockSheetService = new Mock<SheetsService>();
    }

    [Fact]
    public async Task GivenGetAllData_ThenReturnInfo()
    {
        //_mockGoogleSheetService.Setup(x => x.GetBatchData(It.IsAny<List<string>>(), It.IsAny<string>())).ReturnsAsync(new BatchGetValuesByDataFilterResponse());
        _mockSheetService.Setup(x => x.)
        var result = await _googleSheetService.GetBatchData([], "");
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GivenGetAllData_WithInvalidSpreadsheetId_ReturnException()
    {
        _mockGoogleSheetService.Setup(x => x.GetBatchData(It.IsAny<List<string>>(), It.IsAny<string>()));
        var result = await _mockGoogleSheetService.Object.GetBatchData([], "");
        result.Should().BeNull();
    }

    [Fact]
    public async Task GivenGetSheetData_WithValidSheetId_ThenReturnInfo()
    {
        _mockGoogleSheetService.Setup(x => x.GetSheetData(It.IsAny<string>())).ReturnsAsync(new ValueRange());

        var result = await _mockGoogleSheetService.Object.GetSheetData("");
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GivenGetSheetData_WithInvalidSpreadsheetId_ReturnNull()
    {
        _mockGoogleSheetService.Setup(x => x.GetSheetData(It.IsAny<string>()));

        var result = await _mockGoogleSheetService.Object.GetSheetData("");
        result.Should().BeNull();
    }

    [Fact]
    public async Task GivenGetSheetInfo_WithSheetId_ThenReturnInfo()
    {
        _mockGoogleSheetService.Setup(x => x.GetSheetInfo()).ReturnsAsync(new Spreadsheet());

        var result = await _mockGoogleSheetService.Object.GetSheetInfo();
        result.Should().NotBeNull();
    }

    //[Fact]
    //public async Task GivenGetSheetInfo_WithSheetId_ThenCheckSpreadsheet()
    //{
    //    var result = await _googleSheetService.GetSheetInfo();
    //    result.Should().NotBeNull();

    //    var sheets = GigSheetHelpers.GetMissingSheets(result!);
    //    sheets.Should().BeEmpty();

    //    // TODO: Make a test to remove a sheet and see if it finds the missing one.
    //}

    [Fact]
    public async Task GivenGetSheetInfo_WithInvalidSheetId_ThenReturnNull()
    {
        _mockGoogleSheetService.Setup(x => x.GetSheetInfo());

        var result = await _mockGoogleSheetService.Object.GetSheetInfo();
        result.Should().BeNull();
    }

    [Fact]
    public async Task GivenAppendData_WithValidSheetId_ThenReturnInfo()
    {
        _mockGoogleSheetService.Setup(x => x.AppendData(It.IsAny<ValueRange>(), It.IsAny<string>())).ReturnsAsync(new AppendValuesResponse());

        var result = await _mockGoogleSheetService.Object.AppendData(new ValueRange(), string.Empty);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GivenAppendData_WithInvalidSheetId_ThenReturnNull()
    {
        _mockGoogleSheetService.Setup(x => x.AppendData(It.IsAny<ValueRange>(), It.IsAny<string>()));

        var result = await _mockGoogleSheetService.Object.AppendData(new ValueRange(), "");
        result.Should().BeNull();
    }

    [Fact]
    public async Task GivenCreateSheets_WithValidSheetIdAndRequest_ThenReturnInfo()
    {
        _mockGoogleSheetService.Setup(x => x.CreateSheets(It.IsAny<BatchUpdateSpreadsheetRequest>())).ReturnsAsync(new BatchUpdateSpreadsheetResponse());

        var result = await _mockGoogleSheetService.Object.CreateSheets(new BatchUpdateSpreadsheetRequest());
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GivenCreateSheets_WithInvalidSheetId_ThenReturnNull()
    {
        _mockGoogleSheetService.Setup(x => x.CreateSheets(It.IsAny<BatchUpdateSpreadsheetRequest>()));

        var result = await _mockGoogleSheetService.Object.CreateSheets(new BatchUpdateSpreadsheetRequest());
        result.Should().BeNull();
    }
}
