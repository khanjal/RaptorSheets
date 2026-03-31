using RaptorSheets.Job.Managers;
using RaptorSheets.Test.Common.Helpers;

namespace RaptorSheets.Job.Tests.Data.Attributes;

/// <summary>
/// Skips test if user secrets (Google credentials) are not configured.
/// Use this for integration tests that require actual Google Sheets access.
/// </summary>
public class FactCheckUserSecretsAttribute : FactAttribute
{
    public FactCheckUserSecretsAttribute()
    {
        var credential = TestConfigurationHelpers.GetJsonCredential();

        if (!GoogleCredentialHelpers.IsCredentialFilled(credential))
        {
            Skip = "User secrets not configured. Run: dotnet user-secrets set GoogleCredential '{...}' --project RaptorSheets.Test.Common.csproj";
        }
    }
}

/// <summary>
/// Skips theory if user secrets (Google credentials) are not configured.
/// Use this for integration tests that require actual Google Sheets access.
/// </summary>
public class TheoryCheckUserSecretsAttribute : TheoryAttribute
{
    public TheoryCheckUserSecretsAttribute()
    {
        var credential = TestConfigurationHelpers.GetJsonCredential();

        if (!GoogleCredentialHelpers.IsCredentialFilled(credential))
        {
            Skip = "User secrets not configured. Run: dotnet user-secrets set GoogleCredential '{...}' --project RaptorSheets.Test.Common.csproj";
        }
    }
}
