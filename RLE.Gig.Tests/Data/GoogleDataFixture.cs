using Google.Apis.Sheets.v4.Data;
using RLE.Gig.Enums;
using RLE.Gig.Tests.Data.Helpers;
using RLE.Gig.Utilities.Google;

namespace RLE.Gig.Tests.Data;

public class GoogleDataFixture : IAsyncLifetime // https://xunit.net/docs/shared-context
{
    public async Task InitializeAsync()
    {
        var spreadsheetId = TestConfigurationHelper.GetSpreadsheetId();
        var credential = TestConfigurationHelper.GetJsonCredential();

        var googleSheetService = new GoogleSheetService(credential, spreadsheetId);
        var sheets = Enum.GetValues(typeof(SheetEnum)).Cast<SheetEnum>().ToList();
        var result = await googleSheetService.GetBatchData(sheets);

        valueRanges = result?.ValueRanges;
    }

    Task IAsyncLifetime.DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public IList<MatchedValueRange>? valueRanges { get; private set; }
}