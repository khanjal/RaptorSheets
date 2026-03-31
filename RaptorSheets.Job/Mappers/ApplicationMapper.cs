using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Job.Constants;
using RaptorSheets.Job.Entities;
using RaptorSheets.Job.Helpers;

namespace RaptorSheets.Job.Mappers;

/// <summary>
/// Application mapper for configuring the Applications sheet with formulas, validations, and formatting.
/// This mapper leverages the GenericSheetMapper for entity-driven configuration.
/// </summary>
public static class ApplicationMapper
{
    /// <summary>
    /// Retrieves the configured Applications sheet.
    /// Includes formulas, validations, and formatting specific to the Applications sheet.
    /// </summary>
    public static SheetModel GetSheet()
    {
        return GenericSheetMapper<ApplicationEntity>.GetSheet(
            SheetsConfig.ApplicationSheet,
            ConfigureApplicationFormulas
        );
    }

    /// <summary>
    /// Configures formulas specific to the Applications sheet.
    /// This method handles formulas that cannot be defined at the entity level.
    /// </summary>
    /// <param name="sheet">The Applications sheet model to configure.</param>
    private static void ConfigureApplicationFormulas(SheetModel sheet)
    {
        // Note: UpdateColumns() has already been called by GenericSheetMapper
        // We can now safely use GetLocalRange methods

        var dateRange = sheet.GetLocalRange(SheetsConfig.HeaderNames.Date);
        var companyRange = sheet.GetLocalRange(SheetsConfig.HeaderNames.Company);
        var jobTitleRange = sheet.GetLocalRange(SheetsConfig.HeaderNames.JobTitle);

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
                        jobTitleRange);
                    break;

                case var _ when headerName == SheetsConfig.HeaderNames.InterviewCount:
                    var keyRange = sheet.GetLocalRange(SheetsConfig.HeaderNames.Key);
                    header.Formula = JobFormulaBuilder.BuildInterviewCountFormula(
                        dateRange,
                        SheetsConfig.HeaderNames.InterviewCount,
                        keyRange);
                    break;

                case var _ when headerName == SheetsConfig.HeaderNames.DaysActive:
                    var decisionDateRange = sheet.GetLocalRange(SheetsConfig.HeaderNames.DecisionDate);
                    header.Formula = JobFormulaBuilder.BuildDaysActiveFormula(
                        dateRange,
                        SheetsConfig.HeaderNames.DaysActive,
                        dateRange,
                        decisionDateRange);
                    break;

                case var _ when headerName == SheetsConfig.HeaderNames.PayAvg:
                    var payLowRange = sheet.GetLocalRange(SheetsConfig.HeaderNames.PayLow);
                    var payHighRange = sheet.GetLocalRange(SheetsConfig.HeaderNames.PayHigh);
                    header.Formula = JobFormulaBuilder.BuildPayAverageFormula(
                        dateRange,
                        SheetsConfig.HeaderNames.PayAvg,
                        payLowRange,
                        payHighRange);
                    break;

                default:
                    // All other configuration (notes, validations, formatting) handled by ColumnAttribute
                    break;
            }
        });
    }
}
