using System.Text.Json;
using System.Text.Json.Nodes;

namespace RaptorSheets.Sample.Web.Services;

/// <summary>
/// Lets the Settings page write straight to the same local secrets.json `dotnet user-secrets` would
/// write to, so a first-time user never has to touch the CLI. Since RaptorSheets.Sample.Web and
/// RaptorSheets.Test (the integration test suite's shared infra) declare the same UserSecretsId,
/// both projects read the same file - the credentials are one service account either way.
///
/// Only credentials and the 4 spreadsheets:test:{domain} IDs live here. A user's own real
/// spreadsheet connections live in <see cref="LocalConnectionsStore"/> instead (connections.json,
/// same folder, never user secrets) - see that class for why: unlike a single fixed "live" ID per
/// domain, connections are a user-managed list (multiple per domain type allowed, plus a
/// non-strongly-typed "generic" type) and don't belong in a single flat key/value secrets file.
/// spreadsheets:test:* is what RaptorSheets.Test's integration suite reads (see
/// TestConfigurationHelpers) and deletes/regenerates on every run (CleanSlateSheetFixture) - editing
/// it here is a convenience for contributors who'd otherwise reach for the CLI to set up their own
/// local test spreadsheet, not something a typical Sample.Web user needs. user-secrets is nothing
/// more than a JSON file at a well-known path - this reads it (if it exists), merges in whatever
/// changed, and writes it back, preserving any other keys already there.
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
        string? GigTestSpreadsheetId,
        string? StockTestSpreadsheetId,
        string? JobTestSpreadsheetId,
        string? HomeTestSpreadsheetId);

    public SecretsSnapshot GetCurrentConfig() => new(
        configurationRoot["google_credentials:type"],
        configurationRoot["google_credentials:private_key_id"],
        configurationRoot["google_credentials:client_email"],
        configurationRoot["google_credentials:client_id"],
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
    /// The 4 test spreadsheet IDs behave differently: unlike the private key, they're always
    /// visibly prefilled with the current value (nothing about them is secret), so a blank field
    /// here is a deliberate "clear it" rather than "I didn't touch this" - see WriteSettings.
    /// </summary>
    public sealed record SettingsUpdate(
        string? CredentialType,
        string? PrivateKeyId,
        string? PrivateKey,
        string? ClientEmail,
        string? ClientId,
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
        var path = GetSecretsPath();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        var secrets = File.Exists(path)
            ? JsonNode.Parse(File.ReadAllText(path)) as JsonObject ?? new JsonObject()
            : new JsonObject();

        var googleCredentials = secrets["google_credentials"] as JsonObject ?? new JsonObject();
        var spreadsheets = secrets["spreadsheets"] as JsonObject ?? new JsonObject();
        var test = spreadsheets["test"] as JsonObject ?? new JsonObject();

        // `dotnet user-secrets set "spreadsheets:test:gig" "..."` (and, historically, an older
        // version of this method) writes a *flat* top-level key literally named
        // "spreadsheets:test:gig" - JsonNode.Parse treats that as one opaque string key, not a path,
        // so secrets["spreadsheets"] above finds nothing for a secrets.json that only has the flat
        // form. Migrate each flat legacy key's *value* into the nested object it corresponds to
        // before removing it, rather than just deleting it - otherwise a save that never touches
        // that section (e.g. only adding a connection, which still calls this method for the 4
        // always-submitted test-spreadsheet fields) deletes the flat key with nothing written to
        // replace it, silently erasing a value this method never merged in from disk in the first
        // place. This previously wiped a live Google service-account key from a connections-only
        // save. `?? value` on each merge keeps whatever the *new* nested object already has -
        // migration must never override a value this same call is actively setting below.
        var legacyFlatKeys = secrets.Select(kvp => kvp.Key)
            .Where(key => key.StartsWith("google_credentials:", StringComparison.Ordinal) || key.StartsWith("spreadsheets:test:", StringComparison.Ordinal))
            .ToList();

        foreach (var key in legacyFlatKeys)
        {
            var value = secrets[key]?.GetValue<string>();

            if (key.StartsWith("google_credentials:", StringComparison.Ordinal))
            {
                var subKey = key["google_credentials:".Length..];
                googleCredentials[subKey] ??= value;
            }
            else
            {
                var subKey = key["spreadsheets:test:".Length..];
                test[subKey] ??= value;
            }

            secrets.Remove(key);
        }

        // The 5 credential fields are independently optional - see SettingsUpdate's docs - so only
        // ones actually provided this call overwrite the (now legacy-migrated) existing object.
        SetIfProvided(googleCredentials, "type", update.CredentialType);
        SetIfProvided(googleCredentials, "private_key_id", update.PrivateKeyId);
        SetIfProvided(googleCredentials, "private_key", update.PrivateKey);
        SetIfProvided(googleCredentials, "client_email", update.ClientEmail);
        SetIfProvided(googleCredentials, "client_id", update.ClientId);
        secrets["google_credentials"] = googleCredentials;

        // Unlike credential fields, blank here means "remove this domain's test spreadsheet ID" -
        // the Settings page always submits all 4 (every field is visibly prefilled, so there's no
        // "field genuinely absent" case the way there is for the never-redisplayed private key).
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

    private static string GetSecretsPath() => Path.Combine(LocalStoragePaths.GetUserSecretsRootFolder(), "secrets.json");
}
