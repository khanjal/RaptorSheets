using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Models.Google;
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

        var positionRange = sheet.GetLocalRange(SheetsConfig.HeaderNames.Position);
        var appJobTitleRange = applicationSheet.GetRange(SheetsConfig.HeaderNames.JobTitle);

        sheet.Headers.ForEach(header =>
        {
            var headerName = header!.Name.ToString()!.Trim();

            switch (headerName)
            {
                case var _ when headerName == SheetsConfig.HeaderNames.Position:
                    // Get unique positions from Applications
                    header.Formula = $@"=ARRAYFORMULA(IF(LEN({appJobTitleRange})=0,"""",
                        UNIQUE(FILTER({appJobTitleRange},LEN({appJobTitleRange})>0))))";
                    break;

                case var _ when headerName == SheetsConfig.HeaderNames.ApplicationCount:
                    // Count applications for this position
                    header.Formula = $@"=ARRAYFORMULA(IF(LEN({positionRange})=0,"""",
                        COUNTIF({appJobTitleRange},{positionRange})))";
                    break;

                default:
                    break;
            }
        });
    }
}
