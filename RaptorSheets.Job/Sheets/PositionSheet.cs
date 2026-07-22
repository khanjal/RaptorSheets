using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Job.Constants;
using RaptorSheets.Job.Entities;

namespace RaptorSheets.Job.Sheets;

/// <summary>
/// Positions sheet definition - layout and formulas for the Positions sheet.
/// For data mapping operations, use GenericSheetMapper&lt;PositionEntity&gt; directly.
/// </summary>
public static class PositionSheet
{
    internal static SheetModel BaseSheet => new()
    {
        Name = SheetsConfig.SheetNames.Positions,
        CellColor = SheetColor.LIGHT_CYAN,
        TabColor = SheetColor.CYAN,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<PositionEntity>()
    };

    public static SheetModel GetSheet()
    {
        return GenericSheetMapper<PositionEntity>.GetSheet(
            BaseSheet,
            ConfigurePositionFormulas
        );
    }

    private static void ConfigurePositionFormulas(SheetModel sheet)
    {
        var applicationSheet = ApplicationSheet.GetSheet();
        var interviewSheet = InterviewSheet.GetSheet();

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
