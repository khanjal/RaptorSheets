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
/// (RaptorSheets.Core.Managers.IGoogleSheetManager&lt;TEntity&gt;) and every domain's SheetEntity
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
    where TManager : class, IGoogleSheetManager<TEntity>
    where TEntity : SheetEntityBase<TSheets>, new()
    where TSheets : new()
{
    /// <summary>Null until TryGetManager (or the concrete subclass's own typed accessor, if it has
    /// one) has been called at least once - see Connect.</summary>
    protected TManager? Manager { get; private set; }

    private string? _error;
    private bool _attempted;

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
    }

    private void Connect()
    {
        // Same keys RaptorSheets.Test.Common reads (see TestConfigurationHelpers) - Sample.Web and
        // Test.Common share one UserSecretsId, so they need to agree on the shape too.
        var spreadsheetId = configuration[$"spreadsheets:{DomainName}"];
        var credentials = configuration.GetSection("google_credentials").Get<Dictionary<string, string>>();

        if (string.IsNullOrWhiteSpace(spreadsheetId) || credentials is not { Count: > 0 })
        {
            _error = $"No {DomainLabel} spreadsheet configured. Set \"spreadsheets:{DomainName}\" and " +
                      "\"google_credentials\" with dotnet user-secrets - see docs/SAMPLE-APP.md.";
            return;
        }

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

    public async Task<string?> GetSpreadsheetTitleAsync()
    {
        var info = await Manager!.GetSpreadsheetInfo();
        return info?.Properties?.Title;
    }

    /// <inheritdoc cref="ISheetOperations.InsertDemoDataAsync"/>
    public abstract Task<List<MessageEntity>> InsertDemoDataAsync();
}
