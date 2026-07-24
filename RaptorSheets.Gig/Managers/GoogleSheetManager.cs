using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Logging;
using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Managers;
using RaptorSheets.Core.Models;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Helpers;

namespace RaptorSheets.Gig.Managers;

/// <summary>
/// Main interface for Google Sheet operations in the Gig domain.
/// Provides CRUD operations, metadata access, and demo data functionality.
/// </summary>
/// <summary>
/// Extends the shared <see cref="IGoogleSheetManager{TEntity}"/> CRUD/metadata/layout surface with
/// Gig's own demo-data generation, which takes a date range rather than the seed-only or
/// seed-plus-date-range shapes other domains use.
/// </summary>
public interface IGoogleSheetManager : IGoogleSheetManager<SheetEntity>
{
    // Demo Data Generation
    SheetEntity GenerateDemoData(DateTime? startDate = null, DateTime? endDate = null, int? seed = null);
}

/// <summary>
/// Main Google Sheet Manager for the Gig domain. Handles all interactions with the Google Sheets API.
///
/// Domain-agnostic read/metadata/layout/heal orchestration (GetSheets, GetAllSheets, sheet
/// properties, tab names, layouts, InsertMissingColumns, GetSpreadsheetInfo, GetBatchData) is
/// inherited from <see cref="GoogleSheetManagerBase{TEntity}"/>. This class adds only the Gig-specific
/// pieces: constructors, the CreateMissingSheetsAsync self-heal hook, and the domain write operations
/// (ordered CreateSheets, ChangeSheetData, DeleteSheets) plus demo-data generation and the static
/// header-check helpers.
/// </summary>
public class GoogleSheetManager : GoogleSheetManagerBase<SheetEntity>, IGoogleSheetManager
{
    #region Construction

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
    /// Restores sheets found missing entirely during <see cref="GoogleSheetManagerBase{TEntity}.GetSheets"/>
    /// self-heal, using Gig's own ordered/indexed creation so the desired positions are preserved.
    /// </summary>
    protected override Task<SheetEntity> CreateMissingSheetsAsync(Dictionary<string, int> missingIndexMap, CancellationToken cancellationToken = default)
    {
        return CreateSheets(missingIndexMap, cancellationToken);
    }

    /// <summary>
    /// Backs <see cref="GoogleSheetManagerBase{TEntity}.CreateSheets"/> and
    /// <see cref="GoogleSheetManagerBase{TEntity}.DeleteSheets"/> (for temp-sheet creation) with
    /// Gig's fully-configured AddSheet requests (headers, formatting, validation, colors).
    /// </summary>
    protected override BatchUpdateSpreadsheetRequest GenerateSheetsRequest(List<string> sheetNames)
    {
        return GenerateSheetsHelpers.Generate(sheetNames);
    }

    #endregion

    #region Create Operations

    // This 1-arg overload exists because C# requires exact arity to implicitly satisfy
    // IGoogleSheetManager's single-parameter CreateSheets(List<string>) - an inherited method's
    // optional parameter doesn't count for interface matching the way it does for ordinary callers.
    public async Task<SheetEntity> CreateSheets(List<string> sheets, CancellationToken cancellationToken = default)
    {
        return await CreateSheets(sheets, null, cancellationToken);
    }

    /// <summary>
    /// Creates sheets using a title->desiredIndex map. The map's keys are the sheet titles to create.
    /// </summary>
    public async Task<SheetEntity> CreateSheets(Dictionary<string,int> sheetsWithIndices, CancellationToken cancellationToken = default)
    {
        if (sheetsWithIndices == null || sheetsWithIndices.Count == 0)
        {
            return await CreateSheets(new List<string>(), cancellationToken);
        }

        // Order titles deterministically using a helper (stable, testable).
        var sheets = SheetOrderingHelper.OrderSheetTitlesByIndex(sheetsWithIndices);

        return await CreateSheets(sheets, sheetsWithIndices, cancellationToken);
    }

    #endregion

    #region Read Operations

    public async Task<SheetEntity> GetSheet(string sheet, CancellationToken cancellationToken = default)
    {
        var sheetExists = GenerateSheetsHelpers.GetSheetNames()
            .Any(name => string.Equals(name, sheet, StringComparison.OrdinalIgnoreCase));

        if (!sheetExists)
        {
            return new SheetEntity { Messages = [MessageHelpers.CreateErrorMessage($"Sheet {sheet.ToUpperInvariant()} does not exist", MessageType.GET_SHEETS)] };
        }

        return await GetSheets([sheet], cancellationToken);
    }

    #endregion

    #region Update Operations

    // Single source of truth for the ChangeSheetData dispatch: count/data accessors AND request
    // builder per sheet, instead of a separate accessor map plus an easily-out-of-sync switch.
    // Shared dispatch logic (ResolveSheetsWithData/BuildChangeRequests) lives in
    // GoogleRequestHelpers so any domain can reuse the same pattern with its own map.
    private static readonly Dictionary<string, GoogleRequestHelpers.SheetChangeAccessor<SheetEntity>> _sheetAccessors =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [SheetsConfig.SheetNames.Expenses] = new(
                entity => entity.Sheets.Expenses.Count,
                entity => entity.Sheets.Expenses,
                (data, properties) => GigRequestHelpers.ChangeExpensesSheetData(data as List<ExpenseEntity> ?? [], properties)),
            [SheetsConfig.SheetNames.Setup] = new(
                entity => entity.Sheets.Setup.Count,
                entity => entity.Sheets.Setup,
                (data, properties) => GigRequestHelpers.ChangeSetupSheetData(data as List<SetupEntity> ?? [], properties)),
            [SheetsConfig.SheetNames.Shifts] = new(
                entity => entity.Sheets.Shifts.Count,
                entity => entity.Sheets.Shifts,
                (data, properties) => GigRequestHelpers.ChangeShiftSheetData(data as List<ShiftEntity> ?? [], properties)),
            [SheetsConfig.SheetNames.Trips] = new(
                entity => entity.Sheets.Trips.Count,
                entity => entity.Sheets.Trips,
                (data, properties) => GigRequestHelpers.ChangeTripSheetData(data as List<TripEntity> ?? [], properties))
        };

    public async Task<SheetEntity> ChangeSheetData(List<string> sheets, SheetEntity sheetEntity, CancellationToken cancellationToken = default)
    {
        var (sheetsWithData, resolveMessages) = GoogleRequestHelpers.ResolveSheetsWithData(sheets, sheetEntity, _sheetAccessors);
        sheetEntity.Messages.AddRange(resolveMessages);

        if (sheetsWithData.Count == 0)
        {
            sheetEntity.Messages.Add(MessageHelpers.CreateWarningMessage("No data to change", MessageType.GENERAL));
            return sheetEntity;
        }

        var sheetInfo = await GetSheetProperties(sheets, cancellationToken);
        var (requests, buildMessages) = GoogleRequestHelpers.BuildChangeRequests(sheetsWithData, sheetEntity, _sheetAccessors, sheetInfo);
        sheetEntity.Messages.AddRange(buildMessages);

        var batchUpdateSpreadsheetRequest = new BatchUpdateSpreadsheetRequest { Requests = requests };
        var batchUpdateSpreadsheetResponse = await _googleSheetService.BatchUpdateSpreadsheet(batchUpdateSpreadsheetRequest, cancellationToken);

        if (batchUpdateSpreadsheetResponse == null)
        {
            var spreadsheetInfo = await _googleSheetService.GetSheetInfo(cancellationToken);
            if (spreadsheetInfo != null)
            {
                sheetEntity.Messages.AddRange(await HandleMissingSheets(spreadsheetInfo, cancellationToken));
            }
            sheetEntity.Messages.Add(MessageHelpers.CreateErrorMessage($"Unable to save data", MessageType.SAVE_DATA));
        }

        return sheetEntity;
    }

    #endregion

    #region Header Validation

    /// <summary>
    /// Checks a spreadsheet's tab names for sheets that don't correspond to any known Gig sheet.
    /// Only needs sheet tab metadata (no grid/cell data), so it's safe to call with a cheap
    /// <c>GetSheetInfo()</c> (no ranges) result. Known-sheet header validation (missing/renamed/
    /// reordered columns) is handled separately, per-sheet, using data already fetched via batchGet.
    /// Static so callers can use it off the type without a manager instance; thin shim over
    /// <see cref="GigSheetHelpers"/>.
    /// </summary>
    public static List<MessageEntity> CheckUnknownSheets(Spreadsheet sheetInfoResponse)
    {
        return GigSheetHelpers.CheckUnknownSheets(sheetInfoResponse);
    }

    public static List<MessageEntity> CheckSheetHeaders(Spreadsheet sheetInfoResponse)
    {
        return GigSheetHelpers.CheckSheetHeaders(sheetInfoResponse);
    }

    /// <summary>
    /// Same as <see cref="CheckSheetHeaders(Spreadsheet)"/>, but also reports which columns are
    /// missing entirely and where they should be inserted, for use with <see cref="InsertMissingColumns"/>.
    /// </summary>
    public static List<MessageEntity> CheckSheetHeaders(Spreadsheet sheetInfoResponse, out Dictionary<string, List<ColumnInsertionInfo>> missingColumns)
    {
        return GigSheetHelpers.CheckSheetHeaders(sheetInfoResponse, out missingColumns);
    }

    #endregion

    #region Demo Data Generation

    /// <summary>
    /// Generates demo data without inserting it into the spreadsheet.
    /// Allows inspection, modification, or testing before insertion.
    /// This is the core method - consuming applications can wrap this with convenience methods.
    /// </summary>
    /// <param name="startDate">Start date for demo data (defaults to 30 days ago)</param>
    /// <param name="endDate">End date for demo data (defaults to today)</param>
    /// <param name="seed">Optional seed for deterministic/reproducible demo data (useful for testing)</param>
    /// <returns>SheetEntity populated with realistic demo data (Shifts, Trips, Expenses)</returns>
    public SheetEntity GenerateDemoData(DateTime? startDate = null, DateTime? endDate = null, int? seed = null)
    {
        var start = startDate ?? DateTime.Today.AddDays(-30);
        var end = endDate ?? DateTime.Today;

        return DemoHelpers.GenerateDemoData(start, end, seed);
    }

    #endregion

    #region Private Helpers

    private async Task<List<MessageEntity>> HandleMissingSheets(Spreadsheet? spreadsheet, CancellationToken cancellationToken = default)
    {
        var messages = new List<MessageEntity>();
        if (spreadsheet != null)
        {
            var missingSheets = SheetHelpers.CheckSheets<SheetName>(spreadsheet);

            if (missingSheets.Count != 0)
            {
                messages.AddRange(SheetHelpers.CheckSheets(missingSheets));

                // Compute a title->desiredIndex map for missing sheets using the canonical ordered sheet list.
                // This ensures insertion indices are computed relative to the full expected ordering,
                // not just the missing subset (avoids incorrectly appending sheets).
                var allSheets = GenerateSheetsHelpers.GetSheetNames();
                var missingIndexMap = SheetInitializationHelper.GetMissingSheets(spreadsheet, allSheets);

                messages.AddRange((await CreateSheets(missingIndexMap, cancellationToken)).Messages);
            }
        }
        else
        {
            messages.Add(MessageHelpers.CreateErrorMessage($"Unable to retrieve sheet(s)", MessageType.GET_SHEETS));
        }

        return messages;
    }

    #endregion
}
