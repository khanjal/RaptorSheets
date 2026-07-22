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
/// Interview Types sheet definition - validation reference sheet for interview type dropdown
/// values, calculated from Interviews.
/// </summary>
public static class InterviewTypeSheet
{
    internal static SheetModel BaseSheet => new()
    {
        Name = SheetsConfig.SheetNames.InterviewTypes,
        CellColor = SheetColor.LIGHT_GRAY,
        TabColor = SheetColor.LIGHT_GRAY,
        FreezeColumnCount = 0,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<InterviewTypeEntity>()
    };

    public static SheetModel GetSheet()
    {
        return GenericSheetMapper<InterviewTypeEntity>.GetSheet(BaseSheet, ConfigureInterviewTypeFormulas);
    }

    private static void ConfigureInterviewTypeFormulas(SheetModel sheet)
    {
        var intSheet = InterviewSheet.GetSheet();

        var valueRange = sheet.GetLocalRange(SheetsConfig.HeaderNames.InterviewType);
        var sourceRange = intSheet.GetRange(SheetsConfig.HeaderNames.InterviewType, 2);
        var intKeyRange = intSheet.GetRange(SheetsConfig.HeaderNames.Key, 2);

        ReferenceSheetFormulaHelper.ApplyReferenceCountFormulas(
            sheet,
            SheetsConfig.HeaderNames.InterviewType,
            valueRange,
            sourceRange,
            ReferenceSheetFormulaHelper.BuildApplicationCountFromInterviewDimensionFormula(valueRange, sourceRange, intKeyRange),
            sourceRange);
    }
}
