using Google.Apis.Sheets.v4.Data;
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
        return SheetGenerationHelper.Generate(sheets, GetSheetModel, header => JobSheetHelpers.GetDataValidation(header.Validation));
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
}
