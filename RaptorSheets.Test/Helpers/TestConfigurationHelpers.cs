using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace RaptorSheets.Test.Common.Helpers;

public static class TestConfigurationHelpers
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
            { "type", _configuration["google_credentials:type"] ?? "service_account" },
            { "privateKeyId", _configuration["google_credentials:private_key_id"] ?? "" },
            { "privateKey", _configuration["google_credentials:private_key"] ?? "" },
            { "clientEmail", _configuration["google_credentials:client_email"] ?? "" },
            { "clientId", _configuration["google_credentials:client_id"] ?? "" }
        };

        return parameters;
    }

    public static string GetGigSpreadsheet()
    {
        GetConfiguration();

        return _configuration["spreadsheets:gig"] ?? string.Empty;
    }

    public static string GetStockSpreadsheet()
    {
        GetConfiguration();

        return _configuration["spreadsheets:stock"] ?? string.Empty;
    }
}
