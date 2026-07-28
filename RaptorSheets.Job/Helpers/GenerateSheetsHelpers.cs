using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Managers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Job.Constants;
using RaptorSheets.Job.Sheets;

namespace RaptorSheets.Job.Helpers;

public static class GenerateSheetsHelpers
{
    internal static BatchUpdateSpreadsheetRequest Generate(List<string> sheets)
    {
        return SheetGenerationHelper.Generate(sheets, GetSheetModel, header => JobSheetHelpers.GetDataValidation(header.Validation));
    }

    public static List<string> GetSheetNames()
    {
        return SheetsConfig.SheetUtilities.GetAllSheetNames();
    }

    // Case-insensitive name -> factory lookup, same convention as SheetRegistry<TEntity>'s own
    // _factories dictionary - O(1) instead of a sequential switch, and reads as a flat table instead
    // of N near-identical "var s when string.Equals(...)" arms.
    private static readonly Dictionary<string, Func<SheetModel>> _sheetModelFactories = new(StringComparer.OrdinalIgnoreCase)
    {
        [SheetsConfig.SheetNames.Applications] = ApplicationSheet.GetSheet,
        [SheetsConfig.SheetNames.Interviews] = InterviewSheet.GetSheet,
        [SheetsConfig.SheetNames.Companies] = CompanySheet.GetSheet,
        [SheetsConfig.SheetNames.Positions] = PositionSheet.GetSheet,
        [SheetsConfig.SheetNames.Sites] = SiteSheet.GetSheet,
        [SheetsConfig.SheetNames.Decisions] = DecisionSheet.GetSheet,
        [SheetsConfig.SheetNames.InterviewTypes] = InterviewTypeSheet.GetSheet,
        [SheetsConfig.SheetNames.InterviewOutcomes] = InterviewOutcomeSheet.GetSheet,
        [SheetsConfig.SheetNames.Schedules] = ScheduleSheet.GetSheet,
        [SheetsConfig.SheetNames.CompanyDetails] = CompanyDetailSheet.GetSheet,
        [SheetsConfig.SheetNames.PositionDetails] = PositionDetailSheet.GetSheet,
        [SheetsConfig.SheetNames.Setup] = SetupSheet.GetSheet,
        // DeleteSheets' temp-sheet safety mechanism asks for a bare AddSheet request for this
        // specific ad-hoc name.
        [SheetManagerBase.TempSheetName] = () => new SheetModel { Name = SheetManagerBase.TempSheetName },
    };

    private static SheetModel GetSheetModel(string sheet)
    {
        if (_sheetModelFactories.TryGetValue(sheet, out var factory))
        {
            return factory();
        }

        // Anything unrecognized is a genuine caller error and should still throw.
        throw new NotImplementedException($"Sheet model not found for: {sheet}");
    }
}
