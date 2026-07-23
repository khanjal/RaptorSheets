using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Home.Constants;
using RaptorSheets.Home.Entities;

namespace RaptorSheets.Home.Sheets;

/// <summary>
/// Maintenance Log sheet definition. Entirely entity-driven, no custom formulas.
/// </summary>
public static class MaintenanceSheet
{
    internal static SheetModel BaseSheet => new()
    {
        Name = SheetsConfig.SheetNames.Maintenance,
        TabColor = SheetColor.ORANGE,
        CellColor = SheetColor.LIGHT_RED,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<MaintenanceEntity>()
    };

    public static SheetModel GetSheet()
    {
        return GenericSheetMapper<MaintenanceEntity>.GetSheet(BaseSheet);
    }
}
