using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Home.Constants;
using RaptorSheets.Home.Entities;

namespace RaptorSheets.Home.Sheets;

/// <summary>
/// Project sheet definition. Entirely entity-driven, no custom formulas.
/// </summary>
public static class ProjectSheet
{
    internal static SheetModel BaseSheet => new()
    {
        Name = SheetsConfig.SheetNames.Projects,
        TabColor = SheetColor.GREEN,
        CellColor = SheetColor.LIGHT_GREEN,
        FontColor = SheetColor.WHITE, // GREEN is a dark TabColor - see SheetColor for the dark/light list
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<ProjectEntity>()
    };

    public static SheetModel GetSheet()
    {
        return GenericSheetMapper<ProjectEntity>.GetSheet(BaseSheet);
    }
}
