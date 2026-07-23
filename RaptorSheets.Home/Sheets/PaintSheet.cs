using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Home.Constants;
using RaptorSheets.Home.Entities;

namespace RaptorSheets.Home.Sheets;

/// <summary>
/// Paint sheet definition. Entirely entity-driven, no custom formulas.
/// </summary>
public static class PaintSheet
{
    internal static SheetModel BaseSheet => new()
    {
        Name = SheetsConfig.SheetNames.Paints,
        TabColor = SheetColor.MAGENTA,
        // LIGHT_RED, not PINK: PINK resolves to the exact same RGB as MAGENTA (SheetHelpers.GetColor),
        // which made the header row and alternating rows indistinguishable.
        CellColor = SheetColor.LIGHT_RED,
        FontColor = SheetColor.WHITE, // MAGENTA is a dark TabColor - see SheetColor for the dark/light list
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<PaintEntity>()
    };

    public static SheetModel GetSheet()
    {
        return GenericSheetMapper<PaintEntity>.GetSheet(BaseSheet);
    }
}
