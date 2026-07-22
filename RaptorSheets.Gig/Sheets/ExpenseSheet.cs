using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;

namespace RaptorSheets.Gig.Sheets;

/// <summary>
/// Expense sheet definition - entity-driven only, no custom cross-sheet formulas.
/// </summary>
public static class ExpenseSheet
{
    internal static SheetModel BaseSheet => new()
    {
        Name = SheetsConfig.SheetNames.Expenses,
        TabColor = SheetColor.ORANGE,
        CellColor = SheetColor.LIGHT_RED,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<ExpenseEntity>()
    };

    public static SheetModel GetSheet()
    {
        return GenericSheetMapper<ExpenseEntity>.GetSheet(BaseSheet);
    }
}
