using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Home.Constants;

namespace RaptorSheets.Home.Mappers;

/// <summary>
/// Room mapper for Room sheet configuration. Configures the calculated Sq. Ft column (L x W).
/// For data mapping operations, use GenericSheetMapper&lt;RoomEntity&gt; directly.
/// </summary>
public static class RoomMapper
{
    public static SheetModel GetSheet()
    {
        var sheet = SheetsConfig.RoomSheet;
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
            squareFeet.Format = FormatEnum.NUMBER;
        }

        return sheet;
    }
}
