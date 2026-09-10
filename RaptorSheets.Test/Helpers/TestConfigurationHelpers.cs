using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace RaptorSheets.Test.Common.Helpers;

public static class TestConfigurationHelpers
{
    private static IConfigurationRoot? _configuration;

    public static void GetConfiguration()
    {
        _configuration ??= new ConfigurationBuilder()
                            .AddEnvironmentVariables() // For GitHub Action Secrets
                            .AddUserSecrets(Assembly.GetExecutingAssembly(), true) // For Local User Secrets
                            .Build();
    }

    public static Dictionary<string, string> GetJsonCredential()
    {
        GetConfiguration();

        var parameters = new Dictionary<string, string>
        {
            { "type", _configuration!["google_credentials:type"] ?? "service_account" },
            { "privateKeyId", _configuration["google_credentials:private_key_id"] ?? "" },
            { "privateKey", _configuration["google_credentials:private_key"] ?? "" },
            { "clientEmail", _configuration["google_credentials:client_email"] ?? "" },
            { "clientId", _configuration["google_credentials:client_id"] ?? "" }
        };

        return parameters;
    }

    // These read the dedicated test spreadsheet under spreadsheets:test:* - never the
    // spreadsheets:live:* slot RaptorSheets.Sample.Web's Settings page writes to, since this suite
    // deletes and regenerates every sheet it touches (see CleanSlateSheetFixture) and must never run
    // against anyone's real data.
    public static string GetGigSpreadsheet()
    {
        GetConfiguration();

        return _configuration!["spreadsheets:test:gig"] ?? string.Empty;
    }

    /// <summary>
    /// Separate spreadsheet for the load tier, so bulk writes cannot disturb the state the contract
    /// and workflow tests read. Returns empty rather than falling back to the shared Gig spreadsheet
    /// - silently borrowing it is exactly what the split exists to prevent.
    /// </summary>
    public static string GetGigLoadSpreadsheet()
    {
        GetConfiguration();
        return _configuration!["spreadsheets:test:gigload"] ?? string.Empty;
    }

    public static string GetStockSpreadsheet()
    {
        GetConfiguration();

        return _configuration!["spreadsheets:test:stock"] ?? string.Empty;
    }

    public static string GetHomeSpreadsheet()
    {
        GetConfiguration();

        return _configuration!["spreadsheets:test:home"] ?? string.Empty;
    }

    public static string GetJobSpreadsheet()
    {
        GetConfiguration();

        return _configuration!["spreadsheets:test:job"] ?? string.Empty;
    }

    /// <summary>
    /// Core's own dedicated test spreadsheet - not shared with any domain. Used for live integration
    /// coverage of domain-agnostic SheetManagerBase plumbing (self-heal, reapply, insertion, dependent
    /// formula refresh) that the four domain-owned spreadsheets above only ever exercised by accident.
    /// </summary>
    public static string GetCoreSpreadsheet()
    {
        GetConfiguration();

        return _configuration!["spreadsheets:test:core"] ?? string.Empty;
    }
}
