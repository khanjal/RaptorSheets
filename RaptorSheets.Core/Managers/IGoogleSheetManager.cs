using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Models;
using RaptorSheets.Core.Models.Google;

namespace RaptorSheets.Core.Managers;

/// <summary>
/// The CRUD/metadata/layout surface every domain's manager exposes identically - the part of each
/// domain's own <c>IGoogleSheetManager</c> that was previously redeclared byte-for-byte in Gig,
/// Stock, Job, and Home (each already implements every member here via
/// <see cref="GoogleSheetManagerBase{TEntity}"/>, so extending this costs nothing per domain).
/// A domain's own interface should extend this and add only what's genuinely domain-specific -
/// demo-data generation today, which differs enough per domain (Gig takes a date range, Stock/Home
/// take a seed, Job takes both) that unifying it isn't worthwhile.
/// </summary>
/// <typeparam name="TEntity">The domain's top-level SheetEntity type.</typeparam>
public interface IGoogleSheetManager<TEntity> where TEntity : class, ISheetEntity, new()
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
    Task<Spreadsheet?> GetSpreadsheetInfo(List<string>? ranges = null, CancellationToken cancellationToken = default);
    Task<BatchGetValuesByDataFilterResponse?> GetBatchData(List<string> sheets, CancellationToken cancellationToken = default);

    // Header Management
    SheetModel? GetSheetLayout(string sheet);
    List<SheetModel> GetSheetLayouts(List<string> sheets);
    Task<TEntity> InsertMissingColumns(Dictionary<string, List<ColumnInsertionInfo>> missingColumns, CancellationToken cancellationToken = default);
}
