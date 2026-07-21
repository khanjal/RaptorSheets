using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Managers;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Home.Constants;
using RaptorSheets.Home.Entities;
using RaptorSheets.Home.Enums;
using RaptorSheets.Home.Mappers;

namespace RaptorSheets.Home.Helpers;

public static class GenerateSheetsHelpers
{
    public static BatchUpdateSpreadsheetRequest Generate(List<string> sheets)
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
            var sheetModel = GetSheetModel(sheet);
            var random = new Random();
            sheetModel.Id = random.Next();

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
            var s when string.Equals(s, SheetsConfig.SheetNames.Appliances, StringComparison.OrdinalIgnoreCase) => ApplianceMapper.GetSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.Projects, StringComparison.OrdinalIgnoreCase) => GenericSheetMapper<ProjectEntity>.GetSheet(SheetsConfig.ProjectSheet),
            var s when string.Equals(s, SheetsConfig.SheetNames.Maintenance, StringComparison.OrdinalIgnoreCase) => GenericSheetMapper<MaintenanceEntity>.GetSheet(SheetsConfig.MaintenanceSheet),
            var s when string.Equals(s, SheetsConfig.SheetNames.Doors, StringComparison.OrdinalIgnoreCase) => GenericSheetMapper<DoorEntity>.GetSheet(SheetsConfig.DoorSheet),
            var s when string.Equals(s, SheetsConfig.SheetNames.Paints, StringComparison.OrdinalIgnoreCase) => GenericSheetMapper<PaintEntity>.GetSheet(SheetsConfig.PaintSheet),
            var s when string.Equals(s, SheetsConfig.SheetNames.Power, StringComparison.OrdinalIgnoreCase) => GenericSheetMapper<PowerEntity>.GetSheet(SheetsConfig.PowerSheet),
            var s when string.Equals(s, SheetsConfig.SheetNames.Rooms, StringComparison.OrdinalIgnoreCase) => RoomMapper.GetSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.Contacts, StringComparison.OrdinalIgnoreCase) => GenericSheetMapper<ContactEntity>.GetSheet(SheetsConfig.ContactSheet),
            var s when string.Equals(s, SheetsConfig.SheetNames.Stats, StringComparison.OrdinalIgnoreCase) => GenericSheetMapper<StatEntity>.GetSheet(SheetsConfig.StatSheet),
            // DeleteSheets' temp-sheet safety mechanism asks for a bare AddSheet request for this
            // specific ad-hoc name - anything else unrecognized is a genuine caller error.
            var s when string.Equals(s, GoogleSheetManagerBase.TempSheetName, StringComparison.OrdinalIgnoreCase) => new SheetModel { Name = s },
            _ => throw new NotImplementedException($"Sheet model not found for: {sheet}"),
        };
    }

    private static void GenerateHeadersFormatAndProtection(
        SheetModel sheet,
        BatchUpdateSpreadsheetRequest batchUpdateSpreadsheetRequest,
        List<RepeatCellRequest> repeatCellRequests)
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

            // If whole sheet isn't protected then protect formula columns
            if (!string.IsNullOrEmpty(header.Formula) && !sheet.ProtectSheet)
            {
                batchUpdateSpreadsheetRequest.Requests.Add(GoogleRequestHelpers.GenerateColumnProtection(range));
            }

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
                var formatToUse = header.Format ?? FormatEnum.NUMBER;

                repeatCellModel.CellFormat = !string.IsNullOrEmpty(header.FormatPattern)
                    ? SheetHelpers.GetCellFormat(formatToUse, header.FormatPattern)
                    : SheetHelpers.GetCellFormat(formatToUse);
            }

            if (!string.IsNullOrEmpty(header.Validation))
            {
                var columnRange = $"{header.Column}2:{header.Column}";
                repeatCellModel.DataValidation = HomeSheetHelpers.GetDataValidation(header.Validation.GetValueFromName<ValidationEnum>(), columnRange);
            }

            repeatCellRequests.Add(GoogleRequestHelpers.GenerateRepeatCellRequest(repeatCellModel));
        }
    }
}
