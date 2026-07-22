using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Job.Constants;
using RaptorSheets.Job.Entities;

namespace RaptorSheets.Job.Sheets;

/// <summary>
/// Position Details sheet definition - optional user-entered details for a position.
/// Entirely entity-driven, no custom formulas.
/// </summary>
public static class PositionDetailSheet
{
    internal static SheetModel BaseSheet => new()
    {
        Name = SheetsConfig.SheetNames.PositionDetails,
        CellColor = SheetColor.LIGHT_PURPLE,
        TabColor = SheetColor.PURPLE,
        FontColor = SheetColor.WHITE,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        ProtectSheet = false,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<PositionDetailEntity>()
    };

    public static SheetModel GetSheet()
    {
        return GenericSheetMapper<PositionDetailEntity>.GetSheet(BaseSheet);
    }
}
