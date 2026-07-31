using System.Collections;
using System.Reflection;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Models.Google;

namespace RaptorSheets.Sample.Web.Services;

/// <summary>
/// A live connection to one spreadsheet (see <see cref="SpreadsheetConnection"/>), bound at the time
/// <see cref="ISheetOperations.TryConnect"/> or <see cref="GenericSheetOperations.TryConnect"/> built
/// it - the domain-agnostic subset every connection supports, typed or generic. Everything here maps
/// to a method on <c>RaptorSheets.Core.Managers.SheetManagerBase</c> that never touches that domain's
/// registry/canonical-sheet-list, which is what makes it safe for a "generic" connection (no compiled
/// <c>[Column]</c> schema at all) to support too.
/// </summary>
public interface IConnectedSheet
{
    Task<string?> GetSpreadsheetTitleAsync();
    Task<List<string>> GetAllSheetTabNamesAsync();

    /// <summary>Reads a sheet's structure back from the live spreadsheet - the actual/current shape,
    /// in contrast with GetSheetLayout's configured/expected shape from this domain's [Column]
    /// attributes. Works for any live tab name, not just ones this domain's registry knows about, so
    /// it's the one that can inspect a hand-built tab RaptorSheets has never heard of.</summary>
    Task<SheetModel?> GetLiveSheetStructureAsync(string sheetName);

    /// <summary>Raw cell values by row/column position, with no assumption that row 0 is a header or
    /// that the sheet is a simple one-row-per-record table - for a sheet that isn't (a dashboard with
    /// scattered fields, multiple mini-tables, a transposed matrix, ...). Ragged rows, bounded to
    /// maxRows.</summary>
    Task<List<List<string?>>> GetLiveSheetRawValuesAsync(string sheetName, int maxRows = 200);
}

/// <summary>
/// What a typed (gig/stock/job/home) connection additionally supports over the plain
/// <see cref="IConnectedSheet"/> surface - everything that needs a compiled entity/schema to map
/// against. A "generic" connection (see <see cref="GenericSheetOperations"/>) deliberately only ever
/// gets <see cref="IConnectedSheet"/>, never this - there's no entity type for it to map rows onto.
/// </summary>
public interface ITypedConnectedSheet : IConnectedSheet
{
    SheetModel? GetSheetLayout(string sheetName);
    Task<(object SheetsContainer, List<MessageEntity> Messages)> GetSheetAsync(string sheetName);
    Task<List<MessageEntity>> ChangeSheetDataAsync(string sheetName, PropertyInfo listProperty, IList dirtyRows);
    Task<List<MessageEntity>> CreateSheetAsync(string sheetName);

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

    Task<Dictionary<string, IReadOnlyList<string>>> GetReferenceValuesAsync(
        IReadOnlyList<SheetDescriptor> referenceDescriptors, CancellationToken cancellationToken = default);
}
