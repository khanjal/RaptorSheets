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
/// both projects read the same file - the credentials are one service account either way.
///
/// The spreadsheet IDs are split into two independent slots per domain, spreadsheets:live:{domain}
/// and spreadsheets:test:{domain} - both editable here, but kept deliberately separate in the UI
/// (see Settings.razor's two sections) since they mean very different things. spreadsheets:live:* is
/// a user's own real data. spreadsheets:test:* is what RaptorSheets.Test's integration suite reads
/// (see TestConfigurationHelpers) and deletes/regenerates on every run (CleanSlateSheetFixture) -
/// editing it here is a convenience for contributors who'd otherwise reach for the CLI to set up
/// their own local test spreadsheet, not something a typical Sample.Web user needs. Recording real
/// data must never end up on the spreadsheet the tests wipe, which is why SheetOperationsBase only
/// ever falls back to spreadsheets:test:* for *display* when spreadsheets:live:* is unset - it never
/// treats them as interchangeable beyond that read-only convenience. user-secrets is nothing more
/// than a JSON file at a well-known path - this reads it (if it exists), merges in whatever changed,
/// and writes it back, preserving any other keys already there.
/// </summary>
public class UserSecretsWriter(IConfigurationRoot configurationRoot)
{
    public sealed record WriteResult(bool Success, string? Error);

    /// <summary>Everything the Settings page needs to prefill - deliberately never includes the
    /// private key itself (the other 4 credential fields aren't secret, so they're safe to show
    /// and re-edit; the private key stays replace-only).</summary>
    public sealed record SecretsSnapshot(
        string? CredentialType,
        string? PrivateKeyId,
        string? ClientEmail,
        string? ClientId,
        string? GigLiveSpreadsheetId,
        string? StockLiveSpreadsheetId,
        string? JobLiveSpreadsheetId,
        string? HomeLiveSpreadsheetId,
        string? GigTestSpreadsheetId,
        string? StockTestSpreadsheetId,
        string? JobTestSpreadsheetId,
        string? HomeTestSpreadsheetId);

    public SecretsSnapshot GetCurrentConfig() => new(
        configurationRoot["google_credentials:type"],
        configurationRoot["google_credentials:private_key_id"],
        configurationRoot["google_credentials:client_email"],
        configurationRoot["google_credentials:client_id"],
        configurationRoot["spreadsheets:live:gig"],
        configurationRoot["spreadsheets:live:stock"],
        configurationRoot["spreadsheets:live:job"],
        configurationRoot["spreadsheets:live:home"],
        configurationRoot["spreadsheets:test:gig"],
        configurationRoot["spreadsheets:test:stock"],
        configurationRoot["spreadsheets:test:job"],
        configurationRoot["spreadsheets:test:home"]);

    /// <summary>
    /// The 5 credential fields are independently optional - a blank/null value leaves whatever's
    /// already saved untouched. That's because the Settings page prefills type/client_email/
    /// client_id/private_key_id from what's already saved but never the private key, so saving
    /// without having typed a new key must not wipe the existing one - each credential field is
    /// merged individually rather than replaced as an atomic unit.
    ///
    /// The 8 spreadsheet IDs (4 live, 4 test) behave differently: unlike the private key, they're
    /// always visibly prefilled with the current value (nothing about them is secret), so a blank
    /// field here is a deliberate "clear it" rather than "I didn't touch this" - see WriteSettings.
    /// </summary>
    public sealed record SettingsUpdate(
        string? CredentialType,
        string? PrivateKeyId,
        string? PrivateKey,
        string? ClientEmail,
        string? ClientId,
        string? GigLiveSpreadsheetId,
        string? StockLiveSpreadsheetId,
        string? JobLiveSpreadsheetId,
        string? HomeLiveSpreadsheetId,
        string? GigTestSpreadsheetId,
        string? StockTestSpreadsheetId,
        string? JobTestSpreadsheetId,
        string? HomeTestSpreadsheetId);

    /// <summary>
    /// This method only ever writes the discrete field values already on <paramref name="update"/> -
    /// it never parses JSON itself, since "paste the whole key to autofill the fields" is a UI
    /// convenience, not something the write path needs to know about.
    /// </summary>
    public WriteResult WriteSettings(SettingsUpdate update)
    {
        var hasAnyCredentialField = !string.IsNullOrWhiteSpace(update.CredentialType)
            || !string.IsNullOrWhiteSpace(update.PrivateKeyId)
            || !string.IsNullOrWhiteSpace(update.PrivateKey)
            || !string.IsNullOrWhiteSpace(update.ClientEmail)
            || !string.IsNullOrWhiteSpace(update.ClientId);

        var path = GetSecretsPath();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        var secrets = File.Exists(path)
            ? JsonNode.Parse(File.ReadAllText(path)) as JsonObject ?? new JsonObject()
            : new JsonObject();

        if (hasAnyCredentialField)
        {
            var googleCredentials = secrets["google_credentials"] as JsonObject ?? new JsonObject();
            SetIfProvided(googleCredentials, "type", update.CredentialType);
            SetIfProvided(googleCredentials, "private_key_id", update.PrivateKeyId);
            SetIfProvided(googleCredentials, "private_key", update.PrivateKey);
            SetIfProvided(googleCredentials, "client_email", update.ClientEmail);
            SetIfProvided(googleCredentials, "client_id", update.ClientId);
            secrets["google_credentials"] = googleCredentials;
        }

        // Unlike credential fields, blank here means "remove this domain's spreadsheet ID" - the
        // Settings page always submits all 8 (every field is visibly prefilled, so there's no
        // "field genuinely absent" case the way there is for the never-redisplayed private key).
        var spreadsheets = secrets["spreadsheets"] as JsonObject ?? new JsonObject();

        var live = spreadsheets["live"] as JsonObject ?? new JsonObject();
        SetOrRemove(live, "gig", update.GigLiveSpreadsheetId);
        SetOrRemove(live, "stock", update.StockLiveSpreadsheetId);
        SetOrRemove(live, "job", update.JobLiveSpreadsheetId);
        SetOrRemove(live, "home", update.HomeLiveSpreadsheetId);
        spreadsheets["live"] = live;

        var test = spreadsheets["test"] as JsonObject ?? new JsonObject();
        SetOrRemove(test, "gig", update.GigTestSpreadsheetId);
        SetOrRemove(test, "stock", update.StockTestSpreadsheetId);
        SetOrRemove(test, "job", update.JobTestSpreadsheetId);
        SetOrRemove(test, "home", update.HomeTestSpreadsheetId);
        spreadsheets["test"] = test;

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

    private static void SetOrRemove(JsonObject target, string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            target.Remove(key);
        }
        else
        {
            target[key] = value.Trim();
        }
    }

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
