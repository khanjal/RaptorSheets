using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Job.Constants;
using RaptorSheets.Job.Entities;

namespace RaptorSheets.Job.Sheets;

/// <summary>
/// Company Details sheet definition - optional user-entered details for a company.
/// Entirely entity-driven, no custom formulas.
/// </summary>
public static class CompanyDetailSheet
{
    internal static SheetModel BaseSheet => new()
    {
        Name = SheetsConfig.SheetNames.CompanyDetails,
        CellColor = SheetColor.LIGHT_PURPLE,
        TabColor = SheetColor.PURPLE,
        FontColor = SheetColor.WHITE,
        FreezeColumnCount = 1,
        FreezeRowCount = 1,
        ProtectSheet = false,
        Headers = EntitySheetConfigHelper.GenerateHeadersFromEntity<CompanyDetailEntity>()
    };

    public static SheetModel GetSheet()
    {
        return GenericSheetMapper<CompanyDetailEntity>.GetSheet(BaseSheet);
    }
}
