using System.Collections;
using System.Reflection;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Factories;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Home.Entities;

namespace RaptorSheets.Sample.Web.Services;

/// <summary>Connects to the Home spreadsheet on first use - see GigSheetOperations for the shared
/// reasoning. Unlike Gig/Job/Stock, Home has zero ProtectSheet=true sheets - even Rooms/Contacts
/// (the sheets its own dropdowns validate against) are plain user-editable input sheets. That falls
/// out naturally here: EntityGrid's ReadOnly flag already comes from GetSheetLayout(sheet)
/// .ProtectSheet per sheet, not any Home-specific assumption.</summary>
public class HomeSheetOperations(
    ISheetManagerFactory<RaptorSheets.Home.Managers.IGoogleSheetManager> factory,
    IConfiguration configuration,
    ReferenceSheetCache cache) : ISheetOperations
{
    private RaptorSheets.Home.Managers.IGoogleSheetManager? _manager;
    private string? _error;
    private bool _attempted;

    public string DomainName => "home";
    public string DomainLabel => "Home Maintenance";
    public Type SheetsType => typeof(HomeSheets);
    public Type SheetNamesType => typeof(RaptorSheets.Home.Constants.SheetsConfig.SheetNames);
    public IReadOnlySet<string> ExcludedSheetNames { get; } = new HashSet<string>();

    public IReadOnlyDictionary<string, string> ValidationSheetMap { get; } = new Dictionary<string, string>
    {
        [RaptorSheets.Home.Constants.SheetsConfig.ValidationNames.RangeRoom] = RaptorSheets.Home.Constants.SheetsConfig.SheetNames.Rooms,
        [RaptorSheets.Home.Constants.SheetsConfig.ValidationNames.RangeContact] = RaptorSheets.Home.Constants.SheetsConfig.SheetNames.Contacts,
    };

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
        var spreadsheetId = configuration["spreadsheets:home"];
        var credentials = configuration.GetSection("google_credentials").Get<Dictionary<string, string>>();

        if (string.IsNullOrWhiteSpace(spreadsheetId) || credentials is not { Count: > 0 })
        {
            _error = $"No {DomainLabel} spreadsheet configured. Set \"spreadsheets:home\" and \"google_credentials\" " +
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
