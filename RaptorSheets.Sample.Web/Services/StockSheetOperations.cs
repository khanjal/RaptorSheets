using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Factories;
using RaptorSheets.Stock.Entities;

namespace RaptorSheets.Sample.Web.Services;

/// <summary>Connects to the Stock spreadsheet on first use - see GigSheetOperations for the shared
/// reasoning. Stock has no validated/dropdown columns today, so its ValidationSheetMap is empty.</summary>
public class StockSheetOperations(
    ISheetManagerFactory<RaptorSheets.Stock.Managers.ISheetManager> factory,
    IConfiguration configuration,
    ReferenceSheetCache cache)
    : SheetOperationsBase<RaptorSheets.Stock.Managers.ISheetManager, SheetEntity, StockSheets>(factory, configuration, cache)
{
    public override string DomainName => "stock";
    public override string DomainLabel => "Stock Tracking";
    public override Type SheetsType => typeof(StockSheets);

    // Stock names sheets via RaptorSheets.Stock.Enums.SheetName + [Description], not a
    // Constants.SheetsConfig.SheetNames class - its Sheets-container property names (Accounts,
    // Stocks, Tickers) already match their real tab names, so no resolution is needed.
    public override Type? SheetNamesType => null;
    public override IReadOnlySet<string> ExcludedSheetNames { get; } = new HashSet<string>();
    public override IReadOnlyDictionary<string, string> ValidationSheetMap { get; } = new Dictionary<string, string>();

    // PopulateDemoData directly, not SetupDemo - sheet creation is CreateAllSheetsAsync's job
    // (see IConnectedSheet.InsertDemoDataAsync).
    protected override async Task<List<MessageEntity>> InsertDemoDataAsync(RaptorSheets.Stock.Managers.ISheetManager manager)
    {
        var result = await manager.PopulateDemoData();
        return result.Messages;
    }
}
