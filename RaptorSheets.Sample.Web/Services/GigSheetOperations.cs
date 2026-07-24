using System.Collections;
using System.Reflection;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Factories;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Entities;

namespace RaptorSheets.Sample.Web.Services;

/// <summary>
/// Connects to the Gig spreadsheet on first use rather than at startup, so a fresh clone without
/// user secrets set shows a setup message in the page instead of crashing the circuit. Also exposes
/// the strongly-typed manager (<see cref="TryGetTypedManager"/>) for Home.razor's Gig-only setup
/// wizard, which needs CreateAllSheets/GenerateDemoData - operations that aren't part of
/// <see cref="ISheetOperations"/> because their signatures genuinely differ per domain.
/// </summary>
public class GigSheetOperations(
    ISheetManagerFactory<RaptorSheets.Gig.Managers.IGoogleSheetManager> factory,
    IConfiguration configuration,
    ReferenceSheetCache cache) : ISheetOperations
{
    private RaptorSheets.Gig.Managers.IGoogleSheetManager? _manager;
    private string? _error;
    private bool _attempted;

    public string DomainName => "gig";
    public string DomainLabel => "Gig";
    public Type SheetsType => typeof(GigSheets);
    public Type SheetNamesType => typeof(RaptorSheets.Gig.Constants.SheetsConfig.SheetNames);
    public IReadOnlySet<string> ExcludedSheetNames { get; } = new HashSet<string>();

    // Which reference sheet backs the dropdown for a given [Column]'s ValidationPattern - mirrors
    // Gig's own (internal) GigSheetHelpers.GetSheetForRange, built from the same public constants
    // since that mapping isn't itself public.
    public IReadOnlyDictionary<string, string> ValidationSheetMap { get; } = new Dictionary<string, string>
    {
        [RaptorSheets.Gig.Constants.SheetsConfig.ValidationNames.RangeAddress] = RaptorSheets.Gig.Constants.SheetsConfig.SheetNames.Addresses,
        [RaptorSheets.Gig.Constants.SheetsConfig.ValidationNames.RangeName] = RaptorSheets.Gig.Constants.SheetsConfig.SheetNames.Names,
        [RaptorSheets.Gig.Constants.SheetsConfig.ValidationNames.RangePlace] = RaptorSheets.Gig.Constants.SheetsConfig.SheetNames.Places,
        [RaptorSheets.Gig.Constants.SheetsConfig.ValidationNames.RangeRegion] = RaptorSheets.Gig.Constants.SheetsConfig.SheetNames.Regions,
        [RaptorSheets.Gig.Constants.SheetsConfig.ValidationNames.RangeService] = RaptorSheets.Gig.Constants.SheetsConfig.SheetNames.Services,
        [RaptorSheets.Gig.Constants.SheetsConfig.ValidationNames.RangeType] = RaptorSheets.Gig.Constants.SheetsConfig.SheetNames.Types,
    };

    public bool TryGetManager(out object? manager, out string? error)
    {
        var found = TryGetTypedManager(out var typed, out error);
        manager = typed;
        return found;
    }

    public bool TryGetTypedManager(out RaptorSheets.Gig.Managers.IGoogleSheetManager? manager, out string? error)
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

    public async Task<List<string>> GetAllSheetTabNamesAsync() => await _manager!.GetAllSheetTabNames();

    public SheetModel? GetSheetLayout(string sheetName) => _manager!.GetSheetLayout(sheetName);

    public async Task<(object SheetsContainer, List<MessageEntity> Messages)> GetSheetAsync(string sheetName)
    {
        var result = await _manager!.GetSheet(sheetName);
        return (result.Sheets, result.Messages);
    }

    public async Task<List<MessageEntity>> ChangeSheetDataAsync(string sheetName, PropertyInfo listProperty, IList dirtyRows)
    {
        var entity = new SheetEntity();
        listProperty.SetValue(entity.Sheets, dirtyRows);
        var result = await _manager!.ChangeSheetData([sheetName], entity);
        return result.Messages;
    }

    public async Task<List<MessageEntity>> CreateSheetAsync(string sheetName)
    {
        var result = await _manager!.CreateSheets([sheetName]);
        return result.Messages;
    }

    public Task<Dictionary<string, IReadOnlyList<string>>> GetReferenceValuesAsync(
        IReadOnlyList<SheetDescriptor> referenceDescriptors, CancellationToken cancellationToken = default) =>
        cache.GetIdentityValuesAsync(
            DomainName,
            async (names, ct) => (await _manager!.GetSheets(names, ct)).Sheets,
            referenceDescriptors,
            cancellationToken);
}
