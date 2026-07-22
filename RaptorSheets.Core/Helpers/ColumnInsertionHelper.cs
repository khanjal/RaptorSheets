using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Models;
using RaptorSheets.Core.Services;

namespace RaptorSheets.Core.Helpers;

/// <summary>
/// Builds and executes the Google Sheets batch request that physically inserts columns detected
/// as missing by <see cref="HeaderHelpers.CheckSheetHeaders(IList{object}, Models.Google.SheetModel, out List{ColumnInsertionInfo})"/>
/// (via <see cref="Registries.SheetRegistry{TEntity}"/>). Generic over the domain's SheetEntity type
/// so every domain package (Gig, Stock, and future ones) gets this for free.
/// </summary>
public static class ColumnInsertionHelper
{
    /// <summary>
    /// Builds the InsertDimension + UpdateCells requests for every missing column, one sheet at a
    /// time. Columns within a sheet are inserted right-to-left (highest index first) so earlier
    /// insertions don't shift the index of columns still to be inserted.
    /// </summary>
    public static List<Request> BuildInsertRequests(Dictionary<string, List<ColumnInsertionInfo>> missingColumns)
    {
        var requests = new List<Request>();

        foreach (var (_, columns) in missingColumns)
        {
            var sortedColumns = columns.OrderByDescending(c => c.ColumnIndex).ToList();

            foreach (var column in sortedColumns)
            {
                requests.Add(GoogleRequestHelpers.GenerateInsertColumnDimension(
                    column.SheetId,
                    column.ColumnIndex,
                    column.ColumnIndex + 1,
                    inheritFromBefore: true));

                var headerRow = new RowData
                {
                    Values = [new CellData { UserEnteredValue = new ExtendedValue { StringValue = column.ColumnName } }]
                };

                requests.Add(GoogleRequestHelpers.GenerateUpdateCellsRequest(
                    column.SheetId,
                    rowIndex: 0,
                    rows: [headerRow],
                    startColumnIndex: column.ColumnIndex));
            }
        }

        return requests;
    }

    /// <summary>
    /// Inserts every missing column described in <paramref name="missingColumns"/> in a single
    /// batch request and returns a result entity describing what happened.
    /// </summary>
    /// <param name="additionalRequests">
    /// Extra requests folded into the same batch (e.g. dependent-sheet header-formula refreshes
    /// from <see cref="Managers.GoogleSheetManagerBase{TEntity}.AutoHealMissingColumnsAsync"/>), so
    /// they land in one atomic API call instead of a separate follow-up one.
    /// </param>
    public static async Task<TEntity> InsertMissingColumnsAsync<TEntity>(
        IGoogleSheetService googleSheetService,
        Dictionary<string, List<ColumnInsertionInfo>> missingColumns,
        IEnumerable<Request>? additionalRequests = null)
        where TEntity : class, ISheetEntity, new()
    {
        var entity = new TEntity();

        if (missingColumns == null || missingColumns.Count == 0)
        {
            entity.Messages.Add(MessageHelpers.CreateInfoMessage("No missing columns to insert", MessageType.CHECK_SHEET));
            return entity;
        }

        foreach (var (sheetName, columns) in missingColumns)
        {
            foreach (var column in columns)
            {
                entity.Messages.Add(MessageHelpers.CreateInfoMessage(
                    $"Inserting column '{column.ColumnName}' at index {column.ColumnIndex} in sheet '{sheetName}'",
                    MessageType.CHECK_SHEET));
            }
        }

        var requests = BuildInsertRequests(missingColumns);

        if (additionalRequests != null)
        {
            requests.AddRange(additionalRequests);
        }

        var batchRequest = new BatchUpdateSpreadsheetRequest { Requests = requests };
        var result = await googleSheetService.BatchUpdateSpreadsheet(batchRequest);

        if (result != null)
        {
            entity.Messages.Add(MessageHelpers.CreateInfoMessage(
                $"Successfully inserted {missingColumns.Sum(kv => kv.Value.Count)} missing column(s)",
                MessageType.CHECK_SHEET));
        }
        else
        {
            entity.Messages.Add(MessageHelpers.CreateErrorMessage("Failed to insert missing columns", MessageType.CHECK_SHEET));
        }

        return entity;
    }
}
