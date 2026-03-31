using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Job.Constants;
using RaptorSheets.Job.Entities;
using RaptorSheets.Job.Mappers;

namespace RaptorSheets.Job.Helpers;

/// <summary>
/// Helper methods for Google Sheets operations in the Job domain.
/// </summary>
public static class JobSheetHelpers
{
    public static List<string> GetSheetNames()
    {
        return SheetsConfig.SheetUtilities.GetAllSheetNames();
    }

    public static SheetModel? GetSheetLayout(string sheetName)
    {
        return sheetName switch
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
            var s when string.Equals(s, SheetsConfig.SheetNames.Setup, StringComparison.OrdinalIgnoreCase) 
                => ValidationMapper.GetSetupSheet(),
            _ => null
        };
    }

    public static List<SheetModel> GetSheetLayouts(List<string> sheetNames)
    {
        return sheetNames
            .Select(GetSheetLayout)
            .Where(sheet => sheet != null)
            .Cast<SheetModel>()
            .ToList();
    }

    public static List<SheetModel> GetMissingSheets(Spreadsheet spreadsheet)
    {
        var existingSheetNames = spreadsheet.Sheets
            .Select(x => x.Properties.Title)
            .ToList();

        var allSheetNames = GetSheetNames();

        var missingSheetNames = allSheetNames
            .Where(name => !existingSheetNames.Any(existing => 
                string.Equals(existing, name, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        return GetSheetLayouts(missingSheetNames);
    }

    public static SheetEntity? MapData(BatchGetValuesByDataFilterResponse response)
    {
        if (response?.ValueRanges == null || !response.ValueRanges.Any())
            return null;

        var sheetEntity = new SheetEntity();

        foreach (var valueRange in response.ValueRanges)
        {
            var sheetName = valueRange.ValueRange.Range?.Split('!')[0].Trim('\'');
            if (string.IsNullOrEmpty(sheetName) || valueRange.ValueRange.Values == null || valueRange.ValueRange.Values.Count <= 1)
                continue;

            var values = valueRange.ValueRange.Values.Skip(1).ToList();

            switch (sheetName)
            {
                case var s when string.Equals(s, SheetsConfig.SheetNames.Applications, StringComparison.OrdinalIgnoreCase):
                    sheetEntity.Applications = GenericSheetMapper<ApplicationEntity>
                        .MapFromRangeData(values);
                    break;
                case var s when string.Equals(s, SheetsConfig.SheetNames.Interviews, StringComparison.OrdinalIgnoreCase):
                    sheetEntity.Interviews = GenericSheetMapper<InterviewEntity>
                        .MapFromRangeData(values);
                    break;
                case var s when string.Equals(s, SheetsConfig.SheetNames.Companies, StringComparison.OrdinalIgnoreCase):
                    sheetEntity.Companies = GenericSheetMapper<CompanyEntity>
                        .MapFromRangeData(values);
                    break;
                case var s when string.Equals(s, SheetsConfig.SheetNames.Positions, StringComparison.OrdinalIgnoreCase):
                    sheetEntity.Positions = GenericSheetMapper<PositionEntity>
                        .MapFromRangeData(values);
                    break;
                case var s when string.Equals(s, SheetsConfig.SheetNames.Sites, StringComparison.OrdinalIgnoreCase):
                    sheetEntity.Sites = GenericSheetMapper<SiteEntity>
                        .MapFromRangeData(values);
                    break;
                case var s when string.Equals(s, SheetsConfig.SheetNames.Decisions, StringComparison.OrdinalIgnoreCase):
                    sheetEntity.Decisions = GenericSheetMapper<DecisionEntity>
                        .MapFromRangeData(values);
                    break;
                case var s when string.Equals(s, SheetsConfig.SheetNames.InterviewTypes, StringComparison.OrdinalIgnoreCase):
                    sheetEntity.InterviewTypes = GenericSheetMapper<InterviewTypeEntity>
                        .MapFromRangeData(values);
                    break;
                case var s when string.Equals(s, SheetsConfig.SheetNames.InterviewOutcomes, StringComparison.OrdinalIgnoreCase):
                    sheetEntity.InterviewOutcomes = GenericSheetMapper<InterviewOutcomeEntity>
                        .MapFromRangeData(values);
                    break;
                case var s when string.Equals(s, SheetsConfig.SheetNames.Schedules, StringComparison.OrdinalIgnoreCase):
                    sheetEntity.Schedules = GenericSheetMapper<ScheduleEntity>
                        .MapFromRangeData(values);
                    break;
                case var s when string.Equals(s, SheetsConfig.SheetNames.Setup, StringComparison.OrdinalIgnoreCase):
                    sheetEntity.Setup = GenericSheetMapper<SetupEntity>
                        .MapFromRangeData(values);
                    break;
                default:
                    break;
            }
        }

        return sheetEntity;
    }
}
