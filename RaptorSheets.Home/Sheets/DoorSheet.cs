using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Home.Constants;
using RaptorSheets.Home.Entities;

namespace RaptorSheets.Home.Sheets;

/// <summary>
/// Door sheet definition. Entirely entity-driven, no custom formulas.
/// </summary>
public static class DoorSheet
{
    internal static SheetModel BaseSheet => new()
    {
        Name = SheetsConfig.SheetNames.Doors,
        TabColor = SheetColor.DARK_YELLOW,
        CellColor = SheetColor.LIGHT_YELLOW,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<DoorEntity>()
    };

    public static SheetModel GetSheet()
    {
        return GenericSheetMapper<DoorEntity>.GetSheet(BaseSheet);
    }
}
