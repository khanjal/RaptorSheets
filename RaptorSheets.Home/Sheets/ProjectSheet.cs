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
        // No explicit FontColor - GREEN is now Google's real bright/light Green swatch (#00ff00,
        // see issue #89's palette rebase), so the default BLACK reads fine.
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<ProjectEntity>()
    };

    public static SheetModel GetSheet()
    {
        return GenericSheetMapper<ProjectEntity>.GetSheet(BaseSheet);
    }
}
