using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Managers;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Job.Constants;
using RaptorSheets.Job.Entities;
using RaptorSheets.Job.Mappers;

namespace RaptorSheets.Job.Helpers;

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
            var s when string.Equals(s, SheetsConfig.SheetNames.Applications, StringComparison.OrdinalIgnoreCase) => ApplicationMapper.GetSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.Interviews, StringComparison.OrdinalIgnoreCase) => InterviewMapper.GetSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.Companies, StringComparison.OrdinalIgnoreCase) => CompanyMapper.GetSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.Positions, StringComparison.OrdinalIgnoreCase) => PositionMapper.GetSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.Sites, StringComparison.OrdinalIgnoreCase) => ValidationMapper.GetSiteSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.Decisions, StringComparison.OrdinalIgnoreCase) => ValidationMapper.GetDecisionSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.InterviewTypes, StringComparison.OrdinalIgnoreCase) => ValidationMapper.GetInterviewTypeSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.InterviewOutcomes, StringComparison.OrdinalIgnoreCase) => ValidationMapper.GetInterviewOutcomeSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.Schedules, StringComparison.OrdinalIgnoreCase) => ValidationMapper.GetScheduleSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.CompanyDetails, StringComparison.OrdinalIgnoreCase) => GenericSheetMapper<CompanyDetailEntity>.GetSheet(SheetsConfig.CompanyDetailSheet),
            var s when string.Equals(s, SheetsConfig.SheetNames.PositionDetails, StringComparison.OrdinalIgnoreCase) => GenericSheetMapper<PositionDetailEntity>.GetSheet(SheetsConfig.PositionDetailSheet),
            var s when string.Equals(s, SheetsConfig.SheetNames.Setup, StringComparison.OrdinalIgnoreCase) => ValidationMapper.GetSetupSheet(),
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
                repeatCellModel.DataValidation = JobSheetHelpers.GetDataValidation(header.Validation);
            }

            repeatCellRequests.Add(GoogleRequestHelpers.GenerateRepeatCellRequest(repeatCellModel));
        }
    }
}
