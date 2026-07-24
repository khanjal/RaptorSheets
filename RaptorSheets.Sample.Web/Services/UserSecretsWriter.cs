using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace RaptorSheets.Sample.Web.Services;

/// <summary>
/// Lets the Settings page write straight to the same local secrets.json `dotnet user-secrets` would
/// write to, so a first-time user never has to touch the CLI. Since RaptorSheets.Sample.Web and
/// RaptorSheets.Test (the integration test suite's shared infra) declare the same UserSecretsId,
/// this configures both projects at once - the credentials are one service account either way, and
/// the spreadsheet IDs land under the exact keys TestConfigurationHelpers reads. user-secrets is
/// nothing more than a JSON file at a well-known path - this reads it (if it exists), merges in
/// whatever changed, and writes it back, preserving any other keys already there.
/// </summary>
public class UserSecretsWriter(IConfigurationRoot configurationRoot)
{
    public sealed record WriteResult(bool Success, string? Error);

    /// <summary>Everything the Settings page needs to show as "currently configured" - deliberately
    /// never includes the private key itself, only the client_email (safe to display, confirms
    /// which service account is active without exposing the secret).</summary>
    public sealed record SecretsSnapshot(
        string? ClientEmail,
        string? GigSpreadsheetId,
        string? StockSpreadsheetId,
        string? JobSpreadsheetId,
        string? HomeSpreadsheetId);

    public SecretsSnapshot GetCurrentConfig() => new(
        configurationRoot["google_credentials:client_email"],
        configurationRoot["spreadsheets:gig"],
        configurationRoot["spreadsheets:stock"],
        configurationRoot["spreadsheets:job"],
        configurationRoot["spreadsheets:home"]);

    /// <summary>
    /// Every parameter is independently optional - a blank/null value leaves whatever's already
    /// saved untouched, so this works equally well for "just change the Job spreadsheet ID" and
    /// "replace the whole service account". <paramref name="serviceAccountJson"/> replaces
    /// credentials wholesale when provided (never partially merged - a service account key is one
    /// atomic unit) rather than being re-displayed and edited field-by-field.
    /// </summary>
    public WriteResult WriteSettings(
        string? serviceAccountJson,
        string? gigSpreadsheetId,
        string? stockSpreadsheetId,
        string? jobSpreadsheetId,
        string? homeSpreadsheetId)
    {
        Dictionary<string, string?>? credentialFields = null;

        if (!string.IsNullOrWhiteSpace(serviceAccountJson))
        {
            try
            {
                using var parsed = JsonDocument.Parse(serviceAccountJson);
                var root = parsed.RootElement;
                credentialFields = new Dictionary<string, string?>
                {
                    ["type"] = GetString(root, "type"),
                    ["private_key_id"] = GetString(root, "private_key_id"),
                    ["private_key"] = GetString(root, "private_key"),
                    ["client_email"] = GetString(root, "client_email"),
                    ["client_id"] = GetString(root, "client_id"),
                };
            }
            catch (JsonException)
            {
                return new WriteResult(false,
                    "That doesn't look like valid JSON - paste the whole service-account key file Google Cloud downloaded.");
            }

            var missing = credentialFields.Where(kv => string.IsNullOrWhiteSpace(kv.Value)).Select(kv => kv.Key).ToList();
            if (missing.Count > 0)
            {
                return new WriteResult(false, $"Missing field(s) in the pasted JSON: {string.Join(", ", missing)}.");
            }
        }

        var hasAnySpreadsheetId = !string.IsNullOrWhiteSpace(gigSpreadsheetId)
            || !string.IsNullOrWhiteSpace(stockSpreadsheetId)
            || !string.IsNullOrWhiteSpace(jobSpreadsheetId)
            || !string.IsNullOrWhiteSpace(homeSpreadsheetId);

        if (credentialFields is null && !hasAnySpreadsheetId)
        {
            return new WriteResult(false, "Nothing to save - paste a credentials key and/or fill in a spreadsheet ID.");
        }

        var path = GetSecretsPath();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        var secrets = File.Exists(path)
            ? JsonNode.Parse(File.ReadAllText(path)) as JsonObject ?? new JsonObject()
            : new JsonObject();

        if (credentialFields is not null)
        {
            var googleCredentials = new JsonObject();
            foreach (var (key, value) in credentialFields)
            {
                googleCredentials[key] = value;
            }
            secrets["google_credentials"] = googleCredentials;
        }

        var spreadsheets = secrets["spreadsheets"] as JsonObject ?? new JsonObject();
        SetIfProvided(spreadsheets, "gig", gigSpreadsheetId);
        SetIfProvided(spreadsheets, "stock", stockSpreadsheetId);
        SetIfProvided(spreadsheets, "job", jobSpreadsheetId);
        SetIfProvided(spreadsheets, "home", homeSpreadsheetId);
        secrets["spreadsheets"] = spreadsheets;

        File.WriteAllText(path, secrets.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

        // The JSON file provider underlying user-secrets doesn't watch for changes it made itself
        // via a plain File.WriteAllText - force a reload so the write is visible immediately.
        configurationRoot.Reload();

        return new WriteResult(true, null);
    }

    private static void SetIfProvided(JsonObject target, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            target[key] = value.Trim();
        }
    }

    private static string? GetString(JsonElement root, string property) =>
        root.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static string GetSecretsPath()
    {
        var userSecretsId = Assembly.GetEntryAssembly()?.GetCustomAttribute<UserSecretsIdAttribute>()?.UserSecretsId
            ?? throw new InvalidOperationException("This assembly has no UserSecretsId configured.");

        var userSecretsRoot = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft", "UserSecrets")
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".microsoft", "usersecrets");

        return Path.Combine(userSecretsRoot, userSecretsId, "secrets.json");
    }
}
