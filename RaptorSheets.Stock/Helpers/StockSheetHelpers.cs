using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Models;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Registries;
using RaptorSheets.Stock.Enums;
using RaptorSheets.Stock.Sheets;
using RaptorSheets.Stock.Entities;

namespace RaptorSheets.Stock.Helpers;

public static class StockSheetHelpers
{
    public static List<SheetModel> GetSheets()
    {
        var sheets = new List<SheetModel>
        {
            AccountSheet.GetSheet(),
            StockSheet.GetSheet(),
            TickerSheet.GetSheet()
        };

        return sheets;
    }

    /// <summary>
    /// The shared registry backing this domain's header/row-mapping/missing-column orchestration.
    /// Exposed so <see cref="RaptorSheets.Core.Managers.SheetManagerBase"/>'s generic
    /// GetSheetsCoreAsync/AutoHealMissingColumnsAsync can operate on it directly.
    /// </summary>
    public static SheetRegistry<SheetEntity> Registry => s_registry;

    private static readonly SheetRegistry<SheetEntity> s_registry = BuildRegistry();

    private static SheetRegistry<SheetEntity> BuildRegistry()
    {
        var registry = new SheetRegistry<SheetEntity>();

        // Register with each sheet's GetSheet() (not a bare header-only model) so the registry's
        // factory returns the real, formula-laden SheetModel - GetSheetLayout/RefreshHeaderFormulasAsync
        // and SheetRegistry.GetDependents' cross-sheet formula scan both rely on that, matching the
        // convention Gig/Job already follow (e.g. TripSheet.GetSheet passed directly to RegisterGeneric).
        registry.RegisterGeneric<SheetEntity, AccountEntity>(SheetName.ACCOUNTS.GetDescription(), AccountSheet.GetSheet,
            (se, rows) => se.Sheets.Accounts = rows);

        registry.RegisterGeneric<SheetEntity, StockEntity>(SheetName.STOCKS.GetDescription(), StockSheet.GetSheet,
            (se, rows) => se.Sheets.Stocks = rows);

        registry.RegisterGeneric<SheetEntity, TickerEntity>(SheetName.TICKERS.GetDescription(), TickerSheet.GetSheet,
            (se, rows) => se.Sheets.Tickers = rows);

        return registry;
    }

    internal static List<SheetModel> GetMissingSheets(Spreadsheet spreadsheet)
    {
        var canonicalNames = Enum.GetValues<SheetName>().Select(e => e.GetDescription());
        return s_registry.GetMissingSheets(spreadsheet, canonicalNames);
    }

    /// <summary>
    /// Checks a spreadsheet's tab names for sheets that don't correspond to any known Stock sheet.
    /// Only needs sheet tab metadata (no grid/cell data).
    /// </summary>
    internal static List<MessageEntity> CheckUnknownSheets(Spreadsheet spreadsheet)
    {
        return s_registry.CheckUnknownSheets(spreadsheet);
    }

    /// <summary>
    /// Full header validation against grid-data (IncludeGridData=true) spreadsheet metadata.
    /// </summary>
    internal static List<MessageEntity> CheckSheetHeaders(Spreadsheet spreadsheet)
    {
        return s_registry.CheckSheetHeaders(spreadsheet);
    }

    /// <summary>
    /// Same as <see cref="CheckSheetHeaders(Spreadsheet)"/>, but also reports which columns are
    /// missing entirely and where they should be inserted, for use with
    /// <see cref="RaptorSheets.Core.Helpers.ColumnInsertionHelper"/>.
    /// </summary>
    internal static List<MessageEntity> CheckSheetHeaders(Spreadsheet spreadsheet, out Dictionary<string, List<ColumnInsertionInfo>> missingColumns)
    {
        return s_registry.CheckSheetHeaders(spreadsheet, out missingColumns);
    }

    /// <summary>
    /// Detects columns missing entirely from a batchGet response, reusing the header row already
    /// present in each range - no extra API call. SheetId is left at 0; the caller fills it in.
    /// </summary>
    internal static Dictionary<string, List<ColumnInsertionInfo>> DetectMissingColumns(BatchGetValuesByDataFilterResponse response)
    {
        return s_registry.DetectMissingColumns(response);
    }

    public static SheetModel? GetSheetLayout(string sheetName)
    {
        return s_registry.GetSheetLayout(sheetName);
    }

    public static List<SheetModel> GetSheetLayouts(IEnumerable<string> sheetNames)
    {
        return s_registry.GetSheetLayouts(sheetNames);
    }

    public static DataValidationRule GetDataValidation(Validation validation)
    {
        return validation switch
        {
            Validation.BOOLEAN => GoogleValidationHelper.CreateBooleanRule(),
            Validation.RANGE_ACCOUNT or Validation.RANGE_TICKER
                => GoogleValidationHelper.CreateOneOfRangeRule($"{GetSheetForRange(validation)?.GetDescription()}!A2:A"),
            _ => new DataValidationRule()
        };
    }

    private static SheetName? GetSheetForRange(Validation validationEnum)
    {
        return validationEnum switch
        {
            Validation.RANGE_ACCOUNT => SheetName.ACCOUNTS,
            Validation.RANGE_TICKER => SheetName.TICKERS,
            _ => null
        };
    }

    internal static SheetEntity? MapData(BatchGetValuesByDataFilterResponse response)
    {
        return s_registry.MapData(response);
    }
}
