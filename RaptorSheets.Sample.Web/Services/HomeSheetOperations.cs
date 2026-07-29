using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Factories;
using RaptorSheets.Home.Entities;

namespace RaptorSheets.Sample.Web.Services;

/// <summary>Connects to the Home spreadsheet on first use - see GigSheetOperations for the shared
/// reasoning. Unlike Gig/Job/Stock, Home has zero ProtectSheet=true sheets - even Rooms/Contacts
/// (the sheets its own dropdowns validate against) are plain user-editable input sheets. That falls
/// out naturally here: EntityGrid's ReadOnly flag already comes from GetSheetLayout(sheet)
/// .ProtectSheet per sheet, not any Home-specific assumption.</summary>
public class HomeSheetOperations(
    ISheetManagerFactory<RaptorSheets.Home.Managers.ISheetManager> factory,
    IConfiguration configuration,
    ReferenceSheetCache cache)
    : SheetOperationsBase<RaptorSheets.Home.Managers.ISheetManager, SheetEntity, HomeSheets>(factory, configuration, cache)
{
    public override string DomainName => "home";
    public override string DomainLabel => "Home Maintenance";
    public override Type SheetsType => typeof(HomeSheets);
    public override Type SheetNamesType => typeof(RaptorSheets.Home.Constants.SheetsConfig.SheetNames);
    public override IReadOnlySet<string> ExcludedSheetNames { get; } = new HashSet<string>();

    public override IReadOnlyDictionary<string, string> ValidationSheetMap { get; } = new Dictionary<string, string>
    {
        [RaptorSheets.Home.Constants.SheetsConfig.ValidationNames.RangeRoom] = RaptorSheets.Home.Constants.SheetsConfig.SheetNames.Rooms,
        [RaptorSheets.Home.Constants.SheetsConfig.ValidationNames.RangeContact] = RaptorSheets.Home.Constants.SheetsConfig.SheetNames.Contacts,
    };

    // PopulateDemoData directly, not SetupDemo - sheet creation is CreateAllSheetsAsync's job
    // (see ISheetOperations.InsertDemoDataAsync).
    public override async Task<List<MessageEntity>> InsertDemoDataAsync()
    {
        var result = await Manager!.PopulateDemoData();
        return result.Messages;
    }
}
