using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Managers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Job.Constants;
using RaptorSheets.Job.Sheets;

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
            var s when string.Equals(s, SheetsConfig.SheetNames.Applications, StringComparison.OrdinalIgnoreCase) => ApplicationSheet.GetSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.Interviews, StringComparison.OrdinalIgnoreCase) => InterviewSheet.GetSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.Companies, StringComparison.OrdinalIgnoreCase) => CompanySheet.GetSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.Positions, StringComparison.OrdinalIgnoreCase) => PositionSheet.GetSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.Sites, StringComparison.OrdinalIgnoreCase) => SiteSheet.GetSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.Decisions, StringComparison.OrdinalIgnoreCase) => DecisionSheet.GetSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.InterviewTypes, StringComparison.OrdinalIgnoreCase) => InterviewTypeSheet.GetSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.InterviewOutcomes, StringComparison.OrdinalIgnoreCase) => InterviewOutcomeSheet.GetSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.Schedules, StringComparison.OrdinalIgnoreCase) => ScheduleSheet.GetSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.CompanyDetails, StringComparison.OrdinalIgnoreCase) => CompanyDetailSheet.GetSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.PositionDetails, StringComparison.OrdinalIgnoreCase) => PositionDetailSheet.GetSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.Setup, StringComparison.OrdinalIgnoreCase) => SetupSheet.GetSheet(),
            // DeleteSheets' temp-sheet safety mechanism asks for a bare AddSheet request for this
            // specific ad-hoc name - anything else unrecognized is a genuine caller error.
            var s when string.Equals(s, GoogleSheetManagerBase.TempSheetName, StringComparison.OrdinalIgnoreCase) => new SheetModel { Name = s },
            _ => throw new NotImplementedException($"Sheet model not found for: {sheet}"),
        };
    }
}
