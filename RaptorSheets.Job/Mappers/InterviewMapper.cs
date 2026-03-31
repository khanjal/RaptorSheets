using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Job.Constants;
using RaptorSheets.Job.Entities;

namespace RaptorSheets.Job.Mappers;

/// <summary>
/// Interview mapper for configuring the Interviews sheet with formulas, validations, and formatting.
/// This mapper leverages the GenericSheetMapper for entity-driven configuration.
/// </summary>
public static class InterviewMapper
{
    /// <summary>
    /// Retrieves the configured Interviews sheet.
    /// Includes formulas, validations, and formatting specific to the Interviews sheet.
    /// </summary>
    public static SheetModel GetSheet()
    {
        return GenericSheetMapper<InterviewEntity>.GetSheet(
            SheetsConfig.InterviewSheet,
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

        sheet.Headers.ForEach(header =>
        {
            var headerName = header!.Name.ToString()!.Trim();

            switch (headerName)
            {
                case var _ when headerName == SheetsConfig.HeaderNames.Key:
                    // Formula to generate key matching application: Company-JobTitle-0 (default)
                    // Users should update the number to match their application if needed
                    header.Formula = $@"=ARRAYFORMULA(IF(LEN({companyRange})=0,"""",
                        {companyRange}&""-""&{jobTitleRange}&""-0""))";
                    break;

                case var _ when headerName == SheetsConfig.HeaderNames.InterviewRound:
                    // Calculate interview round by counting previous interviews for same key
                    var keyRange = sheet.GetLocalRange(SheetsConfig.HeaderNames.Key);
                    header.Formula = $@"=ARRAYFORMULA(IF(LEN({keyRange})=0,"""",
                        COUNTIFS({keyRange},{keyRange},
                                 ROW({keyRange}),""<=""&ROW({keyRange}))))";
                    break;

                default:
                    // All other configuration (notes, validations, formatting) handled by ColumnAttribute
                    break;
            }
        });
    }
}
