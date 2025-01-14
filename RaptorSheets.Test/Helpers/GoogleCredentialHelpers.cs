namespace RaptorSheets.Test.Common.Helpers;

public static class GoogleCredentialHelpers
{
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
