using RaptorSheets.Core.Factories;
using RaptorSheets.Gig.Managers;

namespace RaptorSheets.Sample.Web.Services;

/// <summary>
/// Connects to the Gig test spreadsheet on first use rather than at startup, so a fresh clone
/// without user secrets set shows a setup message in the page instead of crashing the circuit.
/// </summary>
public class GigConnectionProvider(ISheetManagerFactory<IGoogleSheetManager> factory, IConfiguration configuration)
{
    private IGoogleSheetManager? _manager;
    private string? _error;
    private bool _attempted;

    public bool TryGetManager(out IGoogleSheetManager? manager, out string? error)
    {
        if (!_attempted)
        {
            _attempted = true;
            Connect();
        }

        manager = _manager;
        error = _error;
        return _manager is not null;
    }

    /// <summary>Forgets the cached attempt so the next TryGetManager call reconnects - for after the
    /// setup form writes new secrets, since configuration.Reload() alone doesn't invalidate this.</summary>
    public void Reset()
    {
        _attempted = false;
        _manager = null;
        _error = null;
    }

    private void Connect()
    {
        // Same keys RaptorSheets.Test.Common reads (see TestConfigurationHelpers) - the two
        // projects share one UserSecretsId, so they need to agree on the shape too.
        var spreadsheetId = configuration["spreadsheets:gig"];
        var credentials = configuration.GetSection("google_credentials").Get<Dictionary<string, string>>();

        if (string.IsNullOrWhiteSpace(spreadsheetId) || credentials is not { Count: > 0 })
        {
            _error = "No Gig spreadsheet configured. Set \"spreadsheets:gig\" and \"google_credentials\" " +
                      "with dotnet user-secrets - see docs/SAMPLE-APP.md.";
            return;
        }

        try
        {
            _manager = factory.Create(credentials, spreadsheetId);
        }
        catch (Exception ex)
        {
            // Genuinely user-input-shaped credentials (missing/empty fields) throw ArgumentException,
            // but malformed PEM/PKCS8 *content* - e.g. a private key truncated by a bad copy-paste -
            // throws whatever Google.Apis.Auth's own ASN.1 decoder happens to throw
            // (NotSupportedException, FormatException, CryptographicException, ...), not something
            // this constructor's own validation controls. This is the boundary where arbitrary
            // user-supplied credential text meets the system, so catching broadly here and showing a
            // message is correct - the alternative is an uncaught exception tearing down the circuit.
            _error = $"Couldn't connect to the Gig spreadsheet: {ex.Message}";
        }
    }
}
