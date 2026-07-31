using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Factories;
using RaptorSheets.Gig.Entities;

namespace RaptorSheets.Sample.Web.Services;

/// <summary>
/// Connects to a Gig spreadsheet on demand - see SheetOperationsBase for the shared reasoning.
/// </summary>
public class GigSheetOperations(
    ISheetManagerFactory<RaptorSheets.Gig.Managers.ISheetManager> factory,
    IConfiguration configuration,
    ReferenceSheetCache cache)
    : SheetOperationsBase<RaptorSheets.Gig.Managers.ISheetManager, SheetEntity, GigSheets>(factory, configuration, cache)
{
    public override string DomainName => "gig";
    public override string DomainLabel => "Gig Work";
    public override Type SheetsType => typeof(GigSheets);
    public override Type SheetNamesType => typeof(RaptorSheets.Gig.Constants.SheetsConfig.SheetNames);
    public override IReadOnlySet<string> ExcludedSheetNames { get; } = new HashSet<string>();

    // Which reference sheet backs the dropdown for a given [Column]'s ValidationPattern - mirrors
    // Gig's own (internal) GigSheetHelpers.GetSheetForRange, built from the same public constants
    // since that mapping isn't itself public.
    public override IReadOnlyDictionary<string, string> ValidationSheetMap { get; } = new Dictionary<string, string>
    {
        [RaptorSheets.Gig.Constants.SheetsConfig.ValidationNames.RangeAddress] = RaptorSheets.Gig.Constants.SheetsConfig.SheetNames.Addresses,
        [RaptorSheets.Gig.Constants.SheetsConfig.ValidationNames.RangeName] = RaptorSheets.Gig.Constants.SheetsConfig.SheetNames.Names,
        [RaptorSheets.Gig.Constants.SheetsConfig.ValidationNames.RangePlace] = RaptorSheets.Gig.Constants.SheetsConfig.SheetNames.Places,
        [RaptorSheets.Gig.Constants.SheetsConfig.ValidationNames.RangeRegion] = RaptorSheets.Gig.Constants.SheetsConfig.SheetNames.Regions,
        [RaptorSheets.Gig.Constants.SheetsConfig.ValidationNames.RangeService] = RaptorSheets.Gig.Constants.SheetsConfig.SheetNames.Services,
        [RaptorSheets.Gig.Constants.SheetsConfig.ValidationNames.RangeType] = RaptorSheets.Gig.Constants.SheetsConfig.SheetNames.Types,
    };

    // Gig has no PopulateDemoData convenience method (unlike Stock/Job/Home) - this replicates the
    // write half of the sequence documented in RaptorSheets.Gig's README by hand (sheet creation is
    // CreateAllSheetsAsync's job, not this one - see IConnectedSheet.InsertDemoDataAsync).
    protected override async Task<List<MessageEntity>> InsertDemoDataAsync(RaptorSheets.Gig.Managers.ISheetManager manager)
    {
        var demoData = manager.GenerateDemoData();
        var result = await manager.ChangeSheetData(["Shifts", "Trips", "Expenses"], demoData);
        return result.Messages;
    }
}
