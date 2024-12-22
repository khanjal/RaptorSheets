using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Services;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Test.Helpers;

namespace RaptorSheets.Gig.Tests.Data;

public class GoogleDataFixture : IAsyncLifetime // https://xunit.net/docs/shared-context
{
    public async Task InitializeAsync()
    {
        var spreadsheetId = TestConfigurationHelpers.GetGigSpreadsheet();
        var credential = TestConfigurationHelpers.GetJsonCredential();

        if (!GoogleCredentialHelpers.IsCredentialFilled(credential))
            return;

        var googleSheetService = new GoogleSheetService(credential, spreadsheetId);
        var sheets = Enum.GetValues(typeof(SheetEnum)).Cast<SheetEnum>().ToList();
        var result = await googleSheetService.GetBatchData(sheets.Select(x => x.GetDescription()).ToList());

        ValueRanges = result?.ValueRanges;
    }

    Task IAsyncLifetime.DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public IList<MatchedValueRange>? ValueRanges { get; private set; }
}