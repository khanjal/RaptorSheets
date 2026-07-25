using System.Collections;
using System.Reflection;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Models.Google;

namespace RaptorSheets.Sample.Web.Services;

/// <summary>
/// The generic surface Sheet.razor and NavMenu.razor drive every domain through, so neither needs
/// to know whether it's actually talking to Gig/Stock/Job/Home's own strongly-typed
/// IGoogleSheetManager/SheetEntity. Each domain's own manager interface
/// (RaptorSheets.{Domain}.Managers.IGoogleSheetManager) already implements this exact CRUD/layout
/// surface via RaptorSheets.Core.Managers.IGoogleSheetManager&lt;TEntity&gt; - this just re-exposes
/// it without the TEntity type parameter, since TEntity differs per domain and a Blazor page can't
/// be generic per route segment.
///
/// Demo-data *generation* itself isn't part of this surface: each domain's own GenerateDemoData has
/// a different signature (Gig takes a date range, Stock/Home take a seed, Job takes both), so there's
/// no shared method to call it generically with custom parameters - that stays domain-specific (see
/// GigSheetOperations.TryGetTypedManager, used only by Home.razor's Gig-only setup wizard, which
/// wants Gig's actual date-range demo data). InsertDemoDataAsync below covers the simpler "just fill
/// it with something to look at" case with every parameter defaulted, which every domain can expose
/// identically regardless of what its own generator's signature looks like.
/// </summary>
public interface ISheetOperations
{
    /// <summary>Route segment, e.g. "gig".</summary>
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

    bool TryGetManager(out object? manager, out string? error);
    void Reset();

    Task<List<string>> GetAllSheetTabNamesAsync();
    SheetModel? GetSheetLayout(string sheetName);
    Task<(object SheetsContainer, List<MessageEntity> Messages)> GetSheetAsync(string sheetName);
    Task<List<MessageEntity>> ChangeSheetDataAsync(string sheetName, PropertyInfo listProperty, IList dirtyRows);
    Task<List<MessageEntity>> CreateSheetAsync(string sheetName);

    Task<Dictionary<string, IReadOnlyList<string>>> GetReferenceValuesAsync(
        IReadOnlyList<SheetDescriptor> referenceDescriptors, CancellationToken cancellationToken = default);

    /// <summary>Creates every sheet this domain expects that isn't already a tab - never touches a
    /// sheet that already exists. Deliberately separate from InsertDemoDataAsync: unlike creating
    /// sheets, inserting demo data is NOT safe to run against a spreadsheet that already has real
    /// rows in it (see InsertDemoDataAsync) and needs its own explicit confirmation.</summary>
    Task<List<MessageEntity>> CreateAllSheetsAsync();

    /// <summary>Generates and writes demo data, every parameter defaulted - for a from-Settings
    /// "just give me something to look at" action, not a precise dataset. Assumes the target sheets
    /// already exist (call CreateAllSheetsAsync first if they might not).
    ///
    /// This does NOT check whether the sheet already holds real data before writing - every domain's
    /// GenerateDemoData assigns RowId starting fresh from 2 every time, and the underlying write path
    /// decides overwrite-vs-append purely by comparing RowId against the sheet's total grid row count
    /// (usually 1000+), so RowId 2 lands in the "overwrite this literal spreadsheet row" branch almost
    /// unconditionally. Calling this against a spreadsheet with existing rows WILL silently overwrite
    /// the first several of them, not just add demo rows alongside them. Callers must confirm the
    /// target sheets are empty of real data first (Settings.razor's CheckSheetsAsync does this).</summary>
    Task<List<MessageEntity>> InsertDemoDataAsync();

    /// <summary>The connected spreadsheet's own title (from Google Sheets, not this app), or null if
    /// not connected - lets Settings confirm "yes, this is the spreadsheet you meant to connect."</summary>
    Task<string?> GetSpreadsheetTitleAsync();
}
