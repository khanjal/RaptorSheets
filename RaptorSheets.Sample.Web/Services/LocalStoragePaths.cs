using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace RaptorSheets.Sample.Web.Services;

/// <summary>
/// Two separate local (never-committed) folders, deliberately not the same one: credentials
/// (<see cref="UserSecretsWriter"/>) live wherever `dotnet user-secrets` itself puts secrets.json,
/// since RaptorSheets.Test's integration suite reads the exact same file via the standard .NET
/// user-secrets mechanism - that one has to match. Spreadsheet connections
/// (<see cref="LocalConnectionsStore"/>) aren't secret and nothing else needs to find them by
/// convention, so they get this app's own plain local-data folder instead of being nested inside
/// "UserSecrets" - despite the name, neither folder was ever inside the repo tree, so neither needs
/// a .gitignore entry.
/// </summary>
internal static class LocalStoragePaths
{
    public static string GetUserSecretsRootFolder()
    {
        var userSecretsId = Assembly.GetEntryAssembly()?.GetCustomAttribute<UserSecretsIdAttribute>()?.UserSecretsId
            ?? throw new InvalidOperationException("This assembly has no UserSecretsId configured.");

        var userSecretsRoot = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft", "UserSecrets")
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".microsoft", "usersecrets");

        return Path.Combine(userSecretsRoot, userSecretsId);
    }

    /// <summary>This app's own local-data folder (%AppData%\RaptorSheets.Sample.Web on Windows,
    /// ~/.config/RaptorSheets.Sample.Web on Linux/macOS via the same SpecialFolder.ApplicationData
    /// .NET already resolves per-OS) - unrelated to user-secrets, just this app's own state.</summary>
    public static string GetAppDataRootFolder() =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RaptorSheets.Sample.Web");
}
