using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace GigRaptorLib.Tests.Data.Helpers;

public static class TestConfigurationHelper
{
    private static IConfigurationRoot _configuration = new ConfigurationBuilder().Build(); // TODO: See if there is a better way to handle this.
    public static void GetConfiguration()
    {
        _configuration = new ConfigurationBuilder()
                            .AddEnvironmentVariables() // For GitHub Action Secrets
                            .AddUserSecrets(Assembly.GetExecutingAssembly(), true) // For Local User Secrets
                            .Build();
    }

    public static Dictionary<string, string> GetJsonCredential()
    {
        GetConfiguration();

        var parameters = new Dictionary<string, string>
        {
            { "type", _configuration["google_credentials:type"] ?? "" },
            { "projectId", _configuration["google_credentials:project_id"] ?? "" },
            { "privateKeyId", _configuration["google_credentials:private_key_id"] ?? "" },
            { "privateKey", _configuration["google_credentials:private_key"] ?? "" },
            { "clientEmail", _configuration["google_credentials:client_email"] ?? "" },
            { "clientId", _configuration["google_credentials:client_id"] ?? "" },
            { "tokenUrl", _configuration["google_credentials:token_url"] ?? "" }
        };

        return parameters;
    }

    public static string GetSpreadsheetId()
    {
        GetConfiguration();

        return _configuration["spreadsheet_id"] ?? string.Empty;
    }
}
