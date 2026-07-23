using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Home.Constants;
using RaptorSheets.Home.Entities;

namespace RaptorSheets.Home.Sheets;

/// <summary>
/// Power sheet definition. Entirely entity-driven, no custom formulas.
/// </summary>
public static class PowerSheet
{
    internal static SheetModel BaseSheet => new()
    {
        Name = SheetsConfig.SheetNames.Power,
        TabColor = SheetColor.RED,
        CellColor = SheetColor.LIGHT_RED,
        FontColor = SheetColor.WHITE,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<PowerEntity>()
    };

    public static SheetModel GetSheet()
    {
        return GenericSheetMapper<PowerEntity>.GetSheet(BaseSheet);
    }
}
