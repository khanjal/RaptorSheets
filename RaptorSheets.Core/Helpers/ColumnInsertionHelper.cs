using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
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
    /// time. Columns within a sheet are inserted left-to-right (lowest index first): at the moment a
    /// given column's insert request runs, every column with a lower canonical index either already
    /// existed or was already re-inserted earlier in this same loop, so the live grid is always at
    /// least as wide as the target index - insertion never lands beyond the current bound. The
    /// reverse (highest-index-first) order this used to use breaks precisely when many columns are
    /// missing at once and the live sheet's grid has shrunk to fewer columns than the highest target
    /// index - Google rejects an InsertDimension whose StartIndex exceeds the sheet's current column
    /// count ("range.startIndex is larger than current grid size"), silently failing the entire
    /// self-heal batch. Found live restoring Gig's 15-column Daily rollup after deleting every
    /// non-key column - Core's/Stock's much smaller dependent sheets never had enough missing columns
    /// to hit it.
    /// </summary>
    public static List<Request> BuildInsertRequests(Dictionary<string, List<ColumnInsertionInfo>> missingColumns)
    {
        var requests = new List<Request>();

        foreach (var (_, columns) in missingColumns)
        {
            var sortedColumns = columns.OrderBy(c => c.ColumnIndex).ToList();

            foreach (var column in sortedColumns)
            {
                requests.Add(GoogleRequestHelpers.GenerateInsertColumnDimension(
                    column.SheetId,
                    column.ColumnIndex,
                    column.ColumnIndex + 1,
                    inheritFromBefore: true));

                requests.Add(BuildHeaderUpdateRequest(column));

                var formatRequest = GoogleRequestHelpers.GenerateColumnFormatRequest(column.SheetId, column.ColumnIndex, column.Format, column.FormatPattern);
                if (formatRequest != null)
                {
                    requests.Add(formatRequest);
                }

                // Always emit a validation request - set the rule when the column has one, and
                // explicitly clear it when it does not. The insert above uses InheritFromBefore, so
                // Google copies the left-hand neighbour's properties into the newly inserted column,
                // its data validation included. The "set" request is only produced when a rule
                // exists, so for an unvalidated column nothing else undoes that inheritance and the
                // column silently comes back wearing its neighbour's dropdown - Gig's Tags column
                // sits immediately right of the validated Region column and did exactly that.
                // A null rule here means "this column is defined as unvalidated", not "unknown",
                // because the canonical SheetModel is what produced it - so clearing is correct.
                requests.Add(
                    GoogleRequestHelpers.GenerateColumnValidationRequest(column.SheetId, column.ColumnIndex, column.ValidationRule)
                    ?? GoogleRequestHelpers.GenerateColumnValidationClearRequest(column.SheetId, column.ColumnIndex));
            }
        }

        return requests;
    }

    /// <summary>
    /// Mirrors <see cref="SheetHelpers"/>'s header-cell Formula/Protect-implies-formula-cell
    /// convention for a single re-inserted column (GitHub issue #53, gap 1) - a re-inserted formula
    /// column previously got only its header text back and computed nothing underneath it. Doesn't
    /// replicate that method's sheet-level bold/border header styling - this call site only has a
    /// single column's info, not the whole sheet's FontColor/ProtectSheet context.
    /// </summary>
    private static CellData BuildHeaderCell(ColumnInsertionInfo column)
    {
        var value = new ExtendedValue();

        // Use a formula if the column explicitly has one (non-empty) or if it's protected - an empty-
        // string formula only counts when protection is intended (same rule as sheet-creation time).
        if (column.Protect || !string.IsNullOrEmpty(column.Formula))
        {
            value.FormulaValue = column.Formula ?? column.ColumnName;
        }
        else
        {
            value.StringValue = column.ColumnName;
        }

        var cell = new CellData { UserEnteredValue = value };

        if (!string.IsNullOrEmpty(column.Note))
        {
            cell.Note = column.Note;
        }

        return cell;
    }

    /// <summary>
    /// Builds the single-column, row-0-only UpdateCells request that writes a column's header cell
    /// (name/formula/note, via <see cref="BuildHeaderCell"/>) using the narrow
    /// <see cref="Field.USER_ENTERED_VALUE_AND_NOTE"/> field mask - touches nothing but that one
    /// header cell, so it's safe to use both on a freshly-inserted (empty) column
    /// (<see cref="BuildInsertRequests"/>) and, unlike that path's own <see cref="Field.USER_ENTERED_VALUE_AND_FORMAT"/>-
    /// masked sheet-creation write, on an existing, already-populated column too - it never touches
    /// format/validation/data rows (GitHub issue #53, gap 3: reapplying just a drifted column shares
    /// this exact request shape with #53 gap 1's missing-column restoration).
    /// </summary>
    private static Request BuildHeaderUpdateRequest(ColumnInsertionInfo column)
    {
        var headerRow = new RowData
        {
            Values = [BuildHeaderCell(column)]
        };

        return GoogleRequestHelpers.GenerateUpdateCellsRequest(
            column.SheetId,
            rowIndex: 0,
            rows: [headerRow],
            startColumnIndex: column.ColumnIndex,
            fields: Field.USER_ENTERED_VALUE_AND_NOTE.GetDescription());
    }

    /// <summary>
    /// Builds one header-cell-fix request per entry in <paramref name="brokenColumns"/> - the
    /// reapply counterpart to <see cref="BuildInsertRequests"/>'s insertion, for columns that
    /// already exist but whose live Formula has drifted from canonical (GitHub issue #53, gap 3;
    /// detection lives in <see cref="Managers.SheetManagerBase{TEntity}.DetectBrokenColumnsAsync"/>).
    /// No InsertDimension (the column isn't missing) and no format/validation reapply (out of scope -
    /// see DetectBrokenColumnsAsync's own doc comment for why only Formula is safely comparable).
    /// </summary>
    public static List<Request> BuildHeaderFixRequests(List<ColumnInsertionInfo> brokenColumns)
    {
        return brokenColumns.Select(BuildHeaderUpdateRequest).ToList();
    }

    /// <summary>
    /// Inserts every missing column described in <paramref name="missingColumns"/> in a single
    /// batch request and returns a result entity describing what happened.
    /// </summary>
    /// <param name="additionalRequests">
    /// Extra requests folded into the same batch (e.g. dependent-sheet header-formula refreshes
    /// from <see cref="Managers.SheetManagerBase{TEntity}.AutoHealMissingColumnsAsync"/>), so
    /// they land in one atomic API call instead of a separate follow-up one.
    /// </param>
    public static async Task<TEntity> InsertMissingColumnsAsync<TEntity>(
        IGoogleSheetService googleSheetService,
        Dictionary<string, List<ColumnInsertionInfo>> missingColumns,
        IEnumerable<Request>? additionalRequests = null,
        CancellationToken cancellationToken = default)
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
        var result = await googleSheetService.BatchUpdateSpreadsheet(batchRequest, cancellationToken);

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
