using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Home.Constants;
using RaptorSheets.Home.Entities;

namespace RaptorSheets.Home.Sheets;

/// <summary>
/// Room sheet definition. Configures the calculated Sq. Ft column (L x W).
/// For data mapping operations, use GenericSheetMapper&lt;RoomEntity&gt; directly.
/// </summary>
public static class RoomSheet
{
    internal static SheetModel BaseSheet => new()
    {
        Name = SheetsConfig.SheetNames.Rooms,
        TabColor = SheetColor.CYAN,
        CellColor = SheetColor.LIGHT_CYAN,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<RoomEntity>()
    };

    public static SheetModel GetSheet()
    {
        var sheet = BaseSheet;
        sheet.Headers.UpdateColumns();

        var lengthRange = sheet.GetLocalRange(SheetsConfig.HeaderNames.RoomLength);
        var widthRange = sheet.GetLocalRange(SheetsConfig.HeaderNames.RoomWidth);

        var squareFeet = sheet.Headers.FirstOrDefault(h => h.Name == SheetsConfig.HeaderNames.SquareFeet);
        if (squareFeet != null)
        {
            // Sq. Ft = L x W (spilling ARRAYFORMULA, keyed off the Length column)
            squareFeet.Formula = GoogleFormulaBuilder.WrapWithArrayFormula(
                lengthRange,
                SheetsConfig.HeaderNames.SquareFeet,
                $"IF(ISBLANK({widthRange}), \"\", {lengthRange}*{widthRange})");
            squareFeet.Format = Format.NUMBER;
        }

        return sheet;
    }
}
