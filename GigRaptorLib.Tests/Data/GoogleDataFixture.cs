using GigRaptorLib.Tests.Data.Helpers;
using GigRaptorLib.Utilities.Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4.Data;

namespace GigRaptorLib.Tests.Data;

public class GoogleDataFixture : IAsyncLifetime // https://xunit.net/docs/shared-context
{
    public async Task InitializeAsync()
    {
        var configuration = TestConfigurationHelper.GetConfiguration();
        var spreadsheetId = configuration.GetSection("spreadsheet_id").Value;

        var jsonCredential = new JsonCredentialParameters
        {
            Type = configuration.GetSection("google_credentials:type").Value,
            ProjectId = configuration.GetSection("google_credentials:project_id").Value,
            PrivateKeyId = configuration.GetSection("google_credentials:private_key_id").Value,
            PrivateKey = configuration.GetSection("google_credentials:private_key").Value,
            ClientEmail = configuration.GetSection("google_credentials:client_email").Value,
            ClientId = configuration.GetSection("google_credentials:client_id").Value,
            TokenUrl = configuration.GetSection("google_credentials:token_url").Value
        };

        var credential = GoogleCredential.FromJsonParameters(jsonCredential);

        var googleSheetHelper = new GoogleSheetHelper(credential);
        var result = await googleSheetHelper.GetBatchData(spreadsheetId!);

        valueRanges = result?.ValueRanges;
    }

    Task IAsyncLifetime.DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public IList<MatchedValueRange>? valueRanges { get; private set; }
}