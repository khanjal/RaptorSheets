using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Job.Constants;
using RaptorSheets.Job.Entities;

namespace RaptorSheets.Job.Mappers;

/// <summary>
/// Company mapper for Companies sheet configuration.
/// For data mapping operations, use GenericSheetMapper<CompanyEntity> directly.
/// </summary>
public static class CompanyMapper
{
    public static SheetModel GetSheet()
    {
        return GenericSheetMapper<CompanyEntity>.GetSheet(
            SheetsConfig.CompanySheet,
            ConfigureCompanyFormulas
        );
    }

    private static void ConfigureCompanyFormulas(SheetModel sheet)
    {
        var applicationSheet = ApplicationMapper.GetSheet();
        var interviewSheet = InterviewMapper.GetSheet();

        var companyRange = sheet.GetLocalRange(SheetsConfig.HeaderNames.Company);
        var appCompanyRange = applicationSheet.GetRange(SheetsConfig.HeaderNames.Company);
        var intCompanyRange = interviewSheet.GetRange(SheetsConfig.HeaderNames.Company);

        sheet.Headers.ForEach(header =>
        {
            var headerName = header!.Name.ToString()!.Trim();

            switch (headerName)
            {
                case var _ when headerName == SheetsConfig.HeaderNames.Company:
                    // Get unique companies from Applications
                    header.Formula = $@"=ARRAYFORMULA(IF(LEN({appCompanyRange})=0,"""",
                        UNIQUE(FILTER({appCompanyRange},LEN({appCompanyRange})>0))))";
                    break;

                case var _ when headerName == SheetsConfig.HeaderNames.ApplicationCount:
                    // Count applications for this company
                    header.Formula = $@"=ARRAYFORMULA(IF(LEN({companyRange})=0,"""",
                        COUNTIF({appCompanyRange},{companyRange})))";
                    break;

                case var _ when headerName == SheetsConfig.HeaderNames.InterviewCount:
                    // Count interviews for this company
                    header.Formula = $@"=ARRAYFORMULA(IF(LEN({companyRange})=0,"""",
                        COUNTIF({intCompanyRange},{companyRange})))";
                    break;

                default:
                    break;
            }
        });
    }
}
