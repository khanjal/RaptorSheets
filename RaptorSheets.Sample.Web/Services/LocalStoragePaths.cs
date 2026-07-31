using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace RaptorSheets.Sample.Web.Services;

/// <summary>
/// The one folder every piece of this app's local (never-committed) state lives in - the same
/// per-user-secrets-id folder `dotnet user-secrets` itself uses for secrets.json, so credentials
/// (<see cref="UserSecretsWriter"/>) and spreadsheet connections (<see cref="LocalConnectionsStore"/>)
/// sit side by side without needing any .gitignore entry - neither was ever inside the repo tree.
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
}
