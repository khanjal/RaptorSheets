using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Job.Constants;
using RaptorSheets.Job.Entities;
using RaptorSheets.Job.Helpers;

namespace RaptorSheets.Job.Sheets;

/// <summary>
/// Schedules sheet definition - validation reference sheet for schedule dropdown values,
/// calculated from Applications and Interviews.
/// </summary>
public static class ScheduleSheet
{
    internal static SheetModel BaseSheet => new()
    {
        Name = SheetsConfig.SheetNames.Schedules,
        CellColor = SheetColor.LIGHT_GRAY,
        TabColor = SheetColor.LIGHT_GRAY,
        FreezeColumnCount = 0,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<ScheduleEntity>()
    };

    public static SheetModel GetSheet()
    {
        return GenericSheetMapper<ScheduleEntity>.GetSheet(BaseSheet, ConfigureScheduleFormulas);
    }

    private static void ConfigureScheduleFormulas(SheetModel sheet)
    {
        var appSheet = ApplicationSheet.GetSheet();
        var intSheet = InterviewSheet.GetSheet();

        var valueRange = sheet.GetLocalRange(SheetsConfig.HeaderNames.Schedule);
        var sourceRange = appSheet.GetRange(SheetsConfig.HeaderNames.Schedule, 2);
        var appKeyRange = appSheet.GetRange(SheetsConfig.HeaderNames.Key, 2);
        var intKeyRange = intSheet.GetRange(SheetsConfig.HeaderNames.Key, 2);

        ReferenceSheetFormulaHelper.ApplyReferenceCountFormulas(
            sheet,
            SheetsConfig.HeaderNames.Schedule,
            valueRange,
            sourceRange,
            sourceRange,
            ReferenceSheetFormulaHelper.BuildInterviewCountFromApplicationDimensionFormula(valueRange, sourceRange, appKeyRange, intKeyRange));
    }
}
