using System.Text.Json;

namespace RaptorSheets.Sample.Web.Services;

/// <summary>
/// Reads/writes connections.json - the user's own list of spreadsheet connections, kept deliberately
/// separate from user secrets (see UserSecretsWriter's and LocalStoragePaths' doc comments): unlike a
/// single fixed ID per domain, this is a list (multiple connections per domain type, plus "generic"
/// ones), and none of it is secret, so a flat key/value secrets file was never the right shape for it -
/// nor is nesting it inside a folder literally named "UserSecrets", even though nothing in it actually
/// is one. Lives in this app's own local-data folder (LocalStoragePaths.GetAppDataRootFolder) instead,
/// so it needs no .gitignore entry either way - neither file was ever inside the repo tree.
///
/// Whole-list read/serialize/write, unlike UserSecretsWriter's JsonNode merge - nothing else reads or
/// writes this file, so there's no "preserve unknown keys" concern. Always reads fresh from disk
/// (no in-memory cache), so unlike user secrets there's no reload/invalidation step needed after a
/// write - the next GetAll() just sees it.
///
/// Static, not DI-registered - genuinely stateless (every field below is itself static), matching
/// how SheetMetadata is called directly elsewhere in this project rather than injected.
/// </summary>
public static class LocalConnectionsStore
{
    private static readonly object FileLock = new();
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static IReadOnlyList<SpreadsheetConnection> GetAll()
    {
        lock (FileLock)
        {
            return ReadAll();
        }
    }

    public static SpreadsheetConnection Add(string type, string label, string spreadsheetId)
    {
        var connection = new SpreadsheetConnection(Guid.NewGuid().ToString("N"), type, label, spreadsheetId);

        lock (FileLock)
        {
            var all = ReadAll();
            all.Add(connection);
            WriteAll(all);
        }

        return connection;
    }

    public static void Update(SpreadsheetConnection connection)
    {
        lock (FileLock)
        {
            var all = ReadAll();
            var index = all.FindIndex(c => c.Id == connection.Id);

            if (index < 0)
            {
                return;
            }

            all[index] = connection;
            WriteAll(all);
        }
    }

    public static void Remove(string id)
    {
        lock (FileLock)
        {
            var all = ReadAll();
            all.RemoveAll(c => c.Id == id);
            WriteAll(all);
        }
    }

    private static List<SpreadsheetConnection> ReadAll()
    {
        var path = GetConnectionsPath();

        if (!File.Exists(path))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<SpreadsheetConnection>>(File.ReadAllText(path)) ?? [];
    }

    private static void WriteAll(List<SpreadsheetConnection> connections)
    {
        var path = GetConnectionsPath();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(connections, Options));
    }

    private static string GetConnectionsPath() => Path.Combine(LocalStoragePaths.GetAppDataRootFolder(), "connections.json");
}
