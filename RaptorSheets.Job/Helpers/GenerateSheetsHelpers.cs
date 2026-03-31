using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Job.Constants;
using RaptorSheets.Job.Entities;
using RaptorSheets.Job.Mappers;

namespace RaptorSheets.Job.Helpers;

public static class GenerateSheetsHelpers
{
    private static BatchUpdateSpreadsheetRequest? _batchUpdateSpreadsheetRequest;
    private static List<RepeatCellRequest>? _repeatCellRequests;

    public static BatchUpdateSpreadsheetRequest Generate(List<string> sheets)
    {
        if (sheets.Count == 0)
        {
            return new BatchUpdateSpreadsheetRequest { Requests = new List<Request>() };
        }

        _batchUpdateSpreadsheetRequest = new BatchUpdateSpreadsheetRequest();
        _batchUpdateSpreadsheetRequest.Requests = [];
        _repeatCellRequests = [];

        foreach (var sheet in sheets)
        {
            var sheetModel = GetSheetModel(sheet);
            var random = new Random();
            sheetModel.Id = random.Next();

            _batchUpdateSpreadsheetRequest.Requests.Add(GoogleRequestHelpers.GenerateSheetPropertes(sheetModel));

            var appendDimension = GoogleRequestHelpers.GenerateAppendDimension(sheetModel);
            if (appendDimension != null)
            {
                _batchUpdateSpreadsheetRequest.Requests.Add(appendDimension);
            }

            _batchUpdateSpreadsheetRequest.Requests.Add(GoogleRequestHelpers.GenerateAppendCells(sheetModel));
            GenerateHeadersFormatAndProtection(sheetModel);
            _batchUpdateSpreadsheetRequest.Requests.Add(GoogleRequestHelpers.GenerateBandingRequest(sheetModel));
            _batchUpdateSpreadsheetRequest.Requests.Add(GoogleRequestHelpers.GenerateProtectedRangeForHeaderOrSheet(sheetModel));
        }

        foreach (var request in _repeatCellRequests)
        {
            _batchUpdateSpreadsheetRequest.Requests.Add(new Request { RepeatCell = request });
        }

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
            var s when string.Equals(s, SheetsConfig.SheetNames.Applications, StringComparison.OrdinalIgnoreCase) 
                => ApplicationMapper.GetSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.Interviews, StringComparison.OrdinalIgnoreCase) 
                => InterviewMapper.GetSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.Companies, StringComparison.OrdinalIgnoreCase) 
                => CompanyMapper.GetSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.Positions, StringComparison.OrdinalIgnoreCase) 
                => PositionMapper.GetSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.Sites, StringComparison.OrdinalIgnoreCase) 
                => ValidationMapper.GetSiteSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.Decisions, StringComparison.OrdinalIgnoreCase) 
                => ValidationMapper.GetDecisionSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.InterviewTypes, StringComparison.OrdinalIgnoreCase) 
                => ValidationMapper.GetInterviewTypeSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.InterviewOutcomes, StringComparison.OrdinalIgnoreCase) 
                => ValidationMapper.GetInterviewOutcomeSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.Schedules, StringComparison.OrdinalIgnoreCase) 
                => ValidationMapper.GetScheduleSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.CompanyDetails, StringComparison.OrdinalIgnoreCase) 
                => GenericSheetMapper<CompanyDetailEntity>.GetSheet(SheetsConfig.CompanyDetailSheet),
            var s when string.Equals(s, SheetsConfig.SheetNames.PositionDetails, StringComparison.OrdinalIgnoreCase) 
                => GenericSheetMapper<PositionDetailEntity>.GetSheet(SheetsConfig.PositionDetailSheet),
            var s when string.Equals(s, SheetsConfig.SheetNames.Setup, StringComparison.OrdinalIgnoreCase) 
                => ValidationMapper.GetSetupSheet(),
            _ => throw new NotImplementedException($"Sheet model not found for: {sheet}"),
        };
    }

    private static void GenerateHeadersFormatAndProtection(SheetModel sheet)
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

            // If whole sheet isn't protected then protect certain columns
            if (!string.IsNullOrEmpty(header.Formula) && !sheet.ProtectSheet)
            {
                _batchUpdateSpreadsheetRequest!.Requests.Add(GoogleRequestHelpers.GenerateColumnProtection(range));
            }

            // If there's no format or validation then go to next header
            if (header.Format == null && string.IsNullOrEmpty(header.Validation) && string.IsNullOrEmpty(header.FormatPattern))
            {
                continue;
            }

            var repeatCellModel = new Core.Models.Google.RepeatCellModel
            {
                GridRange = range,
            };

            // Apply formatting if Format or FormatPattern exists
            if (header.Format != null || !string.IsNullOrEmpty(header.FormatPattern))
            {
                var formatToUse = header.Format ?? Core.Enums.FormatEnum.NUMBER;

                repeatCellModel.CellFormat = !string.IsNullOrEmpty(header.FormatPattern)
                    ? Core.Helpers.SheetHelpers.GetCellFormat(formatToUse, header.FormatPattern)
                    : Core.Helpers.SheetHelpers.GetCellFormat(formatToUse);
            }

            if (!string.IsNullOrEmpty(header.Validation))
            {
                // Validation range needs = prefix for Google Sheets
                // Sheet names with spaces need to be wrapped in single quotes
                var validationValue = header.Validation;
                if (!validationValue.StartsWith("="))
                {
                    validationValue = $"={validationValue}";
                }

                // Check if the range contains a sheet name with spaces and wrap it in quotes if needed
                if (validationValue.Contains("!") && !validationValue.Contains("'"))
                {
                    var parts = validationValue.Split('!');
                    if (parts[0].Contains(" "))
                    {
                        validationValue = $"='{parts[0].TrimStart('=')}'!{parts[1]}";
                    }
                }

                repeatCellModel.DataValidation = new Google.Apis.Sheets.v4.Data.DataValidationRule
                {
                    Condition = new Google.Apis.Sheets.v4.Data.BooleanCondition
                    {
                        Type = "ONE_OF_RANGE",
                        Values = new List<Google.Apis.Sheets.v4.Data.ConditionValue>
                        {
                            new() { UserEnteredValue = validationValue }
                        }
                    },
                    ShowCustomUi = true,
                    Strict = false
                };
            }

            _repeatCellRequests!.Add(GoogleRequestHelpers.GenerateRepeatCellRequest(repeatCellModel));
        }
    }
}
