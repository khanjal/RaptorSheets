using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Factories;
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

    public bool TryGetTypedManager(out RaptorSheets.Gig.Managers.ISheetManager? manager, out string? error)
    {
        var found = TryGetManager(out var untyped, out error);
        manager = untyped as RaptorSheets.Gig.Managers.ISheetManager;
        return found;
    }

    // Gig has no PopulateDemoData convenience method (unlike Stock/Job/Home) - this replicates the
    // write half of the sequence documented in RaptorSheets.Gig's README by hand (sheet creation is
    // CreateAllSheetsAsync's job, not this one - see ISheetOperations.InsertDemoDataAsync).
    public override async Task<List<MessageEntity>> InsertDemoDataAsync()
    {
        var demoData = Manager!.GenerateDemoData();
        var result = await Manager.ChangeSheetData(["Shifts", "Trips", "Expenses"], demoData);
        return result.Messages;
    }
}
