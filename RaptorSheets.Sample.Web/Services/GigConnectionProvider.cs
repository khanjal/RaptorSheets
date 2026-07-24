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

    private void Connect()
    {
        var spreadsheetId = configuration["Spreadsheets:Gig"];
        var credentials = configuration.GetSection("GoogleCredentials").Get<Dictionary<string, string>>();

        if (string.IsNullOrWhiteSpace(spreadsheetId) || credentials is not { Count: > 0 })
        {
            _error = "No Gig spreadsheet configured. Set \"Spreadsheets:Gig\" and \"GoogleCredentials\" " +
                      "with dotnet user-secrets - see docs/AUTHENTICATION.md.";
            return;
        }

        try
        {
            _manager = factory.Create(credentials, spreadsheetId);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            _error = $"Couldn't connect to the Gig spreadsheet: {ex.Message}";
        }
    }
}
