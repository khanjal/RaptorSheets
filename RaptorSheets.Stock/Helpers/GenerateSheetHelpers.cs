using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Managers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Stock.Enums;
using RaptorSheets.Stock.Mappers;

namespace RaptorSheets.Stock.Helpers;

public static class GenerateSheetHelpers
{
    private static readonly Random _random = new();

    public static BatchUpdateSpreadsheetRequest Generate(List<string> sheets)
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

    /// <summary>
    /// Resolves a sheet name to its configured model. Anything matching a real
    /// <see cref="Enums.SheetName"/> description gets its fully-configured domain sheet; the one
    /// recognized ad-hoc exception is <see cref="GoogleSheetManagerBase.TempSheetName"/>, which
    /// DeleteSheets' safety mechanism needs a bare AddSheet request for. Anything else is a genuine
    /// caller error.
    /// </summary>
    private static SheetModel GetSheetModel(string sheet)
    {
        var sheetEnum = sheet.GetValueFromName<Enums.SheetName>();
        var isKnownSheet = string.Equals(sheetEnum.GetDescription(), sheet, StringComparison.OrdinalIgnoreCase);

        if (isKnownSheet)
        {
            return GetSheetModel(sheetEnum);
        }

        if (string.Equals(sheet, GoogleSheetManagerBase.TempSheetName, StringComparison.OrdinalIgnoreCase))
        {
            return new SheetModel { Name = sheet };
        }

        throw new NotImplementedException($"Sheet model not found for: {sheet}");
    }

    private static SheetModel GetSheetModel(Enums.SheetName sheetEnum)
    {
        return sheetEnum switch
        {
            Enums.SheetName.ACCOUNTS => AccountMapper.GetSheet(),
            Enums.SheetName.STOCKS => StockMapper.GetSheet(),
            Enums.SheetName.TICKERS => TickerMapper.GetSheet(),
            _ => throw new NotImplementedException(),
        };
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
            if (header.Format == null && header.Validation == null)
            {
                return;
            }

            var repeatCellModel = new RepeatCellModel
            {
                GridRange = range,
                CellFormat = (header.Format != null ? SheetHelpers.GetCellFormat((Format)header.Format) : null),
                DataValidation = (header.Validation != null ? StockSheetHelpers.GetDataValidation(header.Validation.GetValueFromName<Validation>()) : null)
            };

            repeatCellRequests.Add(GoogleRequestHelpers.GenerateRepeatCellRequest(repeatCellModel));
        });
    }
}
