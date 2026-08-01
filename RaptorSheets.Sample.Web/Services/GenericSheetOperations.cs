using RaptorSheets.Core.Factories;
using RaptorSheets.Core.Models.Google;

namespace RaptorSheets.Sample.Web.Services;

/// <summary>
/// Connects to a spreadsheet with no compiled [Column] schema at all - a "generic" connection
/// (SpreadsheetConnection.Type == "generic"). Deliberately not an ISheetOperations: it carries no
/// domain metadata (SheetsType, ValidationSheetMap, ...) because none of it means anything for an
/// arbitrary sheet, which is exactly what keeps a generic connection structurally incapable of the
/// typed CRUD ITypedConnectedSheet exposes - it only ever hands back the plain IConnectedSheet
/// surface (structure/raw-data reads), which is all Structure Inspector needs.
///
/// Built on Gig's compiled manager type purely as an invisible carrier - never surfaced past
/// IConnectedSheet. This is provably safe, not a hack: every method IConnectedSheet exposes
/// (GetSpreadsheetTitle, GetAllSheetTabNames, GetLiveSheetStructure(s), GetLiveSheet(s)RawValues) is
/// implemented on RaptorSheets.Core.Managers.SheetManagerBase without ever touching that domain's
/// _registry/_canonicalSheetNames - confirmed by reading each implementation directly. Any
/// already-registered domain's manager type would work identically; Gig is picked only because it's
/// always unconditionally registered (see Program.cs).
/// </summary>
public sealed class GenericSheetOperations(
    ISheetManagerFactory<RaptorSheets.Gig.Managers.ISheetManager> factory,
    IConfiguration configuration)
{
    public bool TryConnect(SpreadsheetConnection connection, out IConnectedSheet? sheet, out string? error)
    {
        sheet = null;

        var credentials = configuration.GetSection("google_credentials").Get<Dictionary<string, string>>();

        if (string.IsNullOrWhiteSpace(connection.SpreadsheetId) || credentials is not { Count: > 0 })
        {
            error = $"No spreadsheet configured for \"{connection.Label}\". Add credentials and a " +
                     "spreadsheet ID in Settings.";
            return false;
        }

        try
        {
            var manager = factory.Create(credentials, connection.SpreadsheetId);
            sheet = new ConnectedSheet(manager);
            error = null;
            return true;
        }
        catch (Exception ex)
        {
            // See SheetOperationsBase.TryConnect's identical catch for why this is caught broadly.
            error = $"Couldn't connect to \"{connection.Label}\": {ex.Message}";
            return false;
        }
    }

    /// <summary>Same idea as TryConnect, but for an arbitrary spreadsheet ID rather than a saved
    /// connection - used by Settings' test-ID section to verify spreadsheets:test:{domain} values,
    /// which aren't SpreadsheetConnections and shouldn't become one just to check a title. Never
    /// referenced domain metadata even when this lived on SheetOperationsBase, so moving it here
    /// (rather than duplicating it once per domain) is a cleanup, not a behavior change.</summary>
    public async Task<string?> GetSpreadsheetTitleForIdAsync(string spreadsheetId)
    {
        var credentials = configuration.GetSection("google_credentials").Get<Dictionary<string, string>>();
        if (string.IsNullOrWhiteSpace(spreadsheetId) || credentials is not { Count: > 0 })
        {
            return null;
        }

        try
        {
            var manager = factory.Create(credentials, spreadsheetId);
            return await manager.GetSpreadsheetTitle();
        }
        catch (Exception)
        {
            return null;
        }
    }

    private sealed class ConnectedSheet(RaptorSheets.Gig.Managers.ISheetManager manager) : IConnectedSheet
    {
        public Task<string?> GetSpreadsheetTitleAsync() => manager.GetSpreadsheetTitle();

        public Task<List<string>> GetAllSheetTabNamesAsync() => manager.GetAllSheetTabNames();

        public Task<SheetModel?> GetLiveSheetStructureAsync(string sheetName) => manager.GetLiveSheetStructure(sheetName);

        public Task<Dictionary<string, SheetModel>> GetLiveSheetStructuresAsync(List<string> sheetNames) =>
            manager.GetLiveSheetStructures(sheetNames);

        public Task<List<List<string?>>> GetLiveSheetRawValuesAsync(string sheetName, int maxRows = 200) =>
            manager.GetLiveSheetRawValues(sheetName, maxRows);

        public Task<Dictionary<string, List<List<string?>>>> GetLiveSheetsRawValuesAsync(List<string> sheetNames, int maxRows = 200) =>
            manager.GetLiveSheetsRawValues(sheetNames, maxRows);
    }
}
