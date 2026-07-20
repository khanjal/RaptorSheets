using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Logging;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Managers;
using RaptorSheets.Core.Models;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Helpers;

namespace RaptorSheets.Gig.Managers;

/// <summary>
/// Main interface for Google Sheet operations in the Gig domain.
/// Provides CRUD operations, metadata access, and demo data functionality.
/// Implemented across partial classes (Crud, Metadata, Demo, Helpers).
/// </summary>
public interface IGoogleSheetManager
{
    // CRUD Operations
    Task<SheetEntity> ChangeSheetData(List<string> sheets, SheetEntity sheetEntity);
    Task<SheetEntity> CreateAllSheets();
    Task<SheetEntity> CreateSheets(List<string> sheets);
    Task<SheetEntity> DeleteAllSheets();
    Task<SheetEntity> DeleteSheets(List<string> sheets);
    Task<SheetEntity> GetSheet(string sheet);
    Task<SheetEntity> GetAllSheets();
    Task<SheetEntity> GetSheets(List<string> sheets);
    
    // Metadata & Properties
    Task<List<PropertyEntity>> GetAllSheetProperties();
    Task<List<PropertyEntity>> GetSheetProperties(List<string> sheets);
    Task<List<string>> GetAllSheetTabNames();
    Task<Spreadsheet?> GetSpreadsheetInfo(List<string>? ranges = null);
    Task<BatchGetValuesByDataFilterResponse?> GetBatchData(List<string> sheets);
    SheetModel? GetSheetLayout(string sheet);
    List<SheetModel> GetSheetLayouts(List<string> sheets);

    // Header Management
    Task<SheetEntity> InsertMissingColumns(Dictionary<string, List<ColumnInsertionInfo>> missingColumns);

    // Demo Data Generation
    SheetEntity GenerateDemoData(DateTime? startDate = null, DateTime? endDate = null, int? seed = null);
}

/// <summary>
/// Main Google Sheet Manager for the Gig domain.
/// Handles all interactions with Google Sheets API.
/// Split into partial classes by functional area:
/// - GoogleSheetManager.cs (Main/Constructor)
/// - GoogleSheetManager.Crud.cs (CRUD operations)
/// - GoogleSheetManager.Metadata.cs (Properties, headers, layouts)
/// - GoogleSheetManager.Demo.cs (Demo data generation)
/// - GoogleSheetManager.Helpers.cs (Private helpers)
/// </summary>
public partial class GoogleSheetManager : GoogleSheetManagerBase<SheetEntity>, IGoogleSheetManager
{
    public GoogleSheetManager(RaptorSheets.Core.Services.IGoogleSheetService googleSheetService, ILogger? logger = null)
        : base(googleSheetService, GigSheetHelpers.Registry, GenerateSheetsHelpers.GetSheetNames(), logger)
    {
    }

    public GoogleSheetManager(string accessToken, string spreadsheetId, ILogger? logger = null)
        : base(accessToken, spreadsheetId, GigSheetHelpers.Registry, GenerateSheetsHelpers.GetSheetNames(), logger)
    {
    }

    public GoogleSheetManager(Dictionary<string, string> parameters, string spreadsheetId, ILogger? logger = null)
        : base(parameters, spreadsheetId, GigSheetHelpers.Registry, GenerateSheetsHelpers.GetSheetNames(), logger)
    {
    }

    /// <summary>
    /// Restores sheets found missing entirely during <see cref="GetSheets"/> self-heal, using Gig's
    /// own ordered/indexed creation so the desired positions are preserved.
    /// </summary>
    protected override Task<SheetEntity> CreateMissingSheetsAsync(Dictionary<string, int> missingIndexMap)
    {
        return CreateSheets(missingIndexMap);
    }
}
