using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Home.Constants;
using RaptorSheets.Home.Entities;

namespace RaptorSheets.Home.Sheets;

/// <summary>
/// Contact sheet definition. Entirely entity-driven, no custom formulas.
/// </summary>
public static class ContactSheet
{
    internal static SheetModel BaseSheet => new()
    {
        Name = SheetsConfig.SheetNames.Contacts,
        TabColor = SheetColor.CYAN,
        CellColor = SheetColor.LIGHT_CYAN,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<ContactEntity>()
    };

    public static SheetModel GetSheet()
    {
        return GenericSheetMapper<ContactEntity>.GetSheet(BaseSheet);
    }
}
