using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Managers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Sheets;

namespace RaptorSheets.Gig.Helpers;

public static class GenerateSheetsHelpers
{
    internal static BatchUpdateSpreadsheetRequest Generate(List<string> sheets)
    {
        if (sheets.Count == 0)
        {
            // Skip unnecessary processing when the collection is empty
            return new BatchUpdateSpreadsheetRequest { Requests = new List<Request>() };
        }

        var batchUpdateSpreadsheetRequest = new BatchUpdateSpreadsheetRequest
        {
            Requests = []
        };
        var repeatCellRequests = new List<RepeatCellRequest>();

        foreach (var sheet in sheets)
        {
            var sheetModel = GetSheetModel(sheet);
            sheetModel.Id = Random.Shared.Next();

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

    public static List<string> GetSheetNames()
    {
        return SheetsConfig.SheetUtilities.GetAllSheetNames();
    }

    // Case-insensitive name -> factory lookup, same convention as SheetRegistry<TEntity>'s own
    // _factories dictionary - O(1) instead of a sequential switch, and reads as a flat table instead
    // of N near-identical "var s when string.Equals(...)" arms.
    private static readonly Dictionary<string, Func<SheetModel>> _sheetModelFactories = new(StringComparer.OrdinalIgnoreCase)
    {
        [SheetsConfig.SheetNames.Addresses] = AddressSheet.GetSheet,
        [SheetsConfig.SheetNames.Daily] = DailySheet.GetSheet,
        [SheetsConfig.SheetNames.Expenses] = ExpenseSheet.GetSheet,
        [SheetsConfig.SheetNames.Monthly] = MonthlySheet.GetSheet,
        [SheetsConfig.SheetNames.Names] = NameSheet.GetSheet,
        [SheetsConfig.SheetNames.Places] = PlaceSheet.GetSheet,
        [SheetsConfig.SheetNames.Deliveries] = DeliverySheet.GetSheet,
        [SheetsConfig.SheetNames.Locations] = LocationSheet.GetSheet,
        [SheetsConfig.SheetNames.Regions] = RegionSheet.GetSheet,
        [SheetsConfig.SheetNames.Services] = ServiceSheet.GetSheet,
        [SheetsConfig.SheetNames.Setup] = SetupSheet.GetSheet,
        [SheetsConfig.SheetNames.Shifts] = ShiftSheet.GetSheet,
        [SheetsConfig.SheetNames.Trips] = TripSheet.GetSheet,
        [SheetsConfig.SheetNames.Types] = TypeSheet.GetSheet,
        [SheetsConfig.SheetNames.Weekdays] = WeekdaySheet.GetSheet,
        [SheetsConfig.SheetNames.Weekly] = WeeklySheet.GetSheet,
        [SheetsConfig.SheetNames.Yearly] = YearlySheet.GetSheet,
        // DeleteSheets' temp-sheet safety mechanism (SheetManagerBase<TEntity>.DeleteSheets) asks for
        // a bare AddSheet request for this specific ad-hoc, non-domain name.
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
        // Ensure headers have proper Column/Index assignments prior to formatting, like Stock implementation
        sheet.Headers.UpdateColumns();

        // Format/Protect Column Cells
        foreach (var header in sheet.Headers)
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
            if (header.Format == null && string.IsNullOrEmpty(header.Validation) && string.IsNullOrEmpty(header.FormatPattern))
            {
                continue;
            }

            var repeatCellModel = new RepeatCellModel
            {
                GridRange = range,
            };

            // Apply formatting if Format or FormatPattern exists
            if (header.Format != null || !string.IsNullOrEmpty(header.FormatPattern))
            {
                var formatToUse = header.Format ?? Format.NUMBER; // Default to NUMBER if only pattern provided

                // FormatPattern is the single source of truth - it's always populated
                // Either from custom pattern or derived from Format
                repeatCellModel.CellFormat = !string.IsNullOrEmpty(header.FormatPattern)
                    ? SheetHelpers.GetCellFormat(formatToUse, header.FormatPattern)
                    : SheetHelpers.GetCellFormat(formatToUse);
            }

            if (!string.IsNullOrEmpty(header.Validation))
            {
                var columnRange = $"{header.Column}2:{header.Column}";
                repeatCellModel.DataValidation = GigSheetHelpers.GetDataValidation(header.Validation.GetValueFromName<Validation>(), columnRange);
            }

            repeatCellRequests.Add(GoogleRequestHelpers.GenerateRepeatCellRequest(repeatCellModel));
        }
    }  
}
