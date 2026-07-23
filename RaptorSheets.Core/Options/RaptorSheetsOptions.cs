namespace RaptorSheets.Core.Options;

/// <summary>
/// Binds a single spreadsheet and its credentials from configuration, for hosts that talk to one
/// known spreadsheet. Apps where the spreadsheet varies per request or per signed-in user should use
/// the manager factory instead - see <see cref="Factories.ISheetManagerFactory{TManager}"/>.
/// </summary>
public class RaptorSheetsOptions
{
    /// <summary>
    /// Spreadsheet id, i.e. the segment in
    /// <c>https://docs.google.com/spreadsheets/d/SPREADSHEET_ID/edit</c>.
    /// </summary>
    public string? SpreadsheetId { get; set; }

    /// <summary>
    /// OAuth access token. Mutually exclusive with <see cref="ServiceAccountCredentials"/>.
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// Service-account credential fields (type, privateKeyId, privateKey, clientEmail, clientId).
    /// Mutually exclusive with <see cref="AccessToken"/>. Keys may be camelCase or snake_case.
    /// </summary>
    public Dictionary<string, string>? ServiceAccountCredentials { get; set; }

    /// <summary>Transient-failure retry behavior. Defaults to <see cref="Models.GoogleRetryOptions.Default"/>.</summary>
    public Models.GoogleRetryOptions Retry { get; set; } = Models.GoogleRetryOptions.Default;

    /// <summary>
    /// Throws when the options can't produce a working client. Called during service resolution so a
    /// misconfigured host fails at startup with a clear message rather than at the first API call.
    /// </summary>
    /// <param name="domainName">Domain these options were registered for, used in the error message.</param>
    public void Validate(string domainName)
    {
        if (string.IsNullOrWhiteSpace(SpreadsheetId))
        {
            throw new InvalidOperationException(
                $"RaptorSheets {domainName}: {nameof(SpreadsheetId)} is required.");
        }

        var hasAccessToken = !string.IsNullOrWhiteSpace(AccessToken);
        var hasServiceAccount = ServiceAccountCredentials is { Count: > 0 };

        if (hasAccessToken == hasServiceAccount)
        {
            var problem = hasAccessToken ? "both were supplied" : "neither was supplied";

            throw new InvalidOperationException(
                $"RaptorSheets {domainName}: exactly one of {nameof(AccessToken)} or " +
                $"{nameof(ServiceAccountCredentials)} is required, but {problem}.");
        }
    }
}
