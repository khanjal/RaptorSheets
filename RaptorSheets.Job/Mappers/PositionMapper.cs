using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Job.Constants;
using RaptorSheets.Job.Entities;

namespace RaptorSheets.Job.Mappers;

/// <summary>
/// Position mapper for Positions sheet configuration.
/// For data mapping operations, use GenericSheetMapper<PositionEntity> directly.
/// </summary>
public static class PositionMapper
{
    public static SheetModel GetSheet()
    {
        return GenericSheetMapper<PositionEntity>.GetSheet(
            SheetsConfig.PositionSheet,
            ConfigurePositionFormulas
        );
    }

    private static void ConfigurePositionFormulas(SheetModel sheet)
    {
        var applicationSheet = ApplicationMapper.GetSheet();
        var interviewSheet = InterviewMapper.GetSheet();

        var positionRange = sheet.GetLocalRange(SheetsConfig.HeaderNames.Position);
        // Start source range at row 2 to exclude header row (consistent with Gig patterns)
        var appJobTitleRange = applicationSheet.GetRange(SheetsConfig.HeaderNames.JobTitle, 2);
        var intJobTitleRange = interviewSheet.GetRange(SheetsConfig.HeaderNames.JobTitle, 2);

        sheet.Headers.ForEach(header =>
        {
            var headerName = header!.Name.ToString()!.Trim();

            switch (headerName)
            {
                case var _ when headerName == SheetsConfig.HeaderNames.Position:
                    // Unique positions list with embedded header (Gig-style array literal)
                    header.Formula = GoogleFormulaBuilder.BuildArrayLiteralUniqueFilteredSorted(
                        SheetsConfig.HeaderNames.Position,
                        appJobTitleRange);
                    break;

                case var _ when headerName == SheetsConfig.HeaderNames.ApplicationCount:
                    // Count applications per position with embedded header
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaCountIf(
                        positionRange,
                        SheetsConfig.HeaderNames.ApplicationCount,
                        appJobTitleRange);
                    break;

                case var _ when headerName == SheetsConfig.HeaderNames.InterviewCount:
                    // Count interviews per position with embedded header
                    header.Formula = GoogleFormulaBuilder.BuildArrayFormulaCountIf(
                        positionRange,
                        SheetsConfig.HeaderNames.InterviewCount,
                        intJobTitleRange);
                    break;

                default:
                    break;
            }
        });
    }
}
