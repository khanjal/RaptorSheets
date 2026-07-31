namespace RaptorSheets.Sample.Web.Services;

/// <summary>
/// The generic surface Sheet.razor and NavMenu.razor drive every domain through, so neither needs
/// to know whether it's actually talking to Gig/Stock/Job/Home's own strongly-typed
/// ISheetManager/SheetEntity. Each domain's own manager interface
/// (RaptorSheets.{Domain}.Managers.ISheetManager) already implements this exact CRUD/layout
/// surface via RaptorSheets.Core.Managers.ISheetManager&lt;TEntity&gt; - this just re-exposes
/// it without the TEntity type parameter, since TEntity differs per domain and a Blazor page can't
/// be generic per route segment.
///
/// One instance per domain TYPE (still DI-registered exactly as before), not per connection - a
/// domain type can now have zero, one, or many <see cref="SpreadsheetConnection"/>s pointed at
/// different spreadsheets (see LocalConnectionsStore), so this interface only carries the static
/// metadata that's true regardless of which spreadsheet you connect to, plus <see cref="TryConnect"/>
/// to build a live handle for one specific connection. It intentionally holds no connection state
/// itself - building a manager per connection is cheap (see ISheetManagerFactory's own doc comment),
/// so there's nothing to cache and nothing to invalidate when Settings saves a change.
/// </summary>
public interface ISheetOperations
{
    /// <summary>Route segment / connection Type value, e.g. "gig".</summary>
    string DomainName { get; }

    /// <summary>Display label, e.g. "Gig Work" - deliberately more than the bare domain name, since
    /// "Home" alone reads as this app's own Home page rather than the home-maintenance domain.</summary>
    string DomainLabel { get; }

    /// <summary>The domain's Sheets container (e.g. GigSheets) - reflected by SheetMetadata.</summary>
    Type SheetsType { get; }

    /// <summary>The domain's Constants.SheetsConfig.SheetNames class - resolves each Sheets-container
    /// property to its real spreadsheet tab name, which isn't always the same as the C# property
    /// name (e.g. Job's InterviewTypes property is really the "Interview Types" tab). Null for a
    /// domain with no such class (Stock, which names sheets via an enum instead and needs no
    /// resolution since its property names already match their tab names).</summary>
    Type? SheetNamesType { get; }

    /// <summary>Sheets-container properties with no backing tab yet (e.g. Job's Weekly/Monthly/Summary
    /// analytics DTOs) - excluded from the nav and from sheet discovery entirely.</summary>
    IReadOnlySet<string> ExcludedSheetNames { get; }

    /// <summary>Column ValidationPattern -&gt; the reference sheet name that backs its dropdown.
    /// Empty for a domain with no validated columns (Stock, today).</summary>
    IReadOnlyDictionary<string, string> ValidationSheetMap { get; }

    /// <summary>
    /// Builds a live handle for one specific connection - credentials come from user secrets,
    /// <paramref name="connection"/> supplies the spreadsheet ID. A fresh manager is built on every
    /// call (no per-domain cached instance), so this is safe to call as often as needed and doesn't
    /// need a matching "reset" step after Settings changes something.
    /// </summary>
    bool TryConnect(SpreadsheetConnection connection, out ITypedConnectedSheet? sheet, out string? error);
}
