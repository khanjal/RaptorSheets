using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Models.Google;

namespace RaptorSheets.Core.Helpers;

/// <summary>
/// Builds the AddSheet batch-update request (properties, headers, formats, validation,
/// protection, banding) for a set of sheet names. Every domain's own <c>GenerateSheetsHelpers.Generate</c>
/// was previously a near-identical ~120-line copy of this orchestration, differing only in how a
/// sheet name resolves to its <see cref="SheetModel"/> and how a header's raw <c>Validation</c>
/// string resolves to a concrete <see cref="DataValidationRule"/> - both genuinely domain-specific,
/// so they're taken as delegates rather than hoisted themselves.
/// </summary>
public static class SheetGenerationHelper
{
    /// <summary>
    /// Builds the full AddSheet batch request for <paramref name="sheets"/>.
    /// </summary>
    /// <param name="sheets">Sheet names to generate requests for.</param>
    /// <param name="getSheetModel">Resolves a sheet name to its domain-configured SheetModel (headers, formulas, colors, protection).</param>
    /// <param name="getDataValidation">Resolves a header's raw <c>Validation</c> string to a concrete data validation rule; only called for headers where Validation is set.</param>
    public static BatchUpdateSpreadsheetRequest Generate(
        List<string> sheets,
        Func<string, SheetModel> getSheetModel,
        Func<SheetCellModel, DataValidationRule?> getDataValidation)
    {
        if (sheets.Count == 0)
        {
            return new BatchUpdateSpreadsheetRequest { Requests = new List<Request>() };
        }

        var batchUpdateSpreadsheetRequest = new BatchUpdateSpreadsheetRequest
        {
            Requests = []
        };
        var repeatCellRequests = new List<RepeatCellRequest>();

        foreach (var sheet in sheets)
        {
            var sheetModel = getSheetModel(sheet);
            sheetModel.Id = Random.Shared.Next();

            batchUpdateSpreadsheetRequest.Requests.Add(GoogleRequestHelpers.GenerateSheetPropertes(sheetModel));

            var appendDimension = GoogleRequestHelpers.GenerateAppendDimension(sheetModel);
            if (appendDimension != null)
            {
                batchUpdateSpreadsheetRequest.Requests.Add(appendDimension);
            }

            batchUpdateSpreadsheetRequest.Requests.Add(GoogleRequestHelpers.GenerateAppendCells(sheetModel));
            GenerateHeadersFormatAndProtection(sheetModel, batchUpdateSpreadsheetRequest, repeatCellRequests, getDataValidation);
            batchUpdateSpreadsheetRequest.Requests.Add(GoogleRequestHelpers.GenerateBandingRequest(sheetModel));
            batchUpdateSpreadsheetRequest.Requests.Add(GoogleRequestHelpers.GenerateProtectedRangeForHeaderOrSheet(sheetModel));
        }

        foreach (var request in repeatCellRequests)
        {
            batchUpdateSpreadsheetRequest.Requests.Add(new Request { RepeatCell = request });
        }

        return batchUpdateSpreadsheetRequest;
    }

    private static void GenerateHeadersFormatAndProtection(
        SheetModel sheet,
        BatchUpdateSpreadsheetRequest batchUpdateSpreadsheetRequest,
        List<RepeatCellRequest> repeatCellRequests,
        Func<SheetCellModel, DataValidationRule?> getDataValidation)
    {
        // Ensure headers have proper Column/Index assignments prior to formatting
        sheet.Headers.UpdateColumns();

        foreach (var header in sheet.Headers)
        {
            var range = new GridRange
            {
                SheetId = sheet.Id,
                StartColumnIndex = header.Index,
                EndColumnIndex = header.Index + 1,
                StartRowIndex = 1,
            };

            // If whole sheet isn't protected then protect formula columns individually
            if (!string.IsNullOrEmpty(header.Formula) && !sheet.ProtectSheet)
            {
                batchUpdateSpreadsheetRequest.Requests.Add(GoogleRequestHelpers.GenerateColumnProtection(range));
            }

            // If there's no format or validation then go to the next header
            if (header.Format == null && string.IsNullOrEmpty(header.Validation) && string.IsNullOrEmpty(header.FormatPattern))
            {
                continue;
            }

            var repeatCellModel = new RepeatCellModel
            {
                GridRange = range,
            };

            if (header.Format != null || !string.IsNullOrEmpty(header.FormatPattern))
            {
                var formatToUse = header.Format ?? Format.NUMBER; // Default to NUMBER if only pattern provided

                // FormatPattern is the single source of truth - always populated either from a
                // custom pattern or derived from Format
                repeatCellModel.CellFormat = !string.IsNullOrEmpty(header.FormatPattern)
                    ? SheetHelpers.GetCellFormat(formatToUse, header.FormatPattern)
                    : SheetHelpers.GetCellFormat(formatToUse);
            }

            if (!string.IsNullOrEmpty(header.Validation))
            {
                repeatCellModel.DataValidation = getDataValidation(header);
            }

            repeatCellRequests.Add(GoogleRequestHelpers.GenerateRepeatCellRequest(repeatCellModel));
        }
    }
}
