using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Models;
using RaptorSheets.Core.Models.Google;

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
    Task<TEntity> GetAllSheets(CancellationToken cancellationToken = default);
    Task<TEntity> GetAllSheets(bool includeStructure, CancellationToken cancellationToken = default);
    Task<TEntity> GetSheets(List<string> sheets, CancellationToken cancellationToken = default);
    Task<TEntity> GetSheets(List<string> sheets, bool includeStructure, CancellationToken cancellationToken = default);

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
}
