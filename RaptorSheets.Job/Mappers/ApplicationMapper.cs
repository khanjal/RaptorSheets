using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Job.Constants;
using RaptorSheets.Job.Entities;

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
        var dateRange = sheet.GetLocalRange(SheetsConfig.HeaderNames.Date);
        var companyRange = sheet.GetLocalRange(SheetsConfig.HeaderNames.Company);
        var jobTitleRange = sheet.GetLocalRange(SheetsConfig.HeaderNames.JobTitle);

        // Get Interview sheet for counting
        var interviewSheet = InterviewMapper.GetSheet();
        var interviewKeyRange = interviewSheet.GetRange(SheetsConfig.HeaderNames.Key);

        sheet.Headers.ForEach(header =>
        {
            var headerName = header!.Name.ToString()!.Trim();

            switch (headerName)
            {
                case var _ when headerName == SheetsConfig.HeaderNames.Key:
                    // Formula to generate unique key: Company-JobTitle-0 (default)
                    // Users can modify the number if they have multiple applications for same company/position
                    header.Formula = $@"=ARRAYFORMULA(IF(LEN({companyRange})=0,"""",
                        {companyRange}&""-""&{jobTitleRange}&""-0""))";
                    break;

                case var _ when headerName == SheetsConfig.HeaderNames.InterviewCount:
                    // Count interviews for this application based on key match
                    var keyRange = sheet.GetLocalRange(SheetsConfig.HeaderNames.Key);
                    header.Formula = $@"=ARRAYFORMULA(IF(LEN({keyRange})=0,"""",
                        COUNTIF({interviewKeyRange},{keyRange})))";
                    break;

                case var _ when headerName == SheetsConfig.HeaderNames.DaysActive:
                    // Calculate days between application and decision date
                    // If no decision date, calculate to today
                    var decisionDateRange = sheet.GetLocalRange(SheetsConfig.HeaderNames.DecisionDate);
                    header.Formula = $@"=ARRAYFORMULA(IF(LEN({dateRange})=0,"""",
                        IF(LEN({decisionDateRange})>0,
                            {decisionDateRange}-{dateRange},
                            TODAY()-{dateRange})))";
                    break;

                case var _ when headerName == SheetsConfig.HeaderNames.PayAvg:
                    // Calculate average of low and high pay
                    var payLowRange = sheet.GetLocalRange(SheetsConfig.HeaderNames.PayLow);
                    var payHighRange = sheet.GetLocalRange(SheetsConfig.HeaderNames.PayHigh);
                    header.Formula = $@"=ARRAYFORMULA(IF(LEN({dateRange})=0,"""",
                        IF(AND(ISNUMBER({payLowRange}),ISNUMBER({payHighRange})),
                            ({payLowRange}+{payHighRange})/2,
                            """")))";
                    break;

                default:
                    // All other configuration (notes, validations, formatting) handled by ColumnAttribute
                    break;
            }
        });
    }
}
