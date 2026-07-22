using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Job.Constants;
using RaptorSheets.Job.Entities;

namespace RaptorSheets.Job.Sheets;

/// <summary>
/// Companies sheet definition - layout and formulas for the Companies sheet.
/// For data mapping operations, use GenericSheetMapper&lt;CompanyEntity&gt; directly.
/// </summary>
public static class CompanySheet
{
    internal static SheetModel BaseSheet => new()
    {
        Name = SheetsConfig.SheetNames.Companies,
        CellColor = SheetColor.LIGHT_CYAN,
        TabColor = SheetColor.CYAN,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<CompanyEntity>()
    };

    public static SheetModel GetSheet()
    {
        return GenericSheetMapper<CompanyEntity>.GetSheet(
            BaseSheet,
            ConfigureCompanyFormulas
        );
    }

    private static void ConfigureCompanyFormulas(SheetModel sheet)
    {
        var applicationSheet = ApplicationSheet.GetSheet();
        var interviewSheet = InterviewSheet.GetSheet();

        var companyRange = sheet.GetLocalRange(SheetsConfig.HeaderNames.Company);
        // Start source ranges at row 2 to exclude header row (matches Gig patterns)
        var appCompanyRange = applicationSheet.GetRange(SheetsConfig.HeaderNames.Company, 2);
        var intCompanyRange = interviewSheet.GetRange(SheetsConfig.HeaderNames.Company, 2);

        sheet.Headers.ForEach(header =>
        {
            var headerName = header!.Name.ToString()!.Trim();

            switch (headerName)
            {
                case var _ when headerName == SheetsConfig.HeaderNames.Company:
                    // Unique companies list with embedded header (Gig-style array literal)
                    header.Formula = GoogleFormulaBuilder.BuildArrayLiteralUniqueFilteredSorted(
                        SheetsConfig.HeaderNames.Company,
                        appCompanyRange);
                    break;

                case var _ when headerName == SheetsConfig.HeaderNames.ApplicationCount:
                    // Count applications per company with embedded header
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaCountIf(
                        companyRange,
                        SheetsConfig.HeaderNames.ApplicationCount,
                        appCompanyRange);
                    break;

                case var _ when headerName == SheetsConfig.HeaderNames.InterviewCount:
                    // Count interviews per company with embedded header
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaCountIf(
                        companyRange,
                        SheetsConfig.HeaderNames.InterviewCount,
                        intCompanyRange);
                    break;

                default:
                    break;
            }
        });
    }
}
