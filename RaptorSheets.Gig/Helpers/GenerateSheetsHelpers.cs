using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Managers;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Mappers;

namespace RaptorSheets.Gig.Helpers;

public static class GenerateSheetsHelpers
{
    public static BatchUpdateSpreadsheetRequest Generate(List<string> sheets)
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

    private static SheetModel GetSheetModel(string sheet)
    {
        return sheet switch
        {
            var s when string.Equals(s, SheetsConfig.SheetNames.Addresses, StringComparison.OrdinalIgnoreCase) => AddressMapper.GetSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.Daily, StringComparison.OrdinalIgnoreCase) => DailyMapper.GetSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.Expenses, StringComparison.OrdinalIgnoreCase) => GenericSheetMapper<ExpenseEntity>.GetSheet(SheetsConfig.ExpenseSheet),
            var s when string.Equals(s, SheetsConfig.SheetNames.Monthly, StringComparison.OrdinalIgnoreCase) => MonthlyMapper.GetSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.Names, StringComparison.OrdinalIgnoreCase) => NameMapper.GetSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.Places, StringComparison.OrdinalIgnoreCase) => PlaceMapper.GetSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.Deliveries, StringComparison.OrdinalIgnoreCase) => DeliveryMapper.GetSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.Locations, StringComparison.OrdinalIgnoreCase) => LocationMapper.GetSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.Regions, StringComparison.OrdinalIgnoreCase) => RegionMapper.GetSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.Services, StringComparison.OrdinalIgnoreCase) => ServiceMapper.GetSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.Setup, StringComparison.OrdinalIgnoreCase) => GenericSheetMapper<SetupEntity>.GetSheet(SheetsConfig.SetupSheet),
            var s when string.Equals(s, SheetsConfig.SheetNames.Shifts, StringComparison.OrdinalIgnoreCase) => ShiftMapper.GetSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.Trips, StringComparison.OrdinalIgnoreCase) => TripMapper.GetSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.Types, StringComparison.OrdinalIgnoreCase) => TypeMapper.GetSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.Weekdays, StringComparison.OrdinalIgnoreCase) => WeekdayMapper.GetSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.Weekly, StringComparison.OrdinalIgnoreCase) => WeeklyMapper.GetSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.Yearly, StringComparison.OrdinalIgnoreCase) => YearlyMapper.GetSheet(),
            // DeleteSheets' temp-sheet safety mechanism (GoogleSheetManagerBase<TEntity>.DeleteSheets)
            // asks for a bare AddSheet request for this specific ad-hoc, non-domain name - anything
            // else unrecognized is a genuine caller error and should still throw.
            var s when string.Equals(s, GoogleSheetManagerBase.TempSheetName, StringComparison.OrdinalIgnoreCase) => new SheetModel { Name = s },
            _ => throw new NotImplementedException($"Sheet model not found for: {sheet}"),
        };
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
