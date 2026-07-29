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
    Task<TEntity> GetAllSheets(CancellationToken cancellationToken = default);
    Task<TEntity> GetSheets(List<string> sheets, CancellationToken cancellationToken = default);

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
}
