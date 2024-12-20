namespace RaptorSheets.Core.Helpers;

public static class GoogleCredentialHelpers
{
    public static bool IsCredentialFilled(Dictionary<string, string> credentials)
    {
        if (credentials["type"] != string.Empty && credentials["privateKeyId"] != string.Empty && credentials["privateKey"] != string.Empty && credentials["clientEmail"] != string.Empty && credentials["clientId"] != string.Empty)
            return true;

        return false;
    }

    public static bool IsCredentialAndSpreadsheetId(Dictionary<string, string> credentials, string spreadsheetId)
    {
        if (IsCredentialFilled(credentials) && !string.IsNullOrEmpty(spreadsheetId))
            return true;

        return false;
    }
}
