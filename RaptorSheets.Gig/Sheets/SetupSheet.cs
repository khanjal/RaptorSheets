using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;

namespace RaptorSheets.Gig.Sheets;

/// <summary>
/// Setup sheet definition - entity-driven only, no custom cross-sheet formulas.
/// </summary>
public static class SetupSheet
{
    internal static SheetModel BaseSheet => new()
    {
        Name = SheetsConfig.SheetNames.Setup,
        CellColor = SheetColor.LIGHT_PURPLE,
        TabColor = SheetColor.PURPLE,
        FontColor = SheetColor.WHITE,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        ProtectSheet = true,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<SetupEntity>()
    };

    public static SheetModel GetSheet()
    {
        return GenericSheetMapper<SetupEntity>.GetSheet(BaseSheet);
    }
}
