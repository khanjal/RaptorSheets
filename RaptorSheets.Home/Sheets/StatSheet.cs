using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Home.Constants;
using RaptorSheets.Home.Entities;

namespace RaptorSheets.Home.Sheets;

/// <summary>
/// Stat sheet definition. Entirely entity-driven, no custom formulas.
/// </summary>
public static class StatSheet
{
    internal static SheetModel BaseSheet => new()
    {
        Name = SheetsConfig.SheetNames.Stats,
        TabColor = SheetColor.PURPLE,
        CellColor = SheetColor.LIGHT_PURPLE,
        FontColor = SheetColor.WHITE,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<StatEntity>()
    };

    public static SheetModel GetSheet()
    {
        return GenericSheetMapper<StatEntity>.GetSheet(BaseSheet);
    }
}
