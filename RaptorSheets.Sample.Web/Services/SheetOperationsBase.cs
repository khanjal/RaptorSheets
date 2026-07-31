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
///
/// Stateless factory, not a cached-connection singleton: TryConnect builds a fresh manager on every
/// call (cheap - see ISheetManagerFactory's own doc comment) and wraps it in a private
/// TypedConnectedSheet, rather than caching one Manager per domain the way earlier versions of this
/// class did. That's what lets a domain type have zero, one, or many live connections at once - the
/// old model could only ever represent "the one live spreadsheet for this domain."
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
    public abstract string DomainName { get; }
    public abstract string DomainLabel { get; }
    public abstract Type SheetsType { get; }
    public abstract Type? SheetNamesType { get; }
    public abstract IReadOnlySet<string> ExcludedSheetNames { get; }
    public abstract IReadOnlyDictionary<string, string> ValidationSheetMap { get; }

    public bool TryConnect(SpreadsheetConnection connection, out ITypedConnectedSheet? sheet, out string? error)
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
            sheet = new TypedConnectedSheet(manager, this, cache, connection);
            error = null;
            return true;
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
            error = $"Couldn't connect to \"{connection.Label}\": {ex.Message}";
            return false;
        }
    }

    /// <summary>Domain-specific demo-data write for the manager TryConnect just built - Gig has no
    /// PopulateDemoData convenience method the way Stock/Job/Home do, so this can't be unified across
    /// domains into a single shared implementation.</summary>
    protected abstract Task<List<MessageEntity>> InsertDemoDataAsync(TManager manager);

    private sealed class TypedConnectedSheet(
        TManager manager,
        SheetOperationsBase<TManager, TEntity, TSheets> owner,
        ReferenceSheetCache cache,
        SpreadsheetConnection connection) : ITypedConnectedSheet
    {
        public Task<string?> GetSpreadsheetTitleAsync() => manager.GetSpreadsheetTitle();

        public Task<List<string>> GetAllSheetTabNamesAsync() => manager.GetAllSheetTabNames();

        public Task<SheetModel?> GetLiveSheetStructureAsync(string sheetName) => manager.GetLiveSheetStructure(sheetName);

        public Task<List<List<string?>>> GetLiveSheetRawValuesAsync(string sheetName, int maxRows = 200) =>
            manager.GetLiveSheetRawValues(sheetName, maxRows);

        public SheetModel? GetSheetLayout(string sheetName) => manager.GetSheetLayout(sheetName);

        public async Task<(object SheetsContainer, List<MessageEntity> Messages)> GetSheetAsync(string sheetName)
        {
            var result = await manager.GetSheet(sheetName);
            return (result.Sheets!, result.Messages);
        }

        public async Task<List<MessageEntity>> ChangeSheetDataAsync(string sheetName, PropertyInfo listProperty, IList dirtyRows)
        {
            var entity = new TEntity();
            listProperty.SetValue(entity.Sheets, dirtyRows);
            var result = await manager.ChangeSheetData([sheetName], entity);
            return result.Messages;
        }

        public async Task<List<MessageEntity>> CreateSheetAsync(string sheetName)
        {
            var result = await manager.CreateSheets([sheetName]);
            return result.Messages;
        }

        public async Task<List<MessageEntity>> CreateAllSheetsAsync()
        {
            var result = await manager.CreateAllSheets();
            return result.Messages;
        }

        public Task<List<MessageEntity>> InsertDemoDataAsync() => owner.InsertDemoDataAsync(manager);

        public Task<Dictionary<string, IReadOnlyList<string>>> GetReferenceValuesAsync(
            IReadOnlyList<SheetDescriptor> referenceDescriptors, CancellationToken cancellationToken = default) =>
            cache.GetIdentityValuesAsync(
                $"{owner.DomainName}:{connection.SpreadsheetId}",
                async (names, ct) => (await manager.GetSheets(names, ct)).Sheets!,
                referenceDescriptors,
                cancellationToken);
    }
}
