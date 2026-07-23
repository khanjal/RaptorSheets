using System.Security.Cryptography;

namespace RaptorSheets.Test.Common.Helpers;

public static class GoogleCredentialHelpers
{
    /// <summary>
    /// Builds service-account parameters carrying a well-formed, freshly generated RSA private key.
    /// <para>
    /// The key is generated per call and never associated with a real Google account, so nothing here
    /// is a secret. It has to be genuine key material rather than a placeholder string because the
    /// credential layer parses it during construction - see
    /// <see cref="CreateMalformedServiceAccountParameters"/> for the case where it cannot.
    /// </para>
    /// </summary>
    public static Dictionary<string, string> CreateServiceAccountParameters(string? privateKeyPem = null)
    {
        using var rsa = RSA.Create(2048);

        return new Dictionary<string, string>
        {
            { "type", "service_account" },
            { "privateKeyId", "test-key-id" },
            { "privateKey", privateKeyPem ?? rsa.ExportPkcs8PrivateKeyPem() },
            { "clientEmail", "test@example.com" },
            { "clientId", "123" }
        };
    }

    /// <summary>
    /// Builds service-account parameters whose private key cannot be parsed as PEM-encoded RSA key
    /// material, for asserting that unusable credentials fail at construction.
    /// </summary>
    public static Dictionary<string, string> CreateMalformedServiceAccountParameters()
    {
        return CreateServiceAccountParameters(
            "-----BEGIN PRIVATE KEY-----\\nMIIB...not-a-real-key...\\n-----END PRIVATE KEY-----\\n");
    }

    public static bool IsCredentialFilled(Dictionary<string, string> credentials)
    {
        if (credentials["type"] != string.Empty && credentials["privateKeyId"] != string.Empty && credentials["privateKey"] != string.Empty && credentials["clientEmail"] != string.Empty && credentials["clientId"] != string.Empty)
            return true;

        return false;
    }

    public static bool IsCredentialAndSpreadsheetId(string spreadsheetId)
    {
        if (IsCredentialFilled(TestConfigurationHelpers.GetJsonCredential()) && !string.IsNullOrEmpty(spreadsheetId))
            return true;

        return false;
    }
}
