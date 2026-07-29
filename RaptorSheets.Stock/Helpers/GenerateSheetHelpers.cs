using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Managers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Stock.Enums;
using RaptorSheets.Stock.Sheets;

namespace RaptorSheets.Stock.Helpers;

public static class GenerateSheetHelpers
{
    private static readonly Random _random = new();

    internal static BatchUpdateSpreadsheetRequest Generate(List<string> sheets)
    {
        var batchUpdateSpreadsheetRequest = new BatchUpdateSpreadsheetRequest { Requests = [] };

        if (sheets.Count == 0)
        {
            return batchUpdateSpreadsheetRequest;
        }

        var repeatCellRequests = new List<RepeatCellRequest>();

        foreach (var sheet in sheets)
        {
            var sheetModel = GetSheetModel(sheet);
            sheetModel.Id = _random.Next();

            batchUpdateSpreadsheetRequest.Requests.Add(GoogleRequestHelpers.GenerateSheetPropertes(sheetModel));

            var appendDimension = GoogleRequestHelpers.GenerateAppendDimension(sheetModel);
            if (appendDimension != null)
            {
                batchUpdateSpreadsheetRequest.Requests.Add(appendDimension);
            }

            batchUpdateSpreadsheetRequest.Requests.Add(GoogleRequestHelpers.GenerateAppendCells(sheetModel));
            GenerateHeadersFormatAndProtection(sheetModel, batchUpdateSpreadsheetRequest, repeatCellRequests);
            batchUpdateSpreadsheetRequest.Requests.Add(GoogleRequestHelpers.GenerateBandingRequest(sheetModel));
            batchUpdateSpreadsheetRequest.Requests.Add(GoogleRequestHelpers.GenerateProtectedRangeForHeaderOrSheet(sheetModel));
        }

        foreach (var request in repeatCellRequests)
        {
            batchUpdateSpreadsheetRequest.Requests.Add(new Request { RepeatCell = request });
        }

        return batchUpdateSpreadsheetRequest;
    }

    // Case-insensitive name -> factory lookup, same convention as SheetRegistry<TEntity>'s own
    // _factories dictionary (and now Gig/Home/Job's GenerateSheetsHelpers) - O(1) instead of a
    // string->enum round-trip plus a second switch on the enum.
    private static readonly Dictionary<string, Func<SheetModel>> _sheetModelFactories = new(StringComparer.OrdinalIgnoreCase)
    {
        [SheetName.ACCOUNTS.GetDescription()] = AccountSheet.GetSheet,
        [SheetName.STOCKS.GetDescription()] = StockSheet.GetSheet,
        [SheetName.TICKERS.GetDescription()] = TickerSheet.GetSheet,
        // DeleteSheets' temp-sheet safety mechanism asks for a bare AddSheet request for this
        // specific ad-hoc name.
        [SheetManagerBase.TempSheetName] = () => new SheetModel { Name = SheetManagerBase.TempSheetName },
    };

    private static SheetModel GetSheetModel(string sheet)
    {
        if (_sheetModelFactories.TryGetValue(sheet, out var factory))
        {
            return factory();
        }

        // Anything unrecognized is a genuine caller error and should still throw.
        throw new NotImplementedException($"Sheet model not found for: {sheet}");
    }

    private static void GenerateHeadersFormatAndProtection(
        SheetModel sheet,
        BatchUpdateSpreadsheetRequest batchUpdateSpreadsheetRequest,
        List<RepeatCellRequest> repeatCellRequests)
    {
        // Format/Protect Column Cells
        sheet.Headers.ForEach(header =>
        {
            var range = new GridRange
            {
                SheetId = sheet.Id,
                StartColumnIndex = header.Index,
                EndColumnIndex = header.Index + 1,
                StartRowIndex = 1,
            };

            // If whole sheet isn't protected then protect certain columns
            if (!string.IsNullOrEmpty(header.Formula) && !sheet.ProtectSheet)
            {
                batchUpdateSpreadsheetRequest.Requests.Add(GoogleRequestHelpers.GenerateColumnProtection(range));
            }

            // If there's no format or validation then go to next header
            if (header.Format == null && string.IsNullOrEmpty(header.Validation))
            {
                return;
            }

            var repeatCellModel = new RepeatCellModel
            {
                GridRange = range,
                CellFormat = (header.Format != null ? SheetHelpers.GetCellFormat((Format)header.Format) : null),
                DataValidation = (!string.IsNullOrEmpty(header.Validation) ? StockSheetHelpers.GetDataValidation(header.Validation.GetValueFromName<Validation>()) : null)
            };

            repeatCellRequests.Add(GoogleRequestHelpers.GenerateRepeatCellRequest(repeatCellModel));
        });
    }
}
