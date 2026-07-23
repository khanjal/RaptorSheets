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
/// Interviews sheet definition - layout and formulas for the Interviews sheet.
/// This sheet leverages the GenericSheetMapper for entity-driven configuration.
/// </summary>
public static class InterviewSheet
{
    internal static SheetModel BaseSheet => new()
    {
        Name = SheetsConfig.SheetNames.Interviews,
        CellColor = SheetColor.LIGHT_GRAY,
        TabColor = SheetColor.GREEN,
        FontColor = SheetColor.WHITE,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        ProtectSheet = false,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<InterviewEntity>()
    };

    /// <summary>
    /// Retrieves the configured Interviews sheet.
    /// Includes formulas, validations, and formatting specific to the Interviews sheet.
    /// </summary>
    public static SheetModel GetSheet()
    {
        return GenericSheetMapper<InterviewEntity>.GetSheet(
            BaseSheet,
            ConfigureInterviewFormulas
        );
    }

    /// <summary>
    /// Configures formulas specific to the Interviews sheet.
    /// This method handles formulas that cannot be defined at the entity level.
    /// </summary>
    /// <param name="sheet">The Interviews sheet model to configure.</param>
    private static void ConfigureInterviewFormulas(SheetModel sheet)
    {
        var dateRange = sheet.GetLocalRange(SheetsConfig.HeaderNames.Date);
        var companyRange = sheet.GetLocalRange(SheetsConfig.HeaderNames.Company);
        var jobTitleRange = sheet.GetLocalRange(SheetsConfig.HeaderNames.JobTitle);
        var duplicateRange = sheet.GetLocalRange(SheetsConfig.HeaderNames.Duplicate);

        sheet.Headers.ForEach(header =>
        {
            var headerName = header!.Name.ToString()!.Trim();

            switch (headerName)
            {
                case var _ when headerName == SheetsConfig.HeaderNames.Key:
                    header.Formula = JobFormulaBuilder.BuildKeyFormula(
                        dateRange,
                        SheetsConfig.HeaderNames.Key,
                        companyRange,
                        jobTitleRange,
                        duplicateRange);
                    break;

                case var _ when headerName == SheetsConfig.HeaderNames.InterviewRound:
                    var keyRange = sheet.GetLocalRange(SheetsConfig.HeaderNames.Key);
                    header.Formula = JobFormulaBuilder.BuildInterviewRoundFormula(
                        dateRange,
                        SheetsConfig.HeaderNames.InterviewRound,
                        keyRange);
                    break;

                case var _ when headerName == SheetsConfig.HeaderNames.Duplicate:
                    // Duplicate count per company+jobtitle on Interviews sheet
                    header.Formula = JobFormulaBuilder.BuildDuplicateCountFormula(
                        dateRange,
                        SheetsConfig.HeaderNames.Duplicate,
                        companyRange,
                        jobTitleRange);
                    break;

                default:
                    // All other configuration (notes, validations, formatting) handled by ColumnAttribute
                    break;
            }
        });
    }
}
