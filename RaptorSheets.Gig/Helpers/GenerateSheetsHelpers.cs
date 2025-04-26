using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Mappers;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Extensions;
using SheetEnum = RaptorSheets.Gig.Enums.SheetEnum;
using RaptorSheets.Common.Mappers;

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
        var sheetNames = Enum.GetNames(typeof(SheetEnum)).ToList();
        sheetNames.AddRange([.. Enum.GetNames(typeof(Common.Enums.SheetEnum))]);
        return sheetNames;
    }

    private static SheetModel GetSheetModel(string sheet)
    {
        return sheet.ToUpper() switch
        {
            nameof(SheetEnum.ADDRESSES) => AddressMapper.GetSheet(),
            nameof(SheetEnum.DAILY) => DailyMapper.GetSheet(),
            nameof(SheetEnum.MONTHLY) => MonthlyMapper.GetSheet(),
            nameof(SheetEnum.NAMES) => NameMapper.GetSheet(),
            nameof(SheetEnum.PLACES) => PlaceMapper.GetSheet(),
            nameof(SheetEnum.REGIONS) => RegionMapper.GetSheet(),
            nameof(SheetEnum.SERVICES) => ServiceMapper.GetSheet(),
            nameof(Common.Enums.SheetEnum.SETUP) => SetupMapper.GetSheet(),
            nameof(SheetEnum.SHIFTS) => ShiftMapper.GetSheet(),
            nameof(SheetEnum.TRIPS) => TripMapper.GetSheet(),
            nameof(SheetEnum.TYPES) => TypeMapper.GetSheet(),
            nameof(SheetEnum.WEEKDAYS) => WeekdayMapper.GetSheet(),
            nameof(SheetEnum.WEEKLY) => WeeklyMapper.GetSheet(),
            nameof(SheetEnum.YEARLY) => YearlyMapper.GetSheet(),
            _ => throw new NotImplementedException(),
        };
    }

    private static void GenerateHeadersFormatAndProtection(SheetModel sheet)
    {
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
                repeatCellModel.DataValidation = GigSheetHelpers.GetDataValidation(header.Validation.GetValueFromName<ValidationEnum>());
            }

            _repeatCellRequests!.Add(GoogleRequestHelpers.GenerateRepeatCellRequest(repeatCellModel));
        });
    }  
}
