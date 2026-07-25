using System.Collections;
using System.Reflection;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Factories;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Stock.Entities;

namespace RaptorSheets.Sample.Web.Services;

/// <summary>Connects to the Stock spreadsheet on first use - see GigSheetOperations for the shared
/// reasoning. Stock has no validated/dropdown columns today, so its ValidationSheetMap is empty.</summary>
public class StockSheetOperations(
    ISheetManagerFactory<RaptorSheets.Stock.Managers.IGoogleSheetManager> factory,
    IConfiguration configuration,
    ReferenceSheetCache cache) : ISheetOperations
{
    private RaptorSheets.Stock.Managers.IGoogleSheetManager? _manager;
    private string? _error;
    private bool _attempted;

    public string DomainName => "stock";
    public string DomainLabel => "Stock Tracking";
    public Type SheetsType => typeof(StockSheets);
    // Stock names sheets via RaptorSheets.Stock.Enums.SheetName + [Description], not a
    // Constants.SheetsConfig.SheetNames class - its Sheets-container property names (Accounts,
    // Stocks, Tickers) already match their real tab names, so no resolution is needed.
    public Type? SheetNamesType => null;
    public IReadOnlySet<string> ExcludedSheetNames { get; } = new HashSet<string>();
    public IReadOnlyDictionary<string, string> ValidationSheetMap { get; } = new Dictionary<string, string>();

    public bool TryGetManager(out object? manager, out string? error)
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

    public void Reset()
    {
        _attempted = false;
        _manager = null;
        _error = null;
    }

    private void Connect()
    {
        var spreadsheetId = configuration["spreadsheets:stock"];
        var credentials = configuration.GetSection("google_credentials").Get<Dictionary<string, string>>();

        if (string.IsNullOrWhiteSpace(spreadsheetId) || credentials is not { Count: > 0 })
        {
            _error = $"No {DomainLabel} spreadsheet configured. Set \"spreadsheets:stock\" and \"google_credentials\" " +
                      "with dotnet user-secrets - see docs/SAMPLE-APP.md.";
            return;
        }

        try
        {
            _manager = factory.Create(credentials, spreadsheetId);
        }
        catch (Exception ex)
        {
            _error = $"Couldn't connect to the {DomainLabel} spreadsheet: {ex.Message}";
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

    public async Task<List<MessageEntity>> CreateAllSheetsAsync()
    {
        var result = await _manager!.CreateAllSheets();
        return result.Messages;
    }

    // PopulateDemoData directly, not SetupDemo - sheet creation is now CreateAllSheetsAsync's job
    // (see ISheetOperations.InsertDemoDataAsync).
    public async Task<List<MessageEntity>> InsertDemoDataAsync()
    {
        var result = await _manager!.PopulateDemoData();
        return result.Messages;
    }

    public async Task<string?> GetSpreadsheetTitleAsync()
    {
        var info = await _manager!.GetSpreadsheetInfo();
        return info?.Properties?.Title;
    }
}
