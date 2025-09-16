using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Mappers;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Common.Mappers;
using RaptorSheets.Gig.Constants;

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
        var upper = sheet.ToUpperInvariant();
        return upper switch
        {
            var s when s == SheetsConfig.SheetNames.Addresses.ToUpperInvariant() => AddressMapper.GetSheet(),
            var s when s == SheetsConfig.SheetNames.Daily.ToUpperInvariant() => DailyMapper.GetSheet(),
            var s when s == SheetsConfig.SheetNames.Expenses.ToUpperInvariant() => ExpenseMapper.GetSheet(),
            var s when s == SheetsConfig.SheetNames.Monthly.ToUpperInvariant() => MonthlyMapper.GetSheet(),
            var s when s == SheetsConfig.SheetNames.Names.ToUpperInvariant() => NameMapper.GetSheet(),
            var s when s == SheetsConfig.SheetNames.Places.ToUpperInvariant() => PlaceMapper.GetSheet(),
            var s when s == SheetsConfig.SheetNames.Regions.ToUpperInvariant() => RegionMapper.GetSheet(),
            var s when s == SheetsConfig.SheetNames.Services.ToUpperInvariant() => ServiceMapper.GetSheet(),
            var s when s == SheetsConfig.SheetNames.Setup.ToUpperInvariant() => SetupMapper.GetSheet(),
            var s when s == SheetsConfig.SheetNames.Shifts.ToUpperInvariant() => ShiftMapper.GetSheet(),
            var s when s == SheetsConfig.SheetNames.Trips.ToUpperInvariant() => TripMapper.GetSheet(),
            var s when s == SheetsConfig.SheetNames.Types.ToUpperInvariant() => TypeMapper.GetSheet(),
            var s when s == SheetsConfig.SheetNames.Weekdays.ToUpperInvariant() => WeekdayMapper.GetSheet(),
            var s when s == SheetsConfig.SheetNames.Weekly.ToUpperInvariant() => WeeklyMapper.GetSheet(),
            var s when s == SheetsConfig.SheetNames.Yearly.ToUpperInvariant() => YearlyMapper.GetSheet(),
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
