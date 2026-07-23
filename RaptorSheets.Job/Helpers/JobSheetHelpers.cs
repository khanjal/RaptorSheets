using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Core.Registries;
using RaptorSheets.Job.Constants;
using RaptorSheets.Job.Entities;
using RaptorSheets.Job.Sheets;

namespace RaptorSheets.Job.Helpers;

/// <summary>
/// Helper methods for Google Sheets operations in the Job domain.
///
/// Per-sheet dispatch (headers, row mapping, missing-column detection) is delegated to a shared
/// RaptorSheets.Core.Registries.SheetRegistry&lt;SheetEntity&gt;. Reference sheets (Companies,
/// Positions, Sites, Decisions, Interview Types/Outcomes, Schedules) are formula-calculated from the
/// Applications/Interviews data and are read-only.
/// </summary>
public static class JobSheetHelpers
{
    public static List<string> GetSheetNames()
    {
        return SheetsConfig.SheetUtilities.GetAllSheetNames();
    }

    /// <summary>
    /// The shared registry backing this domain's header/row-mapping/missing-column orchestration.
    /// </summary>
    public static SheetRegistry<SheetEntity> Registry => s_registry;

    private static readonly SheetRegistry<SheetEntity> s_registry = BuildRegistry();

    private static SheetRegistry<SheetEntity> BuildRegistry()
    {
        var registry = new SheetRegistry<SheetEntity>();

        registry.RegisterGeneric<SheetEntity, ApplicationEntity>(SheetsConfig.SheetNames.Applications, ApplicationSheet.GetSheet, (se, rows) => se.Sheets.Applications = rows);
        registry.RegisterGeneric<SheetEntity, InterviewEntity>(SheetsConfig.SheetNames.Interviews, InterviewSheet.GetSheet, (se, rows) => se.Sheets.Interviews = rows);
        registry.RegisterGeneric<SheetEntity, CompanyDetailEntity>(SheetsConfig.SheetNames.CompanyDetails, CompanyDetailSheet.GetSheet, (se, rows) => se.Sheets.CompanyDetails = rows);
        registry.RegisterGeneric<SheetEntity, PositionDetailEntity>(SheetsConfig.SheetNames.PositionDetails, PositionDetailSheet.GetSheet, (se, rows) => se.Sheets.PositionDetails = rows);
        registry.RegisterGeneric<SheetEntity, CompanyEntity>(SheetsConfig.SheetNames.Companies, CompanySheet.GetSheet, (se, rows) => se.Sheets.Companies = rows);
        registry.RegisterGeneric<SheetEntity, PositionEntity>(SheetsConfig.SheetNames.Positions, PositionSheet.GetSheet, (se, rows) => se.Sheets.Positions = rows);
        registry.RegisterGeneric<SheetEntity, SiteEntity>(SheetsConfig.SheetNames.Sites, SiteSheet.GetSheet, (se, rows) => se.Sheets.Sites = rows);
        registry.RegisterGeneric<SheetEntity, DecisionEntity>(SheetsConfig.SheetNames.Decisions, DecisionSheet.GetSheet, (se, rows) => se.Sheets.Decisions = rows);
        registry.RegisterGeneric<SheetEntity, InterviewTypeEntity>(SheetsConfig.SheetNames.InterviewTypes, InterviewTypeSheet.GetSheet, (se, rows) => se.Sheets.InterviewTypes = rows);
        registry.RegisterGeneric<SheetEntity, InterviewOutcomeEntity>(SheetsConfig.SheetNames.InterviewOutcomes, InterviewOutcomeSheet.GetSheet, (se, rows) => se.Sheets.InterviewOutcomes = rows);
        registry.RegisterGeneric<SheetEntity, ScheduleEntity>(SheetsConfig.SheetNames.Schedules, ScheduleSheet.GetSheet, (se, rows) => se.Sheets.Schedules = rows);
        registry.RegisterGeneric<SheetEntity, SetupEntity>(SheetsConfig.SheetNames.Setup, SetupSheet.GetSheet, (se, rows) => se.Sheets.Setup = rows);

        return registry;
    }

    public static List<SheetModel> GetMissingSheets(Spreadsheet spreadsheet)
    {
        return s_registry.GetMissingSheets(spreadsheet, GetSheetNames());
    }

    public static List<MessageEntity> CheckUnknownSheets(Spreadsheet spreadsheet)
    {
        return s_registry.CheckUnknownSheets(spreadsheet);
    }

    public static List<MessageEntity> CheckSheetHeaders(Spreadsheet spreadsheet)
    {
        return s_registry.CheckSheetHeaders(spreadsheet);
    }

    public static List<MessageEntity> CheckSheetHeaders(Spreadsheet spreadsheet, out Dictionary<string, List<ColumnInsertionInfo>> missingColumns)
    {
        return s_registry.CheckSheetHeaders(spreadsheet, out missingColumns);
    }

    public static Dictionary<string, List<ColumnInsertionInfo>> DetectMissingColumns(BatchGetValuesByDataFilterResponse response)
    {
        return s_registry.DetectMissingColumns(response);
    }

    public static SheetModel? GetSheetLayout(string sheetName)
    {
        return s_registry.GetSheetLayout(sheetName);
    }

    public static List<SheetModel> GetSheetLayouts(IEnumerable<string> sheetNames)
    {
        return s_registry.GetSheetLayouts(sheetNames);
    }

    /// <summary>
    /// Builds a data-validation rule from a header's validation string. Job stores the actual A1
    /// range (sheet names with spaces already single-quoted) directly on the column, so a range
    /// dropdown is produced via <see cref="GoogleValidationHelper.CreateOneOfRangeRule"/>.
    /// </summary>
    public static DataValidationRule GetDataValidation(string? validation)
    {
        if (string.IsNullOrEmpty(validation))
        {
            return new DataValidationRule();
        }

        return GoogleValidationHelper.CreateOneOfRangeRule(validation);
    }

    public static SheetEntity? MapData(Spreadsheet spreadsheet)
    {
        return s_registry.MapData(spreadsheet);
    }

    public static SheetEntity? MapData(BatchGetValuesByDataFilterResponse response)
    {
        return s_registry.MapData(response);
    }
}
