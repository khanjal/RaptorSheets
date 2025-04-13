using Google.Apis.Sheets.v4.Data;
using Moq;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Core.Services;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Gig.Helpers;
using RaptorSheets.Gig.Tests.Data.Attributes;
using RaptorSheets.Test.Common.Helpers;

namespace RaptorSheets.Gig.Tests.Integration.Services;

public class GoogleSheetServiceTests
{
    private readonly string? _spreadsheetId;
    private readonly Dictionary<string, string> _credential;
    private readonly GoogleSheetService _googleSheetService = null!;

    private readonly List<string> _sheets = GigSheetHelpers.GetSheetNames();

    public GoogleSheetServiceTests()
    {
        _spreadsheetId = TestConfigurationHelpers.GetGigSpreadsheet();
        _credential = TestConfigurationHelpers.GetJsonCredential();

        if (GoogleCredentialHelpers.IsCredentialFilled(_credential))
            _googleSheetService = new GoogleSheetService(_credential, _spreadsheetId);
    }

    [FactCheckUserSecrets]
    public async Task GivenGetSheetInfo_ThenReturnInfo()
    {
        var result = await _googleSheetService.GetSheetInfo(_sheets);
        Assert.NotNull(result);
        Assert.NotNull(result!.Sheets);
        Assert.Equal(GigSheetHelpers.GetSheetNames().Count, result!.Sheets.Count);

        var sheet = GigSheetHelpers.MapData(result!);

        Assert.NotNull(sheet);

        // TODO: Look into maybe spot checking each entity to ensure there is some data there.
    }

    [FactCheckUserSecrets]
    public async Task GivenGetBatchData_ThenReturnInfo()
    {
        var result = await _googleSheetService.GetBatchData(_sheets);
        Assert.NotNull(result);
        Assert.NotNull(result!.ValueRanges);
        Assert.Equal(GigSheetHelpers.GetSheetNames().Count, result!.ValueRanges.Count);

        var sheet = GigSheetHelpers.MapData(result!);

        Assert.NotNull(sheet);

        // TODO: Look into maybe spot checking each entity to ensure there is some data there.
    }

    [FactCheckUserSecrets]
    public async Task GivenGetAllData_WithInvalidSpreadsheetId_ReturnException()
    {
        var googleSheetService = new GoogleSheetService(_credential, "invalid");
        var result = await googleSheetService.GetBatchData(_sheets);
        Assert.Null(result);
    }

    [FactCheckUserSecrets]
    public async Task GivenGetSheetData_WithValidSheetId_ThenReturnInfo()
    {
        var random = new Random();
        var randomEnum = random.NextEnum<SheetEnum>();

        var result = await _googleSheetService.GetSheetData(randomEnum.GetDescription());
        Assert.NotNull(result);
        Assert.NotNull(result!.Values);

        // TODO: Test some data
    }

    [FactCheckUserSecrets]
    public async Task GivenGetSheetData_WithInvalidSpreadsheetId_ReturnNull()
    {
        var googleSheetService = new GoogleSheetService(_credential, "invalid");
        var result = await googleSheetService.GetSheetData(new SheetEnum().GetDescription());
        Assert.Null(result);
    }

    [FactCheckUserSecrets]
    public async Task GivenGetSheetInfo_WithSheetId_ThenReturnInfo()
    {
        var result = await _googleSheetService.GetSheetInfo();
        Assert.NotNull(result);
        Assert.NotNull(result!.Properties);

        Assert.False(string.IsNullOrEmpty(result!.Properties.Title));
    }

    [FactCheckUserSecrets]
    public async Task GivenGetSheetInfo_WithSheetId_ThenCheckSpreadsheet()
    {
        var result = await _googleSheetService.GetSheetInfo();
        Assert.NotNull(result);

        var sheets = GigSheetHelpers.GetMissingSheets(result!);
        Assert.Empty(sheets);

        // TODO: Make a test to remove a sheet and see if it finds the missing one.
    }

    [FactCheckUserSecrets]
    public async Task GivenGetSheetInfo_WithInvalidSheetId_ThenReturnNull()
    {
        var googleSheetService = new GoogleSheetService(_credential, "invalid");
        var result = await googleSheetService.GetSheetInfo();
        Assert.Null(result);
    }

    [Fact]
    public async Task GivenAppendData_WithValidSheetId_ThenReturnInfo()
    {
        var googleSheetService = new Mock<IGoogleSheetService>();
        googleSheetService.Setup(x => x.AppendData(It.IsAny<ValueRange>(), It.IsAny<string>())).ReturnsAsync(new AppendValuesResponse());
        var result = await googleSheetService.Object.AppendData(new ValueRange(), string.Empty);
        Assert.NotNull(result);
    }

    [FactCheckUserSecrets]
    public async Task GivenAppendData_WithInvalidSheetId_ThenReturnNull()
    {
        var googleSheetService = new GoogleSheetService(_credential, "invalid");
        var result = await googleSheetService.AppendData(new ValueRange(), "");
        Assert.Null(result);
    }

    [Fact]
    public async Task GivenCreateSheets_WithValidSheetIdAndRequest_ThenReturnInfo()
    {
        var googleSheetService = new Mock<IGoogleSheetService>();
        googleSheetService.Setup(x => x.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>())).ReturnsAsync(new BatchUpdateSpreadsheetResponse());
        var result = await googleSheetService.Object.BatchUpdateSpreadsheet(new BatchUpdateSpreadsheetRequest());
        Assert.NotNull(result);
    }

    [FactCheckUserSecrets]
    public async Task GivenCreateSheets_WithInvalidSheetId_ThenReturnNull()
    {
        var googleSheetService = new GoogleSheetService(_credential, "invalid");
        var result = await googleSheetService.BatchUpdateSpreadsheet(new BatchUpdateSpreadsheetRequest());
        Assert.Null(result);
    }
}