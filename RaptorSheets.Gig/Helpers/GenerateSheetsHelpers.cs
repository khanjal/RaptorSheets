using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Mappers;

namespace RaptorSheets.Gig.Helpers;

public static class GenerateSheetsHelpers
{
    private static BatchUpdateSpreadsheetRequest? _batchUpdateSpreadsheetRequest;
    private static List<RepeatCellRequest>? _repeatCellRequests;

    public static BatchUpdateSpreadsheetRequest Generate(List<string> sheets)
    {
        _batchUpdateSpreadsheetRequest = new BatchUpdateSpreadsheetRequest();
        _batchUpdateSpreadsheetRequest.Requests = [];
        _repeatCellRequests = [];

        sheets.ForEach(sheet =>
        {
            var sheetModel = GetSheetModel(sheet);
            var random = new Random();
            sheetModel.Id = random.Next();

            _batchUpdateSpreadsheetRequest!.Requests.Add(GoogleRequestHelpers.GenerateSheetPropertes(sheetModel));

            var appendDimension = GoogleRequestHelpers.GenerateAppendDimension(sheetModel);
            if (appendDimension != null)
            {
                _batchUpdateSpreadsheetRequest!.Requests.Add(appendDimension);
            }

            _batchUpdateSpreadsheetRequest!.Requests.Add(GoogleRequestHelpers.GenerateAppendCells(sheetModel));
            GenerateHeadersFormatAndProtection(sheetModel);
            _batchUpdateSpreadsheetRequest!.Requests.Add(GoogleRequestHelpers.GenerateBandingRequest(sheetModel));
            _batchUpdateSpreadsheetRequest!.Requests.Add(GoogleRequestHelpers.GenerateProtectedRangeForHeaderOrSheet(sheetModel));
        });

        _repeatCellRequests.ForEach(request =>
        {
            _batchUpdateSpreadsheetRequest.Requests.Add(new Request { RepeatCell = request });
        });

        return _batchUpdateSpreadsheetRequest;
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
            var s when string.Equals(s, SheetsConfig.SheetNames.Regions, StringComparison.OrdinalIgnoreCase) => RegionMapper.GetSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.Services, StringComparison.OrdinalIgnoreCase) => ServiceMapper.GetSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.Setup, StringComparison.OrdinalIgnoreCase) => GenericSheetMapper<SetupEntity>.GetSheet(SheetsConfig.SetupSheet),
            var s when string.Equals(s, SheetsConfig.SheetNames.Shifts, StringComparison.OrdinalIgnoreCase) => ShiftMapper.GetSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.Trips, StringComparison.OrdinalIgnoreCase) => TripMapper.GetSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.Types, StringComparison.OrdinalIgnoreCase) => TypeMapper.GetSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.Weekdays, StringComparison.OrdinalIgnoreCase) => WeekdayMapper.GetSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.Weekly, StringComparison.OrdinalIgnoreCase) => WeeklyMapper.GetSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.Yearly, StringComparison.OrdinalIgnoreCase) => YearlyMapper.GetSheet(),
            _ => throw new NotImplementedException($"Sheet model not found for: {sheet}"),
        };
    }

    private static void GenerateHeadersFormatAndProtection(SheetModel sheet)
    {
        // Ensure headers have proper Column/Index assignments prior to formatting, like Stock implementation
        sheet.Headers.UpdateColumns();

        // Format/Protect Column Cells
        sheet!.Headers.ForEach(header =>
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
                _batchUpdateSpreadsheetRequest!.Requests.Add(GoogleRequestHelpers.GenerateColumnProtection(range));
            }

            // If there's no format or validation then go to next header
            if (header.Format == null && string.IsNullOrEmpty(header.Validation))
            {
                return;
            }

            var repeatCellModel = new RepeatCellModel
            {
                GridRange = range,
            };

            if (header.Format != null)
            {
                repeatCellModel.CellFormat = SheetHelpers.GetCellFormat((FormatEnum)header.Format);
            }

            if (!string.IsNullOrEmpty(header.Validation))
            {
                var columnRange = $"{header.Column}2:{header.Column}";
                repeatCellModel.DataValidation = GigSheetHelpers.GetDataValidation(header.Validation.GetValueFromName<ValidationEnum>(), columnRange);
            }

            _repeatCellRequests!.Add(GoogleRequestHelpers.GenerateRepeatCellRequest(repeatCellModel));
        });
    }  
}
