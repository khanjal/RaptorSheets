using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Models;
using RaptorSheets.Core.Models.Google;
using System.Runtime.CompilerServices;

namespace RaptorSheets.Core.Managers;

/// <summary>
/// The CRUD/metadata/layout surface every domain's manager exposes identically - the part of each
/// domain's own <c>ISheetManager</c> that was previously redeclared byte-for-byte in Gig,
/// Stock, Job, and Home (each already implements every member here via
/// <see cref="SheetManagerBase{TEntity}"/>, so extending this costs nothing per domain).
/// A domain's own interface should extend this and add only what's genuinely domain-specific -
/// demo-data generation today, which differs enough per domain (Gig takes a date range, Stock/Home
/// take a seed, Job takes both) that unifying it isn't worthwhile.
/// </summary>
/// <typeparam name="TEntity">The domain's top-level SheetEntity type.</typeparam>
public interface ISheetManager<TEntity> where TEntity : class, ISheetEntity, new()
{
    // CRUD Operations
    Task<TEntity> ChangeSheetData(List<string> sheets, TEntity sheetEntity, CancellationToken cancellationToken = default);
    Task<TEntity> CreateAllSheets(CancellationToken cancellationToken = default);

    /// <summary>
    /// Same as the overload above, but when <paramref name="removeDefaultSheet"/> is true deletes
    /// Google's default "Sheet1" instead of moving it to the end. Only safe for a caller that
    /// created the spreadsheet itself and therefore knows that tab is untouched - nothing here
    /// checks it for data.
    /// </summary>
    Task<TEntity> CreateAllSheets(bool removeDefaultSheet, CancellationToken cancellationToken = default)
        => AsBase().CreateAllSheets(removeDefaultSheet, cancellationToken);
    Task<TEntity> CreateSheets(List<string> sheets, CancellationToken cancellationToken = default);
    Task<TEntity> DeleteAllSheets(CancellationToken cancellationToken = default);
    Task<TEntity> DeleteSheets(List<string> sheets, CancellationToken cancellationToken = default);
    Task<TEntity> GetSheet(string sheet, CancellationToken cancellationToken = default);

    /// <summary>
    /// Same as the overload above, but when <paramref name="includeStructure"/> is true also
    /// populates each returned entity's <c>Structures</c> with a live read of every requested
    /// sheet's structure (see <see cref="GetLiveSheetStructure"/>) in the same API call used to fetch
    /// data - no second round trip. Defaults to false so existing callers are unaffected.
    /// </summary>
    Task<TEntity> GetSheet(string sheet, bool includeStructure, CancellationToken cancellationToken = default);

    /// <summary>
    /// Same as the overloads above, but when <paramref name="autoHeal"/> is false missing columns are
    /// reported in Messages instead of being inserted. Only column healing is gated: a sheet that
    /// doesn't exist at all is still (re)created, since there would otherwise be nothing to read.
    /// Every other overload behaves as if <paramref name="autoHeal"/> were true.
    /// </summary>
    Task<TEntity> GetSheet(string sheet, bool includeStructure, bool autoHeal, CancellationToken cancellationToken = default)
        => AsBase().GetSheet(sheet, includeStructure, autoHeal, cancellationToken);
    Task<TEntity> GetAllSheets(CancellationToken cancellationToken = default);
    Task<TEntity> GetAllSheets(bool includeStructure, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="GetSheet(string, bool, bool, CancellationToken)"/>
    Task<TEntity> GetAllSheets(bool includeStructure, bool autoHeal, CancellationToken cancellationToken = default)
        => AsBase().GetAllSheets(includeStructure, autoHeal, cancellationToken);
    Task<TEntity> GetSheets(List<string> sheets, CancellationToken cancellationToken = default);
    Task<TEntity> GetSheets(List<string> sheets, bool includeStructure, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="GetSheet(string, bool, bool, CancellationToken)"/>
    Task<TEntity> GetSheets(List<string> sheets, bool includeStructure, bool autoHeal, CancellationToken cancellationToken = default)
        => AsBase().GetSheets(sheets, includeStructure, autoHeal, cancellationToken);

    // Metadata & Properties
    Task<List<PropertyEntity>> GetAllSheetProperties(CancellationToken cancellationToken = default);
    Task<List<PropertyEntity>> GetSheetProperties(List<string> sheets, CancellationToken cancellationToken = default);
    Task<List<string>> GetAllSheetTabNames(CancellationToken cancellationToken = default);

    /// <summary>The connected spreadsheet's own title (from Google Sheets, not anything typed into
    /// this library) - a RaptorSheets-native replacement for reading Spreadsheet.Properties.Title off
    /// the raw Google response, which this interface deliberately does not expose: Google.Apis.Sheets.v4
    /// types are an internal implementation detail of Core, not part of the public contract.</summary>
    Task<string?> GetSpreadsheetTitle(CancellationToken cancellationToken = default);

    // Header Management
    SheetModel? GetSheetLayout(string sheet);
    List<SheetModel> GetSheetLayouts(List<string> sheets);
    Task<TEntity> InsertMissingColumns(Dictionary<string, List<ColumnInsertionInfo>> missingColumns, CancellationToken cancellationToken = default);

    /// <summary>
    /// Re-writes the header row of each given sheet from its canonical layout, fixing cross-sheet
    /// formulas that went stale after another sheet's headers changed - see
    /// <see cref="SheetManagerBase{TEntity}.RefreshHeaderFormulasAsync(IEnumerable{string}, CancellationToken)"/>.
    /// </summary>
    Task RefreshHeaderFormulasAsync(IEnumerable<string> sheetNames, CancellationToken cancellationToken = default)
        => AsBase().RefreshHeaderFormulasAsync(sheetNames, cancellationToken);

    /// <summary>
    /// Refreshes the header formulas of every sheet that depends on any of
    /// <paramref name="changedSheetNames"/> - see
    /// <see cref="SheetManagerBase{TEntity}.RefreshDependentSheetsAsync(IEnumerable{string}, CancellationToken)"/>.
    /// </summary>
    Task RefreshDependentSheetsAsync(IEnumerable<string> changedSheetNames, CancellationToken cancellationToken = default)
        => AsBase().RefreshDependentSheetsAsync(changedSheetNames, cancellationToken);

    /// <summary>
    /// Finds columns present on the live sheet whose header formula has drifted from canonical.
    /// Opt-in and read-only; pass the result to <see cref="ReapplyColumnFormulas"/> to fix them.
    /// Missing columns and validation drift aren't checked, and an empty list also comes back for an
    /// unregistered sheet or a failed live read - it means "nothing to reapply", not "verified healthy".
    /// </summary>
    Task<List<ColumnInsertionInfo>> DetectBrokenColumnsAsync(string sheet, CancellationToken cancellationToken = default)
        => AsBase().DetectBrokenColumnsAsync(sheet, cancellationToken);

    /// <summary>
    /// Rewrites just the header cell of each column in <paramref name="brokenColumns"/> with its
    /// canonical formula; data rows beneath are never touched.
    /// </summary>
    Task<TEntity> ReapplyColumnFormulas(string sheet, List<ColumnInsertionInfo> brokenColumns, CancellationToken cancellationToken = default)
        => AsBase().ReapplyColumnFormulas(sheet, brokenColumns, cancellationToken);

    /// <summary>
    /// Re-applies formatting from a sheet's canonical SheetModel onto the live sheet - see
    /// <see cref="Managers.SheetManagerBase{TEntity}.ReapplyFormatting(string, FormattingOptionsEntity?, CancellationToken)"/>.
    /// </summary>
    Task<TEntity> ReapplyFormatting(string sheet, FormattingOptionsEntity? options = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="ReapplyFormatting(string, FormattingOptionsEntity?, CancellationToken)"/>
    Task<TEntity> ReapplyFormatting(List<string> sheets, FormattingOptionsEntity? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Live Sheet Structure: reads a sheet's structure (headers, formats, validation, notes, freeze
    /// counts, protection, ...) back from the live spreadsheet - the actual/current shape, in
    /// contrast with <see cref="GetSheetLayout"/>'s configured/expected shape from this domain's
    /// <c>[Column]</c> attributes. Works for any live sheet name, not just ones this domain knows
    /// about - only the header row and the first data row are fetched (format/validation live on the
    /// latter, never the header cell itself), so it's cheap.
    /// </summary>
    Task<SheetModel?> GetLiveSheetStructure(string sheet, CancellationToken cancellationToken = default);
    Task<Dictionary<string, SheetModel>> GetLiveSheetStructures(List<string> sheets, CancellationToken cancellationToken = default);
    Task<Dictionary<string, SheetModel>> GetAllLiveSheetStructures(CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads a sheet's raw cell values back from the live spreadsheet by row/column position, with no
    /// assumption that row 0 is a header or that data is a simple one-row-per-record table - unlike
    /// <see cref="GetSheet"/>/<see cref="GetLiveSheetStructure"/>, both of which assume that shape.
    /// Useful for a sheet that doesn't follow it (a dashboard packing multiple mini-tables, label:value
    /// pairs scattered at arbitrary positions, a transposed matrix, ...) so a caller can see what's
    /// actually there before deciding how - or whether - to model it. Rows are ragged (only as long as
    /// their own last populated cell) and bounded to <paramref name="maxRows"/> so a preview of a huge
    /// sheet doesn't pull the whole thing; works for any live sheet name, registered or not.
    /// </summary>
    Task<List<List<string?>>> GetLiveSheetRawValues(string sheet, int maxRows = 200, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="GetLiveSheetRawValues(string, int, CancellationToken)"/>
    /// <summary>Same as the single-sheet overload, but for multiple live sheets in one batched call -
    /// keyed by sheet name (case-insensitive).</summary>
    Task<Dictionary<string, List<List<string?>>>> GetLiveSheetsRawValues(List<string> sheets, int maxRows = 200, CancellationToken cancellationToken = default);

    // Every member added since the last published packages (core/v5.0.2, gig/v5.0.7) carries a
    // default body that forwards to SheetManagerBase. Each domain package bundles its own copy of
    // Core, so an app can pair a domain manager compiled against an older Core with a newer Core.dll.
    // SheetManagerBase's methods aren't virtual, so without a default body for every such member that
    // manager fails to load with a TypeLoadException - one missing body is enough. Anything compiled
    // against this version binds to the base methods directly and never reaches these.
    // S3215 (interface cast to a concrete type) is the point here: the base is the implementation.
#pragma warning disable S3215
    private SheetManagerBase<TEntity> AsBase([CallerMemberName] string member = "")
        => this as SheetManagerBase<TEntity>
           ?? throw new NotSupportedException($"{GetType().Name} implements ISheetManager<{typeof(TEntity).Name}> without deriving from SheetManagerBase<{typeof(TEntity).Name}>, so it must implement every overload of {member} itself.");
#pragma warning restore S3215
}
