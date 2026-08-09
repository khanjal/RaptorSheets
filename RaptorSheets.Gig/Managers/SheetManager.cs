using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Logging;
using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
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
/// Extends the shared <see cref="ISheetManager{TEntity}"/> CRUD/metadata/layout surface with
/// Gig's own demo-data generation, which takes a date range rather than the seed-only or
/// seed-plus-date-range shapes other domains use.
/// </summary>
public interface ISheetManager : ISheetManager<SheetEntity>
{
    // Demo Data Generation
    SheetEntity GenerateDemoData(DateTime? startDate = null, DateTime? endDate = null, int? seed = null);
}

/// <summary>
/// Main Google Sheet Manager for the Gig domain. Handles all interactions with the Google Sheets API.
///
/// Domain-agnostic read/metadata/layout/heal orchestration (GetSheets, GetAllSheets, sheet
/// properties, tab names, layouts, InsertMissingColumns, GetSpreadsheetInfo, GetBatchData) is
/// inherited from <see cref="SheetManagerBase{TEntity}"/>. This class adds only the Gig-specific
/// pieces: constructors, the CreateMissingSheetsAsync self-heal hook, and the domain write operations
/// (ordered CreateSheets, ChangeSheetData, DeleteSheets) plus demo-data generation and the static
/// header-check helpers.
/// </summary>
public class SheetManager : SheetManagerBase<SheetEntity>, ISheetManager
{
    #region Construction

    public SheetManager(RaptorSheets.Core.Services.IGoogleSheetService googleSheetService, ILogger? logger = null)
        : base(googleSheetService, GigSheetHelpers.Registry, GenerateSheetsHelpers.GetSheetNames(), logger)
    {
    }

    public SheetManager(string accessToken, string spreadsheetId, ILogger? logger = null)
        : base(accessToken, spreadsheetId, GigSheetHelpers.Registry, GenerateSheetsHelpers.GetSheetNames(), logger)
    {
    }

    public SheetManager(Dictionary<string, string> parameters, string spreadsheetId, ILogger? logger = null)
        : base(parameters, spreadsheetId, GigSheetHelpers.Registry, GenerateSheetsHelpers.GetSheetNames(), logger)
    {
    }

    /// <summary>
    /// Backs <see cref="SheetManagerBase{TEntity}.CreateSheets"/> and
    /// <see cref="SheetManagerBase{TEntity}.DeleteSheets"/> (for temp-sheet creation) with
    /// Gig's fully-configured AddSheet requests (headers, formatting, validation, colors).
    /// </summary>
    protected override BatchUpdateSpreadsheetRequest GenerateSheetsRequest(List<string> sheetNames)
    {
        return GenerateSheetsHelpers.Generate(sheetNames);
    }

    /// <summary>
    /// Resolves a self-healed column's raw Validation name (e.g. "RANGE_SERVICE") into a concrete
    /// data validation rule, restoring dropdowns on a re-inserted column (GitHub issue #103) the
    /// same way <see cref="GenerateSheetsHelpers"/> already does at sheet-creation time.
    /// </summary>
    protected override DataValidationRule? GetDataValidation(ColumnInsertionInfo column)
    {
        if (string.IsNullOrEmpty(column.Validation))
        {
            return null;
        }

        var columnRange = $"{column.ColumnLetter}2:{column.ColumnLetter}";
        return GigSheetHelpers.GetDataValidation(column.Validation.GetValueFromName<Validation>(), columnRange);
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
        return await ChangeSheetDataCoreAsync(sheets, sheetEntity, _sheetAccessors, HandleMissingSheets, cancellationToken);
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
