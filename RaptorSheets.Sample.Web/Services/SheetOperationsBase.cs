using System.Collections;
using System.Reflection;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Factories;
using RaptorSheets.Core.Managers;
using RaptorSheets.Core.Models.Google;

namespace RaptorSheets.Sample.Web.Services;

/// <summary>
/// The CRUD/connect surface every domain's ISheetOperations shares byte-for-byte, since it all
/// routes through the one generic contract every domain's own manager interface already implements
/// (RaptorSheets.Core.Managers.ISheetManager&lt;TEntity&gt;) and every domain's SheetEntity
/// already extends (RaptorSheets.Core.Entities.SheetEntityBase&lt;TSheets&gt;). A concrete subclass
/// (GigSheetOperations, StockSheetOperations, ...) only needs to supply the handful of things that
/// genuinely differ per domain: the identifying properties (DomainName/DomainLabel/SheetsType/...)
/// and InsertDemoDataAsync, whose implementation differs because Gig has no PopulateDemoData
/// convenience method the way Stock/Job/Home do.
/// </summary>
// S2436: all 3 parameters are load-bearing, not incidental - TManager and TEntity alone can't give
// strongly-typed access to TEntity.Sheets (declared on SheetEntityBase<TSheets>, not on TEntity's
// own generic parameter list), and dropping TSheets in favor of reflection is exactly the tradeoff
// this class exists to avoid (see ChangeSheetDataAsync/GetSheetAsync/GetReferenceValuesAsync below,
// all of which read/write .Sheets directly rather than through PropertyInfo).
#pragma warning disable S2436
public abstract class SheetOperationsBase<TManager, TEntity, TSheets>(
    ISheetManagerFactory<TManager> factory,
    IConfiguration configuration,
    ReferenceSheetCache cache) : ISheetOperations
    where TManager : class, ISheetManager<TEntity>
    where TEntity : SheetEntityBase<TSheets>, new()
    where TSheets : new()
{
    /// <summary>Null until TryGetManager (or the concrete subclass's own typed accessor, if it has
    /// one) has been called at least once - see Connect.</summary>
    protected TManager? Manager { get; private set; }

    private string? _error;
    private bool _attempted;
    private bool _usingTestFallback;

    /// <inheritdoc/>
    public bool UsingTestFallback => _usingTestFallback;

    public abstract string DomainName { get; }
    public abstract string DomainLabel { get; }
    public abstract Type SheetsType { get; }
    public abstract Type? SheetNamesType { get; }
    public abstract IReadOnlySet<string> ExcludedSheetNames { get; }
    public abstract IReadOnlyDictionary<string, string> ValidationSheetMap { get; }

    public bool TryGetManager(out object? manager, out string? error)
    {
        if (!_attempted)
        {
            _attempted = true;
            Connect();
        }

        manager = Manager;
        error = _error;
        return Manager is not null;
    }

    /// <summary>Forgets the cached attempt so the next TryGetManager call reconnects - for after the
    /// setup form writes new secrets, since configuration.Reload() alone doesn't invalidate this.</summary>
    public void Reset()
    {
        _attempted = false;
        Manager = null;
        _error = null;
        _usingTestFallback = false;
    }

    private void Connect()
    {
        // spreadsheets:live:{domain} is the user's own data, written by the Settings page - this is
        // what Connect prefers whenever it's set. spreadsheets:test:{domain} is the dedicated
        // spreadsheet RaptorSheets.Test.Common reads exclusively (see TestConfigurationHelpers) and
        // that suite deletes and regenerates on every run - falling back to it here is only ever a
        // "nothing configured yet, here's something to look at" convenience, flagged via
        // UsingTestFallback so the UI can warn it isn't permanent storage.
        var liveSpreadsheetId = configuration[$"spreadsheets:live:{DomainName}"];
        var testSpreadsheetId = configuration[$"spreadsheets:test:{DomainName}"];
        var credentials = configuration.GetSection("google_credentials").Get<Dictionary<string, string>>();

        var spreadsheetId = string.IsNullOrWhiteSpace(liveSpreadsheetId) ? testSpreadsheetId : liveSpreadsheetId;

        if (string.IsNullOrWhiteSpace(spreadsheetId) || credentials is not { Count: > 0 })
        {
            _error = $"No {DomainLabel} spreadsheet configured. Set \"spreadsheets:live:{DomainName}\" and " +
                      "\"google_credentials\" with dotnet user-secrets - see docs/SAMPLE-APP.md.";
            return;
        }

        _usingTestFallback = string.IsNullOrWhiteSpace(liveSpreadsheetId);

        try
        {
            Manager = factory.Create(credentials, spreadsheetId);
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
            _error = $"Couldn't connect to the {DomainLabel} spreadsheet: {ex.Message}";
            _usingTestFallback = false;
        }
    }

    public async Task<List<string>> GetAllSheetTabNamesAsync() => await Manager!.GetAllSheetTabNames();

    public SheetModel? GetSheetLayout(string sheetName) => Manager!.GetSheetLayout(sheetName);

    public async Task<(object SheetsContainer, List<MessageEntity> Messages)> GetSheetAsync(string sheetName)
    {
        var result = await Manager!.GetSheet(sheetName);
        return (result.Sheets!, result.Messages);
    }

    public async Task<List<MessageEntity>> ChangeSheetDataAsync(string sheetName, PropertyInfo listProperty, IList dirtyRows)
    {
        var entity = new TEntity();
        listProperty.SetValue(entity.Sheets, dirtyRows);
        var result = await Manager!.ChangeSheetData([sheetName], entity);
        return result.Messages;
    }

    public async Task<List<MessageEntity>> CreateSheetAsync(string sheetName)
    {
        var result = await Manager!.CreateSheets([sheetName]);
        return result.Messages;
    }

    public async Task<List<MessageEntity>> CreateAllSheetsAsync()
    {
        var result = await Manager!.CreateAllSheets();
        return result.Messages;
    }

    public Task<Dictionary<string, IReadOnlyList<string>>> GetReferenceValuesAsync(
        IReadOnlyList<SheetDescriptor> referenceDescriptors, CancellationToken cancellationToken = default) =>
        cache.GetIdentityValuesAsync(
            DomainName,
            async (names, ct) => (await Manager!.GetSheets(names, ct)).Sheets!,
            referenceDescriptors,
            cancellationToken);

    public async Task<string?> GetSpreadsheetTitleAsync() => await Manager!.GetSpreadsheetTitle();

    public async Task<string?> GetSpreadsheetTitleForIdAsync(string spreadsheetId)
    {
        var credentials = configuration.GetSection("google_credentials").Get<Dictionary<string, string>>();
        if (string.IsNullOrWhiteSpace(spreadsheetId) || credentials is not { Count: > 0 })
        {
            return null;
        }

        try
        {
            // Deliberately a throwaway manager, not assigned to Manager - this must not disturb the
            // cached TryGetManager/Connect state (which resolves spreadsheets:live:{domain}, falling
            // back to spreadsheets:test:{domain}), since this checks a specific, caller-supplied ID
            // instead.
            var probeManager = factory.Create(credentials, spreadsheetId);
            return await probeManager.GetSpreadsheetTitle();
        }
        catch (Exception)
        {
            // Same reasoning as Connect()'s catch: arbitrary user-pasted text meeting the system here,
            // not a bug - a bad ID or malformed credentials just means "can't verify," not a crash.
            return null;
        }
    }

    /// <inheritdoc cref="ISheetOperations.InsertDemoDataAsync"/>
    public abstract Task<List<MessageEntity>> InsertDemoDataAsync();
}
