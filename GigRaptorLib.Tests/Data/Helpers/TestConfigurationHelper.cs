using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Configuration;
using System.Reflection;

namespace GigRaptorLib.Tests.Data.Helpers;

public static class TestConfigurationHelper
{
    private static IConfigurationRoot _configuration;
    public static void GetConfiguration()
    {
        _configuration = new ConfigurationBuilder()
                            .AddUserSecrets(Assembly.GetExecutingAssembly())
                            .Build();
    }

    public static GoogleCredential GetJsonCredential()
    {
        GetConfiguration();

        var jsonCredential = new JsonCredentialParameters
        {
            Type = _configuration["google_credentials:type"],
            ProjectId = _configuration["google_credentials:project_id"],
            PrivateKeyId = _configuration["google_credentials:private_key_id"],
            PrivateKey = _configuration["google_credentials:private_key"],
            ClientEmail = _configuration["google_credentials:client_email"],
            ClientId = _configuration["google_credentials:client_id"],
            TokenUrl = _configuration["google_credentials:token_url"]
        };

        var credential = GoogleCredential.FromJsonParameters(jsonCredential);
        return credential;
    }

    public static string GetSpreadsheetId()
    {
        GetConfiguration();

        return _configuration["spreadsheet_id"] ?? string.Empty;
    }
}
