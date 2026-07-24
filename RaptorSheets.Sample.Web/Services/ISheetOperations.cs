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
/// wants Gig's actual date-range demo data). CreateDemoDataAsync below covers the simpler "just fill
/// it with something to look at" case with every parameter defaulted, which every domain can expose
/// identically regardless of what its own generator's signature looks like.
/// </summary>
public interface ISheetOperations
{
    /// <summary>Route segment, e.g. "gig".</summary>
    string DomainName { get; }

    /// <summary>Display label, e.g. "Gig".</summary>
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

    /// <summary>Creates any missing sheets and fills them with demo data, every parameter defaulted -
    /// for a from-Settings "just give me something to look at" action, not a precise dataset.</summary>
    Task<List<MessageEntity>> CreateDemoDataAsync();
}
