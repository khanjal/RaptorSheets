using GigRaptorLib.Tests.Data.Helpers;
using GigRaptorLib.Utilities.Google;
using Google.Apis.Sheets.v4.Data;

namespace GigRaptorLib.Tests.Data;


public class GoogleDataFixture : IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        var configuration = TestConfigurationHelper.GetConfiguration();
        var spreadsheetId = configuration.GetSection("spreadsheet_id").Value;

        var googleSheetHelper = new GoogleSheetHelper();
        var result = await googleSheetHelper.GetAllData(spreadsheetId!);

        valueRanges = result;
    }

    Task IAsyncLifetime.DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public IList<MatchedValueRange> valueRanges { get; private set; }
}