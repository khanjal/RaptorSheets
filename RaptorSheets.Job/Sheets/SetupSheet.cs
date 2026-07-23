using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Job.Constants;
using RaptorSheets.Job.Entities;

namespace RaptorSheets.Job.Sheets;

/// <summary>
/// Setup sheet definition - administrative/configuration sheet.
/// Entirely entity-driven, no custom formulas.
/// </summary>
public static class SetupSheet
{
    internal static SheetModel BaseSheet => new()
    {
        Name = SheetsConfig.SheetNames.Setup,
        TabColor = SheetColor.ORANGE,
        CellColor = SheetColor.LIGHT_YELLOW,
        FontColor = SheetColor.WHITE,
        FreezeColumnCount = 0,
        FreezeRowCount = 1,
        ProtectSheet = false,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<SetupEntity>()
    };

    public static SheetModel GetSheet()
    {
        return GenericSheetMapper<SetupEntity>.GetSheet(BaseSheet);
    }
}
