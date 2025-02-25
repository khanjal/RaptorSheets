using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Mappers;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Extensions;

namespace RaptorSheets.Gig.Helpers;

public static class GenerateSheetsHelpers
{
    private static BatchUpdateSpreadsheetRequest? _batchUpdateSpreadsheetRequest;
    private static List<RepeatCellRequest>? _repeatCellRequests;

    public static BatchUpdateSpreadsheetRequest Generate(List<SheetEnum> sheets)
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
            _batchUpdateSpreadsheetRequest!.Requests.AddRange(GoogleRequestHelpers.GenerateAppendDimension(sheetModel));
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

    private static SheetModel GetSheetModel(SheetEnum sheetEnum)
    {
        return sheetEnum switch
        {
            SheetEnum.ADDRESSES => AddressMapper.GetSheet(),
            SheetEnum.DAILY => DailyMapper.GetSheet(),
            SheetEnum.MONTHLY => MonthlyMapper.GetSheet(),
            SheetEnum.NAMES => NameMapper.GetSheet(),
            SheetEnum.PLACES => PlaceMapper.GetSheet(),
            SheetEnum.REGIONS => RegionMapper.GetSheet(),
            SheetEnum.SERVICES => ServiceMapper.GetSheet(),
            SheetEnum.SHIFTS => TripMapper.GetSheet(),
            SheetEnum.TRIPS => TripMapper.GetSheet(),
            SheetEnum.TYPES => TypeMapper.GetSheet(),
            SheetEnum.WEEKDAYS => WeekdayMapper.GetSheet(),
            SheetEnum.WEEKLY => WeeklyMapper.GetSheet(),
            SheetEnum.YEARLY => YearlyMapper.GetSheet(),
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
            if (header.Format == null && header.Validation == null)
            {
                return;
            }

            var repeatCellModel = new RepeatCellModel
            {
                GridRange = range,
                CellFormat = (header.Format != null ? SheetHelpers.GetCellFormat((FormatEnum)header.Format) : null),
                DataValidation = (header.Validation != null ? GigSheetHelpers.GetDataValidation(header.Validation.GetValueFromName<ValidationEnum>()) : null)
            };

            _repeatCellRequests!.Add(GoogleRequestHelpers.GenerateRepeatCellRequest(repeatCellModel));
        });
    }  
}
