using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Models;
using RaptorSheets.Gig.Helpers;

namespace RaptorSheets.Gig.Managers;

/// <summary>
/// Header-validation entry points for the Gig domain.
///
/// Sheet properties, tab names, layouts, and missing-column insertion are inherited from
/// <see cref="RaptorSheets.Core.Managers.GoogleSheetManagerBase{TEntity}"/>. Only the static
/// header/unknown-sheet checks live here, since callers use them statically off the type (no manager
/// instance / no credentials needed) - they're thin shims over <see cref="GigSheetHelpers"/>.
/// </summary>
public partial class GoogleSheetManager
{
    #region Header Validation

    /// <summary>
    /// Checks a spreadsheet's tab names for sheets that don't correspond to any known Gig sheet.
    /// Only needs sheet tab metadata (no grid/cell data), so it's safe to call with a cheap
    /// <c>GetSheetInfo()</c> (no ranges) result. Known-sheet header validation (missing/renamed/
    /// reordered columns) is handled separately, per-sheet, using data already fetched via batchGet.
    /// </summary>
    public static List<MessageEntity> CheckUnknownSheets(Spreadsheet sheetInfoResponse)
    {
        return GigSheetHelpers.CheckUnknownSheets(sheetInfoResponse);
    }

    public static List<MessageEntity> CheckSheetHeaders(Spreadsheet sheetInfoResponse)
    {
        return GigSheetHelpers.CheckSheetHeaders(sheetInfoResponse);
    }

    /// <summary>
    /// Same as <see cref="CheckSheetHeaders(Spreadsheet)"/>, but also reports which columns are
    /// missing entirely and where they should be inserted, for use with <see cref="InsertMissingColumns"/>.
    /// </summary>
    public static List<MessageEntity> CheckSheetHeaders(Spreadsheet sheetInfoResponse, out Dictionary<string, List<ColumnInsertionInfo>> missingColumns)
    {
        return GigSheetHelpers.CheckSheetHeaders(sheetInfoResponse, out missingColumns);
    }

    #endregion
}
